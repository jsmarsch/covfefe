namespace Covfefe
{
    partial class TrayForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrayForm));
            this.CovfefeNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // CovfefeNotifyIcon
            // 
            this.CovfefeNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("CovfefeNotifyIcon.Icon")));
            this.CovfefeNotifyIcon.Text = "notifyIcon1";
            this.CovfefeNotifyIcon.Visible = true;
            // 
            // TrayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "TrayForm";
            this.Text = "TrayForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon CovfefeNotifyIcon;
    }
}