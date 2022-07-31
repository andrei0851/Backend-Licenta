using System;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Backend.Services;

public class MailService : IMailService
{
    
    public IConfiguration _config { get; }
    
    public MailService(IConfiguration configuration)
    {
        _config = configuration;
    }
    public async Task sendEmail(string toName, string toAddress, string subject, BodyBuilder body)
    {
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            "WW Cars Trade",
            "pupucasu2011@gmail.com"
        ));
        message.To.Add(new MailboxAddress(
            toName,
            toAddress
        ));
        message.Subject = subject;
        message.Body = body.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();
        // SecureSocketOptions.StartTls force a secure connection over TLS
        await client.ConnectAsync("smtp.sendgrid.net", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(
            userName: "", // the userName is the exact string "apikey" and not the API key itself.
            password: "" // password is the API key
        );
        await client.SendAsync(message);
    }
}