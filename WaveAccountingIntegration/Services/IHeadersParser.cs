using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveAccountingIntegration.Services
{
	public interface IHeadersParser
	{
		Dictionary<string, string> ParseHeadersFromFile(string path);
	}
}
