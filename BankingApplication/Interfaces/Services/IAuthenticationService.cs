using BankingApplication.Models.Requests;
using BankingApplication.Models.Results.Authentication;

namespace BankingApplication.Interfaces.Services;

public interface IAuthenticationService
{
    StartAuthenticationResult StartAtmAuthentication(StartAuthenticationRequest request);
    CompleteAuthenticationResult CompleteAuthentication(CompleteAuthenticationRequest request);
}