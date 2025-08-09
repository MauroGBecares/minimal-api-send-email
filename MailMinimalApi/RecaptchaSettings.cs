namespace MailMinimalApi
{
    public class RecaptchaSettings
    {
        public string SecretKey { get; set; }
    }
    public record ReCaptchaResponse
        (
    bool success,
    float score,
    string action,
    DateTime challenge_ts,
    string hostname,
    List<string> error_codes
);
}
