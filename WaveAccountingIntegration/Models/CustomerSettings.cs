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
	}
}