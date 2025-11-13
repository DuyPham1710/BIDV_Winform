using BIDV_Winform.dto;
using BIDV_Winform.helper;
using BIDV_Winform.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BIDV_Winform.Services
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public string RedirectUrl { get; set; } // Sẽ chứa Location header
        public string ErrorMessage { get; set; }
    }

    public class BIDVService
    {
        private readonly CookieContainer _cookieJar = new CookieContainer();
        private HttpClient _httpClient;
        private HtmlAgilityPack.HtmlDocument _doc;
        // URI mà chúng ta mong đợi khi thành công
        private const string ExpectedRedirectUriStart = "https://www.bidv.vn/BIDVDirect/";

        public string State { get; private set; }
        public string Nonce { get; private set; }
        public string CodeChallenge { get; private set; }
        public string CodeVerifier { get; private set; }

        public const string RedirectUri = "https://www.bidv.vn/BIDVDirect/?locale=vi-vn";
        public const string ClientId = "ibank-fo";

        public BIDVService()
        {
            //InitHttpClient();

            // Sinh state và nonce giống hàm h() trong JS (UUID v4)
            State = BIDVHelper.GenerateUuidV4();
            Nonce = BIDVHelper.GenerateUuidV4();

            // Tạo cặp PKCE verifier/challenge
            CodeVerifier = BIDVHelper.GenerateRandomString(96); // JS dùng D(96)
            CodeChallenge = BIDVHelper.GeneratePkceChallenge(CodeVerifier);
        }

        public void AddCookie(string loginUrl, string name, string value, string domain, string path = "/", bool secure = false, bool httpOnly = false, DateTime? expires = null)
        {
            try
            {
                // Domain may include leading dot; CookieContainer.Add requires a Uri
                var host = domain?.TrimStart('.') ?? "www.bidv.vn";
                var uri = new Uri($"https://{host}");
                var cookie = new Cookie(name, value, path, domain)
                {
                    Secure = secure,
                    HttpOnly = httpOnly
                };
                if (expires.HasValue)
                {
                    cookie.Expires = expires.Value;
                }
                _cookieJar.Add(uri, cookie);
                System.Diagnostics.Debug.WriteLine($"Imported cookie: {name}={value}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding cookie: {ex.Message}");
            }
        }


        public void InitHttpClient()
        {
          //  var baseUri = new Uri("https://www.bidv.vn/");
            //_cookieJar.Add(baseUri, new Cookie("I_DEVICE_ID",
            //    "YzE5Y2VhZjdmNjhhNGMwZGFkNjMwYTE5M2FkZTQ5Y2U2N2FmN2Y4YjAyMThmNjFhMGExMTJjZDcwM2JjMjBjZA=="));


            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = _cookieJar,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler);
            // Thêm header mô phỏng trình duyệt
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<CaptchaResponseModel?> GetCaptchaAsync(string url)
        {
           // string apiInteractionId = BIDVHelper.GenerateUUID();

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var payload = new { };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.TryAddWithoutValidation("x-api-interaction-id", "9a1e7c25-41f0-4a2c-8bcb-bc1df493f21e");
            request.Headers.TryAddWithoutValidation("x-client-id", "aecd62541229200280e2d68da852a77c");
            request.Headers.TryAddWithoutValidation("channel", "BIDVDIRECT");
            request.Headers.TryAddWithoutValidation("timestamp", timestamp);

            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            //System.Diagnostics.Debug.WriteLine("Raw Response:");
            //System.Diagnostics.Debug.WriteLine(responseBody);

            var captchaResponse = JsonSerializer.Deserialize<CaptchaResponseModel>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return captchaResponse;
        }

        public string GetLoginUrl()
        {
            string encodedRedirectUri = Uri.EscapeDataString(RedirectUri);
            return $"https://www.bidv.vn/sso/direct/realms/bidvdirect/protocol/openid-connect/auth?" +
                   $"client_id={ClientId}" +
                   $"&redirect_uri={encodedRedirectUri}" +
                   $"&state={State}" +
                   $"&response_mode=fragment" +
                   $"&response_type=code" +
                   $"&scope=openid" +
                   $"&nonce={Nonce}" +
                   $"&code_challenge={CodeChallenge}" +
                   $"&code_challenge_method=S256" +
                   $"&ui_locales=vi-vn";
        }
        public void updateHtml(string html)
        {
            //   File.WriteAllText("debugHomePage.html", html);
            _doc = new HtmlAgilityPack.HtmlDocument();
            _doc.LoadHtml(html);
        }

        public async Task LoadPageAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string html = await response.Content.ReadAsStringAsync();
          
            File.WriteAllText("debugLogin1.html", html);
            _doc = new HtmlAgilityPack.HtmlDocument();
            _doc.LoadHtml(html);
        }

        public string? GetActionUrl(string baseUrl)
        {
            if (_doc == null) return null;

            var formNode = _doc.DocumentNode.SelectSingleNode("//form[@id='kc-form-login']");
            var action = formNode?.GetAttributeValue("action", "");

            if (string.IsNullOrEmpty(action)) return null;

            Uri baseUri = new Uri(baseUrl);
            Uri actionUri = new Uri(baseUri, action);

            return actionUri.ToString();
        }

        public string? GetValueById(string id)
        {
            if (_doc == null) return null;

            var inputNode = _doc.DocumentNode.SelectSingleNode($"//input[@id='{id}']");
            var value = inputNode?.GetAttributeValue("value", "");

            if (string.IsNullOrEmpty(value)) return null;

        
            return value.ToString();
        }
        public void PrintCookies(string domain = "https://www.bidv.vn/")
        {
            try
            {
                Uri uri = new Uri(domain);
                var cookies = _cookieJar.GetCookies(uri);

                System.Diagnostics.Debug.WriteLine($"🍪 Cookies for {domain}: ({cookies.Count})");
                foreach (Cookie cookie in cookies)
                {
                    System.Diagnostics.Debug.WriteLine($"{cookie.Domain}\t{cookie.Name}={cookie.Value}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error printing cookies: {ex.Message}");
            }
        }


        /// <summary>
        /// Gọi API đăng nhập authenticate với payload đã mã hóa
        /// </summary>
        public async Task<LoginResult> LoginAsync(string encryptedPayload, string actionUrl, string loginUrl)
        {
            try
            {
                // 1. Chuẩn bị Form Data
                var formData = new Dictionary<string, string>
                {
                    { "encrypted_payload", encryptedPayload }
                };
                var content = new FormUrlEncodedContent(formData);

                // 2. Gửi POST Request

                System.Diagnostics.Debug.WriteLine("=== Before Login Request ===");
                PrintCookies(loginUrl);
                PrintCookies(loginUrl);

                HttpResponseMessage response = await _httpClient.PostAsync(actionUrl, content);

                System.Diagnostics.Debug.WriteLine("=== After Login Request ===");
                PrintCookies(loginUrl);
                PrintCookies(loginUrl);
                string html = await response.Content.ReadAsStringAsync();
                File.WriteAllText("debug.html", html);
                MessageBox.Show(response.StatusCode.ToString());
                // 3. Xử lý phản hồi (Response)
                // Chúng ta CHỈ tìm kiếm Status 302 Found
                if (response.StatusCode == HttpStatusCode.Found) // 302
                {
                    string location = response.Headers.Location?.OriginalString;

                    if (string.IsNullOrEmpty(location))
                    {
                        return new LoginResult { ErrorMessage = "Lỗi: Server trả về 302 nhưng không có Location header." };
                    }

                    // 4. Phân biệt thành công hay thất bại
                    if (location.StartsWith(ExpectedRedirectUriStart))
                    {
                        // THÀNH CÔNG! Chuyển hướng về trang app với 'code'
                        return new LoginResult { IsSuccess = true, RedirectUrl = location };
                    }
                    else
                    {
                        // THẤT BẠI! (Ví dụ: sai mật khẩu, sai captcha)
                        // Server chuyển hướng về lại trang login
                        return new LoginResult { ErrorMessage = "Thông tin đăng nhập hoặc Captcha không chính xác." };
                    }
                }

                // Nếu server trả về 200 OK (nghĩa là nó chỉ tải lại trang login)
                if (response.IsSuccessStatusCode)
                {
                    return new LoginResult { ErrorMessage = "Đăng nhập thất bại (Server trả về 200 OK, có thể do session hết hạn)." };
                }

                // Các lỗi khác (500, 404...)
                return new LoginResult { ErrorMessage = $"Lỗi máy chủ: {response.StatusCode}" };
            }
            catch (Exception ex)
            {
                return new LoginResult { ErrorMessage = $"Lỗi mạng: {ex.Message}" };
            }
        }


        // ---- Helpers ----

        // ✅ Sinh UUID v4 tương đương hàm h() trong JS
     
    }
}
