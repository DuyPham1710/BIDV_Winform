using BIDV_Winform.dto;
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
        /// <summary>
        /// Tạo ra encrypted_payload bằng cách KÝ (JWS) rồi MÃ HÓA (JWE)
        /// </summary>
        /// <param name="payload">Đối tượng chứa thông tin đăng nhập</param>
        /// <param name="publicKeyJweBase64">Key công khai của BIDV (để mã hóa)</param>
        /// <param name="privateKeyJwsBase64">Key bí mật của bạn (để ký)</param>
        /// <returns>Chuỗi encrypted_payload</returns>
        public static string GenerateEncryptedPayload(LoginPayloadDto payload, string privateKeyJwsBase64, string publicKeyJweBase64)
        {
            try
            {
                // ----- BƯỚC 1: CHUẨN BỊ DỮ LIỆU GỐC -----
                // Dùng class LoginPayload (với [JsonProperty(Order=...)])
                string jsonPayload = JsonConvert.SerializeObject(payload);
                byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

                // ----- BƯỚC 2: KÝ TÊN (SIGNING) -> TẠO JWS (BẰNG TAY) -----

                // Lấy JWS private key (như code cũ)
                string jwsKeyString = Encoding.UTF8.GetString(Convert.FromBase64String(privateKeyJwsBase64));
                var jwsJwk = JsonConvert.DeserializeObject<Dictionary<string, string>>(jwsKeyString);
                var rsaParamsPrivate = new RSAParameters
                {
                    Modulus = Base64UrlDecode(jwsJwk["n"]),
                    Exponent = Base64UrlDecode(jwsJwk["e"]),
                    D = Base64UrlDecode(jwsJwk["d"]),
                    P = Base64UrlDecode(jwsJwk["p"]),
                    Q = Base64UrlDecode(jwsJwk["q"]),
                    DP = Base64UrlDecode(jwsJwk["dp"]),
                    DQ = Base64UrlDecode(jwsJwk["dq"]),
                    InverseQ = Base64UrlDecode(jwsJwk["qi"])
                };
                RSA rsaJwsKey = RSA.Create();
                rsaJwsKey.ImportParameters(rsaParamsPrivate);

                // 1. Tạo Header CHỈ có 'alg' (GIỐNG HỆT JS)
                var jwsHeader = new Dictionary<string, object>
                {
                    { "alg", "RS512" }
                };

                // 2. Serialize header và Base64Url encode
                string protectedHeader = Base64Url.Encode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jwsHeader)));

                // 3. Base64Url encode payload
                string encodedPayload = Base64Url.Encode(payloadBytes);

                // 4. Tạo chuỗi để ký
                string inputToSign = $"{protectedHeader}.{encodedPayload}";
                byte[] inputBytes = Encoding.ASCII.GetBytes(inputToSign);

                // 5. Ký (Sign) bằng thuật toán SHA512 với PKCS1 padding
                byte[] signatureBytes = rsaJwsKey.SignData(inputBytes, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);

                // 6. Base64Url encode chữ ký
                string signature = Base64Url.Encode(signatureBytes);

                // 7. Ghép lại thành chuỗi JWS hoàn chỉnh
                string jws = $"{protectedHeader}.{encodedPayload}.{signature}";

                // ----- BƯỚC 3: MÃ HÓA (ENCRYPTING) -> TẠO JWE -----
                // (Phần này giữ nguyên như cũ, vì nó mã hóa chuỗi 'jws')
                string jweKeyString = Encoding.UTF8.GetString(Convert.FromBase64String(publicKeyJweBase64));
                var jweJwk = JsonConvert.DeserializeObject<Dictionary<string, string>>(jweKeyString);
                var rsaParamsPublic = new RSAParameters
                {
                    Modulus = Base64UrlDecode(jweJwk["n"]),
                    Exponent = Base64UrlDecode(jweJwk["e"])
                };
                RSA rsaJweKey = RSA.Create();
                rsaJweKey.ImportParameters(rsaParamsPublic);

                // Mã hóa toàn bộ chuỗi JWS (ở Bước 2)
                string jwe = JWT.Encode(
                    jws,
                    rsaJweKey,
                    JweAlgorithm.RSA_OAEP,
                    JweEncryption.A256GCM
                );

                // ----- BƯỚC 4: HOÀN TẤT -----
                return jwe;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi nghiêm trọng khi tạo JWE/JWS: " + ex.ToString());
                throw new InvalidOperationException("Không thể tạo payload mã hóa.", ex);
            }
        }

        /// <summary>
        /// Hàm hỗ trợ giải mã Base64URL (chuẩn của JWE/JWS)
        /// </summary>
        private static byte[] Base64UrlDecode(string input)
        {
            string output = input.Replace('-', '+').Replace('_', '/');
            switch (output.Length % 4) // Bù lại các ký tự '=' đã bị cắt
            {
                case 0: break;
                case 2: output += "=="; break;
                case 3: output += "="; break;
                default: throw new ArgumentException("Chuỗi Base64URL không hợp lệ!");
            }
            return Convert.FromBase64String(output);
        }

        public static string CreateEncryptedPayload(string jsonPayload, string privateJwsBase64, string publicJweBase64)
        {
            // Giải mã Base64 để lấy JSON JWK
            var privateJwkJson = Encoding.UTF8.GetString(Convert.FromBase64String(privateJwsBase64));
            var publicJwkJson = Encoding.UTF8.GetString(Convert.FromBase64String(publicJweBase64));

            // Parse JSON -> đối tượng chứa thông số RSA
            var privateParams = JsonConvert.DeserializeObject<JwkRsaKey>(privateJwkJson);
            var publicParams = JsonConvert.DeserializeObject<JwkRsaKey>(publicJwkJson);

            // Chuyển private JWK -> RSAParameters
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

            // Chuyển public JWK -> RSAParameters
            RSA publicRsa = RSA.Create();
            RSAParameters publicKeyParams = new RSAParameters
            {
                Modulus = Base64Url.Decode(publicParams.n),
                Exponent = Base64Url.Decode(publicParams.e)
            };
            publicRsa.ImportParameters(publicKeyParams);

            // Encode JSON payload (đã serialize từ DTO)
            var payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

            // Tạo JWS: ký payload bằng private key (RS512)
            string jws = JWT.Encode(payloadBytes, privateRsa, JwsAlgorithm.RS512);

            // Mã hoá JWS thành JWE (RSA-OAEP + A256GCM)
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
