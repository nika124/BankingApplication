using BankingApplication.Models.Results;

namespace BankingApplication.Interfaces.Services;

public interface IPinService
{
    ServiceResult ValidatePin(int cardId, string pin);
    ServiceResult ChangePin(int cardId, string newPin);
}
