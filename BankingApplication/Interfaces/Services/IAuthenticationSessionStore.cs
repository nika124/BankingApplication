using BankingApplication.Enums;
using BankingApplication.Models;
using BankingApplication.Models.Results;

namespace BankingApplication.Interfaces.Services;

public interface IAuthenticationSessionStore
{
    Guid CreateSession(SessionType sessionType, SessionStatus status, TimeSpan lifetime);
    bool SetSessionValue(Guid sessionId, string key, string value);
    string? GetSessionValue(Guid sessionId, string key);
    SessionStorage? GetSessionStorage(Guid sessionId);
    Session? GetPendingSession(Guid sessionId, SessionType sessionType);
    bool ActivateSession(Guid sessionId, SessionType sessionType);
    ServiceResult UseActiveSession(Guid sessionId, SessionType sessionType, out SessionStorage? storage);
}
