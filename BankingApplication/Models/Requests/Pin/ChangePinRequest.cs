namespace BankingApplication.Models.Requests.Pin;

public class ChangePinRequest
{
    public Guid SessionId { get; set; }
    public string CurrentPin { get; set; } = string.Empty;
    public string NewPin { get; set; } = string.Empty;
    public string ConfirmNewPin { get; set; } = string.Empty;
}
