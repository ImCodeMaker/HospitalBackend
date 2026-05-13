using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Infrastructure.Identity.Entities;
using HospitalApp.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalApp.Infrastructure.Persistence.Seeds;

public static class MockDataSeeder
{
    private static readonly Random _rng = new(42);

    public static async Task SeedAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        if (await db.Patients.AnyAsync())
        {
            logger.LogInformation("Mock data: patients already exist, skipping.");
            return;
        }

        logger.LogInformation("Mock data: starting bulk seed…");

        // ── Specialties ──
        var specialties = new[]
        {
            new Specialty { Name = "Medicina General", Code = "GEN", Type = SpecialtyTypeEnum.GeneralPractitioner, DefaultConsultDurationMinutes = 30 },
            new Specialty { Name = "Cardiología", Code = "CAR", Type = SpecialtyTypeEnum.Cardiologist, DefaultConsultDurationMinutes = 45 },
            new Specialty { Name = "Odontología", Code = "ODO", Type = SpecialtyTypeEnum.Odontologist, DefaultConsultDurationMinutes = 60 },
            new Specialty { Name = "Pediatría", Code = "PED", Type = SpecialtyTypeEnum.GeneralPractitioner, DefaultConsultDurationMinutes = 30 },
            new Specialty { Name = "Ginecología", Code = "GIN", Type = SpecialtyTypeEnum.GeneralPractitioner, DefaultConsultDurationMinutes = 45 },
        };
        await db.Specialties.AddRangeAsync(specialties);
        await db.SaveChangesAsync();

        // ── Insurance companies ──
        var insurers = new[]
        {
            new InsuranceCompany { Name = "ARS Humano", ContactEmail = "contacto@humano.com.do", ContactPhone = "809-555-0100" },
            new InsuranceCompany { Name = "ARS Universal", ContactEmail = "info@universal.com.do", ContactPhone = "809-555-0200" },
            new InsuranceCompany { Name = "SeNaSa", ContactEmail = "info@senasa.gob.do", ContactPhone = "809-555-0300" },
        };
        await db.InsuranceCompanies.AddRangeAsync(insurers);
        await db.SaveChangesAsync();

        // ── Doctor / staff users ──
        var doctorSeeds = new (string email, string first, string last, Guid? specId, string roleName, EmployeeRoleEnum empRole)[]
        {
            ("dr.garcia@lovasalud.com", "Carmen", "García", specialties[0].Id, ApplicationRole.Roles.Doctor, EmployeeRoleEnum.Doctor),
            ("dr.perez@lovasalud.com", "Luis", "Pérez", specialties[1].Id, ApplicationRole.Roles.Doctor, EmployeeRoleEnum.Doctor),
            ("dra.santos@lovasalud.com", "Ana", "Santos", specialties[2].Id, ApplicationRole.Roles.Doctor, EmployeeRoleEnum.Doctor),
            ("dr.morales@lovasalud.com", "José", "Morales", specialties[3].Id, ApplicationRole.Roles.Doctor, EmployeeRoleEnum.Doctor),
            ("recepcion@lovasalud.com", "María", "Reyes", null, ApplicationRole.Roles.Receptionist, EmployeeRoleEnum.Receptionist),
            ("lab@lovasalud.com", "Pedro", "Vargas", null, ApplicationRole.Roles.LabTechnician, EmployeeRoleEnum.LabTechnician),
            ("enfermera@lovasalud.com", "Rosa", "Ureña", null, ApplicationRole.Roles.Nurse, EmployeeRoleEnum.Nurse),
        };

        var doctorUsers = new Dictionary<string, ApplicationUser>();
        foreach (var seed in doctorSeeds)
        {
            var existing = await userManager.FindByEmailAsync(seed.email);
            if (existing is not null) { doctorUsers[seed.email] = existing; continue; }
            var u = new ApplicationUser
            {
                UserName = seed.email, Email = seed.email, FirstName = seed.first, LastName = seed.last,
                IsActive = true, EmailConfirmed = true, SpecialtyId = seed.specId,
            };
            var res = await userManager.CreateAsync(u, "Lova@2026!");
            if (!res.Succeeded)
            {
                logger.LogWarning("Failed to seed user {Email}: {Err}", seed.email, string.Join(",", res.Errors.Select(e => e.Description)));
                continue;
            }
            await userManager.AddToRoleAsync(u, seed.roleName);
            doctorUsers[seed.email] = u;
        }

        var doctors = doctorUsers.Values.Where(u => u.SpecialtyId != null).ToList();

