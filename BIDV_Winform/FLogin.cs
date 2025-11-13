using BIDV_Winform.Constansts;
using BIDV_Winform.dto;
using BIDV_Winform.helper;
using BIDV_Winform.Services;
using Newtonsoft.Json;
using System.Buffers.Text;
using System.Net;
using System.Web;
using Microsoft.Web.WebView2.WinForms;

namespace BIDV_Winform
{
    public partial class FLogin : Form
    {
        private BIDVService _bidvService;
        private string _captchaTransId;
        private string _actionUrl;
        private string _loginUrl;
        private WebView2 _webView;

        public FLogin()
        {
            InitializeComponent();
            _bidvService = new BIDVService();
        }

        private async void FLogin_Load(object sender, EventArgs e)
        {
            string loginUrl = _bidvService.GetLoginUrl();
            _loginUrl = loginUrl;
            
            if (loginUrl != null) {
                try
                {
                    _webView = new WebView2();
                    _webView.Visible = false;
                    _webView.CreateControl();
                    this.Controls.Add(_webView);
                    await _webView.EnsureCoreWebView2Async();

                    // Điều hướng sang trang login (WebView2 sẽ chạy JS trên trang)
                    _webView.CoreWebView2.Navigate(loginUrl);

                    // đợi navigation hoàn tất
                    TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                    void handler(object s, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs a)
                    {
                        tcs.TrySetResult(true);
                    }

                    _webView.CoreWebView2.NavigationCompleted += handler;
                    await tcs.Task;
                    _webView.CoreWebView2.NavigationCompleted -= handler;

                   

                    // Import tất cả cookie từ WebView2 CookieManager -> CookieContainer
                    var cookieManager = _webView.CoreWebView2.CookieManager;
                    var cwCookies = await cookieManager.GetCookiesAsync(loginUrl);
                    foreach (var c in cwCookies)
                    {
                        // CoreWebView2Cookie properties: Name, Value, Domain, Path, IsHttpOnly, IsSecure, Expires (nullable DateTimeOffset)
                        //DateTime? expires = null;

                        _bidvService.AddCookie(
                            loginUrl,
                            c.Name,
                            c.Value,
                            c.Domain,
                            c.Path ?? "/",
                            c.IsSecure,
                            c.IsHttpOnly,
                            c.Expires
                            //expires
                        );
                    }


                    // After importing cookies and xNum we still also populate the parsed HTML in service for other scraping
                    var loginHtml = await _webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                    string loginHtmlStr = string.Empty;

                    if (!string.IsNullOrEmpty(loginHtml) && loginHtml != "null")
                    {
                        try { loginHtmlStr = System.Text.Json.JsonSerializer.Deserialize<string>(loginHtml); } catch { loginHtmlStr = loginHtml.Trim('"'); }
                    }
                    if (!string.IsNullOrEmpty(loginHtmlStr))
                    {
                        _bidvService.updateHtml(loginHtmlStr);
                         File.WriteAllText("debugLogin.html", loginHtmlStr);
                        _bidvService.InitHttpClient();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WebView2 init error: {ex.Message}");
                    // fallback to HttpClient fetch
                    await _bidvService.LoadPageAsync(loginUrl);
                }

                await LoadCaptcha();

                _actionUrl = _bidvService.GetActionUrl(loginUrl);           
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
                typeSubmit = ""
            };

            string jsonPayload = JsonConvert.SerializeObject(loginData);

            string publicKey = _bidvService.GetValueById("publicKeyJwe");
            string privateKey = _bidvService.GetValueById("privateKeyJws");


            string encryptedPayload = BidvEncryptor.CreateEncryptedPayload(jsonPayload, privateKey, publicKey);

    //        MessageBox.Show(encryptedPayload);
            LoginResult result = await _bidvService.LoginAsync(encryptedPayload, _actionUrl, _loginUrl);

            if (result.IsSuccess)
            {
                // THÀNH CÔNG! Lấy 'code' từ Location header
                string locationUrl = result.RedirectUrl;
                MessageBox.Show("Đăng nhập thành công! Đang phân tích URL:\n" + locationUrl);

                // Dùng Uri để phân tích fragment (phần sau dấu #)
                var uri = new Uri(locationUrl);
                string fragment = uri.Fragment; // Sẽ là: "#state=...&code=..."

                if (string.IsNullOrEmpty(fragment))
                {
                    MessageBox.Show("Lỗi: Không tìm thấy fragment (#) trong URL redirect.");
                    return;
                }

                // Dùng HttpUtility.ParseQueryString để bóc tách các tham số
                var queryParams = HttpUtility.ParseQueryString(fragment.Substring(1)); // Bỏ dấu #

                string code = queryParams["code"];
                string state = queryParams["state"];

                if (!string.IsNullOrEmpty(code))
                {
                    MessageBox.Show($"LẤY CODE THÀNH CÔNG!\n\nCode: {code}\n\nState: {state}");

                    // BƯỚC TIẾP THEO: Dùng code và code_verifier để lấy Token
                    // await CallTokenApiAsync(code, _currentCodeVerifier);
                }
                else
                {
                    MessageBox.Show("Lỗi: Đã đăng nhập nhưng không tìm thấy 'code' trong URL.");
                }
            }
            else
            {
                // THẤT BẠI
                MessageBox.Show($"Đăng nhập thất bại: {result.ErrorMessage}");
                // TODO: Tải lại captcha
            }

            await LoadCaptcha();
        }
    }
}
