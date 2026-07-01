namespace ClimateAdvisor.Api.Services;

public interface IEmailService
{
    Task SendContactNotificationAsync(string name, string email, string subject, string message, CancellationToken ct = default);

    /// <summary>Send a raw email to any address (used for SMS gateways).</summary>
    Task SendRawAsync(string toAddress, string subject, string body, CancellationToken ct = default);
}
