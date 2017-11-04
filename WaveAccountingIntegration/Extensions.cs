using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WaveAccountingIntegration
{
	public static class Extensions
	{
		public static string ToUSADateFormat(this DateTime date)
		{
			return $"{date.Month}/{date.Day}/{date.Year}";
		}
	}
}