using BIDV_Winform.Constansts;
using BIDV_Winform.dto;
using BIDV_Winform.helper;
using BIDV_Winform.Services;
using Newtonsoft.Json;
using Microsoft.Web.WebView2.WinForms;

namespace BIDV_Winform
{
    public partial class FLogin : Form
    {
        private BIDVService _bidvService;
        private string _captchaTransId;
        private string _actionUrl;
        private string _loginUrl;
        private readonly WebView2 _webView;

        public FLogin()
        {
            InitializeComponent();
            _bidvService = new BIDVService();

            _webView = new WebView2();
            _webView.Visible = false;
            this.Controls.Add(_webView);
        }

        private async void FLogin_Load(object sender, EventArgs e)
        {
            _bidvService.generateLoginUrl();
            string loginUrl = _bidvService.GetLoginUrl();
            _loginUrl = loginUrl;
            
            if (loginUrl != null) {
                try
                {
                    await _webView.EnsureCoreWebView2Async();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WebView2 init error: {ex.Message}");
                    // fallback to HttpClient fetch
                    await _bidvService.LoadPageAsync(loginUrl);
                }

                // Tải dữ liệu lần đầu tiên
                await RefreshLoginDataAsync();

            }
        }

        private async Task RefreshLoginDataAsync()
        {
            try
            {
                // Lấy URL
                _bidvService.generateLoginUrl();
                _loginUrl = _bidvService.GetLoginUrl();
                if (string.IsNullOrEmpty(_loginUrl))
                {
                    MessageBox.Show("Không thể tạo URL đăng nhập.");
                    return;
                }

                _webView.CoreWebView2.Navigate(_loginUrl);

                // Đợi trang tải xong
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                void handler(object s, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs a)
                {
                    tcs.TrySetResult(true);
                }
                _webView.CoreWebView2.NavigationCompleted += handler;
                await tcs.Task;
                _webView.CoreWebView2.NavigationCompleted -= handler;

                // Lấy Cookies
                var cookieManager = _webView.CoreWebView2.CookieManager;
                var cwCookies = await cookieManager.GetCookiesAsync(_loginUrl);
                foreach (var c in cwCookies)
                {
                    string cookieName = c.Name;
                    string cookieValue = c.Value;

                    //if (cookieName == "I_DEVICE_ID")
                    //{
                    //    cookieValue = GlConstants.I_DEVICE_ID;
                    //}

                    _bidvService.AddCookie(
                        _loginUrl, cookieName, cookieValue, c.Domain, c.Path ?? "/",
                        c.IsSecure, c.IsHttpOnly, c.Expires
                    );
                }

                // Lấy HTML
                var loginHtml = await _webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                string loginHtmlStr = string.Empty;

                if (!string.IsNullOrEmpty(loginHtml) && loginHtml != "null")
                {
                    try { loginHtmlStr = System.Text.Json.JsonSerializer.Deserialize<string>(loginHtml); }
                    catch { loginHtmlStr = loginHtml.Trim('"'); }
                }
                if (!string.IsNullOrEmpty(loginHtmlStr))
                {
                    _bidvService.updateHtml(loginHtmlStr);
                    File.WriteAllText("debugLogin.html", loginHtmlStr);
                    _bidvService.InitHttpClient();
                }

                // Lấy Captcha
                await LoadCaptcha();

                // Lấy Action URL
                _actionUrl = _bidvService.GetActionUrl(_loginUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi làm mới dữ liệu: {ex.Message}");
            }
        }

        private async Task LoadCaptcha()
        {
            var result = await _bidvService.GetCaptchaAsync(GlConstants.GET_CAPTCHA_URL);

            _captchaTransId = result.Data.TransactionId;

            if (result?.Data?.Image != null)
            {
                // Loại bỏ prefix base64 nếu có
                var base64 = result.Data.Image.Replace("data:image/jpeg;base64,", "");
                var bytes = Convert.FromBase64String(base64);

                using (var ms = new MemoryStream(bytes))
                {
                    pictureBoxCaptcha.Image = Image.FromStream(ms);
                }
            }
            else
            {
                MessageBox.Show("Không tải được Captcha!");
            }
        }

        private async void iconRefresh_Click(object sender, EventArgs e)
        {
            await LoadCaptcha();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            var loginData = new LoginPayloadDto
            {
                Username = txtUsername.Text.ToUpper(),
                Password = txtPassword.Text,
                CaptchaValue = txtCaptcha.Text,
                CaptchaTransId = _captchaTransId,
                TypeSubmit = ""
            };

            string jsonPayload = JsonConvert.SerializeObject(loginData);

            string publicKey = _bidvService.GetValueById("publicKeyJwe");
            string privateKey = _bidvService.GetValueById("privateKeyJws");


            string encryptedPayload = BidvEncryptor.GenerateEncryptedPayload(loginData, privateKey, publicKey);

            //        MessageBox.Show(encryptedPayload);
            _bidvService.FormSubmitAsync(encryptedPayload, _actionUrl, _loginUrl);

            await RefreshLoginDataAsync();
            //if (result.IsSuccess)
            //{
            //    // THÀNH CÔNG! Lấy 'code' từ Location header
            //    string locationUrl = result.RedirectUrl;
            //    MessageBox.Show("Đăng nhập thành công! Đang phân tích URL:\n" + locationUrl);

            //    // Dùng Uri để phân tích fragment (phần sau dấu #)
            //    var uri = new Uri(locationUrl);
            //    string fragment = uri.Fragment; // Sẽ là: "#state=...&code=..."

            //    if (string.IsNullOrEmpty(fragment))
            //    {
            //        MessageBox.Show("Lỗi: Không tìm thấy fragment (#) trong URL redirect.");
            //        return;
            //    }

            //    // Dùng HttpUtility.ParseQueryString để bóc tách các tham số
            //    var queryParams = HttpUtility.ParseQueryString(fragment.Substring(1)); // Bỏ dấu #

            //    string code = queryParams["code"];
            //    string state = queryParams["state"];

            //    if (!string.IsNullOrEmpty(code))
            //    {
            //        MessageBox.Show($"LẤY CODE THÀNH CÔNG!\n\nCode: {code}\n\nState: {state}");

            //        // BƯỚC TIẾP THEO: Dùng code và code_verifier để lấy Token
            //        // await CallTokenApiAsync(code, _currentCodeVerifier);
            //    }
            //    else
            //    {
            //        MessageBox.Show("Lỗi: Đã đăng nhập nhưng không tìm thấy 'code' trong URL.");
            //    }
            //}
            //else
            //{
            //    // THẤT BẠI
            //    MessageBox.Show($"Đăng nhập thất bại: {result.ErrorMessage}");
            //    await RefreshLoginDataAsync();
            //}


        }
    }
}
