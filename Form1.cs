//
// C# (.Net Framework)
// IncominHttpServer
// https://github.com/dkxce/IncominHttpServer
// en,ru,1251,utf-8
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace IncominHttpServer
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);

        private HttpServer httpServer = null;
        private HttpInvoke httpInvoke = new HttpInvoke();
        private string tmpTitle = null;
        private ulong qieries = 0;
        private ulong invokes = 0;
        private bool firstclick = true;
        private DateTime PreviousDT = DateTime.MinValue;
        private string MyIP = null;
        private uint ddnsChecks = 0;
        private uint ddnsUpdates = 0;
        private uint ipChecks = 0;

        public Form1()
        {
            InitializeComponent();
            tmpTitle = this.Text;
            this.Text = $"{tmpTitle} (by dkxce)";
            ApplicationLog.form = this;
            prcLog.Text = "Welcome to Incoming HTTP Server by dkxce\r\nOriginal at: https://github.com/dkxce/IncominHttpServer\r\nJust Press START button";
            ddnsLink.Text = "";
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            IntPtr hSysMenu = GetSystemMenu(this.Handle, false);
            AppendMenu(hSysMenu, 0x000, 0x01, "Author: dkxce");
            AppendMenu(hSysMenu, 0x800, 0x02, string.Empty);
            AppendMenu(hSysMenu, 0x000, 0x03, "Create Desktop Shortcut");
            AppendMenu(hSysMenu, 0x000, 0x04, "Create Start Menu Shortcut");            
            AppendMenu(hSysMenu, 0x000, 0x05, "Create Startup Menu Shortcut");            
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if ((m.Msg == 0x112) && ((int)m.WParam == 0x01)) // Author
            {
                try { System.Diagnostics.Process.Start("https://github.com/dkxce/IncominHttpServer"); } catch { };
            };
            if ((m.Msg == 0x112) && ((int)m.WParam == 0x03))
            {
                try
                {
                    string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{tmpTitle}.lnk");
                    ShellLink sl = new ShellLink(file);
                    sl.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    sl.Description = "Incoming HTTP Server";
                    sl.Save();
                }
                catch { };
            };
            if ((m.Msg == 0x112) && ((int)m.WParam == 0x04))
            {
                try
                {
                    string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), $"{tmpTitle}.lnk");
                    ShellLink sl = new ShellLink(file);
                    sl.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    sl.Description = "Incoming HTTP Server";
                    sl.Save();
                }
                catch { };
            };
            if ((m.Msg == 0x112) && ((int)m.WParam == 0x05))
            {
                try
                {
                    string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{tmpTitle}.lnk");
                    ShellLink sl = new ShellLink(file);
                    sl.Arguments = "/trayed";
                    sl.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    sl.Description = "Incoming HTTP Server";
                    sl.Save();
                }
                catch { };
            };
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
                toolStripStatusLabel1.Visible = toolStripStatusLabel2.Visible = false;
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
                toolStripStatusLabel1.Visible = toolStripStatusLabel2.Visible = true;
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
            trayIcon.Visible = false;
            trayIcon.Dispose();
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
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 0) for (int i = 1; i < args.Length; i++) if (args[i].ToLower() == "/trayed")
                    {
                        this.WindowState = FormWindowState.Minimized;
                        this.ShowInTaskbar = false;
                        this.Visible = false;
                    };

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
                if (cfg.dDNSServices != null && cfg.dDNSServices.Length > 0)
                    foreach (DDNSService dnss in cfg.dDNSServices)
                        ddnsView.Items.Add(new DNSListViewItem(dnss));
                automaticDDNSUpdatesToolStripMenuItem.Checked = cfg.AutomaticUpdateDDNS && ddnsView.Items.Count > 0;
                allowTrayNotificationsToolStripMenuItem.Checked = cfg.allowTrayNotifications;
                ipcsCB.Checked = cfg.GetIPCountryOnStartUp;
            }
            catch { };

            addns.Text = $"AutoDDNS: {(automaticDDNSUpdatesToolStripMenuItem.Checked ? "Enabled" : "Disabled")} ({DateTime.Now})";
            TrayIcon(true, (byte)0);

            bool checkIp = ipcsCB.Checked;
            (new System.Threading.Thread(new System.Threading.ThreadStart(() => {
                System.Threading.Thread.Sleep(2000);
                if(checkIp) try { this.BeginInvoke(new Action(() => { GetIPCountry(true); })); } catch { };
                System.Threading.Thread.Sleep(1000);
                try { this.BeginInvoke(new Action(() => { timer_Tick(null, null); timer.Enabled = true; })); } catch { };
            }))).Start();            
        }

        private void TrayIcon(bool show, byte color)
        {
            trayIcon.Visible = show;
            if(color == 0) trayIcon.Icon = IconFromImage(global::IncominHttpServer.Properties.Resources.ddnsr);
            if(color == 1) trayIcon.Icon = IconFromImage(global::IncominHttpServer.Properties.Resources.ddnsb);
            if(color == 2) trayIcon.Icon = IconFromImage(global::IncominHttpServer.Properties.Resources.ddnsg);
            if (!show) return;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Config cfg = new Config();
                cfg.AllowEmptyRequest = allowEmptyCB.Checked;
                cfg.AutomaticUpdateDDNS = automaticDDNSUpdatesToolStripMenuItem.Checked;
                cfg.allowTrayNotifications = allowTrayNotificationsToolStripMenuItem.Checked;
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
                cfg.GetIPCountryOnStartUp = ipcsCB.Checked;
                if (ddnsView.Items.Count > 0)
                {
                    DDNSService[] svcs = new DDNSService[ddnsView.Items.Count];
                    for (int i = 0; i < svcs.Length; i++) svcs[i] = ((DNSListViewItem)ddnsView.Items[i]).service;
                    cfg.dDNSServices = svcs;
                };
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

        private void openDDNSLink(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ddnsLink.Text.Trim().Length == 0) return;
            try { System.Diagnostics.Process.Start(ddnsLink.Text); } catch { };
        }

        private void contextMenuStrip2_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            deBtn.Enabled = ddnsView.SelectedItems.Count == 1;
            ddBtn.Enabled = ddnsView.SelectedItems.Count == 1;
            dwBtr.Enabled = ddnsView.SelectedItems.Count > 0;
            drBtn.Enabled = ddnsView.SelectedItems.Count > 0;
            r1Btn.Enabled = ddnsView.SelectedItems.Count > 0;
            dcBtn.Enabled = ddnsView.Items.Count > 0;
            duBtn.Enabled = ddnsView.Items.Count > 0;
            rrBtn.Enabled = ddnsView.Items.Count > 0;
        }

        private void dcBtn_Click(object sender, EventArgs e)
        {
            if (ddnsView.Items.Count == 0) return;
            if (MessageBox.Show($"Are you sure to Erase all {ddnsView.Items.Count} items?", "Incoming Http Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            ddnsView.Items.Clear();
        }

        private void drBtn_Click(object sender, EventArgs e)
        {
            if (ddnsView.SelectedItems.Count == 0) return;
            if (MessageBox.Show($"Are you sure to Remove {ddnsView.SelectedItems.Count} items?", "Incoming Http Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            for (int i = ddnsView.Items.Count - 1; i >= 0; i--)
                if (ddnsView.Items[i].Selected)
                    ddnsView.Items.RemoveAt(i);
        }

        private void deBtn_Click(object sender, EventArgs e)
        {
            if (ddnsView.SelectedItems.Count != 1) return;
            DNSListViewItem lvi = (DNSListViewItem)ddnsView.SelectedItems[0];
            DDNSService dnss = lvi.service;
            int svc = dnss.type;
            bool tm = this.TopMost; this.TopMost = false;

            for (int i = 0; i < DDNSService.Customize[svc].Length; i++)
            {
                string caption = DDNSService.Customize[svc][i];
                string value = dnss[caption];
                if (InputBox.Show("Edit Dynamic DNS Service", $"{caption}:", ref value) != DialogResult.OK)
                {
                    this.TopMost = tm;
                    lvi.Update();
                    return;
                };
                dnss[caption] = value;
            };
            this.TopMost = tm;
            lvi.Update();
        }

        private void daBtn_Click(object sender, EventArgs e)
        {
            int svc = 0;
            bool tm = this.TopMost; this.TopMost = false;
            if (InputBox.QueryListBox("Add Dynamic DNS Service", "Select Provider:", DDNSService.ServiceNames, ref svc) != DialogResult.OK)
            {
                this.TopMost = tm;
                return;
            };
            DDNSService dnss = new DDNSService() { type = svc };
            for (int i = 0; i < DDNSService.Customize[svc].Length; i++)
            {
                string caption = DDNSService.Customize[svc][i];
                string value = DDNSService.CustomSamples[svc][i];
                if (InputBox.Show("Add Dynamic DNS Service", $"{caption}:", ref value) != DialogResult.OK)
                {
                    this.TopMost = tm;
                    return;
                };
                dnss.settings.Add(new DNSListViewItem.KeyValue(){ Key = caption, Value = value });
            };
            this.TopMost = tm;
            ddnsView.Items.Add(new DNSListViewItem(dnss));
        }

        private void ddnsView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(ddnsView.SelectedItems.Count != 1)
            {
                ddnsLink.Text = "";
            }
            else
            {
                ddnsLink.Text = DDNSService.ServiceUrls[((DNSListViewItem)ddnsView.SelectedItems[0]).service.type];
            };
        }

        private void ddBtn_Click(object sender, EventArgs e)
        {
            if (ddnsView.SelectedItems.Count != 1) return;
            ListViewItem lvi = ddnsView.Items.Add(new DNSListViewItem(((DNSListViewItem)ddnsView.SelectedItems[0]).service.copy()));
            try { ddnsView.SelectedItems[0].Selected = false; } catch { };
            lvi.Selected = true;
            lvi.EnsureVisible();

        }

        private void duBtn_Click(object sender, EventArgs e)
        {
            if (ddnsView.Items.Count == 0) return;
            if (firstclick) prcLog.Clear();
            firstclick = false;
            tabControl1.SelectedIndex = 0;
            for(int i =0;i<ddnsView.Items.Count;i++)
            {
                DDNSService svc = ((DNSListViewItem)ddnsView.Items[i]).service;
                ProcessService(svc);
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
            };
        }

        private (string, string) ProcessResolve(DDNSService svc, bool log = true)
        {
            string host = null;
            if (svc.type == 0) host = svc[DDNSService.Customize[0][2]];
            if (svc.type == 1) host = svc[DDNSService.Customize[1][0]];
            if (string.IsNullOrEmpty(host)) return (null, null);
            try
            {
                IPHostEntry iphe = System.Net.Dns.Resolve(host);
                string ips = "";
                foreach (IPAddress ip in iphe.AddressList)
                    ips += (ips.Length > 0 ? ", " : "") + ip.ToString();
                string als = "";
                foreach(string al in iphe.Aliases)
                    als += (als.Length > 0 ? ", " : "") + al.ToString();
                if (als.Length > 0) als = $" ({als})";
                if(log) ApplicationLog.WriteDatedLn($"Resolve {host} ({iphe.HostName}): {ips} {als}");
                return (host, ips);
            }
            catch (Exception ex)
            {
                if(log) ApplicationLog.WriteDatedLn($"Resolve {host} Error: {ex.Message}");
            };
            return (null, null);
        }

        private bool ProcessService(DDNSService svc)
        {
            if (svc.type == 0)
            {
                ApplicationLog.WriteDatedLn($"Update DDNS {DDNSService.ServiceNames[svc.type]}, {DDNSService.ServiceUrls[svc.type]}");
                string url0 = svc[DDNSService.Customize[0][0]];
                string url1 = svc[DDNSService.Customize[0][1]];
                string hst2 = svc[DDNSService.Customize[0][2]];

                try
                {
                    WebClient wc = new WebClient();
                    string xml = wc.DownloadString(url0);
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(xml);
                    XmlNodeList nl = xd.DocumentElement.SelectNodes("/xml/item/host");
                    foreach (XmlElement nd in nl)
                    {
                        if (nd.ChildNodes[0].Value == hst2)
                        {
                            string addr = nd.ParentNode.SelectSingleNode("address").ChildNodes[0].Value;
                            string url = nd.ParentNode.SelectSingleNode("url").ChildNodes[0].Value;
                            ApplicationLog.WriteLn($"Updating {hst2} (now: {addr}): {url} ...");
                            string resp = wc.DownloadString(url).Trim();
                            ApplicationLog.WriteLn($"Result: {resp}");
                            return true;
                        };
                    };
                }
                catch (Exception ex)
                {
                    ApplicationLog.WriteLn($"Error: {ex.Message}");
                };

                try
                {
                    WebClient wc = new WebClient();
                    string text = wc.DownloadString(url1);                    
                    foreach (string line in text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!line.StartsWith($"{hst2}|")) continue;
                        string[] hau = line.Split(new char[] { '|' }, 3);
                        string addr = hau[1];
                        string url = hau[2];
                        ApplicationLog.WriteLn($"Updating {hst2} (now: {addr}): {url} ...");
                        string resp = wc.DownloadString(url).Trim();
                        ApplicationLog.WriteLn($"Result: {resp}");
                        return true;
                    };
                }
                catch (Exception ex)
                {
                    ApplicationLog.WriteLn($"Error: {ex.Message}");
                };
                ApplicationLog.WriteLn($"No Hosts To Update Found");
            };
            //////////////////////
            if (svc.type == 1)
            {
                ApplicationLog.WriteDatedLn($"Update DDNS {DDNSService.ServiceNames[svc.type]}, {DDNSService.ServiceUrls[svc.type]}");
                string hst = svc[DDNSService.Customize[1][0]];
                string usr = svc[DDNSService.Customize[1][1]];
                string psw = svc[DDNSService.Customize[1][2]];
                string url = $"https://ydns.io/api/v1/update/?host={hst}";

                try
                {
                    WebClient wc = new WebClient();
                    string up = Base64Encode($"{usr}:{psw}");
                    wc.Headers.Add("Authorization", $"Basic {up}");
                    ApplicationLog.WriteLn($"Updating {hst}: {url} ...");
                    string resp = wc.DownloadString(url).Trim();
                    ApplicationLog.WriteLn($"Result: {resp}");
                    return true;
                }
                catch (Exception ex)
                {
                    ApplicationLog.WriteLn($"Error: {ex.Message}");
                    return false;
                };
                ApplicationLog.WriteLn($"No Hosts To Update Found");
            };
            return false;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private void updateDDNSHostsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            duBtn_Click(sender, e);
        }

        private void dwBtn_Click(object sender, EventArgs e)
        {
            if (ddnsView.SelectedItems.Count == 0) return;
            if (firstclick) prcLog.Clear();
            firstclick = false;
            tabControl1.SelectedIndex = 0;
            for (int i = 0; i < ddnsView.SelectedItems.Count; i++)
            {
                DDNSService svc = ((DNSListViewItem)ddnsView.SelectedItems[i]).service;
                ProcessService(svc);
            };
        }

        private void rrBtn_Click(object sender, EventArgs e)
        {
            if (ddnsView.Items.Count == 0) return;
            if (firstclick) prcLog.Clear();
            firstclick = false;
            tabControl1.SelectedIndex = 0;
            for (int i = 0; i < ddnsView.Items.Count; i++)
            {
                DDNSService svc = ((DNSListViewItem)ddnsView.Items[i]).service;
                ProcessResolve(svc);
            };
        }

        private void r1Btn_Click(object sender, EventArgs e)
        {
            if (ddnsView.SelectedItems.Count == 0) return;
            if (firstclick) prcLog.Clear();
            firstclick = false;
            tabControl1.SelectedIndex = 0;
            for (int i = 0; i < ddnsView.SelectedItems.Count; i++)
            {
                DDNSService svc = ((DNSListViewItem)ddnsView.SelectedItems[i]).service;
                ProcessResolve(svc);
            };
        }

        private void resolveDDNSHostsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rrBtn_Click(sender, e);
        }

        private void mipBtn_Click(object sender, EventArgs e)
        {
            if (firstclick) prcLog.Clear();
            firstclick = false;
            tabControl1.SelectedIndex = 0;
            GetIPCountry(false);
        }

        private void GetIPCountry(bool skipErrors = false)
        {
            try
            {
                MyIP = System.Net.IPCountry.GetPublicIP(out string ips);
                if (string.IsNullOrEmpty(MyIP)) MyIP = "127.0.0.1";
                trayIcon.Text = $"My IP: {MyIP}";
                mypLabel.Text = $"My IP: {MyIP}";
                ApplicationLog.WriteLn($"My IP: {MyIP} (by {ips})");
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                string co = System.Net.IPCountry.GetCountryByIP(MyIP, out string cs);
                if(!string.IsNullOrEmpty(co))
                    mypLabel.Text = $"My IP: {MyIP} {co}";                
                ApplicationLog.WriteLn($"My Country: {co} (by {cs})");                
            }
            catch (Exception ex)
            {
                if(!skipErrors)
                    ApplicationLog.WriteLn($"Error IP/Country: {ex.Message}");
            };
        }

        public static Icon IconFromImage(Image img)
        {
            MemoryStream ms = new System.IO.MemoryStream();
            BinaryWriter bw = new System.IO.BinaryWriter(ms);
            // Header
            bw.Write((short)0);   // 0 : reserved
            bw.Write((short)1);   // 2 : 1=ico, 2=cur
            bw.Write((short)1);   // 4 : number of images
                                  // Image directory
            var w = img.Width;
            if (w >= 256) w = 0;
            bw.Write((byte)w);    // 0 : width of image
            var h = img.Height;
            if (h >= 256) h = 0;
            bw.Write((byte)h);    // 1 : height of image
            bw.Write((byte)0);    // 2 : number of colors in palette
            bw.Write((byte)0);    // 3 : reserved
            bw.Write((short)0);   // 4 : number of color planes
            bw.Write((short)0);   // 6 : bits per pixel
            var sizeHere = ms.Position;
            bw.Write((int)0);     // 8 : image size
            var start = (int)ms.Position + 4;
            bw.Write(start);      // 12: offset of image data
                                  // Image data
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var imageSize = (int)ms.Position - start;
            ms.Seek(sizeHere, System.IO.SeekOrigin.Begin);
            bw.Write(imageSize);
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            // And load it
            return new Icon(ms);
        }
        

        private void comboIcons1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.UtcNow.Subtract(PreviousDT);
            if (ts.TotalMinutes < 15) return;

            PreviousDT = DateTime.UtcNow;
            ProcessAutomatic(automaticDDNSUpdatesToolStripMenuItem.Checked && ddnsView.Items.Count > 0, automaticDDNSUpdatesToolStripMenuItem.Checked, allowTrayNotificationsToolStripMenuItem.Checked);
        }

        private void ProcessAutomatic(bool check, bool enabled, bool notify)
        {
            List<DDNSService> svcs = new List<DDNSService>();
            if(ddnsView.Items.Count > 0)
                for (int i = 0; i < ddnsView.Items.Count; i++)
                    svcs.Add(((DNSListViewItem)ddnsView.Items[i]).service);

            if(check) TrayIcon(true, (byte)1);

            (new System.Threading.Thread(new System.Threading.ThreadStart(() => {

                // CHECK IP                
                MyIP = IPCountry.GetPublicIP(out string ips);
                ipChecks++;
                if (string.IsNullOrEmpty(MyIP)) 
                    MyIP = "127.0.0.1";
                this.BeginInvoke(new Action(() => { 
                    trayIcon.Text = $"My IP: {MyIP}"; mypLabel.Text = $"My IP: {MyIP}"; 
                    if(!check) addns.Text = $"AutoDDNS: {(enabled ? "Enabled" : "Disabled")} ({DateTime.Now}) [{ddnsUpdates}/{ddnsChecks}/{ipChecks}]";
                }));
                if (!check) return;

                if (MyIP == "127.0.0.1")
                {
                    if(notify)
                        trayIcon.ShowBalloonTip(15000, "Auto DDNS Updating", $"Updates failed, couldn't resolve IP Address!", ToolTipIcon.Warning);
                    return;
                };

                // CHECK HOSTS
                addns.Text = $"AutoDDNS: Resolving Hosts ({DateTime.Now}) [{ddnsUpdates}/{ddnsChecks}/{ipChecks}] ...";
                bool update = true;
                for (int i = 0; i < svcs.Count; i++)
                {
                    (string host, string ip) = ProcessResolve(svcs[i], false);
                    if (string.IsNullOrEmpty(ip)) break;
                    MatchCollection mx = (new Regex("[\\w\\.]+", RegexOptions.IgnoreCase)).Matches(ip);
                    foreach (Match mc in mx)
                        if (mc.Groups[0].Value == MyIP)
                            update = false;
                };
                ddnsChecks++;
                if (update)
                {
                    addns.Text = $"AutoDDNS: Updating Hosts ({DateTime.Now}) [{ddnsUpdates}/{ddnsChecks}/{ipChecks}] ...";
                    this.BeginInvoke(new Action(() => {
                        bool ok = true;
                        for (int i = 0; i < svcs.Count; i++)
                            if (!ProcessService(svcs[i])) 
                                ok = false;
                        if (notify)
                            trayIcon.ShowBalloonTip(ok ? 7500 : 27000, "Auto DDNS Updating", $"{(ok ? "Hosts Updated Successfull" : "Some Errors Found")}\r\nNew IP: {MyIP}", ok ? ToolTipIcon.Info : ToolTipIcon.Error);
                        addns.Text = $"AutoDDNS: Updated ({DateTime.Now}), next check {PreviousDT.ToLocalTime().AddMinutes(15)} [{++ddnsUpdates}/{ddnsChecks}/{ipChecks}]";
                        TrayIcon(true, (byte)2);
                    }));
                }
                else
                {
                    addns.Text = $"AutoDDNS: No Need Update ({DateTime.Now}), next check {PreviousDT.ToLocalTime().AddMinutes(15)} [{ddnsUpdates}/{ddnsChecks}/{ipChecks}]";
                    this.BeginInvoke(new Action(() => { TrayIcon(true, (byte)2); }));
                };

            }))).Start();
        }

        private void copyIPToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(MyIP);
        }

        private void contextMenuStrip3_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            resolveAllDDNSHostsToolStripMenuItem.Enabled = ddnsView.Items.Count > 0;
            updateAllDDNSHostsToolStripMenuItem.Enabled = ddnsView.Items.Count > 0;
            automaticDDNSUpdatesToolStripMenuItem.Enabled = ddnsView.Items.Count > 0;
            hideWindowToolStripMenuItem.Text = !this.ShowInTaskbar || !this.Visible ? "Show Window" : "Hide Window";
        }

        private void updateAllDDNSHostsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            duBtn_Click(sender, e);
        }

        private void resolveAllDDNSHostsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rrBtn_Click(sender, e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void automaticDDNSUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            automaticDDNSUpdatesToolStripMenuItem.Checked = !automaticDDNSUpdatesToolStripMenuItem.Checked;
            if (automaticDDNSUpdatesToolStripMenuItem.Checked)
                PreviousDT = DateTime.MinValue;
            else
            {
                TrayIcon(true, (byte)0);
                addns.Text = $"AutoDDNS: Disabled ({DateTime.Now}) [0/0/{ipChecks}]";
                ddnsChecks = 0;
                ddnsUpdates = 0;
            };
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Show();
            this.BringToFront();
            this.Activate();            
        }

        private void hideWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.ShowInTaskbar || !this.Visible)
            {
                trayIcon_MouseDoubleClick(null, null);
            }
            else
            {
                this.ShowInTaskbar = false;
                this.Hide();
            };
        }

        private void allowTrayNotificationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowTrayNotificationsToolStripMenuItem.Checked = !allowTrayNotificationsToolStripMenuItem.Checked;
        }
    }

    public class Config: dkxce.IniSaved<Config>
    {
        public bool AutomaticUpdateDDNS = false;
        public bool allowTrayNotifications = false;
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
        public bool GetIPCountryOnStartUp = true;
        public ushort InvokeNumber = 0;
        public DDNSService[] dDNSServices = new DDNSService[0];
    }

    public class DDNSService
    {
        public static string[] ServiceNames = new string[] { 
            "FreeDNS",
            "YDNS"
        };
        public static string[]  ServiceUrls = new string[] { 
            "https://freedns.afraid.org/",
            "https://ydns.io/"
        };
        public static string[][]  Customize = new string[][] { 
            new string[] { "XML API Url (https://freedns.afraid.org/api/)", "ASCII API Url (https://freedns.afraid.org/api/)", "DDNS Host (https://freedns.afraid.org/dynamic/)" },
            new string[] { "DDNS Host (https://ydns.io/hosts/)", "Username (https://ydns.io/user/api)", "Secret (https://ydns.io/user/api)" }
        };
        public static string[][] CustomSamples = new string[][] {
            new string[] { "https://freedns.afraid.org/api/?action=getdyndns&v=2&sha=...&style=xml", "ASCII API Url (https://freedns.afraid.org/api/?action=getdyndns&v=2&sha=...)", "api.mooo.com" },
            new string[] { "example.ydns.io", "********************", "******************************" }
        };

        public int type = -1;
        public List<DNSListViewItem.KeyValue> settings = new List<DNSListViewItem.KeyValue>();
        
        [XmlIgnore]
        public string this[string index]
        {
            get
            {
                for (int i = 0; i < settings.Count; i++)
                    if (settings[i].Key == index)
                        return settings[i].Value;
                return null;
            }
            set
            {
                for (int i = 0; i < settings.Count; i++)
                    if (settings[i].Key == index)
                    {
                        settings[i].Value = value;
                        return;
                    };
                settings.Add(new DNSListViewItem.KeyValue { Key = index, Value = value });
            }
        }

        public DDNSService copy()
        {
            DDNSService res = new DDNSService() { type = this.type };
            foreach (DNSListViewItem.KeyValue kvp in settings)
                res.settings.Add(new DNSListViewItem.KeyValue() { Key = kvp.Key, Value = kvp.Value });
            return res;
        }
    }

    public class DNSListViewItem: ListViewItem
    {
        public class KeyValue
        {
            public string Key;
            public string Value;
        }

        public DDNSService service = null;

        public DNSListViewItem(DDNSService service)
        {
            this.service = service;
            this.Update();
        }

        public void Update()
        {
            this.SubItems.Clear();
            if (service != null)
            {
                this.SubItems[0].Text = DDNSService.ServiceNames[service.type];
                string txt = "";
                foreach (KeyValue kvp in service.settings)
                {
                    if (kvp.Key.StartsWith("DDNS Host"))
                        txt = $"{kvp.Value}" + (txt.Length > 0 ? " | " : "") + txt;
                    else
                        txt += (txt.Length > 0 ? " | " : "") + $"{kvp.Value}";
                };
                this.SubItems.Add(txt);
            };
        }        
    }
}
