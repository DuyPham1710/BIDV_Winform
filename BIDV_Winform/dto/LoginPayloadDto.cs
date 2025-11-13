using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDV_Winform.dto
{
    /// <summary>
    /// Lớp chứa payload, khớp với cấu trúc JSON
    /// (Sử dụng [JsonProperty] để đảm bảo tên trường JSON chính xác)
    /// </summary>
    public class LoginPayloadDto
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("captchaValue")]
        public string CaptchaValue { get; set; }

        [JsonProperty("captchaTransId")]
        public string CaptchaTransId { get; set; }

        [JsonProperty("typeSubmit")]
        public string typeSubmit { get; set; } = "";
    }
}