        // ── Employees mirroring users ──
        foreach (var seed in doctorSeeds)
        {
            if (!doctorUsers.TryGetValue(seed.email, out var u)) continue;
            var emp = new Employee
            {
                UserId = u.Id, FirstName = seed.first, LastName = seed.last,
                NationalId = $"40{_rng.Next(10_000_000, 99_999_999)}",
                Role = seed.empRole, SpecialtyId = seed.specId,
                StartDate = DateTime.UtcNow.AddYears(-_rng.Next(1, 8)),
                EmploymentType = EmploymentTypeEnum.FullTime,
                Salary = seed.empRole == EmployeeRoleEnum.Doctor ? 120_000 + _rng.Next(0, 50_000) : 35_000 + _rng.Next(0, 15_000),
                Status = EmployeeStatusEnum.Active,
                EmergencyContactPhone = "809-555-" + _rng.Next(1000, 9999),
            };
            await db.Employees.AddAsync(emp);
        }
        await db.SaveChangesAsync();

        // ── Patients (20) ──
        var first1 = new[] { "Juan", "María", "Pedro", "Ana", "Carlos", "Sofía", "Diego", "Lucía", "Miguel", "Valentina", "Andrés", "Camila", "Rafael", "Daniela", "Sebastián", "Isabel", "Fernando", "Patricia", "Roberto", "Elena" };
        var last1 = new[] { "Martínez", "Rodríguez", "Hernández", "López", "González", "Pérez", "Sánchez", "Ramírez", "Cruz", "Torres", "Flores", "Rivera", "Jiménez", "Reyes", "Morales", "Ortiz", "Castillo", "Vargas", "Ramos", "Aguilar" };

        var patients = new List<Patient>();
        for (var i = 0; i < 20; i++)
        {
            var age = _rng.Next(2, 85);
            var p = new Patient
            {
                FirstName = first1[i % first1.Length],
                LastName = last1[(i + 3) % last1.Length],
                DocumentType = DocumentTypeEnum.Cedula,
                DocumentNumber = $"00{_rng.Next(10000000, 99999999)}",
                Nationality = "Dominicana",
                HomeAddress = $"Calle {_rng.Next(1, 99)} #{_rng.Next(1, 999)}, Santo Domingo",
                BirthDate = DateTime.UtcNow.AddYears(-age).AddDays(_rng.Next(0, 364)),
                Gender = i % 2 == 0 ? GendersEnum.Female : GendersEnum.Male,
                Status = i == 19 ? PatientsStatus.PendingVerification : PatientsStatus.Active,
                Email = $"{first1[i % first1.Length].ToLowerInvariant()}.{last1[(i + 3) % last1.Length].ToLowerInvariant()}@example.com",
                Phone = "829-" + _rng.Next(100, 999) + "-" + _rng.Next(1000, 9999),
                BloodType = (BloodTypeEnum)_rng.Next(0, 8),
                HasInsurance = i % 3 != 0,
                InsuranceCompanyId = i % 3 != 0 ? insurers[i % insurers.Length].Id : (Guid?)null,
                InsurancePolicyNumber = i % 3 != 0 ? "POL-" + _rng.Next(100000, 999999) : null,
                InsuranceCoveragePercentage = i % 3 != 0 ? 80m : 0,
            };
            if (age < 18)
            {
                p.GuardianFirstName = "Carmen";
                p.GuardianLastName = last1[(i + 5) % last1.Length];
                p.GuardianRelationship = GuardianRelationshipEnum.Mother;
                p.GuardianPhone = "809-" + _rng.Next(100, 999) + "-" + _rng.Next(1000, 9999);
            }
            patients.Add(p);
        }
        await db.Patients.AddRangeAsync(patients);

