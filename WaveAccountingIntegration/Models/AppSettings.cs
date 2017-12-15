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


		public List<Address> MahayagAddresses { get; set; }

		public class Address
		{
			public string id { get; set; }
			public string Address1 { get; set; }
			public string City { get; set; }
			public string State { get; set; }
			public int ZipCode { get; set; }
		}

		public GoogleAccountSettings  GoogleSettings { get; set; }

		public class GoogleAccountSettings
		{
			public string UserName { get; set; }
			public string Password { get; set; }
		}
	}

	
}