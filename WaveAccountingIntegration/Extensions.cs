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
			return $"{date.Month:00}/{date.Day:00}/{date.Year:0000}";
		}

		public static string ToISODateFormat(this DateTime date)
		{
			return $"{date.Year:0000}-{date.Month:00}-{date.Day:00}";
		}

		public static DateTime GetEndOfTheMonth(this DateTime today)
		{
			return new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
		}

		public static DateTime GetEndOfLeaseDate(this DateTime invoiceDate)
		{
			return DateTime.Now.GetEndOfTheMonth().AddDays(invoiceDate.Day - 1);
		}

		public static string ToCurrency(this decimal amount)
		{
			return amount.ToString("C");
		}

		public static string ToCurrency(this int amount)
		{
			return amount.ToString("C");
		}
	}
}