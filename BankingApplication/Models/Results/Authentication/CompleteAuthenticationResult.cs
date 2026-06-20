using BankingApplication.Models.DTOs.Authentication;

namespace BankingApplication.Models.Results.Authentication;

public class CompleteAuthenticationResult : ServiceResult
{
    public AuthorizedSessionDto? AuthorizedSession { get; set; }
}
