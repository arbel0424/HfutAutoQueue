namespace AutoQueue
{
    partial class QueueForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.Title = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.UserNameText = new System.Windows.Forms.TextBox();
            this.PasswordText = new System.Windows.Forms.TextBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.InfoText = new System.Windows.Forms.Label();
            this.ResultText = new System.Windows.Forms.Label();
            this.AboutButton = new System.Windows.Forms.Button();
            this.NumLimit = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.LimitInfo = new System.Windows.Forms.Label();
            this.picture = new System.Windows.Forms.PictureBox();
            this.CodeResult = new System.Windows.Forms.Label();
            this.CodeLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.NumLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture)).BeginInit();
            this.SuspendLayout();
            // 
            // Title
            // 
            this.Title.AutoSize = true;
            this.Title.Font = new System.Drawing.Font("黑体", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Title.Location = new System.Drawing.Point(17, 19);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(253, 29);
            this.Title.TabIndex = 0;
            this.Title.Text = "财务报账排队助手";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 101);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "用户名:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "密码:";
            // 
            // UserNameText
            // 
            this.UserNameText.Location = new System.Drawing.Point(104, 98);
            this.UserNameText.Name = "UserNameText";
            this.UserNameText.Size = new System.Drawing.Size(130, 21);
            this.UserNameText.TabIndex = 1;
            // 
            // PasswordText
            // 
            this.PasswordText.Location = new System.Drawing.Point(104, 131);
            this.PasswordText.Name = "PasswordText";
            this.PasswordText.PasswordChar = '*';
            this.PasswordText.Size = new System.Drawing.Size(130, 21);
            this.PasswordText.TabIndex = 2;
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(55, 163);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(75, 23);
            this.StartButton.TabIndex = 3;
            this.StartButton.Text = "开始取号";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Location = new System.Drawing.Point(157, 163);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 23);
            this.StopButton.TabIndex = 4;
            this.StopButton.Text = "停止";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.Exit_Click);
            // 
            // InfoText
            // 
            this.InfoText.AutoSize = true;
            this.InfoText.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.InfoText.Location = new System.Drawing.Point(3, 280);
            this.InfoText.Name = "InfoText";
            this.InfoText.Size = new System.Drawing.Size(63, 14);
            this.InfoText.TabIndex = 4;
            this.InfoText.Text = "运行状态";
            // 
            // ResultText
            // 
            this.ResultText.AutoSize = true;
            this.ResultText.Font = new System.Drawing.Font("黑体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ResultText.ForeColor = System.Drawing.Color.Red;
            this.ResultText.Location = new System.Drawing.Point(65, 61);
            this.ResultText.Name = "ResultText";
            this.ResultText.Size = new System.Drawing.Size(156, 19);
            this.ResultText.TabIndex = 1;
            this.ResultText.Text = "输入用户名密码";
            this.ResultText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AboutButton
            // 
            this.AboutButton.Location = new System.Drawing.Point(221, 277);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(53, 23);
            this.AboutButton.TabIndex = 5;
            this.AboutButton.Text = "关于";
            this.AboutButton.UseVisualStyleBackColor = true;
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // NumLimit
            // 
            this.NumLimit.Enabled = false;
            this.NumLimit.LargeChange = 1;
            this.NumLimit.Location = new System.Drawing.Point(70, 192);
            this.NumLimit.Name = "NumLimit";
            this.NumLimit.Size = new System.Drawing.Size(164, 45);
            this.NumLimit.TabIndex = 6;
            this.NumLimit.Scroll += new System.EventHandler(this.NumLimit_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 197);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "取指定号";
            // 
            // LimitInfo
            // 
            this.LimitInfo.AutoSize = true;
            this.LimitInfo.Location = new System.Drawing.Point(102, 225);
            this.LimitInfo.Name = "LimitInfo";
            this.LimitInfo.Size = new System.Drawing.Size(41, 12);
            this.LimitInfo.TabIndex = 8;
            this.LimitInfo.Text = "最前号";
            // 
            // picture
            // 
            this.picture.Location = new System.Drawing.Point(54, 243);
            this.picture.Name = "picture";
            this.picture.Size = new System.Drawing.Size(58, 21);
            this.picture.TabIndex = 9;
            this.picture.TabStop = false;
            // 
            // CodeResult
            // 
            this.CodeResult.AutoSize = true;
            this.CodeResult.Location = new System.Drawing.Point(155, 264);
            this.CodeResult.Name = "CodeResult";
            this.CodeResult.Size = new System.Drawing.Size(0, 12);
            this.CodeResult.TabIndex = 8;
            // 
            // CodeLabel
            // 
            this.CodeLabel.AutoSize = true;
            this.CodeLabel.Location = new System.Drawing.Point(135, 252);
            this.CodeLabel.Name = "CodeLabel";
            this.CodeLabel.Size = new System.Drawing.Size(29, 12);
            this.CodeLabel.TabIndex = 10;
            this.CodeLabel.Text = "YYYY";
            // 
            // QueueForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 300);
            this.Controls.Add(this.CodeLabel);
            this.Controls.Add(this.picture);
            this.Controls.Add(this.CodeResult);
            this.Controls.Add(this.LimitInfo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.NumLimit);
            this.Controls.Add(this.AboutButton);
            this.Controls.Add(this.InfoText);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.PasswordText);
            this.Controls.Add(this.UserNameText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ResultText);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Title);
            this.Name = "QueueForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "财务报账排队助手-0.32应急版";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.QueueForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.NumLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Title;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox UserNameText;
        private System.Windows.Forms.TextBox PasswordText;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Label InfoText;
        private System.Windows.Forms.Label ResultText;
        private System.Windows.Forms.Button AboutButton;
        private System.Windows.Forms.TrackBar NumLimit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label LimitInfo;
        private System.Windows.Forms.PictureBox picture;
        private System.Windows.Forms.Label CodeResult;
        private System.Windows.Forms.Label CodeLabel;
    }
}

