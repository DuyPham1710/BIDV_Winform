using BIDV_Winform.Constansts;
using BIDV_Winform.helper;
using BIDV_Winform.models;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace BIDV_Winform.Services
{
    public class BIDVService
    {
        private readonly CookieContainer _cookieJar = new CookieContainer();
        private HttpClient _httpClient;
        private HtmlAgilityPack.HtmlDocument _doc;

        // URI đợi khi thành công
        private const string ExpectedRedirectUriStart = "https://www.bidv.vn/BIDVDirect/?locale=vi-vn#state=";

        public string CodeVerifier { get; private set; }
        public string Code { get; private set; }

        public void generateLoginUrl()
        {
            LoginUrlModel.State = BIDVHelper.GenerateUuidV4();
            LoginUrlModel.Nonce = BIDVHelper.GenerateUuidV4();

            // Tạo cặp PKCE verifier/challenge
            CodeVerifier = BIDVHelper.GenerateRandomString(96);
            LoginUrlModel.CodeChallenge = BIDVHelper.GeneratePkceChallenge(CodeVerifier);
        }

        public void AddCookie(string loginUrl, string name, string value, string domain, string path = "/", bool secure = false, bool httpOnly = false, DateTime? expires = null)
        {
            try
            {
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
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
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
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.TryAddWithoutValidation("x-api-interaction-id", "9a1e7c25-41f0-4a2c-8bcb-bc1df493f21e");
            request.Headers.TryAddWithoutValidation("x-client-id", "aecd62541229200280e2d68da852a77c");
            request.Headers.TryAddWithoutValidation("channel", "BIDVDIRECT");
            request.Headers.TryAddWithoutValidation("timestamp", timestamp);

            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            var captchaResponse = JsonConvert.DeserializeObject<CaptchaResponseModel>(responseBody);

            return captchaResponse;
        }

        public string GetLoginUrl()
        {
            string encodedRedirectUri = Uri.EscapeDataString(LoginUrlModel.RedirectUri);
            return $"https://www.bidv.vn/sso/direct/realms/bidvdirect/protocol/openid-connect/auth?" +
                   $"client_id={LoginUrlModel.ClientId}" +
                   $"&redirect_uri={encodedRedirectUri}" +
                   $"&state={LoginUrlModel.State}" +
                   $"&response_mode=fragment" +
                   $"&response_type=code" +
                   $"&scope=openid" +
                   $"&nonce={LoginUrlModel.Nonce}" +
                   $"&code_challenge={LoginUrlModel.CodeChallenge}" +
                   $"&code_challenge_method=S256" +
                   $"&ui_locales=vi-vn";
        }
        public void updateHtml(string html)
        {
            _doc = new HtmlAgilityPack.HtmlDocument();
            _doc.LoadHtml(html);
        }

        public async Task LoadPageAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string html = await response.Content.ReadAsStringAsync();
          
          //  File.WriteAllText("debugLogin1.html", html);
            _doc = new HtmlAgilityPack.HtmlDocument();
            _doc.LoadHtml(html);
        }

        public string? GetActionUrlById(string id)
        {
            if (_doc == null) return null;

            var formNode = _doc.DocumentNode.SelectSingleNode($"//form[@id='{id}']");
            var action = formNode?.GetAttributeValue("action", "");

            if (string.IsNullOrEmpty(action)) return null;

            return action.ToString();
        }

        public string? GetValueById(string id)
        {
            if (_doc == null) return null;

            var inputNode = _doc.DocumentNode.SelectSingleNode($"//input[@id='{id}']");
            var value = inputNode?.GetAttributeValue("value", "");

            if (string.IsNullOrEmpty(value)) return null;

        
            return value.ToString();
        }

        //public void PrintCookies(string domain = "https://www.bidv.vn/")
        //{
        //    try
        //    {
        //        Uri uri = new Uri(domain);
        //        var cookies = _cookieJar.GetCookies(uri);

        //        System.Diagnostics.Debug.WriteLine($"🍪 Cookies for {domain}: ({cookies.Count})");
        //        foreach (Cookie cookie in cookies)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"{cookie.Domain}\t{cookie.Name}={cookie.Value}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"⚠️ Error printing cookies: {ex.Message}");
        //    }
        //}


        /// <summary>
        /// Gọi API đăng nhập authenticate với payload đã mã hóa
        /// </summary>
        public async Task<bool> FormSubmitAsync(string encryptedPayload, string actionUrl)
        {
            try
            {
                var formData = new Dictionary<string, string>
                {
                    { "encrypted_payload", encryptedPayload }
                };

                var content = new FormUrlEncodedContent(formData);
   
                HttpResponseMessage response = await _httpClient.PostAsync(actionUrl, content);

                string html = await response.Content.ReadAsStringAsync();
          //      File.WriteAllText("debug.html", html);

                updateHtml(html);

                //      return response.StatusCode == HttpStatusCode.OK;
                if (response.StatusCode == HttpStatusCode.Found) // 302
                {
                    string location = response.Headers.Location?.OriginalString;

                    // Phân biệt thành công hay thất bại
                    if (location.StartsWith(ExpectedRedirectUriStart))
                    {
                        // THÀNH CÔNG! Chuyển hướng về trang app với 'code'
                        Code = BIDVHelper.GetCodeFromUrl(location);
                    }
                    
                    return false; // lấy được code
                }

                // Nếu server trả về 200 OK
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // Các lỗi khác (500, 404...)
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message );
                return false;
               // return new FormResult { ErrorMessage = $"Lỗi mạng: {ex.Message}" };
            }
        }

        public async Task<LoginResponse> GetTokenAsync(LoginRequest request)
        {
            try
            {
                var postData = new Dictionary<string, string>
                {
                    { "code", request.Code },
                    { "grant_type", request.Grant_type },
                    { "client_id", request.Client_id },
                    { "redirect_uri", request.Redirect_uri },
                    { "code_verifier", request.Code_verifier }
                };

                var content = new FormUrlEncodedContent(postData);

                // Gửi Request POST
                HttpResponseMessage response = await _httpClient.PostAsync(GlConstants.TOKEN_URL, content);

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Kiểm tra lỗi
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi GetToken: {response.StatusCode} - {jsonResponse}");
                    return null;
                }

                // Deserialize JSON về đối tượng LoginResponse
                return JsonConvert.DeserializeObject<LoginResponse>(jsonResponse);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception GetToken: {ex.Message}");
                return null;
            }
        }
    }
}
