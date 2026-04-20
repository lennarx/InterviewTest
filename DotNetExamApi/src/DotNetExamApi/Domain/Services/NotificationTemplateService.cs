namespace DotNetExamApi.Domain.Services;

public abstract class NotificationTemplateService
{
    public virtual void Send(string recipient, string content)
    {
        ValidateRecipient(recipient);
        var subject = BuildSubject();
        var body = BuildBody(content);
        Deliver(recipient, subject, body);
        LogSent(recipient);
    }

    protected virtual string BuildSubject() => "DotNetExamApi Notification";

    protected virtual string BuildBody(string content) => $"<html><body>{content}</body></html>";

    protected virtual void ValidateRecipient(string recipient)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient cannot be empty.", nameof(recipient));
    }

    protected virtual void Deliver(string recipient, string subject, string body)
    {
        Console.WriteLine($"[BASE] To: {recipient} | Subject: {subject}");
    }

    protected virtual void LogSent(string recipient)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Notification sent to {recipient}");
    }
}

public class EmailNotificationService : NotificationTemplateService
{
    private readonly string _smtpServer;

    public EmailNotificationService(string smtpServer = "smtp.example.com")
    {
        _smtpServer = smtpServer;
    }

    public override void Send(string recipient, string content)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient cannot be empty.", nameof(recipient));

        var subject = $"Order Update — {DateTime.Now:yyyy-MM-dd}";
        var body = $"<html><body><p>{content}</p><footer>Sent via {_smtpServer}</footer></body></html>";

        Console.WriteLine($"[EMAIL] To: {recipient} | Subject: {subject}");
        Console.WriteLine($"[EMAIL] Body ({body.Length} chars) via {_smtpServer}");
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Email queued for {recipient}");
    }

    protected override string BuildSubject() => $"Order Update — {DateTime.Now:yyyy-MM-dd}";

    protected override string BuildBody(string content)
        => $"<html><body><p>{content}</p></body></html>";
}

public class SmsNotificationService : NotificationTemplateService
{
    public override void Send(string recipient, string content)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient cannot be empty.", nameof(recipient));

        var smsBody = content.Length > 160 ? content[..160] : content;

        Console.WriteLine($"[SMS] To: {recipient}: {smsBody}");
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] SMS queued for {recipient}");
    }

    protected override string BuildBody(string content)
        => content.Length > 160 ? content[..160] : content;

    protected override void Deliver(string recipient, string subject, string body)
    {
        Console.WriteLine($"[SMS] Sending to {recipient}: {body}");
    }
}
