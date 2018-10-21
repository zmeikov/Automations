using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Common.Models;

namespace Common.Services
{
	public class SendGmail : ISendGmail
	{
		public void SendSMS(string toNumberEmail, string body, GoogleAccountSettings _gglSettings)
		{
			var toEmail = new string(toNumberEmail.ToCharArray());
			var smtp = new SmtpClient
			{
				Host = "smtp.gmail.com",
				Port = 587,
				EnableSsl = true,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(_gglSettings.UserName, _gglSettings.Password)
			};
			using (var message = new MailMessage(new MailAddress(_gglSettings.UserName), new MailAddress(toEmail))
			{
				Subject = "SMS alert",
				Body = body
			})
			{
				//just so not to upset Google we pause for radom time before sending email
				Thread.Sleep(new Random().Next(0, 30) * 1000);

				smtp.Send(message);
			}
		}
	}
}