using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using WaveAccountingIntegration.Models;

namespace WaveAccountingIntegration.Services
{
	public class SendGmail : ISendGmail
	{
		public void SendSMS(string toNumberEmail, string body, AppSettings _appSettings)
		{
			var toEmail = new string(toNumberEmail.ToCharArray());
			var smtp = new SmtpClient
			{
				Host = "smtp.gmail.com",
				Port = 587,
				EnableSsl = true,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(_appSettings.GoogleSettings.UserName, _appSettings.GoogleSettings.Password)
			};
			using (var message = new MailMessage(new MailAddress(_appSettings.GoogleSettings.UserName), new MailAddress(toEmail))
			{
				Subject = "SMS alert",
				Body = body
			})
			{
				//just so not to upset Google we pause for radom time before sending email
				Thread.Sleep(new Random().Next(0, 60) * 1000);

				smtp.Send(message);
			}
		}
	}
}