using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WaveAccountingIntegration.Models
{
    public class RestResult<T>
    {
        public int StatusCode { get; set; }

        public T Result { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode <= 299;
    }
}