        // ── Medications (15) ──
        var medSeeds = new (string brand, string generic, MedicationPresentationEnum pres, string strength, string unit, int stock, int min, decimal cost, decimal sale, bool controlled)[]
        {
            ("Tylenol", "Paracetamol", MedicationPresentationEnum.Tablet, "500mg", "tablet", 250, 50, 2.50m, 5.00m, false),
            ("Advil", "Ibuprofeno", MedicationPresentationEnum.Tablet, "400mg", "tablet", 180, 60, 3.00m, 6.50m, false),
            ("Amoxil", "Amoxicilina", MedicationPresentationEnum.Capsule, "500mg", "capsule", 90, 30, 12.00m, 28.00m, false),
            ("Zithromax", "Azitromicina", MedicationPresentationEnum.Tablet, "500mg", "tablet", 24, 15, 45.00m, 95.00m, false),
            ("Lipitor", "Atorvastatina", MedicationPresentationEnum.Tablet, "20mg", "tablet", 8, 30, 18.00m, 42.00m, false),  // low stock
            ("Glucophage", "Metformina", MedicationPresentationEnum.Tablet, "850mg", "tablet", 120, 40, 4.00m, 9.00m, false),
            ("Norvasc", "Amlodipina", MedicationPresentationEnum.Tablet, "5mg", "tablet", 0, 20, 8.00m, 19.00m, false),       // out of stock
            ("Ventolin", "Salbutamol", MedicationPresentationEnum.Inhaler, "100mcg", "inhaler", 14, 10, 95.00m, 220.00m, false),
            ("Omeprazol", "Omeprazol", MedicationPresentationEnum.Capsule, "20mg", "capsule", 200, 60, 3.50m, 8.00m, false),
            ("Loratadina", "Loratadina", MedicationPresentationEnum.Tablet, "10mg", "tablet", 160, 40, 1.80m, 4.50m, false),
            ("Diazepam", "Diazepam", MedicationPresentationEnum.Tablet, "10mg", "tablet", 35, 20, 8.00m, 22.00m, true),       // controlled
            ("Tramadol", "Tramadol", MedicationPresentationEnum.Capsule, "50mg", "capsule", 45, 25, 12.00m, 32.00m, true),    // controlled
            ("Insulin", "Insulina NPH", MedicationPresentationEnum.Injection, "100UI/mL", "vial", 18, 12, 380.00m, 850.00m, false),
            ("Augmentin", "Amoxicilina/Clavulánico", MedicationPresentationEnum.Tablet, "875mg", "tablet", 60, 30, 22.00m, 52.00m, false),
            ("Vitamin D", "Colecalciferol", MedicationPresentationEnum.Capsule, "1000UI", "capsule", 220, 50, 1.50m, 4.00m, false),
        };
        var meds = medSeeds.Select(m => new Medication
        {
            BrandName = m.brand, GenericName = m.generic, Presentation = m.pres,
            Strength = m.strength, UnitOfMeasure = m.unit,
            CurrentStock = m.stock, MinimumStockThreshold = m.min, ReorderQuantity = m.min * 3,
            CostPrice = m.cost, SalePrice = m.sale,
            IsControlledSubstance = m.controlled, ControlledSubstanceClass = m.controlled ? "IV" : null,
            EarliestExpirationDate = DateTime.UtcNow.AddMonths(_rng.Next(3, 24)),
            IsActive = true,
        }).ToList();
        await db.Medications.AddRangeAsync(meds);
        await db.SaveChangesAsync();

        var admin = await userManager.FindByEmailAsync("admin@lovasalud.com");
        var adminId = admin!.Id;

        // ── Appointments (last 30 days + next 7 days) ──
        var now = DateTime.UtcNow;
        var appts = new List<Appointment>();
        for (var dayOffset = -30; dayOffset <= 7; dayOffset++)
        {
            var perDay = _rng.Next(2, 9);
            for (var k = 0; k < perDay; k++)
            {
                var patient = patients[_rng.Next(patients.Count)];
                var doctor = doctors[_rng.Next(doctors.Count)];
                var when = DateTime.SpecifyKind(
                    now.Date.AddDays(dayOffset).AddHours(8 + _rng.Next(0, 10)).AddMinutes(_rng.Next(0, 4) * 15),
                    DateTimeKind.Utc);

                var status =
                    dayOffset < -1 ? (_rng.NextDouble() < 0.15 ? AppointmentStatusEnum.NoShow : AppointmentStatusEnum.Attended)
                    : dayOffset == 0 ? (when < now ? AppointmentStatusEnum.Attended : AppointmentStatusEnum.Confirmed)
                    : AppointmentStatusEnum.Scheduled;

                appts.Add(new Appointment
                {
                    PatientId = patient.Id,
                    AssignedDoctorId = doctor.Id,
                    ScheduledByUserId = adminId,
                    ScheduledDate = when,
                    DurationMinutes = 30,
                    Type = (AppointmentTypeEnum)_rng.Next(0, 5),
                    Status = status,
                    Reason = new[] { "Chequeo general", "Dolor de cabeza", "Control mensual", "Fiebre", "Seguimiento" }[_rng.Next(5)],
                });
            }
        }
        await db.Appointments.AddRangeAsync(appts);
        await db.SaveChangesAsync();

