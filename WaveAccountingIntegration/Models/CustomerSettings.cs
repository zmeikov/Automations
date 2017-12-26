using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WaveAccountingIntegration.Models
{
	public class CustomerSettings
	{
		public bool? ChargeLateFee { get; set; }

		public DateTime? NextLateFeeChargeDate { get; set; }

		public bool? ConsolidateInvoices { get; set; }

		public DateTime? LastSmsAlertSent { get; set; }

		public int? CustomDaysBetweenSmsAlerts { get; set; }

		public bool? SendSmsAlerts { get; set; }

		public string StatementUrl { get; set; }

		public DateTime? EvictonNoticeDate { get; set; }

		public string EvictionCourtCaseNumber { get; set; }

		public string EvictionCourtAssignedJudge { get; set; }
	}
}