namespace IncominHttpServer
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.startstop = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.prcLog = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.qryLog = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.lqrLog = new System.Windows.Forms.TextBox();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.lrsLog = new System.Windows.Forms.TextBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.invLog = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.defInvoke = new System.Windows.Forms.NumericUpDown();
            this.procInvCB = new System.Windows.Forms.CheckBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.oosCB = new System.Windows.Forms.CheckBox();
            this.onTopCB = new System.Windows.Forms.CheckBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.customHeaders = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.responseText = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.onlyLocalCB = new System.Windows.Forms.CheckBox();
            this.defPort = new System.Windows.Forms.NumericUpDown();
            this.defPortLabel = new System.Windows.Forms.Label();
            this.defCodeLabel = new System.Windows.Forms.Label();
            this.defCode = new System.Windows.Forms.NumericUpDown();
            this.allowEmptyCB = new System.Windows.Forms.CheckBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.defInvoke)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.defPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.defCode)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // startstop
            // 
            this.startstop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.startstop.ContextMenuStrip = this.contextMenuStrip1;
            this.startstop.Dock = System.Windows.Forms.DockStyle.Top;
            this.startstop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.startstop.ForeColor = System.Drawing.Color.Black;
            this.startstop.Location = new System.Drawing.Point(0, 0);
            this.startstop.Name = "startstop";
            this.startstop.Size = new System.Drawing.Size(801, 28);
            this.startstop.TabIndex = 0;
            this.startstop.Text = "START";
            this.startstop.UseVisualStyleBackColor = false;
            this.startstop.Click += new System.EventHandler(this.start_stop_click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(130, 26);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.clearToolStripMenuItem.Text = "Clear Logs";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(801, 400);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.prcLog);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(793, 374);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Processing Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // prcLog
            // 
            this.prcLog.BackColor = System.Drawing.Color.Black;
            this.prcLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.prcLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.prcLog.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.prcLog.ForeColor = System.Drawing.Color.White;
            this.prcLog.Location = new System.Drawing.Point(3, 3);
            this.prcLog.Multiline = true;
            this.prcLog.Name = "prcLog";
            this.prcLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.prcLog.Size = new System.Drawing.Size(787, 368);
            this.prcLog.TabIndex = 2;
            this.prcLog.WordWrap = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.qryLog);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(793, 374);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Query Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // qryLog
            // 
            this.qryLog.BackColor = System.Drawing.Color.Navy;
            this.qryLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.qryLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.qryLog.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.qryLog.ForeColor = System.Drawing.Color.White;
            this.qryLog.Location = new System.Drawing.Point(3, 3);
            this.qryLog.Multiline = true;
            this.qryLog.Name = "qryLog";
            this.qryLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.qryLog.Size = new System.Drawing.Size(787, 368);
            this.qryLog.TabIndex = 3;
            this.qryLog.WordWrap = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.lqrLog);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(793, 374);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Last Query";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // lqrLog
            // 
            this.lqrLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lqrLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lqrLog.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lqrLog.Location = new System.Drawing.Point(0, 0);
            this.lqrLog.Multiline = true;
            this.lqrLog.Name = "lqrLog";
            this.lqrLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.lqrLog.Size = new System.Drawing.Size(793, 374);
            this.lqrLog.TabIndex = 3;
            this.lqrLog.WordWrap = false;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.lrsLog);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(793, 374);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Last Response";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // lrsLog
            // 
            this.lrsLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lrsLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lrsLog.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lrsLog.Location = new System.Drawing.Point(0, 0);
            this.lrsLog.Multiline = true;
            this.lrsLog.Name = "lrsLog";
            this.lrsLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.lrsLog.Size = new System.Drawing.Size(793, 374);
            this.lrsLog.TabIndex = 5;
            this.lrsLog.WordWrap = false;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.invLog);
            this.tabPage5.Controls.Add(this.panel1);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(793, 374);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Invoke Data";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // invLog
            // 
            this.invLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.invLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.invLog.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.invLog.Location = new System.Drawing.Point(3, 26);
            this.invLog.Multiline = true;
            this.invLog.Name = "invLog";
            this.invLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.invLog.Size = new System.Drawing.Size(787, 345);
            this.invLog.TabIndex = 3;
            this.invLog.WordWrap = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.defInvoke);
            this.panel1.Controls.Add(this.procInvCB);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(787, 23);
            this.panel1.TabIndex = 4;
            // 
            // defInvoke
            // 
            this.defInvoke.Enabled = false;
            this.defInvoke.Location = new System.Drawing.Point(131, 2);
            this.defInvoke.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.defInvoke.Name = "defInvoke";
            this.defInvoke.Size = new System.Drawing.Size(97, 20);
            this.defInvoke.TabIndex = 8;
            this.defInvoke.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.defInvoke.ValueChanged += new System.EventHandler(this.changeInvokeNumber);
            // 
            // procInvCB
            // 
            this.procInvCB.AutoSize = true;
            this.procInvCB.Location = new System.Drawing.Point(5, 3);
            this.procInvCB.Name = "procInvCB";
            this.procInvCB.Size = new System.Drawing.Size(100, 17);
            this.procInvCB.TabIndex = 7;
            this.procInvCB.Text = "Process Invoke";
            this.procInvCB.UseVisualStyleBackColor = true;
            this.procInvCB.CheckedChanged += new System.EventHandler(this.toggleprocInvCB);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.oosCB);
            this.tabPage4.Controls.Add(this.onTopCB);
            this.tabPage4.Controls.Add(this.linkLabel1);
            this.tabPage4.Controls.Add(this.label5);
            this.tabPage4.Controls.Add(this.customHeaders);
            this.tabPage4.Controls.Add(this.comboBox1);
            this.tabPage4.Controls.Add(this.label4);
            this.tabPage4.Controls.Add(this.responseText);
            this.tabPage4.Controls.Add(this.textBox5);
            this.tabPage4.Controls.Add(this.onlyLocalCB);
            this.tabPage4.Controls.Add(this.defPort);
            this.tabPage4.Controls.Add(this.defPortLabel);
            this.tabPage4.Controls.Add(this.defCodeLabel);
            this.tabPage4.Controls.Add(this.defCode);
            this.tabPage4.Controls.Add(this.allowEmptyCB);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(793, 374);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Settings Page";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // oosCB
            // 
            this.oosCB.AutoSize = true;
            this.oosCB.Checked = true;
            this.oosCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.oosCB.Location = new System.Drawing.Point(515, 9);
            this.oosCB.Name = "oosCB";
            this.oosCB.Size = new System.Drawing.Size(94, 17);
            this.oosCB.TabIndex = 16;
            this.oosCB.Text = "Open On Start";
            this.oosCB.UseVisualStyleBackColor = true;
            // 
            // onTopCB
            // 
            this.onTopCB.AutoSize = true;
            this.onTopCB.Location = new System.Drawing.Point(698, 9);
            this.onTopCB.Name = "onTopCB";
            this.onTopCB.Size = new System.Drawing.Size(86, 17);
            this.onTopCB.TabIndex = 15;
            this.onTopCB.Text = "Stay On Top";
            this.onTopCB.UseVisualStyleBackColor = true;
            this.onTopCB.CheckedChanged += new System.EventHandler(this.toggleonTopCB);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(398, 10);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(112, 13);
            this.linkLabel1.TabIndex = 14;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://localhost:8081/";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.openBrowser);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Custom Headers:";
            // 
            // customHeaders
            // 
            this.customHeaders.Location = new System.Drawing.Point(8, 94);
            this.customHeaders.Multiline = true;
            this.customHeaders.Name = "customHeaders";
            this.customHeaders.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.customHeaders.Size = new System.Drawing.Size(384, 274);
            this.customHeaders.TabIndex = 12;
            this.customHeaders.WordWrap = false;
            this.customHeaders.TextChanged += new System.EventHandler(this.changeResponseHeaders);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "text/plain",
            "text/plain; charset=utf-8",
            "text/html",
            "text/html; charset=utf-8",
            "text/xml",
            "text/xml; charset=utf-8",
            "application/json",
            "application/json; charset=utf-8",
            "application/xml",
            "application/xml; charset=utf-8"});
            this.comboBox1.Location = new System.Drawing.Point(515, 27);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(269, 21);
            this.comboBox1.TabIndex = 11;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.changeDefaultContentType);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(398, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Content Type:";
            // 
            // responseText
            // 
            this.responseText.AutoSize = true;
            this.responseText.Location = new System.Drawing.Point(398, 78);
            this.responseText.Name = "responseText";
            this.responseText.Size = new System.Drawing.Size(82, 13);
            this.responseText.TabIndex = 9;
            this.responseText.Text = "Response Text:";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(398, 94);
            this.textBox5.Multiline = true;
            this.textBox5.Name = "textBox5";
            this.textBox5.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox5.Size = new System.Drawing.Size(386, 274);
            this.textBox5.TabIndex = 8;
            this.textBox5.WordWrap = false;
            this.textBox5.TextChanged += new System.EventHandler(this.changeDefaultResponseText);
            // 
            // onlyLocalCB
            // 
            this.onlyLocalCB.AutoSize = true;
            this.onlyLocalCB.Location = new System.Drawing.Point(8, 29);
            this.onlyLocalCB.Name = "onlyLocalCB";
            this.onlyLocalCB.Size = new System.Drawing.Size(152, 17);
            this.onlyLocalCB.TabIndex = 7;
            this.onlyLocalCB.Text = "Allow Only Local Requests";
            this.onlyLocalCB.UseVisualStyleBackColor = true;
            this.onlyLocalCB.CheckedChanged += new System.EventHandler(this.toggleAllowOnlyLocalhost);
            // 
            // defPort
            // 
            this.defPort.Location = new System.Drawing.Point(295, 5);
            this.defPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.defPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.defPort.Name = "defPort";
            this.defPort.Size = new System.Drawing.Size(97, 20);
            this.defPort.TabIndex = 4;
            this.defPort.Value = new decimal(new int[] {
            8081,
            0,
            0,
            0});
            this.defPort.ValueChanged += new System.EventHandler(this.defPort_ValueChanged);
            // 
            // defPortLabel
            // 
            this.defPortLabel.AutoSize = true;
            this.defPortLabel.Location = new System.Drawing.Point(166, 10);
            this.defPortLabel.Name = "defPortLabel";
            this.defPortLabel.Size = new System.Drawing.Size(66, 13);
            this.defPortLabel.TabIndex = 3;
            this.defPortLabel.Text = "Default Port:";
            // 
            // defCodeLabel
            // 
            this.defCodeLabel.AutoSize = true;
            this.defCodeLabel.Location = new System.Drawing.Point(166, 33);
            this.defCodeLabel.Name = "defCodeLabel";
            this.defCodeLabel.Size = new System.Drawing.Size(123, 13);
            this.defCodeLabel.TabIndex = 2;
            this.defCodeLabel.Text = "Default Response Code:";
            // 
            // defCode
            // 
            this.defCode.Location = new System.Drawing.Point(295, 29);
            this.defCode.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.defCode.Name = "defCode";
            this.defCode.Size = new System.Drawing.Size(97, 20);
            this.defCode.TabIndex = 1;
            this.defCode.Value = new decimal(new int[] {
            404,
            0,
            0,
            0});
            this.defCode.ValueChanged += new System.EventHandler(this.changeDefaultResponseCode);
            // 
            // allowEmptyCB
            // 
            this.allowEmptyCB.AutoSize = true;
            this.allowEmptyCB.Checked = true;
            this.allowEmptyCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.allowEmptyCB.Location = new System.Drawing.Point(8, 6);
            this.allowEmptyCB.Name = "allowEmptyCB";
            this.allowEmptyCB.Size = new System.Drawing.Size(126, 17);
            this.allowEmptyCB.TabIndex = 0;
            this.allowEmptyCB.Text = "Allow Empty Request";
            this.allowEmptyCB.UseVisualStyleBackColor = true;
            this.allowEmptyCB.CheckedChanged += new System.EventHandler(this.toggleAllowEmptyRequest);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(801, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(111, 17);
            this.toolStripStatusLabel1.Text = "Incoming queries: 0";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(115, 17);
            this.toolStripStatusLabel2.Text = "Processed Invokes: 0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(801, 450);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.startstop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Incoming HTTP Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.defInvoke)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.defPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.defCode)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startstop;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox prcLog;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.CheckBox allowEmptyCB;
        private System.Windows.Forms.Label defCodeLabel;
        private System.Windows.Forms.NumericUpDown defCode;
        private System.Windows.Forms.NumericUpDown defPort;
        private System.Windows.Forms.Label defPortLabel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TextBox qryLog;
        private System.Windows.Forms.TextBox lqrLog;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TextBox invLog;
        private System.Windows.Forms.CheckBox onlyLocalCB;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label responseText;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox customHeaders;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.NumericUpDown defInvoke;
        private System.Windows.Forms.CheckBox procInvCB;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.TextBox lrsLog;
        private System.Windows.Forms.CheckBox onTopCB;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.CheckBox oosCB;
    }
}