        // ── Consults: 1 per Attended appointment (last 30 days) ──
        var attendedAppts = appts.Where(a => a.Status == AppointmentStatusEnum.Attended).ToList();
        var consults = new List<Consult>();
        var diagnoses = new[]
        {
            ("J06.9", "Infección aguda de vías respiratorias superiores"),
            ("R51", "Cefalea"),
            ("E11.9", "Diabetes mellitus tipo 2"),
            ("I10", "Hipertensión esencial"),
            ("K30", "Dispepsia funcional"),
            ("J45.9", "Asma, no especificada"),
            ("L20.9", "Dermatitis atópica"),
            ("M54.5", "Lumbalgia"),
        };

        foreach (var apt in attendedAppts.Take(60))
        {
            var doc = doctorUsers.Values.FirstOrDefault(u => u.Id == apt.AssignedDoctorId);
            var specId = doc?.SpecialtyId ?? specialties[0].Id;
            var dx = diagnoses[_rng.Next(diagnoses.Length)];
            var weight = 50 + _rng.Next(0, 50);
            var height = 150 + _rng.Next(0, 40);

            consults.Add(new Consult
            {
                PatientId = apt.PatientId,
                SpecialtyId = specId,
                DoctorId = apt.AssignedDoctorId,
                Status = ConsultStatusEnum.Finished,
                WeightKg = weight,
                HeightCm = height,
                Bmi = Math.Round((decimal)weight / (decimal)Math.Pow(height / 100.0, 2), 1),
                BpSystolic = 110 + _rng.Next(0, 40),
                BpDiastolic = 70 + _rng.Next(0, 20),
                HeartRate = 60 + _rng.Next(0, 40),
                TemperatureCelsius = 36 + (decimal)_rng.NextDouble() * 2,
                O2Saturation = 95 + _rng.Next(0, 5),
                ChiefComplaint = apt.Reason,
                ClinicalObservations = "Paciente en buen estado general. Sin hallazgos relevantes adicionales.",
                DiagnosisCodes = dx.Item1,
                DiagnosisDescription = dx.Item2,
                TreatmentPlan = "Reposo, hidratación adecuada y seguir tratamiento prescrito. Volver si síntomas empeoran.",
                StartedAt = apt.ScheduledDate,
                FinishedAt = apt.ScheduledDate.AddMinutes(_rng.Next(15, 50)),
                CreatedAt = apt.ScheduledDate,
            });
        }

        // Add a few open/in-progress consults for today
        var todayAttended = attendedAppts.Where(a => a.ScheduledDate.Date == now.Date).Take(2);
        foreach (var apt in todayAttended)
        {
            var doc = doctorUsers.Values.FirstOrDefault(u => u.Id == apt.AssignedDoctorId);
            consults.Add(new Consult
            {
                PatientId = apt.PatientId,
                SpecialtyId = doc?.SpecialtyId ?? specialties[0].Id,
                DoctorId = apt.AssignedDoctorId,
                Status = ConsultStatusEnum.InProgress,
                ChiefComplaint = apt.Reason,
                StartedAt = apt.ScheduledDate,
                CreatedAt = apt.ScheduledDate,
            });
        }

        await db.Consults.AddRangeAsync(consults);
        await db.SaveChangesAsync();

        // ── Prescriptions for finished consults ──
        var drugs = new[]
        {
            ("Paracetamol", "500mg cada 8 horas", "Oral", 7),
            ("Ibuprofeno", "400mg cada 6 horas", "Oral", 5),
            ("Amoxicilina", "500mg cada 8 horas", "Oral", 10),
            ("Omeprazol", "20mg en ayunas", "Oral", 14),
            ("Loratadina", "10mg cada 24 horas", "Oral", 7),
        };
        var prescriptions = new List<MedicalPrescription>();
        foreach (var c in consults.Where(c => c.Status == ConsultStatusEnum.Finished).Take(40))
        {
            var d = drugs[_rng.Next(drugs.Length)];
            prescriptions.Add(new MedicalPrescription
            {
                ConsultId = c.Id,
                PrescribedByDoctorId = c.DoctorId,
                DrugName = d.Item1,
                Presentation = "Tableta",
                Dosage = d.Item2.Split(" ")[0],
                Frequency = string.Join(" ", d.Item2.Split(" ").Skip(1)),
                RouteOfAdministration = d.Item3,
                DurationDays = d.Item4,
                QuantityToDispense = d.Item4 * 3,
                CreatedAt = c.FinishedAt ?? DateTime.UtcNow,
            });
        }
        await db.MedicalPrescriptions.AddRangeAsync(prescriptions);

