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
            this.covfefeNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.stayAwakeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keepMonitorsOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normalOperationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.closeButton = new System.Windows.Forms.Button();
            this.settingsLabel = new System.Windows.Forms.Label();
            this.startOnLoginCheckbox = new System.Windows.Forms.CheckBox();
            this.notifyContextMenu.SuspendLayout();
            this.titlePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // covfefeNotifyIcon
            // 
            this.covfefeNotifyIcon.ContextMenuStrip = this.notifyContextMenu;
            this.covfefeNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("covfefeNotifyIcon.Icon")));
            this.covfefeNotifyIcon.Text = "Covfefe!";
            this.covfefeNotifyIcon.Visible = true;
            // 
            // notifyContextMenu
            // 
            this.notifyContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stayAwakeToolStripMenuItem,
            this.keepMonitorsOnToolStripMenuItem,
            this.normalOperationToolStripMenuItem,
            this.toolStripSeparator1,
            this.settingsToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitMenuItem});
            this.notifyContextMenu.Name = "notifyContextMenu";
            this.notifyContextMenu.ShowCheckMargin = true;
            this.notifyContextMenu.ShowImageMargin = false;
            this.notifyContextMenu.Size = new System.Drawing.Size(171, 126);
            // 
            // stayAwakeToolStripMenuItem
            // 
            this.stayAwakeToolStripMenuItem.Name = "stayAwakeToolStripMenuItem";
            this.stayAwakeToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.stayAwakeToolStripMenuItem.Text = "Stay Awake!";
            this.stayAwakeToolStripMenuItem.Click += new System.EventHandler(this.stayAwakeToolStripMenuItem_Click);
            // 
            // keepMonitorsOnToolStripMenuItem
            // 
            this.keepMonitorsOnToolStripMenuItem.Name = "keepMonitorsOnToolStripMenuItem";
            this.keepMonitorsOnToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.keepMonitorsOnToolStripMenuItem.Text = "Keep Monitors On";
            this.keepMonitorsOnToolStripMenuItem.Click += new System.EventHandler(this.keepMonitorsToolStripMenuItem_Click);
            // 
            // normalOperationToolStripMenuItem
            // 
            this.normalOperationToolStripMenuItem.Name = "normalOperationToolStripMenuItem";
            this.normalOperationToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.normalOperationToolStripMenuItem.Text = "Normal Operation";
            this.normalOperationToolStripMenuItem.Click += new System.EventHandler(this.normalOperationToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(167, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(167, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(170, 22);
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // titlePanel
            // 
            this.titlePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.titlePanel.Controls.Add(this.closeButton);
            this.titlePanel.Controls.Add(this.settingsLabel);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.titlePanel.Location = new System.Drawing.Point(0, 0);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(357, 36);
            this.titlePanel.TabIndex = 1;
            // 
            // closeButton
            // 
            this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.closeButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.closeButton.Location = new System.Drawing.Point(330, 0);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(27, 36);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "X";
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // settingsLabel
            // 
            this.settingsLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.settingsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.settingsLabel.Location = new System.Drawing.Point(0, 0);
            this.settingsLabel.Name = "settingsLabel";
            this.settingsLabel.Size = new System.Drawing.Size(324, 36);
            this.settingsLabel.TabIndex = 0;
            this.settingsLabel.Text = "Settings";
            this.settingsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.settingsLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.settingsLabel_MouseDown);
            // 
            // startOnLoginCheckbox
            // 
            this.startOnLoginCheckbox.AutoSize = true;
            this.startOnLoginCheckbox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.startOnLoginCheckbox.Location = new System.Drawing.Point(25, 54);
            this.startOnLoginCheckbox.Name = "startOnLoginCheckbox";
            this.startOnLoginCheckbox.Size = new System.Drawing.Size(156, 17);
            this.startOnLoginCheckbox.TabIndex = 2;
            this.startOnLoginCheckbox.Text = "Start Covfefe when I sign in";
            this.startOnLoginCheckbox.UseVisualStyleBackColor = true;
            // 
            // TrayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.ClientSize = new System.Drawing.Size(357, 182);
            this.Controls.Add(this.startOnLoginCheckbox);
            this.Controls.Add(this.titlePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TrayForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TrayForm";
            this.Load += new System.EventHandler(this.TrayForm_Load);
            this.notifyContextMenu.ResumeLayout(false);
            this.titlePanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon covfefeNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip notifyContextMenu;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stayAwakeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keepMonitorsOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem normalOperationToolStripMenuItem;
        private System.Windows.Forms.Panel titlePanel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label settingsLabel;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.CheckBox startOnLoginCheckbox;
    }
}