using Jose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BIDV_Winform.helper
{
    public class BidvEncryptor
    {
        public static string CreateEncryptedPayload(string jsonPayload, string privateJwsBase64, string publicJweBase64)
        {
            // 1️⃣ Giải mã Base64 để lấy JSON JWK
            var privateJwkJson = Encoding.UTF8.GetString(Convert.FromBase64String(privateJwsBase64));
            var publicJwkJson = Encoding.UTF8.GetString(Convert.FromBase64String(publicJweBase64));

            // 2️⃣ Parse JSON -> đối tượng chứa thông số RSA
            var privateParams = JsonConvert.DeserializeObject<JwkRsaKey>(privateJwkJson);
            var publicParams = JsonConvert.DeserializeObject<JwkRsaKey>(publicJwkJson);

            // 3️⃣ Chuyển private JWK -> RSAParameters
            RSA privateRsa = RSA.Create();
            RSAParameters privateKeyParams = new RSAParameters
            {
                Modulus = Base64Url.Decode(privateParams.n),
                Exponent = Base64Url.Decode(privateParams.e),
                D = Base64Url.Decode(privateParams.d),
                P = Base64Url.Decode(privateParams.p),
                Q = Base64Url.Decode(privateParams.q),
                DP = Base64Url.Decode(privateParams.dp),
                DQ = Base64Url.Decode(privateParams.dq),
                InverseQ = Base64Url.Decode(privateParams.qi)
            };
            privateRsa.ImportParameters(privateKeyParams);

            // 4️⃣ Chuyển public JWK -> RSAParameters
            RSA publicRsa = RSA.Create();
            RSAParameters publicKeyParams = new RSAParameters
            {
                Modulus = Base64Url.Decode(publicParams.n),
                Exponent = Base64Url.Decode(publicParams.e)
            };
            publicRsa.ImportParameters(publicKeyParams);

            // 5️⃣ Encode JSON payload (đã serialize từ DTO)
            var payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

            // 6️⃣ Tạo JWS: ký payload bằng private key (RS512)
            string jws = JWT.Encode(payloadBytes, privateRsa, JwsAlgorithm.RS512);

            // 7️⃣ Mã hoá JWS thành JWE (RSA-OAEP + A256GCM)
            string jwe = JWT.Encode(jws, publicRsa, JweAlgorithm.RSA_OAEP, JweEncryption.A256GCM);

            return jwe;
        }
    }

    public class JwkRsaKey
    {
        public string kty { get; set; }
        public string alg { get; set; }
        public string e { get; set; }
        public string n { get; set; }
        public string d { get; set; }
        public string p { get; set; }
        public string q { get; set; }
        public string dp { get; set; }
        public string dq { get; set; }
        public string qi { get; set; }
    }
}
