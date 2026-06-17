using BankingApplication.Interfaces.Services;
using BankingApplication.Models;

namespace BankingApplication.Services;

public class AuthenticationSessionStore : IAuthenticationSessionStore
{
    private readonly Dictionary<Guid, AuthenticationSession> _sessions = new();

    private readonly Dictionary<Guid, PendingAtmAuthentication> _pendingAuthentications = [];

    private readonly Dictionary<Guid, AuthenticationSession> _authorizedSessions = [];

    public Guid CreatePendingAuthentication(
        int cardId,
        int accountId)
    {
        var authentication = new PendingAtmAuthentication
        {
            AuthenticationId = Guid.NewGuid(),
            CardId = cardId,
            AccountId = accountId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(2)
        };

        _pendingAuthentications.Add(
            authentication.AuthenticationId,
            authentication);

        return authentication.AuthenticationId;
    }

    public PendingAtmAuthentication? GetPendingAuthentication(
        Guid authenticationId)
    {
        if (!_pendingAuthentications.TryGetValue(
                authenticationId,
                out var authentication))
        {
            return null;
        }

        if (authentication.ExpiresAt <= DateTime.UtcNow)
        {
            _pendingAuthentications.Remove(authenticationId);
            return null;
        }

        return authentication;
    }

    public void RemovePendingAuthentication(
        Guid authenticationId)
    {
        _pendingAuthentications.Remove(authenticationId);
    }

    public Guid CreateAuthorizedSession(
        int cardId,
        int accountId)
    {
        var session = new AuthenticationSession
        {
            SessionId = Guid.NewGuid(),
            CardId = cardId,
            AccountId = accountId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(2)
        };

        _authorizedSessions.Add(
            session.SessionId,
            session);

        return session.SessionId;
    }

    public AuthenticationSession? ConsumeAuthorizedSession(
        Guid sessionId)
    {
        if (!_authorizedSessions.TryGetValue(
                sessionId,
                out var session))
        {
            return null;
        }

        _authorizedSessions.Remove(sessionId);

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        return session;
    }
}