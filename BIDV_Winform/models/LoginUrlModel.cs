using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDV_Winform.models
{
    public static class LoginUrlModel
    {
        public static string State { get; set; }
        public static string Nonce { get;set; }
        public static string CodeChallenge { get; set; }
        public static string RedirectUri = "https://www.bidv.vn/BIDVDirect/?locale=vi-vn";
        public static string ClientId = "ibank-fo";
    }
}
