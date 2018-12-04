using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Opera;
using Xunit;

namespace Misc
{
    public class WaveOperaRefresh
    {
        private readonly IWebDriver _driver;

        public WaveOperaRefresh()
        {
            _driver = new OperaDriver(new OperaOptions() { BinaryLocation = "C:\\Program Files (x86)\\Opera\\56.0.3051.52\\opera.exe" });
        }

        [Fact]
        public void Refresh()
        {
            _driver.Url = "https://accounting.waveapps.com/dashboard/business/4427109/#/";
            Thread.Sleep(15000);

            
            _driver.Close();
            _driver.Quit();
        }
    }
}
