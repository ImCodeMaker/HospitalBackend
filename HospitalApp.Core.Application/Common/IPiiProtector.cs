namespace HospitalApp.Core.Application.Common;

public interface IPiiProtector
{
    string Protect(string plaintext);
    string? ProtectNullable(string? plaintext);
    string Unprotect(string protectedValue);
    string? UnprotectNullable(string? protectedValue);
}
