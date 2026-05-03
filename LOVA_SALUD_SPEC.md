# LOVA SALUD — Clinical Center Management System
## Technical Specification & Implementation Roadmap v1.0
> Prepared: 2026-05-02 | Status: Draft for Client Review

---

## TABLE OF CONTENTS

1. [Executive Overview](#1-executive-overview)
2. [System Architecture Overview](#2-system-architecture-overview)
3. [Core Modules](#3-core-modules)
   - 3.1 Dashboard
   - 3.2 Patient Management
   - 3.3 Billing & Payments
   - 3.4 Consults
   - 3.5 Medicaments & Inventory
   - 3.6 Follow-ups & Scheduling
4. [Admin & HR Modules](#4-admin--hr-modules)
   - 4.1 HR Panel
   - 4.2 Caja Control (Cash Register)
   - 4.3 Reports & Analytics
   - 4.4 Settings & System Configuration
5. [Role-Based Access Control Matrix](#5-role-based-access-control-matrix)
6. [Database Architecture](#6-database-architecture)
7. [Security & Compliance Framework](#7-security--compliance-framework)
8. [Recommended Tech Stack](#8-recommended-tech-stack)
9. [Phased Implementation Roadmap](#9-phased-implementation-roadmap)
10. [Open Questions & Client Discussion Items](#10-open-questions--client-discussion-items)

---

## 1. Executive Overview

**Lova Salud** is a multi-specialty clinical center management platform designed to digitize and streamline every operational layer of a modern healthcare facility—from patient intake and specialist consultations to billing, inventory, and HR management.

### Vision Statement
Provide clinical staff with the right information at the right moment, reduce administrative friction, and give management real-time visibility into operational and financial performance—all within a secure, HIPAA-conscious architecture designed for the Dominican Republic healthcare context.

### Core Design Principles
1. **Role-First UX** — Every user sees only what they need; no cognitive overload.
2. **Dynamic Clinical Records** — Consult forms adapt per specialty; no one-size-fits-all template.
3. **Audit-Everything** — Every data mutation is logged with actor, timestamp, and delta.
4. **Offline-Resilient** — Critical workflows (patient lookup, consult creation) must tolerate momentary connectivity loss.
5. **Caribbean-First Payments** — Native integration path for CardNET and Azul Dominican payment processors.

### User Roles (System-Wide)
| Role | Primary Persona |
|------|----------------|
| **Specialist / Doctor** | Cardiologist, Odontologist, GP, any clinical specialist |
| **Receptionist** | Front-desk staff; intake, billing, scheduling |
| **Lab Technician** | Executes laboratory/imaging orders |
| **Admin** | Clinic owner/manager; financials, HR, system config |
| **Patient** *(Phase 3)* | Self-service portal, appointment booking |

---

## 2. System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                             │
│  Web App (Next.js)   Mobile PWA (responsive)   Patient Portal  │
└───────────────────────────┬─────────────────────────────────────┘
                            │ HTTPS / JWT
┌───────────────────────────▼─────────────────────────────────────┐
│                      API GATEWAY (NestJS)                       │
│   Auth  │  Rate Limiting  │  Request Logging  │  CORS           │
└──────┬──────────┬──────────┬──────────┬────────┬────────────────┘
       │          │          │          │        │
   Patients   Consults   Billing   Inventory  HR/Admin
   Service    Service    Service   Service    Service
       │          │          │          │        │
┌──────▼──────────▼──────────▼──────────▼────────▼────────────────┐
│                    PostgreSQL (Primary DB)                       │
│  JSONB for dynamic consult fields │ Row-level security policies │
└─────────────────────┬───────────────────────────────────────────┘
                      │
        ┌─────────────┴─────────────┐
    Redis Cache               S3-Compatible Storage
    (Sessions, Queues)        (Medical images, invoices, documents)
```

### Key Architectural Decisions
- **Clean Architecture** (already established in this repository — Application, Domain, Persistence, Identity layers)
- **CQRS pattern** for consult and billing operations (clear separation of reads vs. writes)
- **JSONB specialty fields** in PostgreSQL for dynamic consult forms (avoids EAV complexity while preserving query capability)
- **Event-driven notifications** via background queue (email/SMS follow-ups, low-stock alerts)

---

## 3. Core Modules

---

### 3.1 Dashboard

The dashboard is the system's "mission control." Content adapts entirely to the authenticated user's role.

#### Dashboard Views by Role

**Specialist Dashboard**
- Today's consultation queue (ordered by appointment time or check-in order)
- Next patient card: name, age, consultation type, wait time
- Active consults in progress (own)
- Pending laboratory results awaiting review
- Upcoming follow-up appointments (next 48 hours)
- Quick-action: "Start Consult" for next in queue
- Notification feed: prescription ready for pickup, lab results arrived, patient no-show

**Receptionist Dashboard**
- Today's appointment timeline (all doctors)
- Check-in queue: patients present but not yet in consultation
- Pending billing: consults with status `FINISHED` awaiting payment
- Insurance verifications pending
- Quick-action: "Register Patient", "Create Appointment", "Process Payment"

**Lab Technician Dashboard**
- Pending lab orders queue (ordered by priority)
- In-progress tests
- Results ready for upload
- Imaging orders (X-ray, ultrasound, etc.) pending

**Admin Dashboard**
- Financial KPIs (today / week / month):
  - Total revenue collected
  - Outstanding balances
  - Revenue by specialist
  - Revenue by service type
  - Insurance vs. out-of-pocket split
- Operational KPIs:
  - Patients attended today vs. capacity
  - Average consult duration by doctor
  - No-show rate (daily/weekly)
  - New vs. returning patient ratio
- Inventory alerts: medications at or below reorder threshold
- HR highlights: upcoming payroll date, pending hires, open positions
- Caja status: current drawer balance vs. expected
- Charts:
  - Revenue trend (30-day line chart)
  - Consultations by specialty (donut chart)
  - Patient volume heatmap (hour × day-of-week)
  - Top 10 diagnoses this month

#### Dashboard Infrastructure
- Widgets are configurable per user (drag-to-reorder, show/hide, Phase 2)
- Data refreshes every 60 seconds via WebSocket push or polling
- All KPI widgets support date-range filtering

---

### 3.2 Patient Management

Access: **Specialist, Receptionist, Lab Technician** (read/write scoped). Admin has read-only for audit purposes.

#### Patient Record Fields

**Personal Information**
| Field | Type | Rules |
|-------|------|-------|
| First Name | string | Required |
| Last Name | string | Required |
| Document Type | enum | `CEDULA`, `PASSPORT`, `FOREIGN_ID` |
| Document Number | string | Unique per document type; validated format per type |
| Nationality | string | ISO 3166-1 alpha-2; drives document type validation |
| Date of Birth | date | Required; auto-calculates age |
| Is Minor | computed | `age < 18` at time of registration |
| Sex | enum | `MALE`, `FEMALE`, `OTHER` |
| Home Address | text | Required |
| Phone (Primary) | string | Required; E.164 format |
| Phone (Secondary) | string | Optional |
| Email | string | Optional; used for notifications and portal access |
| Blood Type | enum | `A+`, `A-`, `B+`, `B-`, `AB+`, `AB-`, `O+`, `O-`, `UNKNOWN` |
| Allergies | text array | Free-text tags; critical field, highlighted in consult view |
| Chronic Conditions | text array | Pre-existing diagnoses |

**Guardian Information (Minors Only — shown when `Is Minor = true`)**
| Field | Type | Rules |
|-------|------|-------|
| Guardian First Name | string | Required for minors |
| Guardian Last Name | string | Required for minors |
| Guardian Document Type | enum | Required; ID used for billing |
| Guardian Document Number | string | Required |
| Guardian Relationship | enum | `MOTHER`, `FATHER`, `LEGAL_GUARDIAN`, `OTHER` |
| Guardian Phone | string | Required for minors |
| Guardian Email | string | Preferred for minor notifications |

**Insurance Information**
| Field | Type | Rules |
|-------|------|-------|
| Has Insurance | boolean | Default: false |
| Insurance Company | FK → InsuranceCompany | Required if has insurance |
| Policy Number | string | Required if has insurance |
| Policy Holder Name | string | Required if different from patient |
| Coverage Percentage | decimal | Drives billing calculations |
| Insurance Card Image | file | Stored in S3; JPEG/PNG/PDF |

**Patient Status**
Proposed status lifecycle:

| Status | Description | Transitions To |
|--------|-------------|----------------|
| `ACTIVE` | Default; fully active patient | Any |
| `INACTIVE` | No visits in 12+ months; auto-set or manual | ACTIVE |
| `SUSPENDED` | Outstanding balance; restricted to emergency | ACTIVE (on payment) |
| `TRANSFERRED` | Referred to another facility; records exportable | ACTIVE, ARCHIVED |
| `DECEASED` | Record preserved for legal/insurance purposes | (terminal) |
| `ARCHIVED` | Administratively closed; no new consults allowed | ACTIVE (admin only) |
| `PENDING_VERIFICATION` | New registration, documents not yet confirmed | ACTIVE |

**Patient Timeline (on patient profile)**
Chronological feed showing: all consults, payments, lab results, follow-ups, and document uploads. Filterable by category and date range.

**Duplicate Detection**
On document number entry: system queries existing records. If match found, show alert with link to existing patient before allowing creation.

---

### 3.3 Billing & Payments

Access: **Receptionist** (primary), **Admin** (full access + reports). Specialists see billing status of their own patients but cannot modify.

#### Billing Workflow

```
Consult Created → [PENDING]
     │
     ▼
Doctor Works on Consult → [IN_PROGRESS]
     │
     ▼
Doctor Marks Complete → [FINISHED]   ◄── Required before billing unlocks
     │
     ▼
Receptionist Opens Billing → Invoice Generated → [AWAITING_PAYMENT]
     │
     ├── Full Payment → [PAID] → Receipt generated
     ├── Partial Payment → [PARTIALLY_PAID] → Balance tracked
     └── Insurance Claim Filed → [PENDING_INSURANCE] → Awaiting reimbursement
          │
          ├── Insurance Pays → [PAID]
          └── Denied → [REQUIRES_COLLECTION]
```

#### Invoice Composition

An invoice is automatically assembled from all billable items attached to the consult:

| Line Item Category | Examples |
|--------------------|---------|
| Consultation Fee | "General Consultation — Dr. González — RD$1,500" |
| Procedures Performed | "ECG Interpretation — RD$800" |
| Laboratory Tests | "Complete Blood Count — RD$600" |
| Imaging | "Chest X-Ray — RD$1,200" |
| Medications Dispensed | "Amoxicillin 500mg × 10 — RD$350" |
| Specialist Referral Fee | "Cardiology Referral — RD$500" |

**Line Item Fields**: service name, unit price, quantity, discount applied, insurance coverage amount, patient responsibility amount.

**Invoice Header**: clinic name, RNC (fiscal ID), patient name, insurance policy, date, invoice number (sequential), attending physician, total, tax (ITBIS if applicable).

#### Payment Processing
- **Phase 1**: Manual payment recording (cash, card-present via POS terminal, bank transfer). Receptionist selects payment method and enters amount.
- **Phase 2**: CardNET and Azul payment gateway API integration for card-not-present and e-payment from patient portal.
- Partial payments tracked with running balance.
- Multiple payment methods per invoice allowed (e.g., insurance covers 80%, patient pays 20% cash).

#### Receipt / Ticket Generation
- PDF receipt generated on payment confirmation.
- Fiscal receipt format compliant with DGII (Dominican Tax Authority) electronic invoice requirements (Phase 2).
- Receipt sent to patient email if on file.
- Reprint available from invoice history.

---

### 3.4 Consults

The clinical core of the system. This module is the most complex due to specialty-specific dynamic fields.

#### Consult Lifecycle

```
Create Consult → [OPEN]
     │
     ▼ Doctor opens
[IN_PROGRESS]
     │
     ├── Save draft at any time → remains IN_PROGRESS
     ▼
Doctor submits → [FINISHED]
     │
     ├── Triggers billing unlock
     └── Triggers follow-up scheduling
```

**Guard: Pending Consult Alert**
When creating a new consult for a patient who already has a consult in `OPEN` or `IN_PROGRESS` state, the system displays:
> "This patient has an active consult from [date] with [doctor]. Do you want to continue creating a new consult?"
> [View Existing] [Create New Anyway]

#### Quick Patient Registration (Shortcut)
If the selected patient does not exist, a compact inline form allows creating a minimal patient record (Name, Document, DOB, Phone) directly from the consult creation screen. The record is flagged `PENDING_VERIFICATION` and must be completed later.

#### Consult Structure (All Specialties)

Every consult has a **Common Tab** and a **Specialty Tab**.

##### Common Tab (All Specialties)
| Section | Fields |
|---------|--------|
| **Vital Signs** | Weight (kg), Height (cm), BMI (auto-calculated), Blood Pressure (systolic/diastolic), Heart Rate (bpm), Temperature (°C), O₂ Saturation (%), Respiratory Rate |
| **Chief Complaint** | Text; reason for today's visit |
| **Clinical Notes / Observations** | Rich text; persists across consults (cumulative patient notes) |
| **Current Medications** | Pre-filled from patient record; can be updated here |
| **Allergies Reminder** | Read-only alert banner from patient record |
| **Diagnosis** | ICD-10 code selector + description; supports multiple diagnoses |
| **Treatment Plan** | Rich text; treatment prescribed this visit |
| **Referrals** | Specialist referral fields (to whom, urgency, reason) |
| **Next Appointment** | Date/time picker; auto-creates follow-up record |

##### Specialty Tab — Dynamic Fields by Specialty

**Cardiologist**
| Field | Type |
|-------|------|
| Cardiac Risk Score (Framingham) | calculated |
| Chest Pain Characteristics | multi-select: exertional, rest, pleuritic, positional |
| Chest Pain Scale (1-10) | numeric |
| Palpitations | boolean + text |
| Dyspnea Classification (NYHA) | enum: Class I–IV |
| Peripheral Edema | boolean + location |
| ECG Findings | structured text + image upload |
| Echocardiogram Findings | structured text |
| Ejection Fraction (%) | numeric |
| Cardiac Medications Review | table |
| Stress Test Results | text + file upload |
| Risk Factors | checkboxes: hypertension, diabetes, smoking, family history, obesity, hyperlipidemia |

**Odontologist**
| Field | Type |
|-------|------|
| Dental Chart | interactive SVG tooth map (32 teeth); per-tooth status: healthy, decayed, filled, crowned, missing, implant |
| Procedure Type | enum: cleaning, extraction, filling, root canal, crown, implant, orthodontic adjustment, whitening, surgery |
| Tooth Number(s) Treated | reference to dental chart selection |
| Anesthesia Used | enum: none, local-lidocaine, local-articaine, sedation |
| Anesthesia Quantity (mL) | numeric |
| X-Ray Taken | boolean |
| Dental X-Ray Images | file upload |
| Periodontal Pocket Depth | per-tooth numeric table |
| Plaque Index | numeric |
| Gingival Bleeding Index | numeric |
| Prosthetics Notes | text |
| Next Procedure Plan | text + scheduled date |

**General Practitioner**
| Field | Type |
|-------|------|
| Review of Systems | checkboxes by system: cardiovascular, respiratory, GI, neurological, musculoskeletal, dermatological, psychiatric |
| Physical Exam Findings | per-system text notes |
| Pediatric Growth Percentile | shown only if patient is minor |
| Vaccination History | table: vaccine name, date, batch |
| Vaccination Due | auto-calculated recommendations |
| Sick Note | boolean; generates printable sick leave document |
| Sick Note Duration (days) | numeric; shown if sick note = true |

**Lab Technician**
| Field | Type |
|-------|------|
| Test Type | enum: hematology, biochemistry, urinalysis, microbiology, serology, endocrinology, imaging, other |
| Individual Tests Ordered | multi-select per type (e.g., CBC, CMP, TSH, HbA1c) |
| Sample Type | enum: blood, urine, stool, swab, tissue biopsy, saliva |
| Sample Collection Date/Time | datetime |
| Sample Collected By | FK → user |
| External Lab Sent | boolean + lab name if true |
| Results Entry | per-test: value, unit, reference range, flag (normal/low/high/critical) |
| Result Status | enum: PENDING, PARTIAL, COMPLETE |
| Result Files | PDF/image upload (lab report) |
| Critical Value Alert | auto-triggers notification to ordering physician if value flagged CRITICAL |

**Nurse / Triage**
| Field | Type |
|-------|------|
| Triage Level | enum: 1-Resuscitation, 2-Emergent, 3-Urgent, 4-Less Urgent, 5-Non-Urgent |
| Pain Scale (0-10) | numeric |
| Chief Complaint (triage notes) | text |
| Pre-triage vitals | (same as common vitals block) |
| Administered Medications | table |

##### Medical Prescription Tab
| Feature | Detail |
|---------|--------|
| Medication search | Searches internal inventory + RxNorm/local formulary |
| Per-medication fields | Drug name, presentation (tablet/capsule/syrup/injection), dose, frequency, route of administration, duration (days), quantity to dispense, special instructions |
| Drug interaction check | Alert if two prescribed drugs have known major interactions |
| Duplicate therapy check | Alert if prescribing same drug already in patient's current medications |
| Print/PDF prescription | Clinic letterhead, doctor name, DEA/license number, patient name, date, signature block |
| E-Prescription | Phase 3 — digital signature, pharmacy routing |
| Inventory deduction trigger | On consult finalization: reduces inventory stock for dispensed items |

##### Laboratory & Imaging Orders Tab
| Feature | Detail |
|---------|--------|
| Order creation | Doctor selects from test catalog; sets priority (routine, urgent, STAT) |
| Order routing | Auto-assigns to available lab technician queue |
| Status tracking | Pending → In Progress → Results Available |
| Result review | Doctor receives notification; must mark result as "Reviewed" |
| Imaging orders | X-ray, ultrasound, CT, MRI, mammogram — with body region and clinical indication |
| PACS integration | Phase 2 — DICOM image viewer embedded in consult |
| Result files | Any filetype; stored in S3; viewable inline |

##### Images & Attachments Tab
- Upload: photos, wound images, consult-specific documents
- EXIF metadata stripped on upload for privacy
- Images watermarked with patient ID + date (server-side)
- Max file size: 50MB per file, 500MB per consult

---

### 3.5 Medicaments & Inventory

Access: **Specialist** (view + prescribe), **Receptionist** (view), **Admin** (full CRUD).

#### Medication Record Fields
| Field | Type |
|-------|------|
| Drug Name (Generic) | string |
| Brand Name(s) | array of strings |
| ATC Code | string (WHO classification) |
| Presentation | enum: tablet, capsule, syrup, injection, cream, drops, inhaler, suppository, patch |
| Concentration / Strength | string (e.g., "500mg", "250mg/5mL") |
| Unit of Measure | enum: tablet, mL, vial, tube, blister, bottle |
| Current Stock | integer |
| Minimum Stock Threshold | integer |
| Reorder Quantity | integer |
| Storage Location | string (shelf/bin/refrigerator) |
| Requires Refrigeration | boolean |
| Controlled Substance | boolean + classification |
| Supplier | FK → Supplier |
| Cost Price (RD$) | decimal |
| Sale Price (RD$) | decimal |
| Expiration Date | date (earliest expiring batch) |
| Batch Number | string |
| Notes | text |

#### Stock Management

**Stock Adjustment Types**
| Type | Trigger |
|------|---------|
| Purchase Receipt | Manual entry by admin; increases stock |
| Prescription Dispensed | Auto-deducted when consult is finalized |
| Manual Adjustment | Admin corrects discrepancy; requires reason note |
| Expiry Write-off | Admin marks expired stock removed |
| Return to Supplier | Admin records supplier return |

**Alert System**
| Alert | Threshold | Display |
|-------|-----------|---------|
| Low Stock Warning | Current ≤ Minimum Threshold | Yellow badge on medication card and admin dashboard widget |
| Out of Stock | Current = 0 | Red card; blocked from prescription selection |
| Expiring Soon | Expiration ≤ 30 days | Orange badge |
| Expired | Expiration < today | Red badge; auto-disabled from prescribing |

All inventory alerts appear in:
1. Medication list page (inline badge)
2. Admin dashboard widget ("Inventory Alerts")
3. Daily summary email to admin (configurable)

#### Prescription → Inventory Deduction Flow
1. Doctor prescribes medication in consult (status: OPEN or IN_PROGRESS)
2. On consult finalization (`FINISHED`), system checks stock for each prescribed item
3. If sufficient: stock decremented, transaction logged
4. If insufficient: warning surfaced to receptionist at billing time; flagged in prescription record

---

### 3.6 Follow-ups & Scheduling

Access: **Specialist** (own patients), **Receptionist** (all), **Admin** (view).

#### Follow-up Record
| Field | Type |
|-------|------|
| Patient | FK → Patient |
| Originating Consult | FK → Consult |
| Assigned Doctor | FK → User |
| Scheduled Date/Time | datetime |
| Follow-up Type | enum: ROUTINE_CHECKUP, RESULT_REVIEW, POST_PROCEDURE, PRESCRIPTION_REVIEW, SPECIALIST_REFERRAL |
| Status | enum: SCHEDULED, CONFIRMED, ATTENDED, NO_SHOW, CANCELLED, RESCHEDULED |
| Notes | text |
| Notification Sent | boolean + timestamp |

#### Scheduling Interface
- Calendar view (day/week/month) per doctor or all doctors
- Color-coded by specialty and status
- Drag-to-reschedule (Phase 2)
- Conflict detection: alerts if doctor already booked at selected time
- Buffer time configuration: admin sets minimum gap between appointments per specialty

#### Automated Notifications

| Event | Channel | Timing |
|-------|---------|--------|
| Appointment booked | Email + WhatsApp* | Immediately |
| Appointment reminder | Email + SMS | 24 hours before |
| Appointment reminder | WhatsApp* | 2 hours before |
| No-show detected | Internal alert → Receptionist | 30 min after missed time |
| No-show follow-up to patient | Email + SMS | Same day |
| Lab results available | Email + in-app notification | On result upload |
| Prescription ready for pickup | SMS + in-app | On dispensing confirmation |
| Critical lab value | Push notification + email → Doctor | Immediately |

*WhatsApp via official Business API (Twilio or Meta direct — confirm channel with client).

**Notification Templates**: all messages use configurable templates with variables (`{patient_name}`, `{doctor_name}`, `{appointment_date}`, `{clinic_phone}`).

#### No-Show Management
1. Receptionist manually marks appointment `NO_SHOW` or system auto-suggests after configurable window
2. System queues outreach notification to patient
3. Outreach log tracked on patient record
4. Repeated no-shows flagged on patient profile (configurable threshold, e.g., 3 no-shows)
5. Doctor can add note: "Discussed no-show policy"

---

## 4. Admin & HR Modules

---

### 4.1 HR Panel

Access: **Admin only**.

#### Employee Record
| Field | Type |
|-------|------|
| Full Name | string |
| National ID (Cédula) | string |
| Role | enum: DOCTOR, RECEPTIONIST, LAB_TECHNICIAN, NURSE, ADMIN, CLEANING, SECURITY |
| Specialty (if clinical) | FK → Specialty |
| Medical License Number | string |
| Start Date | date |
| Employment Type | enum: FULL_TIME, PART_TIME, CONTRACT, INTERN |
| Department | string |
| Direct Supervisor | FK → Employee |
| Salary (RD$) | decimal |
| Pay Frequency | enum: WEEKLY, BIWEEKLY, MONTHLY |
| Next Pay Date | date (auto-calculated) |
| Bank Account | string (encrypted at rest) |
| Emergency Contact | name + phone |
| Documents | file array: contract, certifications, DGT card, ID copy |
| Status | enum: ACTIVE, ON_LEAVE, TERMINATED, PROBATION |

#### Performance Metrics (per clinical staff, rolling 30 days)
| KPI | Calculation |
|-----|-------------|
| Patients Attended | Count of `FINISHED` consults |
| Average Consult Duration | Avg(consult_finished_at − consult_started_at) |
| No-show Rate (assigned patients) | no_shows / total_scheduled |
| Patient Satisfaction Score | Avg of post-visit survey responses (Phase 2) |
| Revenue Generated | Sum of invoices linked to doctor's consults |
| Prescriptions Written | Count of prescription records |
| Lab Orders Placed | Count of lab orders |
| Follow-up Compliance Rate | follow_ups_attended / follow_ups_scheduled |
| Avg Time to Result Review | Avg(result_reviewed_at − result_uploaded_at) |

**Performance Dashboard**: tabular view with trend sparklines per KPI; sortable. Export to CSV/PDF.

#### Payroll Register
- Payroll record per pay period per employee: base salary, bonuses, deductions (AFP, ARS, ISR as per Dominican law), net pay, payment date, payment method, reference number.
- Payroll approval workflow: Admin reviews → approves → marks paid.
- Monthly payroll summary exportable for accountant.

#### Recruitment Tracker
| Stage | Fields |
|-------|--------|
| APPLICANT | Name, applied date, position, CV file |
| INTERVIEW_SCHEDULED | Interview date, interviewer |
| INTERVIEWED | Score, notes |
| OFFER_SENT | Offer amount, date sent |
| HIRED | Converts to Employee record |
| REJECTED | Rejection reason |

---

### 4.2 Caja Control (Cash Register / Cash Drawer)

Access: **Receptionist** (own shift), **Admin** (full).

#### Shift Management
- **Open Shift**: Receptionist logs starting cash amount in drawer.
- **Close Shift**: Receptionist counts cash; system compares to expected amount based on transactions.
- **Shift Report**: auto-generated PDF showing all transactions, discrepancies, and closing balance.

#### Transaction Types
| Type | Effect |
|------|--------|
| Payment Received (Cash) | + cash |
| Payment Received (Card) | + card, no cash movement |
| Bank Transfer Received | + digital |
| Cash Refund | − cash |
| Cash Advance to Staff | − cash + requires admin approval |
| Petty Cash Expense | − cash + receipt upload required |

#### Discrepancy Handling
- If close-shift cash ≠ expected: discrepancy record created with difference amount.
- Admin notified of all discrepancies above configurable threshold (e.g., RD$200).
- Discrepancy resolved by admin: explanation note required.

---

### 4.3 Reports & Analytics

Access: **Admin** (all reports). **Specialist** (own clinical reports only).

#### Standard Report Catalog

**Financial Reports**
| Report | Period | Key Metrics |
|--------|--------|-------------|
| Daily Revenue Summary | Daily | Revenue by payment method, by doctor, by service type |
| Accounts Receivable Aging | On-demand | Outstanding balances bucketed: 0-30, 31-60, 61-90, 90+ days |
| Insurance Claims Status | Weekly | Submitted, pending, approved, denied, amounts |
| Revenue by Specialty | Monthly | Breakdown per department |
| Invoice History | Date range | Full invoice list, filterable, exportable |
| Payment Method Distribution | Monthly | Cash vs. card vs. insurance vs. transfer |

**Clinical Reports**
| Report | Period | Key Metrics |
|--------|--------|-------------|
| Consultations Volume | Daily/Weekly/Monthly | Total, by specialty, by doctor |
| Diagnosis Frequency | Monthly | Top diagnoses by ICD-10 code |
| Prescription Analysis | Monthly | Most prescribed medications, total units dispensed |
| Lab Turnaround Time | Monthly | Order-to-result time by test type |
| No-show Analysis | Weekly | Rate by doctor, day of week, time of day |
| Patient Demographics | On-demand | Age distribution, sex ratio, nationality, insurance coverage |
| New vs. Returning Patients | Monthly | Acquisition and retention trends |

**Inventory Reports**
| Report | Period |
|--------|--------|
| Current Stock Levels | On-demand |
| Medications Below Threshold | Daily alert |
| Expiring Medications (next 30/60/90 days) | Weekly |
| Consumption vs. Purchase Trend | Monthly |
| Controlled Substance Log | Monthly (compliance) |

**HR Reports**
| Report | Period |
|--------|--------|
| Staff Performance Summary | Monthly |
| Attendance & Leave Register | Monthly |
| Payroll History | Per pay period |
| Employee Headcount by Role | On-demand |

**Export Formats**: all reports support PDF and CSV/Excel download. Scheduled email delivery configurable per report.

---

### 4.4 Settings & System Configuration

Access: **Admin** (all). **Specialist/Receptionist** (personal settings only).

#### Clinic Settings
- Clinic name, logo, RNC (fiscal ID), address, phone, email
- Operating hours per day of week
- Holidays calendar
- Default appointment duration per specialty
- Time zone
- Currency (RD$ default, configurable)
- ITBIS/tax rate configuration

#### User & Role Management
- Create / deactivate users
- Assign roles and specialty
- Reset passwords (admin-initiated)
- Audit: last login, session history
- Two-factor authentication toggle per user

#### Notification Settings
- Enable/disable notification channels (email, SMS, WhatsApp) globally
- Configure Twilio / SMTP credentials
- Customize notification templates with variable substitution
- Notification quiet hours (e.g., no SMS between 10pm–8am)

#### Insurance Companies Master List
- Name, contact, policy types offered, claim submission instructions
- Coverage percentage defaults per plan type

#### Specialty & Consult Template Management
- Admin can define which fields appear on each specialty's consult tab
- Field types: text, number, enum, boolean, date, file upload, multi-select
- Field ordering and grouping by section
- Required vs. optional designation per field

#### Appearance / Theming
- Light / Dark mode toggle (per user preference, persisted)
- Clinic accent color
- Dashboard widget layout configuration (Phase 2)
- Language: Spanish (default), English (Phase 2 internationalization)

---

## 5. Role-Based Access Control Matrix

Legend: **C** = Create, **R** = Read, **U** = Update, **D** = Delete, **—** = No Access

| Module / Action | Admin | Receptionist | Specialist | Lab Tech | Patient (P3) |
|----------------|-------|-------------|------------|----------|--------------|
| **Dashboard** | Full | Own role | Own role | Own role | Own |
| Patient — Create | — | C | C | — | — |
| Patient — View | R | R | R | R | Own |
| Patient — Update | — | U | U | — | Limited own |
| Patient — Delete / Archive | D | — | — | — | — |
| Patient — Status Change | U | U (limited) | — | — | — |
| Consult — Create | — | C | C | — | — |
| Consult — View | R | R | R | R | Own |
| Consult — Edit (clinical) | — | — | U | — | — |
| Consult — Finalize | — | — | U | — | — |
| Consult — Lab orders | — | — | CRU | CRU | — |
| Consult — Lab results entry | — | — | R | CRUD | — |
| Consult — Delete | D | — | — | — | — |
| Prescription — Create | — | — | C | — | — |
| Prescription — View | R | R | R | R | Own |
| Billing — Create invoice | — | C | — | — | — |
| Billing — Process payment | — | U | — | — | — |
| Billing — View | R | R | Own | — | Own |
| Billing — Refund | U | — | — | — | — |
| Inventory — View | R | R | R | R | — |
| Inventory — Add/Edit | CRUD | — | — | — | — |
| Inventory — Prescribe deduction | — | — | U | — | — |
| Follow-ups — View | R | R | R | — | Own |
| Follow-ups — Create/Edit | — | CRU | CRU | — | — |
| Follow-ups — Send notification | U | U | U | — | — |
| HR — Employee records | CRUD | — | — | — | — |
| HR — Performance view | R | — | Own | — | — |
| HR — Payroll | CRUD | — | — | — | — |
| Caja — Open/Close shift | R | CRU | — | — | — |
| Caja — Transactions | CRUD | CRU | — | — | — |
| Reports — Financial | R | — | — | — | — |
| Reports — Clinical | R | — | Own | — | — |
| Reports — HR | R | — | — | — | — |
| Settings — Clinic | CRUD | — | — | — | — |
| Settings — Users | CRUD | — | — | — | — |
| Settings — Notifications | CRUD | — | — | — | — |
| Settings — Personal | R/U | R/U | R/U | R/U | R/U |
| Audit Log — View | R | — | — | — | — |

---

## 6. Database Architecture

### Technology Choice
**PostgreSQL 15+** with JSONB for dynamic specialty fields. This provides:
- Native JSON querying (no separate EAV tables)
- GIN indexes for fast JSONB searches
- Full ACID compliance
- Row-level security policies for multi-tenant extension

### Core Entity Relationships

```
Patient ─────────┬──── Consult ──────┬──── Prescription
                 │         │          ├──── LabOrder ──── LabResult
                 │         │          ├──── ConsultImage
                 │         │          └──── SpecialtyData (JSONB)
                 │         │
                 ├──── FollowUp
                 ├──── Invoice ───── InvoiceLineItem
                 │         └────────── Payment
                 └──── PatientInsurance
                 
Employee ────────┬──── Shift
                 ├──── PayrollRecord
                 └──── PerformanceSnapshot

Medication ──────┬──── StockTransaction
                 └──── PrescriptionItem

Specialty ───────┴──── ConsultFieldTemplate
                 └──── FieldDefinition (drives JSONB schema)
```

### JSONB Dynamic Fields Strategy

```sql
-- Specialty field template definition
CREATE TABLE specialty_field_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    specialty_id UUID REFERENCES specialties(id),
    field_key VARCHAR(100) NOT NULL,
    field_label VARCHAR(200) NOT NULL,
    field_type VARCHAR(50) NOT NULL, -- text|number|boolean|enum|date|file|multi_select
    field_options JSONB,             -- enum values, validation rules, etc.
    is_required BOOLEAN DEFAULT false,
    display_order INTEGER,
    section_name VARCHAR(100),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Consult stores specialty-specific data in JSONB column
CREATE TABLE consults (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID NOT NULL REFERENCES patients(id),
    specialty_id UUID NOT NULL REFERENCES specialties(id),
    doctor_id UUID NOT NULL REFERENCES users(id),
    status VARCHAR(20) NOT NULL DEFAULT 'OPEN',
    
    -- Common clinical fields (typed columns for query performance)
    chief_complaint TEXT,
    weight_kg DECIMAL(5,2),
    height_cm DECIMAL(5,2),
    bmi DECIMAL(4,1) GENERATED ALWAYS AS (weight_kg / ((height_cm/100)^2)) STORED,
    bp_systolic SMALLINT,
    bp_diastolic SMALLINT,
    heart_rate SMALLINT,
    temperature_c DECIMAL(4,1),
    o2_saturation DECIMAL(4,1),
    respiratory_rate SMALLINT,
    diagnosis_codes TEXT[],       -- ICD-10 array
    treatment_notes TEXT,
    observations TEXT,            -- cumulative cross-consult notes
    
    -- Dynamic specialty-specific fields
    specialty_data JSONB NOT NULL DEFAULT '{}',
    
    -- Metadata
    started_at TIMESTAMPTZ,
    finished_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES users(id)
);

-- GIN index for JSONB field search
CREATE INDEX idx_consults_specialty_data ON consults USING GIN (specialty_data);

-- Example cardiologist specialty_data:
-- {
--   "cardiac_risk_score": 14.2,
--   "chest_pain_type": ["exertional"],
--   "chest_pain_scale": 6,
--   "nyha_class": "II",
--   "ejection_fraction": 55,
--   "ecg_findings": "Normal sinus rhythm",
--   "risk_factors": ["hypertension", "diabetes"]
-- }

-- Example odontologist specialty_data:
-- {
--   "dental_chart": {"11": "decayed", "12": "filled", "16": "missing"},
--   "procedure_type": "extraction",
--   "teeth_treated": [11],
--   "anesthesia": "local-articaine",
--   "anesthesia_ml": 1.8,
--   "xray_taken": true,
--   "periodontal_depths": {"11": 3, "12": 2}
-- }
```

### Audit Trail

```sql
CREATE TABLE audit_log (
    id BIGSERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    record_id UUID NOT NULL,
    action VARCHAR(10) NOT NULL,  -- INSERT | UPDATE | DELETE
    changed_by UUID REFERENCES users(id),
    changed_at TIMESTAMPTZ DEFAULT NOW(),
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT
);
-- Populated by PostgreSQL triggers on every sensitive table
```

### Key Indexes

```sql
-- Patient lookups
CREATE INDEX idx_patients_document ON patients(document_type, document_number);
CREATE INDEX idx_patients_name ON patients USING GIN (to_tsvector('spanish', first_name || ' ' || last_name));
CREATE INDEX idx_patients_status ON patients(status) WHERE status != 'ARCHIVED';

-- Consult queries
CREATE INDEX idx_consults_patient_status ON consults(patient_id, status);
CREATE INDEX idx_consults_doctor_date ON consults(doctor_id, created_at DESC);
CREATE INDEX idx_consults_date ON consults(created_at DESC);

-- Inventory
CREATE INDEX idx_medications_low_stock ON medications(current_stock) WHERE current_stock <= minimum_threshold;
CREATE INDEX idx_medications_expiring ON medications(expiration_date) WHERE expiration_date > NOW();

-- Follow-ups
CREATE INDEX idx_followups_scheduled ON followups(scheduled_date, status) WHERE status = 'SCHEDULED';
CREATE INDEX idx_followups_doctor ON followups(assigned_doctor_id, scheduled_date);
```

---

## 7. Security & Compliance Framework

### Authentication & Authorization
- **JWT** access tokens (15-minute expiry) + refresh tokens (7-day, rotating, stored HTTP-only cookie)
- **Two-factor authentication** (TOTP via authenticator app) optional in Phase 1, required for admin in Phase 2
- **Account lockout** after 5 failed login attempts; 15-minute cooldown
- **Session invalidation** on role change or password reset
- **IP allowlisting** for admin accounts (configurable, Phase 2)

### Data Protection
- All data in transit: **TLS 1.3** minimum
- All data at rest: **AES-256** database encryption
- PII fields (document numbers, bank accounts, addresses): **application-level encryption** using separate key; key rotation supported
- S3 storage: server-side encryption enabled; pre-signed URLs with expiry for file access (never public URLs)
- Passwords: **bcrypt** with cost factor ≥ 12 (or Argon2id)

### Access Controls
- **Row-Level Security (RLS)** in PostgreSQL: doctors can only see consults they created or are assigned to; receptionists see all non-clinical data
- **Field-level masking**: partial document number masking in list views (e.g., "***-456789-8")
- **Minimum privilege principle**: every service account scoped to only required tables

### Audit & Compliance
- **Immutable audit log**: every INSERT/UPDATE/DELETE on patient, consult, billing, prescription, and user tables logged with old/new values
- **Audit log retention**: 7 years (healthcare compliance standard)
- **GDPR/privacy**: patient data export (JSON) and deletion-by-anonymization available (name/contact replaced with "ANONYMIZED", clinical data retained per legal requirement)
- **Controlled substance log**: separate report with non-editable records for regulatory inspection

### HIPAA-Analogous Controls (Dominican Context)
While the Dominican Republic follows Ley 64-00 (General Health Law) and Ley 172-13 (Personal Data Protection), aligning with HIPAA provides a strong international benchmark:
- **Business Associate Agreements (BAA)** with cloud providers (AWS/GCP)
- **Minimum necessary disclosure**: staff see only the patient data required for their role
- **Breach notification procedure**: defined incident response plan (document and implement in Phase 2)
- **Physical safeguards**: session timeout (15 min inactivity auto-logout), screen lock

### Vulnerability Mitigation
| Threat | Mitigation |
|--------|-----------|
| SQL Injection | Parameterized queries only; ORM query builder |
| XSS | CSP headers; React DOM escaping; sanitize rich text input with DOMPurify |
| CSRF | SameSite=Strict cookies; CSRF token on state-changing requests |
| IDOR | UUID primary keys (non-sequential); ownership validated server-side on every request |
| File Upload Attacks | File type whitelist (MIME + extension); antivirus scan on upload; stored outside webroot |
| Brute Force | Rate limiting (100 req/min per IP); account lockout |
| Secrets Exposure | Secrets in environment variables only; `.env` never committed; secrets rotation policy |

---

## 8. Recommended Tech Stack

> Based on the existing .NET Clean Architecture repository structure.

### Backend (Already Established)
| Layer | Technology |
|-------|-----------|
| Framework | **ASP.NET Core 8** (Web API) |
| Architecture | Clean Architecture (Domain / Application / Persistence / Identity / WebAPI) |
| ORM | **Entity Framework Core 8** |
| Database | **PostgreSQL 15+** |
| Authentication | **ASP.NET Core Identity** + **JWT Bearer** |
| Caching | **Redis** (session store, query cache, rate limiting) |
| Background Jobs | **Hangfire** (notifications, scheduled reports, inventory alerts) |
| File Storage | **AWS S3** or compatible (MinIO for self-hosted) |
| Validation | **FluentValidation** |
| Mapping | **AutoMapper** or **Mapster** |
| Logging | **Serilog** → structured JSON → Seq or ELK stack |
| Testing | **xUnit** + **Moq** + **Respawn** (integration tests with real DB) |

### Frontend (Proposed)
| Concern | Technology |
|---------|-----------|
| Framework | **Next.js 14+** (App Router) |
| Language | **TypeScript** (strict mode) |
| UI Component Library | **shadcn/ui** + **Tailwind CSS** |
| State Management | **Zustand** (global); React Query / **TanStack Query** (server state) |
| Forms | **React Hook Form** + **Zod** (schema validation, mirrors backend) |
| Charts / Analytics | **Recharts** or **Tremor** |
| Calendar | **FullCalendar** (follow-ups / appointment scheduling) |
| Rich Text Editor | **Tiptap** (clinical notes, treatment plans) |
| Dental Chart | Custom SVG component (interactive tooth map) |
| PDF Generation | **react-pdf** (client-side) + **QuestPDF** (server-side for invoices) |
| Real-time Updates | **SignalR** (ASP.NET native WebSocket abstraction) |
| Testing | **Vitest** + **React Testing Library** + **Playwright** (E2E) |

### Infrastructure
| Concern | Technology |
|---------|-----------|
| Containerization | **Docker** + **Docker Compose** (dev) |
| Orchestration | **Docker Swarm** (small scale) or **Kubernetes** (Phase 3 scale) |
| CI/CD | **GitHub Actions** |
| Reverse Proxy | **Nginx** or **Caddy** |
| Monitoring | **Grafana** + **Prometheus** |
| Error Tracking | **Sentry** |
| Email | **SendGrid** or **Amazon SES** |
| SMS / WhatsApp | **Twilio** |

---

## 9. Phased Implementation Roadmap

### MVP — Phase 1 (Months 1–4)
**Goal**: Working clinical operations for launch day.

| Module | MVP Scope |
|--------|-----------|
| Auth & RBAC | Login, JWT, 4 roles, basic permissions |
| Patient Management | Full CRUD, minor guardian logic, all status types |
| Consults | Common tab + 2 specialties (GP + 1 requested), prescriptions, lab orders |
| Lab Results | Tech enters results; doctor reviews; file upload |
| Inventory | Basic CRUD, stock deduction on consult finalize, low-stock alerts |
| Billing | Invoice generation, manual payment recording, PDF receipt |
| Follow-ups | Create/view/update; manual notification send |
| Dashboard | Role-based widgets (consultation queue, pending billing, inventory alerts) |
| Settings | Clinic info, user management, basic notification config |
| Audit Log | Core tables covered |

### Phase 2 — Months 5–8
**Goal**: Complete the clinical suite, add financial depth.

| Addition | Detail |
|----------|--------|
| All Specialty Consult Tabs | Cardiology, Odontology + any additional specialties |
| Dynamic Consult Template Builder | Admin configures fields per specialty |
| Automated Notifications | Email + SMS via Twilio for follow-ups, reminders |
| Insurance Billing Workflow | Claim submission, status tracking, partial payment |
| Caja Control | Shift management, discrepancy detection |
| Full HR Panel | Employee records, performance metrics, payroll register |
| Reports Dashboard | All standard reports, CSV/PDF export |
| Drug Interaction Checking | API integration (RxNorm or local formulary) |
| Patient Portal (read-only) | View own records, upcoming appointments |
| 2FA for Admin | TOTP enforcement |
| PACS-Lite | Image viewer for uploaded DICOM files |

### Phase 3 — Months 9–14
**Goal**: Automation, patient engagement, and scale.

| Addition | Detail |
|----------|--------|
| CardNET / Azul Integration | Card-not-present payments; patient portal checkout |
| Patient Portal (interactive) | Appointment booking, prescription history, invoice payment |
| E-Prescriptions | Digital signature, pharmacy routing (requires Dominican eHealth standard compliance) |
| WhatsApp Business API | Appointment confirmations, prescription pickup alerts |
| Mobile App (PWA) | Responsive web wraps as installable app; offline consult data entry |
| Patient Satisfaction Surveys | Post-visit automated survey; results fed to HR KPIs |
| AI-Assisted Triage | Symptom → triage level suggestion (requires regulatory guidance) |
| Multi-Branch Support | Single deployment serves multiple clinic locations |
| HL7 FHIR API | Interoperability with Dominican health network (SISALRIL/SRS) |
| Advanced Analytics | Cohort analysis, predictive appointment fill rate, revenue forecasting |
| Scheduled Report Delivery | Admin configures email delivery cadence per report |

---

## 10. Open Questions & Client Discussion Items

The following items require client input before development begins or during Phase 1 sprint planning:

| # | Topic | Question | Impact |
|---|-------|----------|--------|
| 1 | **Specialties at launch** | Which specialties are present at clinic on Day 1? Need field requirements per specialty before building consult templates. | Blocks consult module development |
| 2 | **Lab in-house vs. external** | Does the clinic run its own lab, or send samples to external labs? Affects lab order routing and result entry workflow. | Affects lab technician module scope |
| 3 | **Insurance companies** | Which insurance companies (ARS) does the clinic accept? Provide list with coverage % defaults and claim submission process. | Required for billing configuration |
| 4 | **Notification channels** | Confirm which channels to activate: email only for Phase 1, or include SMS (Twilio)? WhatsApp desired for Phase 1 or 2? | Affects infrastructure cost and setup |
| 5 | **Fiscal receipts (NCF)** | Is the clinic required to issue NCF (Número de Comprobante Fiscal) per DGII? This significantly affects invoice generation. | Affects billing module and DGII e-Invoice API integration |
| 6 | **Patient portal** | Is a patient-facing portal in scope for Phase 1/2, or deferred entirely? | Determines whether patient role is built early |
| 7 | **Payroll calculation** | Should the system calculate deductions (AFP, ARS, ISR) automatically, or is payroll net amount entered manually? | Affects HR payroll complexity |
| 8 | **Controlled substances** | Does the clinic handle controlled substances? Requires separate ledger and stricter audit controls. | Compliance requirement |
| 9 | **Multi-location** | Is this for a single clinic location now? Future expansion planned? Affects data model decisions made in Phase 1. | Architecture decision (schema design) |
| 10 | **Dental chart** | Confirm with odontologist: Adult (32-tooth FDI notation) only, or also pediatric (primary teeth)? | Affects dental chart component design |
| 11 | **Existing data** | Is there patient or consult data in a current system (paper, Excel, another software) that needs to be migrated? | Requires data migration sprint |
| 12 | **Performance review cycle** | How frequently does admin run HR performance reviews? What threshold triggers a formal warning vs. commendation? | HR module configuration |
| 13 | **Appointment slot length defaults** | What is the standard appointment duration per specialty? (e.g., GP = 20 min, Cardiology = 45 min) | Calendar / scheduling configuration |
| 14 | **Concurrent users** | Expected number of simultaneous users at peak? Affects infrastructure sizing decisions. | Hosting and performance requirements |

---

*Document version 1.0 — For review and approval by Lova Salud stakeholders prior to sprint kickoff.*

*This specification represents the technical interpretation of the client's stated vision. All business rule decisions marked as [CLIENT DECISION REQUIRED] must be confirmed before the relevant module enters active development.*
