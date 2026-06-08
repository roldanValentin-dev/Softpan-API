using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Softpan.Application.Interfaces;

namespace Softpan.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _password;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _host = configuration["SMTP_HOST"] ?? "smtp.gmail.com";
        _port = int.TryParse(configuration["SMTP_PORT"], out var p) ? p : 587;
        _user = configuration["SMTP_USER"]
            ?? throw new InvalidOperationException("SMTP_USER no configurado");
        _password = configuration["SMTP_PASSWORD"]
            ?? throw new InvalidOperationException("SMTP_PASSWORD no configurado");
        _fromName = configuration["SMTP_FROM_NAME"] ?? "Softpan";
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _user));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var body = new TextPart("html")
        {
            Text = htmlBody
        };
        message.Body = body;

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_user, _password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}