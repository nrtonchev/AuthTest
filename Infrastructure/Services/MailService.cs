using Core.Entities.Auth;
using Core.Interfaces;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Infrastructure.Services
{
	public class MailService : IMailService
	{
		private readonly AppSettings appSettings;

		public MailService(IOptions<AppSettings> appSettings)
        {
			this.appSettings = appSettings.Value;
		}

		public void Send(string receiver, string subject, string html, string sender = null)
		{
			var email = new MimeMessage();
			email.From.Add(MailboxAddress.Parse(sender ?? appSettings.EmailFrom));
			email.To.Add(MailboxAddress.Parse(receiver));
			email.Subject = subject;
			email.Body = new TextPart(TextFormat.Html) { Text = html };

			using (var smtp = new SmtpClient())
			{
				smtp.Connect(appSettings.SmtpHost, appSettings.SmtpPort, SecureSocketOptions.StartTls);
				smtp.Authenticate(appSettings.SmtpUser, appSettings.SmtpPass);
				smtp.Send(email);
				smtp.Disconnect(true);
			}
		}
	}
}
