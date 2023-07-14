//
// C# (.Net Framework)
// IncominHttpServer
// https://github.com/dkxce/IncominHttpServer
// en,ru,1251,utf-8
//

using System;
using System.Windows.Forms;

namespace IncominHttpServer
{
    public partial class Form1 : Form
    {
        private HttpServer httpServer = null;
        private HttpInvoke httpInvoke = new HttpInvoke();
        private string tmpTitle = null;
        private ulong qieries = 0;
        private ulong invokes = 0;
        private bool firstclick = true;

        public Form1()
        {
            InitializeComponent();
            tmpTitle = this.Text;
            this.Text = $"{tmpTitle} (by dkxce)";
            ApplicationLog.form = this;
            prcLog.Text = "Welcome to Incoming HTTP Server by dkxce\r\nOriginal at: https://github.com/dkxce/IncominHttpServer\r\nJust Press START button";
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            prcLog.Clear();
            qryLog.Clear();
            lqrLog.Clear();
            invLog.Clear();
            lrsLog.Clear();
        }

        private void start_stop_click(object sender, EventArgs e)
        {
            if (httpServer != null)
            {
                this.Text = $"{tmpTitle} - Stopped";
                ApplicationLog.WriteDatedLn($"Https Server Stopped");
                httpServer.Stop();
                httpServer = null;
                defPort.Enabled = true;
                startstop.Text = "START";
                startstop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            }
            else
            {                
                httpServer = new HttpServer((ushort)defPort.Value);
                httpServer.AllowEmptyRequest = allowEmptyCB.Checked;
                httpServer.AllowOnlyLocalhost = onlyLocalCB.Checked;
                httpServer.DefaultResponseCode = (ushort)defCode.Value;
                httpServer.DefaultResponseText = textBox5.Text.Trim();
                httpServer.ResponseHeaders = customHeaders.Text.Trim();
                httpServer.DefaultContentType = comboBox1.Text.Trim();
                if(procInvCB.Checked)
                    httpServer.onClientRequest += new HttpServer.ClientRequestDelegate(httpInvoke.GetClientRequest);
                httpInvoke.InvokeNumber = (ushort)defInvoke.Value;
                try
                {                    
                    httpServer.Start();
                    if (firstclick) prcLog.Clear();
                    firstclick = false;
                    this.Text = $"{tmpTitle} - Started at {httpServer.ServerPort}";
                    ApplicationLog.WriteDatedLn($"Https Server Started at http://localhost:{httpServer.ServerPort}/");
                    defPort.Enabled = false;
                    startstop.Text = "STOP";
                    startstop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
                    if (oosCB.Checked) openBrowser(sender, null);
                }
                catch (Exception ex)
                {
                    httpServer = null;
                    ApplicationLog.WriteDatedLn($"Error: {ex.Message}");
                    MessageBox.Show($"{ex.Message}", "Incoming Http Server", MessageBoxButtons.OK, MessageBoxIcon.Error);                    
                };
            };
        }

        public void WriteProcLog(string text, bool append = true) 
        {
            if(append)
                this.BeginInvoke(new Action(() => { prcLog.Text = text + prcLog.Text; }));
            else
                this.BeginInvoke(new Action(() => { prcLog.Text = text; }));
        }

        public void WriteQueryLog(string text, bool append = true)
        {
            if (append)
                this.BeginInvoke(new Action(() => { qryLog.Text = text + qryLog.Text; }));
            else
                this.BeginInvoke(new Action(() => { qryLog.Text = text; }));
        }

        public void WriteLastQuery(string text, bool append = true)
        {
            if(append)
                this.BeginInvoke(new Action(() => { lqrLog.Text = text + lqrLog.Text; }));
            else
                this.BeginInvoke(new Action(() => { lqrLog.Text = text; }));
            toolStripStatusLabel1.Text = $"Incoming queries: {++qieries}";
            this.BeginInvoke(new Action(() => { this.Text = $"{tmpTitle} - Started at {httpServer.ServerPort} [{qieries}]"; }));            
        }

        public void WriteInvLog(string text, bool append = true)
        {
            if(append)
                this.BeginInvoke(new Action(() => { invLog.Text = text + invLog.Text; }));
            else
                this.BeginInvoke(new Action(() => { invLog.Text = text; }));
            toolStripStatusLabel2.Text = $"Processed Invokes: {++invokes}";
        }

        public void WriteRespLog(string text, bool append = true)
        {
            if(append)
                this.BeginInvoke(new Action(() => { lrsLog.Text = text + lrsLog.Text; }));
            else
                this.BeginInvoke(new Action(() => { lrsLog.Text = text; }));
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (httpServer != null)
                httpServer.Stop();
        }

        private void toggleAllowEmptyRequest(object sender, EventArgs e)
        {
            if (httpServer != null)
                httpServer.AllowEmptyRequest = allowEmptyCB.Checked;
        }

