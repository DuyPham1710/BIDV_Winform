namespace BIDV_Winform
{
    partial class FOtp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtOtp = new MaskedTextBox();
            lblTitle = new Label();
            label1 = new Label();
            lblResend = new Label();
            btnClose = new Button();
            btnConfirm = new Button();
            lblExpired = new Label();
            SuspendLayout();
            // 
            // txtOtp
            // 
            txtOtp.Location = new Point(243, 105);
            txtOtp.Mask = "000000";
            txtOtp.Name = "txtOtp";
            txtOtp.Size = new Size(225, 27);
            txtOtp.TabIndex = 1;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(90, 51);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(571, 20);
            lblTitle.TabIndex = 3;
            lblTitle.Text = "Please enter the OTP code sent to your phone number *********278 for authentication.";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(225, 193);
            label1.Name = "label1";
            label1.Size = new Size(141, 20);
            label1.TabIndex = 4;
            label1.Text = "Didn’t Receive OTP?";
            // 
            // lblResend
            // 
            lblResend.AutoSize = true;
            lblResend.Location = new Point(384, 193);
            lblResend.Name = "lblResend";
            lblResend.Size = new Size(96, 20);
            lblResend.TabIndex = 5;
            lblResend.Text = "Resend after ";
            lblResend.Click += lblResend_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(178, 266);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(164, 50);
            btnClose.TabIndex = 6;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnConfirm
            // 
            btnConfirm.BackColor = Color.Teal;
            btnConfirm.ForeColor = Color.White;
            btnConfirm.Location = new Point(384, 266);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(164, 50);
            btnConfirm.TabIndex = 7;
            btnConfirm.Text = "Confirm";
            btnConfirm.UseVisualStyleBackColor = false;
            btnConfirm.Click += btnConfirm_Click;
            // 
            // lblExpired
            // 
            lblExpired.AutoSize = true;
            lblExpired.ForeColor = Color.Red;
            lblExpired.Location = new Point(308, 153);
            lblExpired.Name = "lblExpired";
            lblExpired.Size = new Size(115, 20);
            lblExpired.TabIndex = 8;
            lblExpired.Text = "OTP has expired";
            // 
            // FOtp
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(739, 373);
            Controls.Add(lblExpired);
            Controls.Add(btnConfirm);
            Controls.Add(btnClose);
            Controls.Add(lblResend);
            Controls.Add(label1);
            Controls.Add(lblTitle);
            Controls.Add(txtOtp);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FOtp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FOtp";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaskedTextBox txtOtp;
        private Label lblTitle;
        private Label label1;
        private Label lblResend;
        private Button btnClose;
        private Button btnConfirm;
        private Label lblExpired;
    }
}