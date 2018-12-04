using Common.Models;
using System.Collections.Generic;

namespace WaveAccountingIntegration.Models
{
	public class AppSettings
	{
		//This is now fetched via cookies parsing
		//public string Bearer { get; set; }

		public string PersonalGuid { get; set; }

		public string MahayagBusinessGuid { get; set; }
		public int MahayagBusinessId { get; set; }
		public int MahayagLateFeeProductId { get; set; }


		public List<Common.Models.Address> MahayagAddresses { get; set; }

		public GoogleAccountSettings GoogleSettings { get; set; }

		
	}

}