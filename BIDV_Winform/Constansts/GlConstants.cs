using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BIDV_Winform.Constansts
{
    internal class GlConstants
    {
        public static readonly string GET_CAPTCHA_URL = "https://www.bidv.vn/bidv/direct/open-banking/bidv-direct/auth/captcha/get/1.0";
        public static readonly string I_DEVICE_ID = "YzE5Y2VhZjdmNjhhNGMwZGFkNjMwYTE5M2FkZTQ5Y2U2N2FmN2Y4YjAyMThmNjFhMGExMTJjZDcwM2JjMjBjZA==";
        public static readonly string TOKEN_URL = "https://www.bidv.vn/sso/direct/realms/bidvdirect/protocol/openid-connect/token";
    }
}
