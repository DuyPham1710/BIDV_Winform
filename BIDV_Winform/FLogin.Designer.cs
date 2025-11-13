namespace BIDV_Winform
{
    partial class FLogin
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FLogin));
            pictureBox1 = new PictureBox();
            panel1 = new Panel();
            txtPassword = new TextBox();
            btnLogin = new Button();
            iconRefresh = new FontAwesome.Sharp.IconButton();
            pictureBoxCaptcha = new PictureBox();
            txtCaptcha = new TextBox();
            txtUsername = new TextBox();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCaptcha).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.DarkGray;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1212, 736);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(txtPassword);
            panel1.Controls.Add(btnLogin);
            panel1.Controls.Add(iconRefresh);
            panel1.Controls.Add(pictureBoxCaptcha);
            panel1.Controls.Add(txtCaptcha);
            panel1.Controls.Add(txtUsername);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(309, 92);
            panel1.Name = "panel1";
            panel1.Size = new Size(629, 523);
            panel1.TabIndex = 1;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(62, 162);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.PlaceholderText = "MẬT KHẨU";
            txtPassword.Size = new Size(520, 27);
            txtPassword.TabIndex = 13;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.Teal;
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Font = new Font("Times New Roman", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(62, 382);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(521, 61);
            btnLogin.TabIndex = 10;
            btnLogin.Text = "ĐĂNG NHẬP";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // iconRefresh
            // 
            iconRefresh.IconChar = FontAwesome.Sharp.IconChar.Refresh;
            iconRefresh.IconColor = Color.DodgerBlue;
            iconRefresh.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconRefresh.Location = new Point(540, 260);
            iconRefresh.Name = "iconRefresh";
            iconRefresh.Size = new Size(43, 44);
            iconRefresh.TabIndex = 9;
            iconRefresh.UseVisualStyleBackColor = true;
            iconRefresh.Click += iconRefresh_Click;
            // 
            // pictureBoxCaptcha
            // 
            pictureBoxCaptcha.Location = new Point(329, 260);
            pictureBoxCaptcha.Name = "pictureBoxCaptcha";
            pictureBoxCaptcha.Size = new Size(195, 75);
            pictureBoxCaptcha.TabIndex = 8;
            pictureBoxCaptcha.TabStop = false;
            // 
            // txtCaptcha
            // 
            txtCaptcha.Location = new Point(62, 214);
            txtCaptcha.Name = "txtCaptcha";
            txtCaptcha.PlaceholderText = "MÃ XÁC NHẬN";
            txtCaptcha.Size = new Size(516, 27);
            txtCaptcha.TabIndex = 7;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(62, 113);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "TÊN ĐĂNG NHẬP";
            txtUsername.Size = new Size(520, 27);
            txtUsername.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Times New Roman", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(165, 46);
            label1.Name = "label1";
            label1.Size = new Size(294, 25);
            label1.TabIndex = 0;
            label1.Text = "Chào mừng tới BIDV Direct";
            // 
            // FLogin
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1212, 736);
            Controls.Add(panel1);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "FLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FLogin";
            Load += FLogin_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCaptcha).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private Panel panel1;
        private Label label1;
        private PictureBox pictureBoxCaptcha;
        private TextBox txtCaptcha;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private FontAwesome.Sharp.IconButton iconRefresh;
    }
}
