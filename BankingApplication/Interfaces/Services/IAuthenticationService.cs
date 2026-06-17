namespace BankingApplication.Interfaces.Services;

public interface IAuthenticationService
{
    Guid? StartAtmAuthentication(string cardNumber);
    Guid? VerifyAtmPin(Guid authenticationId, string pin);
}