using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDV_Winform.models
{
    public class OtpPayloadRequest
    {
        [JsonProperty("otpValue", Order = 1)]
        public string OtpValue { get; set; }

        [JsonProperty("otpValueCount", Order = 2)]
        public string OtpValueCount { get; set; } = "120";

        [JsonProperty("typeSubmit", Order = 3)]
        public string TypeSubmit { get; set; } = "SUBMIT";
    }
}
