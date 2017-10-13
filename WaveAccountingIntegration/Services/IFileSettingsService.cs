using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WaveAccountingIntegration.Models;

namespace WaveAccountingIntegration.Services
{
    public interface IFileSettingsService
    {
        AppSettings GetSettings();
    }
}
