public class ActivationResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public string ErrorDetails { get; set; }
    public DateTime ActivationDate { get; set; }
    public string LicenseStatus { get; set; }
    public int DaysRemaining { get; set; }
}
