using BIDV_Winform.Constansts;
using BIDV_Winform.helper;
using BIDV_Winform.models;
using BIDV_Winform.Services;
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

                    if (cookieName == "I_DEVICE_ID")
                    {
                        cookieValue = GlConstants.I_DEVICE_ID;
                    }

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
               //     File.WriteAllText("debugLogin.html", loginHtmlStr);
                    _bidvService.InitHttpClient();
                }

                // Lấy Captcha
                await LoadCaptcha();

                // Lấy Action URL
                _actionUrl = _bidvService.GetActionUrlById("kc-form-login");
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
            var req = new FormPayloadRequest
            {
                Username = txtUsername.Text.ToUpper(),
                Password = txtPassword.Text,
                CaptchaValue = txtCaptcha.Text,
                CaptchaTransId = _captchaTransId,
                TypeSubmit = ""
            };

            string publicKey = _bidvService.GetValueById("publicKeyJwe");
            string privateKey = _bidvService.GetValueById("privateKeyJws");


            string encryptedPayload = BidvEncryptor.GenerateEncryptedPayload(req, privateKey, publicKey);

            bool statusCode200 =  await _bidvService.FormSubmitAsync(encryptedPayload, _actionUrl);


            if (statusCode200)
            {
                string actionUrlOtp = _bidvService.GetActionUrlById("kc-otp-login-form");

                if (actionUrlOtp != null)
                {
                    FOtp fOtp = new FOtp(_bidvService, actionUrlOtp);
                    this.Hide();
                    fOtp.ShowDialog();
                    this.Show();
                }
                else
                {
                    MessageBox.Show("action URL OTP is empty!!!");
                }

            }
            else // server có trả về code thì thực hiện call API /token
            {
                var loginDto = new LoginRequest
                {
                    Code = _bidvService.Code,
                    Code_verifier = _bidvService.CodeVerifier,
                };

                LoginResponse tokenResponse = await _bidvService.GetTokenAsync(loginDto);

                if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    FHome fHome = new FHome();
                    this.Hide();
                    fHome.ShowDialog();
                    this.Show();
                    //MessageBox.Show("LẤY TOKEN THÀNH CÔNG!");

                    //// In thử Access Token ra xem
                    //System.Diagnostics.Debug.WriteLine("Access Token: " + tokenResponse.AccessToken);
                    //System.Diagnostics.Debug.WriteLine("Refresh Token: " + tokenResponse.RefreshToken);

                    // TODO: Lưu tokenResponse này lại để dùng cho các API chuyển tiền/lấy số dư sau này
                    // _bidvService.SetAuthToken(tokenResponse.AccessToken);
                }
                else
                {
                    MessageBox.Show("Lỗi: Không lấy được Token.");
                }
            }

            await RefreshLoginDataAsync();  
        }
    }
}
