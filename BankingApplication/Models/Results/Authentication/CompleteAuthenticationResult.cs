using BankingApplication.Models.DTOs.Authentication;

namespace BankingApplication.Models.Results.Authentication;

public class CompleteAuthenticationResult : ServiceResult
{
    public ActiveSessionDto? ActiveSession { get; set; }
}
