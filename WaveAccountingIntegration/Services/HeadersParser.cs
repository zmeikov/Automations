using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WaveAccountingIntegration.Services
{
	public class HeadersParser : IHeadersParser
	{
		public Dictionary<string, string> ParseHeadersFromFile(string path)
		{
			var headers = new Dictionary<string, string>();
			StreamReader sr = new StreamReader(path);

			while (!sr.EndOfStream)
			{
				var lineText = sr.ReadLine();
				if (lineText != null)
				{
					var headerName = lineText.Substring(0, lineText.IndexOf(':'));
					var headerValue = lineText.Substring(lineText.IndexOf(':')+1);

					headers.Add(headerName, headerValue);
				}
			}

			return headers;
		}
	}
}