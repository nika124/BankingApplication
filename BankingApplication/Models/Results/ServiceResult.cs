namespace BankingApplication.Models.Results;

public class ServiceResult
{
    private readonly Dictionary<string, string> _errors = new();

    public bool IsSuccess => _errors.Count == 0;
    public string Message { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Errors => _errors;

    public void AddError(string key, string value)
    {
        _errors.Add(key, value);
    }

    public void AddErrors(IReadOnlyDictionary<string, string> errors)
    {
        foreach (var (key, value) in errors)
            AddError(key, value);
    }
}
