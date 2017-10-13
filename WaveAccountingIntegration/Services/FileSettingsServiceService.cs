using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using WaveAccountingIntegration.Models;

namespace WaveAccountingIntegration.Services
{
    public class FileSettingsServiceService : IFileSettingsService
    {
        private readonly string path = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/AppSettings.json");

        public AppSettings GetSettings()
        {
            StreamReader sr = new StreamReader(path);

            var settings = JsonConvert.DeserializeObject<AppSettings>(sr.ReadToEnd());

            return settings;
        }
    }
}