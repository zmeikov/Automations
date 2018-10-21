using System;
using System.Collections.Generic;
using System.IO;

namespace Common.Services
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
					try
					{
						var headerName = lineText.Substring(0, lineText.IndexOf(':'));
						var headerValue = lineText.Substring(lineText.IndexOf(':') + 1);

						if (!string.IsNullOrWhiteSpace(headerName) && !string.IsNullOrWhiteSpace(headerValue) && headerName != "accept-encoding"  )
							headers.Add(headerName, headerValue);
					}
					catch (Exception)
					{
						//whoops
					}
					
				}
			}

			sr.Close();

			return headers;
		}
	}
}