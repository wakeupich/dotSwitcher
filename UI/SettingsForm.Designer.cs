namespace dotSwitcher.UI
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.buttonCancelSettings = new System.Windows.Forms.Button();
            this.buttonSaveSettings = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.checkBoxTrayIcon = new System.Windows.Forms.CheckBox();
            this.checkBoxAutorun = new System.Windows.Forms.CheckBox();
            this.checkBoxRunAsAdmin = new System.Windows.Forms.CheckBox();
            this.checkBoxSmartSelection = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDelay = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonGithub = new System.Windows.Forms.Button();
            this.textBoxSwitchLayoutHotkey = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.textBoxConvertHotkey = new System.Windows.Forms.TextBox();
            this.textBoxSwitchHotkey = new System.Windows.Forms.TextBox();
            this.groupBoxLayouts = new System.Windows.Forms.GroupBox();
            this.radioAllLayouts = new System.Windows.Forms.RadioButton();
            this.radioSelectedLayouts = new System.Windows.Forms.RadioButton();
            this.checkedListBoxLayouts = new System.Windows.Forms.CheckedListBox();
            this.labelSmartSelectionHelp = new System.Windows.Forms.Label();
            this.checkBoxAutoSwitch = new System.Windows.Forms.CheckBox();
            this.labelAutoSwitchHelp = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBoxLayouts.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancelSettings
            // 
            this.buttonCancelSettings.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancelSettings.Location = new System.Drawing.Point(323, 324);
            this.buttonCancelSettings.Name = "buttonCancelSettings";
            this.buttonCancelSettings.Size = new System.Drawing.Size(75, 25);
            this.buttonCancelSettings.TabIndex = 17;
            this.buttonCancelSettings.Text = "Cancel";
            this.buttonCancelSettings.UseVisualStyleBackColor = true;
            this.buttonCancelSettings.Click += new System.EventHandler(this.buttonCancelSettings_Click);
            // 
            // buttonSaveSettings
            // 
            this.buttonSaveSettings.Location = new System.Drawing.Point(242, 324);
            this.buttonSaveSettings.Name = "buttonSaveSettings";
            this.buttonSaveSettings.Size = new System.Drawing.Size(75, 25);
            this.buttonSaveSettings.TabIndex = 15;
            this.buttonSaveSettings.Text = "Apply";
            this.buttonSaveSettings.UseVisualStyleBackColor = true;
            this.buttonSaveSettings.Click += new System.EventHandler(this.buttonSaveSettings_Click);
            // 
            // buttonExit
            // 
            this.buttonExit.Location = new System.Drawing.Point(12, 324);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(85, 25);
            this.buttonExit.TabIndex = 18;
            this.buttonExit.Text = "Exit program";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // checkBoxAutorun
            // 
            this.checkBoxAutorun.AutoSize = true;
            this.checkBoxAutorun.Location = new System.Drawing.Point(12, 18);
            this.checkBoxAutorun.Name = "checkBoxAutorun";
            this.checkBoxAutorun.Size = new System.Drawing.Size(147, 17);
            this.checkBoxAutorun.TabIndex = 19;
            this.checkBoxAutorun.Text = "Start on Windows startup";
            this.checkBoxAutorun.UseVisualStyleBackColor = true;
            this.checkBoxAutorun.CheckedChanged += new System.EventHandler(this.checkBoxAutorun_CheckedChanged);
            // 
            // checkBoxRunAsAdmin
            // 
            this.checkBoxRunAsAdmin.AutoSize = true;
            this.checkBoxRunAsAdmin.Location = new System.Drawing.Point(12, 41);
            this.checkBoxRunAsAdmin.Name = "checkBoxRunAsAdmin";
            this.checkBoxRunAsAdmin.Size = new System.Drawing.Size(188, 17);
            this.checkBoxRunAsAdmin.TabIndex = 20;
            this.checkBoxRunAsAdmin.Text = "Run as admin (Task Scheduler)";
            this.checkBoxRunAsAdmin.UseVisualStyleBackColor = true;
            this.checkBoxRunAsAdmin.CheckedChanged += new System.EventHandler(this.checkBoxRunAsAdmin_CheckedChanged);
            // 
            // checkBoxTrayIcon
            // 
            this.checkBoxTrayIcon.AutoSize = true;
            this.checkBoxTrayIcon.Location = new System.Drawing.Point(12, 64);
            this.checkBoxTrayIcon.Name = "checkBoxTrayIcon";
            this.checkBoxTrayIcon.Size = new System.Drawing.Size(96, 17);
            this.checkBoxTrayIcon.TabIndex = 21;
            this.checkBoxTrayIcon.Text = "Show tray icon";
            this.checkBoxTrayIcon.UseVisualStyleBackColor = true;
            this.checkBoxTrayIcon.CheckedChanged += new System.EventHandler(this.checkBoxTrayIcon_CheckedChanged);
            // 
            // checkBoxSmartSelection
            // 
            this.checkBoxSmartSelection.AutoSize = true;
            this.checkBoxSmartSelection.Checked = true;
            this.checkBoxSmartSelection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSmartSelection.Location = new System.Drawing.Point(12, 87);
            this.checkBoxSmartSelection.Name = "checkBoxSmartSelection";
            this.checkBoxSmartSelection.Size = new System.Drawing.Size(159, 17);
            this.checkBoxSmartSelection.TabIndex = 22;
            this.checkBoxSmartSelection.Text = "Use smart convert selection";
            this.checkBoxSmartSelection.UseVisualStyleBackColor = true;
            this.checkBoxSmartSelection.CheckedChanged += new System.EventHandler(this.smartSelection_CheckedChanged);
            // 
            // labelSmartSelectionHelp
            // 
            this.labelSmartSelectionHelp.AutoSize = true;
            this.labelSmartSelectionHelp.Cursor = System.Windows.Forms.Cursors.Help;
            this.labelSmartSelectionHelp.Location = new System.Drawing.Point(175, 88);
            this.labelSmartSelectionHelp.Name = "labelSmartSelectionHelp";
            this.labelSmartSelectionHelp.Size = new System.Drawing.Size(13, 13);
            this.labelSmartSelectionHelp.TabIndex = 33;
            this.labelSmartSelectionHelp.Text = "?";
            this.labelSmartSelectionHelp.MouseLeave += new System.EventHandler(this.labelSmartSelectionHelp_MouseLeave);
            this.labelSmartSelectionHelp.MouseHover += new System.EventHandler(this.labelSmartSelectionHelp_MouseHover);
            // 
            // checkBoxAutoSwitch
            // 
            this.checkBoxAutoSwitch.AutoSize = true;
            this.checkBoxAutoSwitch.Checked = true;
            this.checkBoxAutoSwitch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoSwitch.Location = new System.Drawing.Point(12, 110);
            this.checkBoxAutoSwitch.Name = "checkBoxAutoSwitch";
            this.checkBoxAutoSwitch.Size = new System.Drawing.Size(176, 17);
            this.checkBoxAutoSwitch.TabIndex = 34;
            this.checkBoxAutoSwitch.Text = "Auto-switch layout while typing";
            this.checkBoxAutoSwitch.UseVisualStyleBackColor = true;
            this.checkBoxAutoSwitch.CheckedChanged += new System.EventHandler(this.checkBoxAutoSwitch_CheckedChanged);
            // 
            // labelAutoSwitchHelp
            // 
            this.labelAutoSwitchHelp.AutoSize = true;
            this.labelAutoSwitchHelp.Cursor = System.Windows.Forms.Cursors.Help;
            this.labelAutoSwitchHelp.Location = new System.Drawing.Point(191, 111);
            this.labelAutoSwitchHelp.Name = "labelAutoSwitchHelp";
            this.labelAutoSwitchHelp.Size = new System.Drawing.Size(13, 13);
            this.labelAutoSwitchHelp.TabIndex = 35;
            this.labelAutoSwitchHelp.Text = "?";
            this.labelAutoSwitchHelp.MouseLeave += new System.EventHandler(this.labelAutoSwitchHelp_MouseLeave);
            this.labelAutoSwitchHelp.MouseHover += new System.EventHandler(this.labelAutoSwitchHelp_MouseHover);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "Delay before switching:";
            // 
            // textBoxDelay
            // 
            this.textBoxDelay.Location = new System.Drawing.Point(12, 148);
            this.textBoxDelay.Name = "textBoxDelay";
            this.textBoxDelay.Size = new System.Drawing.Size(60, 20);
            this.textBoxDelay.TabIndex = 25;
            this.textBoxDelay.TextChanged += new System.EventHandler(this.textBoxDelay_TextChanged);
            // 
            // buttonGithub
            // 
            this.buttonGithub.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonGithub.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGithub.Image = global::dotSwitcher.Properties.Resources.github;
            this.buttonGithub.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonGithub.Location = new System.Drawing.Point(105, 324);
            this.buttonGithub.Name = "buttonGithub";
            this.buttonGithub.Size = new System.Drawing.Size(114, 25);
            this.buttonGithub.TabIndex = 28;
            this.buttonGithub.Text = "Report an issue";
            this.buttonGithub.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonGithub.UseVisualStyleBackColor = true;
            this.buttonGithub.Click += new System.EventHandler(this.buttonGithub_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Convert last word:";
            // 
            // textBoxSwitchHotkey
            // 
            this.textBoxSwitchHotkey.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxSwitchHotkey.Location = new System.Drawing.Point(6, 36);
            this.textBoxSwitchHotkey.Name = "textBoxSwitchHotkey";
            this.textBoxSwitchHotkey.ReadOnly = true;
            this.textBoxSwitchHotkey.Size = new System.Drawing.Size(174, 20);
            this.textBoxSwitchHotkey.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Convert selection:";
            // 
            // textBoxConvertHotkey
            // 
            this.textBoxConvertHotkey.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxConvertHotkey.Location = new System.Drawing.Point(6, 78);
            this.textBoxConvertHotkey.Name = "textBoxConvertHotkey";
            this.textBoxConvertHotkey.ReadOnly = true;
            this.textBoxConvertHotkey.Size = new System.Drawing.Size(174, 20);
            this.textBoxConvertHotkey.TabIndex = 25;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 13);
            this.label4.TabIndex = 29;
            this.label4.Text = "Switch keyboard layout:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(167, 104);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(13, 13);
            this.label5.TabIndex = 31;
            this.label5.Text = "?";
            this.label5.MouseLeave += new System.EventHandler(this.label5_MouseLeave);
            this.label5.MouseHover += new System.EventHandler(this.label5_MouseHover);
            // 
            // textBoxSwitchLayoutHotkey
            // 
            this.textBoxSwitchLayoutHotkey.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxSwitchLayoutHotkey.Location = new System.Drawing.Point(6, 120);
            this.textBoxSwitchLayoutHotkey.Name = "textBoxSwitchLayoutHotkey";
            this.textBoxSwitchLayoutHotkey.ReadOnly = true;
            this.textBoxSwitchLayoutHotkey.Size = new System.Drawing.Size(174, 20);
            this.textBoxSwitchLayoutHotkey.TabIndex = 30;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxSwitchHotkey);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxConvertHotkey);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBoxSwitchLayoutHotkey);
            this.groupBox1.Location = new System.Drawing.Point(210, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(188, 154);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Hotkeys";
            // 
            // groupBoxLayouts
            // 
            this.groupBoxLayouts.Controls.Add(this.radioAllLayouts);
            this.groupBoxLayouts.Controls.Add(this.radioSelectedLayouts);
            this.groupBoxLayouts.Controls.Add(this.checkedListBoxLayouts);
            this.groupBoxLayouts.Location = new System.Drawing.Point(12, 172);
            this.groupBoxLayouts.Name = "groupBoxLayouts";
            this.groupBoxLayouts.Size = new System.Drawing.Size(386, 140);
            this.groupBoxLayouts.TabIndex = 32;
            this.groupBoxLayouts.TabStop = false;
            this.groupBoxLayouts.Text = "Keyboard layouts in cycle";
            // 
            // radioAllLayouts
            // 
            this.radioAllLayouts.AutoSize = true;
            this.radioAllLayouts.Checked = true;
            this.radioAllLayouts.Location = new System.Drawing.Point(10, 19);
            this.radioAllLayouts.Name = "radioAllLayouts";
            this.radioAllLayouts.Size = new System.Drawing.Size(124, 17);
            this.radioAllLayouts.TabIndex = 0;
            this.radioAllLayouts.TabStop = true;
            this.radioAllLayouts.Text = "All installed layouts";
            this.radioAllLayouts.UseVisualStyleBackColor = true;
            this.radioAllLayouts.CheckedChanged += new System.EventHandler(this.radioLayouts_CheckedChanged);
            // 
            // radioSelectedLayouts
            // 
            this.radioSelectedLayouts.AutoSize = true;
            this.radioSelectedLayouts.Location = new System.Drawing.Point(160, 19);
            this.radioSelectedLayouts.Name = "radioSelectedLayouts";
            this.radioSelectedLayouts.Size = new System.Drawing.Size(130, 17);
            this.radioSelectedLayouts.TabIndex = 1;
            this.radioSelectedLayouts.Text = "Only selected layouts:";
            this.radioSelectedLayouts.UseVisualStyleBackColor = true;
            this.radioSelectedLayouts.CheckedChanged += new System.EventHandler(this.radioLayouts_CheckedChanged);
            // 
            // checkedListBoxLayouts
            // 
            this.checkedListBoxLayouts.CheckOnClick = true;
            this.checkedListBoxLayouts.FormattingEnabled = true;
            this.checkedListBoxLayouts.IntegralHeight = false;
            this.checkedListBoxLayouts.Location = new System.Drawing.Point(10, 42);
            this.checkedListBoxLayouts.Name = "checkedListBoxLayouts";
            this.checkedListBoxLayouts.Size = new System.Drawing.Size(366, 88);
            this.checkedListBoxLayouts.TabIndex = 2;
            this.checkedListBoxLayouts.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxLayouts_ItemCheck);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.ReshowDelay = 100;
            this.toolTip1.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip1_Popup);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.buttonSaveSettings;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancelSettings;
            this.ClientSize = new System.Drawing.Size(410, 362);
            this.Controls.Add(this.labelAutoSwitchHelp);
            this.Controls.Add(this.checkBoxAutoSwitch);
            this.Controls.Add(this.labelSmartSelectionHelp);
            this.Controls.Add(this.groupBoxLayouts);
            this.Controls.Add(this.checkBoxSmartSelection);
            this.Controls.Add(this.checkBoxRunAsAdmin);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonGithub);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxDelay);
            this.Controls.Add(this.checkBoxTrayIcon);
            this.Controls.Add(this.checkBoxAutorun);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonSaveSettings);
            this.Controls.Add(this.buttonCancelSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "dotSwitcher Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxLayouts.ResumeLayout(false);
            this.groupBoxLayouts.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancelSettings;
        private System.Windows.Forms.Button buttonSaveSettings;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.CheckBox checkBoxTrayIcon;
        private System.Windows.Forms.CheckBox checkBoxAutorun;
        private System.Windows.Forms.CheckBox checkBoxRunAsAdmin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDelay;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonGithub;
        private System.Windows.Forms.TextBox textBoxSwitchLayoutHotkey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox textBoxSwitchHotkey;
        private System.Windows.Forms.TextBox textBoxConvertHotkey;
        private System.Windows.Forms.CheckBox checkBoxSmartSelection;
        private System.Windows.Forms.GroupBox groupBoxLayouts;
        private System.Windows.Forms.RadioButton radioAllLayouts;
        private System.Windows.Forms.RadioButton radioSelectedLayouts;
        private System.Windows.Forms.CheckedListBox checkedListBoxLayouts;
        private System.Windows.Forms.Label labelSmartSelectionHelp;
        private System.Windows.Forms.CheckBox checkBoxAutoSwitch;
        private System.Windows.Forms.Label labelAutoSwitchHelp;
    }
}
