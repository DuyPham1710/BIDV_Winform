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
    public static class BIDVHelper
    {
        public static string GenerateUUID()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Mã hóa payload đăng nhập thành chuỗi JWE (encrypted_payload)
        /// </summary>
        /// <param name="payload">Đối tượng chứa thông tin đăng nhập</param>
        /// <returns>Chuỗi JWE (encrypted_payload)</returns>
        //public static string GenerateEncryptedPayload(LoginPayloadDto payload)
        //{
        //    try
        //    {
        //        // ----- BƯỚC 1: CHUẨN BỊ PUBLIC KEY (JWK) -----
        //        // Đây là các thành phần 'n' (Modulus) và 'e' (Exponent)
        //        // từ Khóa Công Khai (publicKeyJwe) của bạn.
        //        string n_base64Url = "z6h5NLuaHh6aCbDxqJCTE6VWISYfzaLJ0sXCxIIy4q6y99vnPn0r20mQoGZ0G1QgLYXAZqD3__tsroG76w-VPl6WEdhGO-ZQwbXXtdXTSjFWd2Fxbns-QD3KbNqtdAkeYJ6ZN5EjfBEAJAEpLU5QPfL1j6uNkoWTolqsA1rzyqdMecSVO3oH5kHDpOK-4qpvPT-qPM-zAHnSRkphH1cvWISR427OlyrHAOsWJn1Ii4TPqXL0_n9qfYc9eBhazT2iOYmhuQmq18edxuu1NSCBLU0cekVURz9ESYmF1scGUdboK4jv4D47S35vdwe_ePS0S1MYUs7Sv_0dEzJOdMmizQ";
        //        string e_base64Url = "AQAB";

        //        // Giải mã Base64URL về mảng byte
        //        byte[] n_bytes = Base64UrlDecode(n_base64Url);
        //        byte[] e_bytes = Base64UrlDecode(e_base64Url);

        //        // Tạo đối tượng khóa RSA từ các tham số
        //        var rsaParams = new RSAParameters
        //        {
        //            Modulus = n_bytes,
        //            Exponent = e_bytes
        //        };

        //        // Dùng RSA.Create() để tương thích đa nền tảng
        //        RSA rsaPublicKey = RSA.Create();
        //        rsaPublicKey.ImportParameters(rsaParams);


        //        // ----- BƯỚC 2: CHUẨN BỊ DỮ LIỆU PAYLOAD -----
        //        // Chuyển đối tượng C# thành chuỗi JSON
        //        string jsonPayload = JsonConvert.SerializeObject(payload);


        //        // ----- BƯỚC 3 & 4: MÃ HÓA (TẠO JWE) -----
        //        // Thư viện jose-jwt sẽ tự động làm Mã Hóa Lai (Hybrid Encryption):
        //        // 1. Tự tạo key AES (A256GCM)
        //        // 2. Dùng key AES mã hóa payload (jsonPayload) -> CIPHERTEXT
        //        // 3. Dùng key RSA (rsaPublicKey) mã hóa key AES -> ENCRYPTED_KEY
        //        // 4. Ghép 5 phần lại (HEADER.KEY.IV.CIPHERTEXT.TAG)

        //        string encryptedPayload = JWT.Encode(
        //            jsonPayload,               // Dữ liệu cần mã hóa
        //            rsaPublicKey,              // Khóa công khai để mã hóa "chìa khóa"
        //            JweAlgorithm.RSA_OAEP,     // Thuật toán 'alg': RSA-OAEP
        //            JweEncryption.A256GCM      // Thuật toán 'enc': A256GCM
        //        );

        //        return encryptedPayload;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Rất quan trọng: ghi lại lỗi nếu có
        //        Console.WriteLine("Lỗi mã hóa JWE: " + ex.Message);
        //        throw; // Hoặc trả về null/string rỗng tùy logic của bạn
        //    }
        //}

        ///// <summary>
        ///// Hàm hỗ trợ giải mã Base64URL (chuẩn của JWE/JWS)
        ///// </summary>
        //private static byte[] Base64UrlDecode(string input)
        //{
        //    string output = input.Replace('-', '+').Replace('_', '/');
        //    switch (output.Length % 4) // Bù lại các ký tự '=' đã bị cắt
        //    {
        //        case 0: break;
        //        case 2: output += "=="; break;
        //        case 3: output += "="; break;
        //        default: throw new ArgumentException("Chuỗi Base64URL không hợp lệ!");
        //    }
        //    return Convert.FromBase64String(output);
        //}

        public static string GenerateUuidV4()
        {
            byte[] buffer = new byte[16];
            RandomNumberGenerator.Fill(buffer);

            // UUID v4
            buffer[6] = (byte)((buffer[6] & 0x0F) | 0x40);
            buffer[8] = (byte)((buffer[8] & 0x3F) | 0x80);

            return new Guid(buffer).ToString();
        }

        // ✅ Sinh chuỗi ngẫu nhiên giống hàm D(96)
        public static string GenerateRandomString(int length)
        {
            const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            var chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = charset[bytes[i] % charset.Length];

            return new string(chars);
        }

        // ✅ Sinh code_challenge từ code_verifier (SHA256 + Base64URL)
        public static string GeneratePkceChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            return Base64UrlEncode(bytes);
        }

        public static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
