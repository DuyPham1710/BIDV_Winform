using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDV_Winform.models
{
    public class LoginRequest
    {
        public string code {  get; set; }
        public string grant_type { get; set; } = "authorization_code";
        public string client_id { get; set; } = "ibank-fo";
        public string redirect_uri { get; set; } = "https://www.bidv.vn/BIDVDirect/?locale=vi-vn";
        public string code_verifier { get; set; }

        public LoginRequest(string code, string code_verifier)
        {
            this.code = code;
            this.code_verifier = code_verifier;
        }
    }
}
