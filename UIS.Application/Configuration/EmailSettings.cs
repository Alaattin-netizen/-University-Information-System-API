namespace UIS.Application.Configuration;

public class EmailSettings
{
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string FromEmail { get; set; } = "uni@university.edu";
    public string FromName { get; set; } = "University Information System";
    public bool EnableSsl { get; set; } = false;
}