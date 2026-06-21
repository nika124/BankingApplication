using BankingApplication.Enums;
using BankingApplication.Interfaces.Services;
using BankingApplication.Models;
using BankingApplication.Models.Results;

namespace BankingApplication.Services;

public class AuthenticationSessionStore : IAuthenticationSessionStore
{
    private static readonly TimeSpan ActiveSessionLifetime = TimeSpan.FromMinutes(2);

    private readonly Dictionary<Guid, Session> _sessions = [];
    private readonly Dictionary<Guid, SessionStorage> _sessionStorage = [];

    public Guid CreateSession(SessionType sessionType, SessionStatus status, TimeSpan lifetime)
    {
        var sessionId = Guid.NewGuid();

        _sessions[sessionId] = new Session
        {
            SessionId = sessionId,
            SessionType = sessionType,
            Status = status,
            ExpiresAt = DateTime.UtcNow.Add(lifetime)
        };

        _sessionStorage[sessionId] = new SessionStorage
        {
            SessionId = sessionId
        };

        return sessionId;
    }

    public bool SetSessionValue(Guid sessionId, string key, string value)
    {
        var storage = GetSessionStorage(sessionId);
        if (storage is null)
        {
            return false;
        }

        storage.Values[key] = value;
        return true;
    }

    public string? GetSessionValue(Guid sessionId, string key)
    {
        var storage = GetSessionStorage(sessionId);
        if (storage is null)
        {
            return null;
        }

        return storage.Values.GetValueOrDefault(key);
    }

    public SessionStorage? GetSessionStorage(Guid sessionId)
    {
        if (!TryGetValidSession(sessionId, out _))
        {
            return null;
        }

        if (!_sessionStorage.TryGetValue(sessionId, out var storedSessionStorage))
        {
            RemoveSession(sessionId);
            return null;
        }

        return storedSessionStorage;
    }

    public Session? GetPendingSession(Guid sessionId, SessionType sessionType)
    {
        if (!TryGetValidSession(sessionId, out var session))
        {
            return null;
        }

        if (session.SessionType != sessionType || session.Status != SessionStatus.Pending)
        {
            return null;
        }

        if (!_sessionStorage.ContainsKey(sessionId))
        {
            RemoveSession(sessionId);
            return null;
        }

        return session;
    }

    public bool ActivateSession(Guid sessionId, SessionType sessionType)
    {
        var session = GetPendingSession(sessionId, sessionType);
        if (session is null)
        {
            return false;
        }

        session.Status = SessionStatus.Active;
        session.ExpiresAt = DateTime.UtcNow.Add(ActiveSessionLifetime);
        return true;
    }

    public ServiceResult UseActiveSession(
        Guid sessionId,
        SessionType sessionType,
        out SessionStorage? storage)
    {
        var result = new ServiceResult
        {
            Message = "Session accepted."
        };
        storage = null;

        if (!TryGetValidSession(sessionId, out var session))
        {
            return AddSessionError(result);
        }

        if (session.SessionType != sessionType || session.Status != SessionStatus.Active)
        {
            return AddSessionError(result);
        }

        if (!_sessionStorage.TryGetValue(sessionId, out var storedSessionStorage))
        {
            RemoveSession(sessionId);
            return AddSessionError(result);
        }

        storage = new SessionStorage
        {
            SessionId = storedSessionStorage.SessionId,
            Values = new Dictionary<string, string>(storedSessionStorage.Values)
        };

        if (sessionType == SessionType.Atm)
        {
            session.Status = SessionStatus.Consumed;
            RemoveSession(sessionId);
        }

        return result;
    }

    private bool TryGetValidSession(Guid sessionId, out Session session)
    {
        if (!_sessions.TryGetValue(sessionId, out session!))
        {
            RemoveSession(sessionId);
            return false;
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            session.Status = SessionStatus.Expired;
            RemoveSession(sessionId);
            return false;
        }

        return true;
    }

    private void RemoveSession(Guid sessionId)
    {
        _sessions.Remove(sessionId);
        _sessionStorage.Remove(sessionId);
    }

    private static ServiceResult AddSessionError(ServiceResult result)
    {
        result.Message = "Session could not be used.";
        result.AddError("SessionId", "Session was not found, expired, inactive, or already used.");
        return result;
    }
}