        // ── Lab orders (some pending, some done) ──
        var labOrders = new List<LabOrder>();
        var testNames = new[] { "Hemograma completo", "Glucosa en ayunas", "Perfil lipídico", "Examen de orina", "TSH", "Hemoglobina A1c" };
        foreach (var c in consults.Take(30))
        {
            if (_rng.NextDouble() > 0.5) continue;
            var done = _rng.NextDouble() > 0.35;
            var test = testNames[_rng.Next(testNames.Length)];
            labOrders.Add(new LabOrder
            {
                ConsultId = c.Id,
                PatientId = c.PatientId,
                OrderedByDoctorId = c.DoctorId,
                TestName = test,
                TestCategory = "Laboratorio",
                Priority = _rng.NextDouble() < 0.2 ? LabTestPriorityEnum.Urgent : LabTestPriorityEnum.Routine,
                Status = done ? "Complete" : (_rng.NextDouble() > 0.5 ? "Pending" : "InProgress"),
                CreatedAt = c.CreatedAt.AddMinutes(_rng.Next(5, 30)),
                ResultsAvailableAt = done ? c.CreatedAt.AddHours(_rng.Next(2, 48)) : null,
            });
        }
        await db.LabOrders.AddRangeAsync(labOrders);
        await db.SaveChangesAsync();

        // ── Invoices + Payments for finished consults ──
        var invoiceCounter = 0;
        var ncfCounter = 1L;
        foreach (var c in consults.Where(c => c.Status == ConsultStatusEnum.Finished).Take(50))
        {
            invoiceCounter++;
            var subtotal = 1000m + _rng.Next(0, 5000);
            var taxAmount = Math.Round(subtotal * 0.18m, 2);
            var total = subtotal + taxAmount;
            var patient = patients.First(p => p.Id == c.PatientId);
            var insuranceCoverage = patient.HasInsurance ? Math.Round(total * patient.InsuranceCoveragePercentage / 100m, 2) : 0m;
            var patientResp = total - insuranceCoverage;
            var paid = _rng.NextDouble() > 0.3 ? patientResp : (_rng.NextDouble() > 0.5 ? Math.Round(patientResp / 2, 2) : 0);
            var status = paid >= patientResp ? InvoiceStatusEnum.Paid
                        : paid > 0 ? InvoiceStatusEnum.PartiallyPaid
                        : InvoiceStatusEnum.AwaitingPayment;

            var inv = new Invoice
            {
                PatientId = c.PatientId, ConsultId = c.Id, CreatedByUserId = adminId,
                InvoiceNumber = $"INV-{c.CreatedAt:yyyyMMdd}-{invoiceCounter:D4}",
                Ncf = $"B02{ncfCounter++:D8}",
                NcfType = NcfTypeEnum.Consumo,
                Status = status,
                Subtotal = subtotal, TaxAmount = taxAmount,
                InsuranceCoverageAmount = insuranceCoverage,
                TotalAmount = total,
                PatientResponsibilityAmount = patientResp,
                PaidAmount = paid,
                PaidAt = status == InvoiceStatusEnum.Paid ? c.FinishedAt : null,
                CreatedAt = c.FinishedAt ?? DateTime.UtcNow,
            };
            await db.Invoices.AddAsync(inv);

            if (paid > 0)
            {
                await db.Payments.AddAsync(new Payment
                {
                    InvoiceId = inv.Id, ReceivedByUserId = adminId,
                    Amount = paid,
                    Method = (PaymentMethodEnum)_rng.Next(0, 4),
                    PaymentDate = inv.CreatedAt.AddMinutes(_rng.Next(10, 1000)),
                });
            }
        }

        // Advance NCF sequence counter
        var seq = await db.NcfSequences.FirstOrDefaultAsync(s => s.Type == NcfTypeEnum.Consumo);
        if (seq is not null)
        {
            seq.CurrentSequence = ncfCounter;
            db.NcfSequences.Update(seq);
        }

        await db.SaveChangesAsync();

        logger.LogInformation("Mock data seeded: {P} patients, {A} appointments, {C} consults, {I} invoices, {M} meds, {Rx} prescriptions, {Lab} lab orders.",
            patients.Count, appts.Count, consults.Count, invoiceCounter, meds.Count, prescriptions.Count, labOrders.Count);
    }
}
