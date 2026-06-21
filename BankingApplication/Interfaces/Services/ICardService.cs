using BankingApplication.Models;
using BankingApplication.Models.Results;

namespace BankingApplication.Interfaces.Services;

public interface ICardService
{
    ServiceResult GetActiveCard(string cardNumber, out Card? card);
}
