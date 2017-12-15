using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveAccountingIntegration.Models;

namespace WaveAccountingIntegration.Services
{
	public interface ISendGmail
	{
		void SendSMS(string toNumberEmail, string body, AppSettings _appSettings);
	}
}
