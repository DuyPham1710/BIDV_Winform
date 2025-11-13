using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDV_Winform.models
{
    public class CaptchaResponseModel
    {
        public int Status { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string TraceId { get; set; }
        public CaptchaData Data { get; set; }
    }

    public class CaptchaData
    {
        public string TransactionId { get; set; }
        public int ExpiresIn { get; set; }
        public string Image { get; set; }
    }

}
