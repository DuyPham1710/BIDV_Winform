using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BIDV_Winform.helper
{
    public static class BIDVHelper
    {
        public static string GenerateUUID()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetCodeFromUrl(string fullUrl)
        {
            try
            {
                var uri = new Uri(fullUrl);

                // Lấy phần Fragment (là chuỗi sau dấu #)
                // Kết quả sẽ là: "#state=...&code=..."
                string fragment = uri.Fragment;

                if (string.IsNullOrEmpty(fragment) || fragment.Length < 2)
                {
                    return null;
                }

                // Bỏ dấu '#' ở đầu đi để thành chuỗi query chuẩn: "state=...&code=..."
                string queryParamsString = fragment.Substring(1);

                // Dùng HttpUtility để tách thành Dictionary (Key-Value)
                var queryParams = HttpUtility.ParseQueryString(queryParamsString);

                string code = queryParams["code"];

                return code;
            }
            catch
            {
                return null;
            }
        }
        public static string GenerateUuidV4()
        {
            byte[] buffer = new byte[16];
            RandomNumberGenerator.Fill(buffer);

            // UUID v4
            buffer[6] = (byte)((buffer[6] & 0x0F) | 0x40);
            buffer[8] = (byte)((buffer[8] & 0x3F) | 0x80);

            return new Guid(buffer).ToString();
        }

        // Sinh chuỗi ngẫu nhiên giống hàm D(96)
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

        // Sinh code_challenge từ code_verifier (SHA256 + Base64URL)
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
