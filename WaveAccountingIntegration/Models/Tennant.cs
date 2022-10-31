using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WaveAccountingIntegration.Models
{
	public class Tennant
	{
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string FullName { get; set; }
		public DateTime DateOfBirth { get; set; }

		public override string ToString()
		{
			return $"{FirstName} {MiddleName} {LastName}";
		}
	}
}