        private void changeDefaultResponseCode(object sender, EventArgs e)
        {
            if (httpServer != null)
                httpServer.DefaultResponseCode = (ushort)defCode.Value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Config cfg = Config.Load("IncominHttpServer.ini");
                allowEmptyCB.Checked = cfg.AllowEmptyRequest;
                onlyLocalCB.Checked = cfg.AllowOnlyLocalhost;
                defCode.Value = cfg.DefaultResponseCode;
                textBox5.Text = cfg.DefaultResponseText.Replace(@"\n","\r\n");
                customHeaders.Text = cfg.ResponseHeaders.Replace(@"\n", "\r\n");
                comboBox1.Text = cfg.DefaultContentType;
                defPort.Value = cfg.SeverPort;
                defInvoke.Value = cfg.InvokeNumber;
                procInvCB.Checked = cfg.ProcessInvoke;
                onTopCB.Checked = cfg.StayOnTop;
                oosCB.Checked = cfg.OpenOnStart;
            }
            catch { };            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Config cfg = new Config();
                cfg.AllowEmptyRequest = allowEmptyCB.Checked;
                cfg.AllowOnlyLocalhost = onlyLocalCB.Checked;
                cfg.DefaultResponseCode = (ushort)defCode.Value;
                cfg.DefaultResponseText = textBox5.Text.Trim().Replace("\r\n", @"\n");
                cfg.ResponseHeaders = customHeaders.Text.Trim().Replace("\r\n", @"\n");
                cfg.DefaultContentType = comboBox1.Text.Trim();
                cfg.SeverPort = (ushort)defPort.Value;
                cfg.InvokeNumber = (ushort)defInvoke.Value;
                cfg.StayOnTop = onTopCB.Checked;
                cfg.OpenOnStart = oosCB.Checked;
                cfg.ProcessInvoke = procInvCB.Checked;
                Config.SaveHere("IncominHttpServer.ini", cfg);
            }
            catch { };
            if (httpServer != null)
            {
                if (MessageBox.Show("Server is Runnning!\r\nAre you Sure to Exit?", "Incoming Http Server", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    e.Cancel = true;
            };
        }

        private void toggleprocInvCB(object sender, EventArgs e)
        {
            defInvoke.Enabled = procInvCB.Checked;
            if (httpServer != null)
            {
                if (procInvCB.Checked)
                {
                    if(httpServer.onClientRequest == null)
                        httpServer.onClientRequest += new HttpServer.ClientRequestDelegate(httpInvoke.GetClientRequest);
                }
                else
                    httpServer.onClientRequest = null;
            };
        }

        private void changeInvokeNumber(object sender, EventArgs e)
        {
            httpInvoke.InvokeNumber = (ushort)defInvoke.Value;
        }

        private void toggleAllowOnlyLocalhost(object sender, EventArgs e)
        {
            if (httpServer != null) httpServer.AllowOnlyLocalhost = onlyLocalCB.Checked;
        }

        private void openBrowser(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { System.Diagnostics.Process.Start(linkLabel1.Text); } catch { };
        }

        private void defPort_ValueChanged(object sender, EventArgs e)
        {
            linkLabel1.Text = $"http://localhost:{defPort.Value}/";
        }

        private void changeDefaultResponseText(object sender, EventArgs e)
        {
            if (httpServer != null) httpServer.DefaultResponseText = textBox5.Text.Trim();
        }

        private void changeDefaultContentType(object sender, EventArgs e)
        {
            if (httpServer != null) httpServer.DefaultContentType = comboBox1.Text.Trim();
        }

        private void changeResponseHeaders(object sender, EventArgs e)
        {
            if (httpServer != null) httpServer.ResponseHeaders = customHeaders.Text.Trim();
        }

        private void toggleonTopCB(object sender, EventArgs e)
        {
            this.TopMost = onTopCB.Checked;
        }

        private void clearCountersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            qieries = 0;
            invokes = 0;
            toolStripStatusLabel1.Text = $"Incoming queries: {qieries}";
            toolStripStatusLabel2.Text = $"Processed Invokes: {invokes}";
            if(httpServer != null)
                this.Text = $"{tmpTitle} - Started at {httpServer.ServerPort} [{qieries}]";            
        }
    }

    public class Config: dkxce.IniSaved<Config>
    {
        public bool AllowEmptyRequest = true;
        public bool AllowOnlyLocalhost = false;
        public ushort DefaultResponseCode = 404;
        public string DefaultResponseText = string.Empty;
        public string ResponseHeaders = string.Empty;
        public string DefaultContentType = string.Empty;
        public ushort SeverPort = 7177;
        public bool ProcessInvoke = false;
        public bool StayOnTop = false;
        public bool OpenOnStart = true;
        public ushort InvokeNumber = 0;
    }
}
