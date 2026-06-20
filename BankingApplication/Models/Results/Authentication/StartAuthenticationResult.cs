using BankingApplication.Models.DTOs.Authentication;

namespace BankingApplication.Models.Results.Authentication;

public class StartAuthenticationResult : ServiceResult
{
    public PendingAuthenticationDto? PendingAuthentication { get; set; }
}