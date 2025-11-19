using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDV_Winform.models
{
    public class LoginRequest
    {
        [JsonProperty("code", Order = 1)]
        public string Code { get; set; }

        [JsonProperty("grant_type", Order = 2)]
        public string Grant_type { get; set; } = "authorization_code";

        [JsonProperty("client_id", Order = 3)]
        public string Client_id { get; set; } = "ibank-fo";

        [JsonProperty("redirect_uri", Order = 4)]
        public string Redirect_uri { get; set; } = "https://www.bidv.vn/BIDVDirect/?locale=vi-vn";

        [JsonProperty("code_verifier", Order = 5)]
        public string Code_verifier { get; set; }
    }
}
