using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDV_Winform.models
{
    /// <summary>
    /// Lớp chứa payload, khớp với cấu trúc JSON
    /// (Sử dụng [JsonProperty] để đảm bảo tên trường JSON chính xác)
    /// </summary>
    public class FormPayloadRequest
    {
        [JsonProperty("username", Order = 1)]
        public string Username { get; set; }

        [JsonProperty("password", Order = 2)]
        public string Password { get; set; }

        [JsonProperty("captchaValue", Order = 3)]
        public string CaptchaValue { get; set; }

        [JsonProperty("captchaTransId", Order = 4)]
        public string CaptchaTransId { get; set; }

        [JsonProperty("typeSubmit", Order = 5)]
        public string TypeSubmit { get; set; } = "";
    }
}
