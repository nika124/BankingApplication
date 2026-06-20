using System.Collections.Concurrent;
using BankingApplication.Interfaces.Services;
using BankingApplication.Models;

namespace BankingApplication.Services;

public class AuthenticationSessionStore : IAuthenticationSessionStore
{
    private readonly ConcurrentDictionary<Guid, PendingAtmAuthentication> _pendingAuthentications = [];
    private readonly ConcurrentDictionary<Guid, AuthenticationSession> _authorizedSessions = [];

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

        _pendingAuthentications[authentication.AuthenticationId] = authentication;

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
            _pendingAuthentications.TryRemove(authenticationId, out _);
            return null;
        }

        return authentication;
    }

    public bool RemovePendingAuthentication(
        Guid authenticationId)
    {
        return _pendingAuthentications.TryRemove(authenticationId, out _);
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

        _authorizedSessions[session.SessionId] = session;

        return session.SessionId;
    }

    public AuthenticationSession? ConsumeAuthorizedSession(
        Guid sessionId)
    {
        if (!_authorizedSessions.TryRemove(
                sessionId,
                out var session))
        {
            return null;
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        return session;
    }
}
