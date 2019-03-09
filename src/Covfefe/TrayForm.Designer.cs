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
            this.delaySleepOnDockUndockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normalOperationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyContextMenu.SuspendLayout();
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
            this.delaySleepOnDockUndockToolStripMenuItem,
            this.normalOperationToolStripMenuItem,
            this.exitMenuItem});
            this.notifyContextMenu.Name = "notifyContextMenu";
            this.notifyContextMenu.ShowCheckMargin = true;
            this.notifyContextMenu.ShowImageMargin = false;
            this.notifyContextMenu.Size = new System.Drawing.Size(228, 114);
            // 
            // stayAwakeToolStripMenuItem
            // 
            this.stayAwakeToolStripMenuItem.Name = "stayAwakeToolStripMenuItem";
            this.stayAwakeToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.stayAwakeToolStripMenuItem.Text = "Stay Awake!";
            this.stayAwakeToolStripMenuItem.Click += new System.EventHandler(this.stayAwakeToolStripMenuItem_Click);
            // 
            // keepMonitorsOnToolStripMenuItem
            // 
            this.keepMonitorsOnToolStripMenuItem.Name = "keepMonitorsOnToolStripMenuItem";
            this.keepMonitorsOnToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.keepMonitorsOnToolStripMenuItem.Text = "Keep Monitors On";
            this.keepMonitorsOnToolStripMenuItem.Click += new System.EventHandler(this.keepMonitorsToolStripMenuItem_Click);
            // 
            // delaySleepOnDockUndockToolStripMenuItem
            // 
            this.delaySleepOnDockUndockToolStripMenuItem.Name = "delaySleepOnDockUndockToolStripMenuItem";
            this.delaySleepOnDockUndockToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.delaySleepOnDockUndockToolStripMenuItem.Text = "Delay Sleep on Dock/Undock";
            this.delaySleepOnDockUndockToolStripMenuItem.Click += new System.EventHandler(this.delaySleepToolStripMenuItem_Click);
            // 
            // normalOperationToolStripMenuItem
            // 
            this.normalOperationToolStripMenuItem.Name = "normalOperationToolStripMenuItem";
            this.normalOperationToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.normalOperationToolStripMenuItem.Text = "Normal Operation";
            this.normalOperationToolStripMenuItem.Click += new System.EventHandler(this.normalOperationToolStripMenuItem_Click);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(227, 22);
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // TrayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 96);
            this.Name = "TrayForm";
            this.Text = "TrayForm";
            this.notifyContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon covfefeNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip notifyContextMenu;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stayAwakeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keepMonitorsOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem delaySleepOnDockUndockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem normalOperationToolStripMenuItem;
    }
}