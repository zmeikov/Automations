using Common.Models;

namespace Common.Services
{
	public interface ISendGmail
	{
		void SendSMS(string toNumberEmail, string body, GoogleAccountSettings _gglSettings);
	}
}
