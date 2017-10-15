namespace WaveAccountingIntegration.Models
{
	public class AppSettings
	{
		public string Bearer { get; set; }

		public string PersonalGuid { get; set; }

		public string MahayagBusinessGuid { get; set; }
		public int MahayagBusinessId { get; set; }
		public int MahayagLateFeeProductId { get; set; }
	}
}