using BankingApplication.Interfaces.Repositories;
using BankingApplication.Interfaces.Services;

namespace BankingApplication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ICardRepository _cardRepository;
    private readonly IPinService _pinService;
    private readonly IAuthenticationSessionStore _sessionStore;
    
    public AuthenticationService(ICardRepository cardRepository, IPinService pinService, IAuthenticationSessionStore sessionStore)
    {
        _cardRepository = cardRepository;
        _pinService = pinService;
        _sessionStore = sessionStore;
    }

    public Guid? StartAtmAuthentication(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return null;
        }

        var card = _cardRepository.GetByCardNumber(cardNumber);

        if (card is null)
        {
            return null;
        }

        if (card.Status != "Active")
        {
            return null;
        }

        return _sessionStore.CreatePendingAuthentication(card.CardId, card.AccountId);
    }
    
    public Guid? VerifyAtmPin(Guid authenticationId, string pin)
    {
        var pendingAuthentication = _sessionStore.GetPendingAuthentication(authenticationId);

        if (pendingAuthentication is null)
        {
            return null;
        }

        var pinIsValid = _pinService.ValidatePin(pendingAuthentication.CardId, pin);

        if (!pinIsValid)
        {
            return null;
        }

        _sessionStore.RemovePendingAuthentication(authenticationId);

        return _sessionStore.CreateAuthorizedSession(pendingAuthentication.CardId, pendingAuthentication.AccountId);
    }
}