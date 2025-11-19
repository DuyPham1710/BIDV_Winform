using BIDV_Winform.helper;
using BIDV_Winform.models;
using BIDV_Winform.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BIDV_Winform
{
    public partial class FOtp : Form
    {
        private BIDVService bidvService;
        private string actionUrl;
        private int _countdown = 60;
        private System.Windows.Forms.Timer _timer;
        public FOtp(BIDVService bidvService, string actionUrl)
        {
            InitializeComponent();

            this.bidvService = bidvService;
            this.actionUrl = actionUrl;

            StartOtpCountdown();
        }

        private void StartOtpCountdown()
        {
            _countdown = 60;

            lblExpired.Visible = false;
            lblResend.Visible = true;
            lblResend.Text = $"Resend after {_countdown}s";

            if (_timer == null)
            {
                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = 1000;   // 1 giây
                _timer.Tick += Timer_Tick;
            }

            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _countdown--;

            if (_countdown > 0)
            {
                lblResend.Text = $"Resend after {_countdown}s";
            }
            else
            {
                _timer.Stop();

                lblResend.Text = "Resend code";
                lblExpired.Visible = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void btnConfirm_Click(object sender, EventArgs e)
        {
            var data = new OtpPayloadRequest
            {
               OtpValue = txtOtp.Text,
            };

            string publicKey = bidvService.GetValueById("publicKeyJwe");
            string privateKey = bidvService.GetValueById("privateKeyJws");


            string encryptedPayload = BidvEncryptor.GenerateEncryptedPayload(data, privateKey, publicKey);

            bool statusCode200= await bidvService.FormSubmitAsync(encryptedPayload, actionUrl);

            if (statusCode200)
            {
                MessageBox.Show("status code 200");
            }
            else
            {
                this.Close();
            }
            //if (result.statusCode.Equals("200"))
            //{
            //    MessageBox.Show("Lỗi xác thực OTP");
            //}
            //else
            //{
            //    MessageBox.Show(result.RedirectUrl, "Xác thực thành công");
            //}
        }

        private void lblResend_Click(object sender, EventArgs e)
        {
            if (_countdown == 0)
            {
                // TODO: gửi lại OTP
                StartOtpCountdown();
            }
        }
    }
}
