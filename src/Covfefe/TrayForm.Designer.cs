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
            this.label1 = new System.Windows.Forms.Label();
            this.defaultSleepModeComboBox = new System.Windows.Forms.ComboBox();
            this.showBalloonTipsCheckbox = new System.Windows.Forms.CheckBox();
            this.reminderCheckbox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.reminderTimeoutTextBox = new System.Windows.Forms.NumericUpDown();
            this.reminderTimer = new System.Windows.Forms.Timer(this.components);
            this.toggleModeComboBox = new System.Windows.Forms.ComboBox();
            this.modeToggleCheckBox = new System.Windows.Forms.CheckBox();
            this.notifyContextMenu.SuspendLayout();
            this.titlePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.reminderTimeoutTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // covfefeNotifyIcon
            // 
            this.covfefeNotifyIcon.ContextMenuStrip = this.notifyContextMenu;
            this.covfefeNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("covfefeNotifyIcon.Icon")));
            this.covfefeNotifyIcon.Text = "Covfefe!";
            this.covfefeNotifyIcon.Visible = true;
            this.covfefeNotifyIcon.DoubleClick += new System.EventHandler(this.covfefeNotifyIcon_DoubleClick);
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
            this.titlePanel.Size = new System.Drawing.Size(420, 36);
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
            this.closeButton.Location = new System.Drawing.Point(393, 0);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(27, 36);
            this.closeButton.TabIndex = 1;
            this.closeButton.TabStop = false;
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
            this.settingsLabel.TabIndex = 1;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.label1.Location = new System.Drawing.Point(22, 160);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "When Covfefe starts, use this mode:";
            // 
            // defaultSleepModeComboBox
            // 
            this.defaultSleepModeComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.defaultSleepModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.defaultSleepModeComboBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.defaultSleepModeComboBox.FormattingEnabled = true;
            this.defaultSleepModeComboBox.Location = new System.Drawing.Point(254, 157);
            this.defaultSleepModeComboBox.Name = "defaultSleepModeComboBox";
            this.defaultSleepModeComboBox.Size = new System.Drawing.Size(121, 21);
            this.defaultSleepModeComboBox.TabIndex = 6;
            // 
            // showBalloonTipsCheckbox
            // 
            this.showBalloonTipsCheckbox.AutoSize = true;
            this.showBalloonTipsCheckbox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.showBalloonTipsCheckbox.Location = new System.Drawing.Point(25, 78);
            this.showBalloonTipsCheckbox.Name = "showBalloonTipsCheckbox";
            this.showBalloonTipsCheckbox.Size = new System.Drawing.Size(221, 17);
            this.showBalloonTipsCheckbox.TabIndex = 3;
            this.showBalloonTipsCheckbox.Text = "Show a notification when I switch modes.";
            this.showBalloonTipsCheckbox.UseVisualStyleBackColor = true;
            // 
            // reminderCheckbox
            // 
            this.reminderCheckbox.AutoSize = true;
            this.reminderCheckbox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.reminderCheckbox.Location = new System.Drawing.Point(25, 101);
            this.reminderCheckbox.Name = "reminderCheckbox";
            this.reminderCheckbox.Size = new System.Drawing.Size(231, 17);
            this.reminderCheckbox.TabIndex = 4;
            this.reminderCheckbox.Text = "Remind me if I prevent sleep for longer than";
            this.reminderCheckbox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.label2.Location = new System.Drawing.Point(330, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "minutes.";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // reminderTimeoutTextBox
            // 
            this.reminderTimeoutTextBox.Location = new System.Drawing.Point(254, 100);
            this.reminderTimeoutTextBox.Maximum = new decimal(new int[] {
            35790,
            0,
            0,
            0});
            this.reminderTimeoutTextBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.reminderTimeoutTextBox.Name = "reminderTimeoutTextBox";
            this.reminderTimeoutTextBox.Size = new System.Drawing.Size(70, 20);
            this.reminderTimeoutTextBox.TabIndex = 8;
            this.reminderTimeoutTextBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // reminderTimer
            // 
            this.reminderTimer.Tick += new System.EventHandler(this.reminderTimer_Tick);
            // 
            // toggleModeComboBox
            // 
            this.toggleModeComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.toggleModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toggleModeComboBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.toggleModeComboBox.FormattingEnabled = true;
            this.toggleModeComboBox.Location = new System.Drawing.Point(254, 126);
            this.toggleModeComboBox.Name = "toggleModeComboBox";
            this.toggleModeComboBox.Size = new System.Drawing.Size(122, 21);
            this.toggleModeComboBox.TabIndex = 6;
            // 
            // modeToggleCheckBox
            // 
            this.modeToggleCheckBox.AutoSize = true;
            this.modeToggleCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(227)))), ((int)(((byte)(227)))));
            this.modeToggleCheckBox.Location = new System.Drawing.Point(25, 128);
            this.modeToggleCheckBox.Name = "modeToggleCheckBox";
            this.modeToggleCheckBox.Size = new System.Drawing.Size(225, 17);
            this.modeToggleCheckBox.TabIndex = 4;
            this.modeToggleCheckBox.Text = "On double-click, toggle Normal Mode and:";
            this.modeToggleCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.ClientSize = new System.Drawing.Size(420, 190);
            this.Controls.Add(this.reminderTimeoutTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.modeToggleCheckBox);
            this.Controls.Add(this.reminderCheckbox);
            this.Controls.Add(this.showBalloonTipsCheckbox);
            this.Controls.Add(this.toggleModeComboBox);
            this.Controls.Add(this.defaultSleepModeComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.startOnLoginCheckbox);
            this.Controls.Add(this.titlePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TrayForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TrayForm";
            this.notifyContextMenu.ResumeLayout(false);
            this.titlePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.reminderTimeoutTextBox)).EndInit();
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox defaultSleepModeComboBox;
        private System.Windows.Forms.CheckBox showBalloonTipsCheckbox;
        private System.Windows.Forms.CheckBox reminderCheckbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown reminderTimeoutTextBox;
        private System.Windows.Forms.Timer reminderTimer;
        private System.Windows.Forms.ComboBox toggleModeComboBox;
        private System.Windows.Forms.CheckBox modeToggleCheckBox;
    }
}