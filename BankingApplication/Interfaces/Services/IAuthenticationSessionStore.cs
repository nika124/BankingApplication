using BankingApplication.Models;

namespace BankingApplication.Interfaces.Services;

public interface IAuthenticationSessionStore
{
    Guid CreatePendingAuthentication(int cardId, int accountId);
    PendingAtmAuthentication? GetPendingAuthentication(Guid authenticationId);
    void RemovePendingAuthentication(Guid authenticationId);
    Guid CreateAuthorizedSession(int cardId, int accountId);
    AuthenticationSession? ConsumeAuthorizedSession(Guid sessionId);
}