//
// C# (.Net Framework)
// IncominHttpServer
// https://github.com/dkxce/IncominHttpServer
// en,ru,1251,utf-8
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace IncominHttpServer
{
    /// <summary>
    ///     GetDrivers HTTP Server for Host Application
    /// </summary>
    public partial class HttpServer : ThreadedHttpServer
    {
        /// <summary>
        ///     Allow Only Local Connection from localhost (127.0.0.1)
        /// </summary>
        public bool AllowOnlyLocalhost = false;

        /// <summary>
        ///     Allow Empty Request
        /// </summary>
        public bool AllowEmptyRequest = true;

        public ushort DefaultResponseCode = 404;
        public string DefaultResponseText = string.Empty;
        public string DefaultContentType  = string.Empty;
        public string ResponseHeaders  = string.Empty;

        public delegate bool ClientRequestDelegate(ClientRequest request); // returns process next
        public ClientRequestDelegate onClientRequest = null;

        public HttpServer() : base(80) { CustomInit(); }
        public HttpServer(int Port) : base(Port) { CustomInit(); }
        public HttpServer(IPAddress IP, int Port) : base(IP, Port) { CustomInit(); }
        ~HttpServer() { this.Dispose(); }
        private void CustomInit()
        {
            ServerName = "dkxce IncominHttpServer HttpServer v0.1";
            _AddContentDisposition = true;
            _ContentDispositionInline = true;

            _headers.Add("Allow", "GET, HEAD"); // Get & Head Only
            _headers.Add("Connection", "close");
            _headers.Add("Server", ServerName);
            _headers.Add("Content-Encoding", "utf-8");
            _headers.Add("Content-Language", System.Globalization.CultureInfo.InstalledUICulture.ToString().ToLower().Substring(0, 2));
            _headers.Add("Access-Control-Allow-Headers", "Accept, WebApp"); // Range, Accept-Language, Content-Language, Content-Type
            _headers.Add("Access-Control-Max-Age", "900"); // 15 min
            _headers.Add("Access-Control-Allow-Methods", "GET, HEAD, POST, OPTIONS");
            _headers.Add("Access-Control-Allow-Origin", "*");
            _headers.Add("Access-Control-Allow-Credentials", "same-origin");
            _headers.Add("Vary", "Origin");            
            _headers.Add("X-Robots-Tag", "noindex, nofollow, noarchive");
        }

        public string GetStringParam(ClientRequest Request, string key, string defaultValue)
        {
            return ((Request.GetParams == null) || (!Request.GetParams.ContainsKey(key)) || (string.IsNullOrEmpty(Request.GetParams[key]))) ? defaultValue : Request.GetParams[key];
        }

        public int GetIntParam(ClientRequest Request, string key, int defaultValue)
        {
            if ((Request.GetParams == null) || (!Request.GetParams.ContainsKey(key)) || (string.IsNullOrEmpty(Request.GetParams[key]))) return defaultValue;
            int res = 0;
            if (int.TryParse(Request.GetParams[key], out res)) return res;
            return defaultValue;
        }

        public bool GetBoolParam(ClientRequest Request, string key, bool defaultValue)
        {
            return ((Request.GetParams == null) || (!Request.GetParams.ContainsKey(key)) || (string.IsNullOrEmpty(Request.GetParams[key]))) ? defaultValue : Request.GetParams[key] == "1";
        }

        /// <summary>
        ///     Get HTTP Client Request, threaded (thread per client)
        ///     -- connection to client will be closed after return --
        ///     -- do not call base method if response any data --
        /// </summary>
        /// <param name="Request"></param>
        protected override void GetClientRequest(ClientRequest Request)
        {
            //if(Request.Authorization == "Bearer BwUE1zBuBkugRENIEnHvKIk7lhV1u8MiWqWiOcsYPZPjl8oq")
            //{
            //    HttpWebRequest wr = (HttpWebRequest)HttpWebRequest.Create("http://localhost:8800/api/v1/pay/buy/");
            //    wr.Headers.Add("Authorization", "Bearer BwUE1zBuBkugRENIEnHvKIk7lhV1u8MiWqWiOcsYPZPjl8oq");
            //    wr.ContentType = Request.Headers["Content-Type"];
            //    wr.Method = "POST";
            //    wr.ContentLength = Request.BodyData.Length;
            //    using (Stream reqstr = wr.GetRequestStream())
            //        reqstr.Write(Request.BodyData, 0, Request.BodyData.Length);
            //    try
            //    {
            //        HttpWebResponse wres = (HttpWebResponse)wr.GetResponse();
            //    }
            //    catch { };
            //};

            // Not GET/HEAD
            if ((Request.Method != "HEAD") & (Request.Method != "GET") & (Request.Method != "POST") & (Request.Method != "PUT") & (Request.Method != "DELETE"))
            {
                ApplicationLog.WriteDatedLn(String.Format("Blocking {0}: {1}", Request.RemoteIP, "Invalid HTTP Method"));
                HttpClientSendError(Request.Client, 501); // Not Implemented
                return;
            };

            // Non-local clients
            if ((AllowOnlyLocalhost) && (!Request.IsLocal))
            {
                ApplicationLog.WriteDatedLn(String.Format("Blocking, cause AllowOnlyLocalhost {0}: {1}", Request.RemoteIP, "Remote Client"));
                HttpClientSendError(Request.Client, 403); // Forbidden
                return;
            };

            // Logging
            ApplicationLog.WriteDatedLn(String.Format("Query from {2}{0}[{3}]: {1} -- {4}", Request.RemoteIP, Request.Query, "", Request.clientID, string.IsNullOrEmpty(Request.UserAgent) ? "" : Request.UserAgent));

            // XML or JSON?
            bool isXML = false;
            bool isJSON = false;
            if (Request.Headers.ContainsKey("accept") && (Request.Headers["accept"] == "application/xml")) isXML = true;
            if (Request.Headers.ContainsKey("Accept") && (Request.Headers["Accept"] == "application/xml")) isXML = true;
            if (Request.Headers.ContainsKey("accept") && (Request.Headers["accept"] == "application/json")) isXML = true;
            if (Request.Headers.ContainsKey("Accept") && (Request.Headers["Accept"] == "application/json")) isXML = true;

            ApplicationLog.WriteQuery(Request.Dump());            

            if (onClientRequest != null)
            {
                try { onClientRequest(Request); }
                catch (Exception ex) { ApplicationLog.WriteDatedLn($"Error: {ex.Message}"); };
            };

            Dictionary<string, string> dopHeaders = ParseHeaders(ResponseHeaders, out _);

            // Empty Request
            if ((AllowEmptyRequest) && (Request.Query == "/")) // Page
            {
                ApplicationLog.WriteDatedLn(String.Format("Response (by Empty Resuest) {2} to {0}[{3}]: {1}", Request.RemoteIP, Request.Query, "200", Request.clientID));

                int code = 200;
                string CodeStr = code.ToString() + " " + ((HttpStatusCode)code).ToString();
                string body = ServerName + $" is running, {DateTime.Now}";
                byte[] data = Encoding.UTF8.GetBytes(body);
                ApplicationLog.WriteResponse(String.Format("RESPONSE TO {0}[{3}]: {1}\r\n{4}{2}\r\n",
                   Request.RemoteIP, Request.Query, body, Request.clientID,
                   GetStandardHeaders(code, dopHeaders, data.Length, "text/html")));

                HttpClientSendData(Request.Client, _responseEnc.GetBytes(body), dopHeaders, code);
                return;
            };

            // Default            
            if (string.IsNullOrEmpty(DefaultResponseText))
            {
                ApplicationLog.WriteDatedLn(String.Format("Response {2} to {0}[{3}]: {1}", Request.RemoteIP, Request.Query, DefaultResponseCode, Request.clientID));
                
                string CodeStr = DefaultResponseCode.ToString() + " " + ((HttpStatusCode)DefaultResponseCode).ToString();
                string body = "<html><body><h1>" + CodeStr + "</h1></body></html>";
                byte[] data = Encoding.UTF8.GetBytes(body);
                ApplicationLog.WriteResponse(String.Format("RESPONSE TO {0}[{3}]: {1}\r\n{4}{2}\r\n",
                   Request.RemoteIP, Request.Query, body, Request.clientID,
                   GetStandardHeaders(DefaultResponseCode, dopHeaders, data.Length, "text/html")));
                
                HttpClientSendError(Request.Client, DefaultResponseCode, dopHeaders);
            }
            else
            {
                string contentType = "text/plain; charset=utf-8";
                if (!string.IsNullOrEmpty(DefaultContentType)) contentType = DefaultContentType;
                string rt = DefaultResponseText.Replace("\r\n", "").Replace("\t"," ");
                if (rt.Length > 40) rt = rt.Substring(0, 40) + $" ... [{DefaultResponseText.Length}]";
                byte[] data = Encoding.UTF8.GetBytes(DefaultResponseText);
                
                ApplicationLog.WriteDatedLn(String.Format("Response to {0}[{3}]: {1} -- {2}", 
                    Request.RemoteIP, Request.Query, rt, Request.clientID));
                
                ApplicationLog.WriteResponse(String.Format("RESPONSE TO {0}[{3}]: {1}\r\n{4}{2}\r\n",
                    Request.RemoteIP, Request.Query, DefaultResponseText, Request.clientID,
                    GetStandardHeaders(DefaultResponseCode, dopHeaders, data.Length, contentType)));
                
                HttpClientSendData(Request.Client, data, dopHeaders, DefaultResponseCode, contentType);
            };

            // call base method if not specified, 501
            // base.GetClientRequest(Request);
        }
        
        #region SendResponse

        public static Dictionary<string,string> ParseHeaders(string headers, out string normallized)
        {
            normallized = "";
            if (string.IsNullOrEmpty(headers) || !headers.Contains(":")) return null;
            Dictionary<string, string> res = new Dictionary<string, string>();
            string[] lines = headers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines)
            {
                string ln = line.Trim();
                if (string.IsNullOrEmpty(ln)) continue;
                if (!char.IsLetterOrDigit(ln[0])) continue;
                string[] kv = ln.Split(new char[] { ':' }, 2);
                if(kv.Length != 2) continue;
                string k = kv[0].Trim();
                string v = kv[1].Trim();
                if (!res.ContainsKey(k))
                {
                    res.Add(k, v);
                    normallized += $"{k}: {v}\r\n";
                };
            };
            return res;
        }

        private string GetStandardHeaders(int ResponseCode, Dictionary<string,string> dopHeaders, int bodylen, string ContentType)
        {

            string header = "HTTP/1.1 " + ResponseCode.ToString() + "\r\n";

            // Main Headers
            this._headers_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                header += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._headers_mutex.ReleaseMutex();

            // Dop Headers
            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    header += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);

            if (!DictHasKeyIgnoreCase(dopHeaders, "Content-type"))
                header += "Content-type: " + ContentType + "\r\n";
            if (!DictHasKeyIgnoreCase(dopHeaders, "Content-Length"))
                header += "Content-Length: " + bodylen.ToString() + "\r\n";
            header += "\r\n";
            return header;
        }

        private void SendResponse(ClientRequest request, int code, byte[] data, string contentType)
        {
            Dictionary<string, string> dh = new Dictionary<string, string>();
            dh.Add("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            dh.Add("Last-Modified", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            HttpClientSendData(request.Client, data, dh, code, contentType);
            ApplicationLog.WriteDatedLn(String.Format("Response {3} to {0}[{4}]: {1}, {2}", request.RemoteIP, contentType, FileSys.BytesToString(data.Length), code, request.clientID));
        }

        private void SendResponse(ClientRequest request, int code, string data, string contentType)
        {
            Dictionary<string, string> dh = new Dictionary<string, string>();
            dh.Add("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            dh.Add("Last-Modified", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            byte[] udata = Encoding.UTF8.GetBytes(data);
            HttpClientSendData(request.Client, udata, dh, code, contentType);
            ApplicationLog.WriteDatedLn(String.Format("Response {3} to {0}[{4}]: {1}, {2}", request.RemoteIP, contentType, FileSys.BytesToString(udata.Length), code, request.clientID));
        }

        private void SendResponse(ClientRequest request, int code, string data, string contentType, IDictionary<string, string> dopHeaders)
        {
            Dictionary<string, string> dh = new Dictionary<string, string>();
            if (dopHeaders != null) foreach (KeyValuePair<string, string> kvp in dopHeaders) dh.Add(kvp.Key, kvp.Value);
            dh.Add("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            dh.Add("Last-Modified", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            byte[] udata = Encoding.UTF8.GetBytes(data);
            HttpClientSendData(request.Client, udata, dh, code, contentType);
            ApplicationLog.WriteDatedLn(String.Format("Response {3} to {0}[{4}]: {1}, {2}", request.RemoteIP, contentType, FileSys.BytesToString(udata.Length), code, request.clientID));
        }

        private void SendResponse(ClientRequest request, byte[] body, IDictionary<string, string> dopHeaders, int ResponseCode, string ContentType)
        {
            Dictionary<string, string> dh = new Dictionary<string, string>();
            if (dopHeaders != null) foreach (KeyValuePair<string, string> kvp in dopHeaders) dh.Add(kvp.Key, kvp.Value);
            dh.Add("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            dh.Add("Last-Modified", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            HttpClientSendData(request.Client, body, dh, ResponseCode, ContentType);
            ApplicationLog.WriteDatedLn(String.Format("Response {3} to {0}[{4}]: {1}, {2}", request.RemoteIP, ContentType, FileSys.BytesToString(body.Length), ResponseCode, request.clientID));
        }

        private void SendResponse(ClientRequest request, int code, DateTime dt, string data, string contentType)
        {
            Dictionary<string, string> dh = new Dictionary<string, string>();
            dh.Add("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            dh.Add("Last-Modified", dt.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            byte[] udata = Encoding.UTF8.GetBytes(data);
            HttpClientSendData(request.Client, udata, dh, code, contentType);
            ApplicationLog.WriteDatedLn(String.Format("Response {3} to {0}[{4}]: {1}, {2}", request.RemoteIP, contentType, FileSys.BytesToString(udata.Length), code, request.clientID));
        }

        private void SendResponse(ClientRequest request, int code, DateTime dt, string data, string contentType, IDictionary<string, string> dopHeaders)
        {
            Dictionary<string, string> dh = new Dictionary<string, string>();
            if (dopHeaders != null) foreach (KeyValuePair<string, string> kvp in dopHeaders) dh.Add(kvp.Key, kvp.Value);
            dh.Add("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            dh.Add("Last-Modified", dt.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT");
            byte[] udata = Encoding.UTF8.GetBytes(data);
            HttpClientSendData(request.Client, udata, dh, code, contentType);
            ApplicationLog.WriteDatedLn(String.Format("Response {3} to {0}[{4}]: {1}, {2}", request.RemoteIP, contentType, FileSys.BytesToString(udata.Length), code, request.clientID));
        }

        #endregion SendResponse

        /// <summary>
        ///     On Error
        /// </summary>
        /// <param name="Client">Client Socket Connection</param>
        /// <param name="id"></param>
        /// <param name="error"></param>
        protected override void onError(TcpClient Client, ulong id, Exception error)
        {
            // if (error is IOException) return; // bad connection
            if ((error.InnerException != null) && (error.InnerException is SocketException)) return; // bad connection
            if (error is SocketException) return; // bad connection

            try { ApplicationLog.WriteDatedLn(String.Format("Error {2} on {0}[{3}]: {1}", ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString(), error.ToString(), 500, id)); } catch { };
            try { HttpClientSendError(Client, 500); } catch { };
        }

        /// <summary>
        ///     Pass File or Browse Folder(s) to HTTP Client by Request
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="HomeDirectory">Home Directory with files to Browse</param>
        /// <param name="subPath">Sub Path to Extract from URL Path</param>
        protected override void PassFileToClientByRequest(ClientRequest Request, string HomeDirectory, string subPath)
        {
            if (String.IsNullOrEmpty(HomeDirectory))
            {
                HttpClientSendError(Request.Client, 403);
                ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 403, Request.RemoteIP, Request.clientID));
                return;
            };
            if (!_allowGetFiles)
            {
                HttpClientSendError(Request.Client, 403);
                ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 403, Request.RemoteIP, Request.clientID));
                return;
            };
            if (String.IsNullOrEmpty(Request.Query))
            {
                HttpClientSendError(Request.Client, 400);
                ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 400, Request.RemoteIP, Request.clientID));
                return;
            };
            if (String.IsNullOrEmpty(Request.Page))
            {
                HttpClientSendError(Request.Client, 403);
                ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 403, Request.RemoteIP, Request.clientID));
                return;
            };
            if ((Request.QueryParams != null) && (Request.QueryParams.Count > 0))
            {
                HttpClientSendError(Request.Client, 400);
                ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 400, Request.RemoteIP, Request.clientID));
                return;
            };

            string path = Request.Page;
            if (!String.IsNullOrEmpty(subPath))
            {
                int i = path.IndexOf(subPath);
                if (i >= 0) path = path.Remove(i, subPath.Length);
            };
            path = path.Replace("/", @"\");
            if (path.IndexOf("/./") >= 0)
            {
                HttpClientSendError(Request.Client, 400);
                ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 400, Request.RemoteIP, Request.clientID));
                return;
            };
            if (path.IndexOf("/../") >= 0)
            {
                HttpClientSendError(Request.Client, 400);
                ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 400, Request.RemoteIP, Request.clientID));
                return;
            };
            if (path.IndexOf("/.../") >= 0)
            {
                HttpClientSendError(Request.Client, 400);
                ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 400, Request.RemoteIP, Request.clientID));
                return;
            };
            path = HomeDirectory + @"\" + path;
            while (path.IndexOf(@"\\") > 0) path = path.Replace(@"\\", @"\");
            string fName = System.IO.Path.GetFileName(path);
            string dName = System.IO.Path.GetDirectoryName(path);
            if ((String.IsNullOrEmpty(dName)) && (String.IsNullOrEmpty(fName)) && (path.EndsWith(@":\")) && (Path.IsPathRooted(path))) dName = path;
            if (!String.IsNullOrEmpty(fName))
            {
                if (!File.Exists(path))
                {
                    HttpClientSendError(Request.Client, 404);
                    ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 404, Request.RemoteIP, Request.clientID));
                    return;
                }
                else
                {
                    List<string> disallowExt = new List<string>(AllowNotFileExt);
                    FileInfo fi = new FileInfo(path);
                    string fExt = fi.Extension.ToLower();
                    if (disallowExt.Contains(fExt))
                    {
                        HttpClientSendError(Request.Client, 403);
                        ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 403, Request.RemoteIP, Request.clientID));
                        return;
                    }
                    else
                    {
                        if (fi.Length > _MaxFileDownloadSize)
                        {
                            HttpClientSendError(Request.Client, 509, String.Format("509 File is too big - {0}, limit - {1}", ToFileSize(fi.Length), ToFileSize(_MaxFileDownloadSize)));
                            ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]: File is too big", 509, Request.RemoteIP, Request.clientID));
                        }
                        else
                        {
                            long nlen = HttpClientSendFile(Request, path, null, 200, null);
                            ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{4}]: {3}, {2}", 200, Request.RemoteIP, FileSys.BytesToString(nlen), Path.GetFileName(path), Request.clientID));
                        };
                        return;
                    };
                };
            }
            else if (!String.IsNullOrEmpty(dName))
            {
                if (!Directory.Exists(path))
                {
                    HttpClientSendError(Request.Client, 404);
                    ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 404, Request.RemoteIP, Request.clientID));
                    return;
                }
                else
                {
                    // load default file (index., ui.)
                    {
                        // index
                        List<string> files = new List<string>(Directory.GetFiles(path, "index.*", SearchOption.TopDirectoryOnly));
                        foreach (string file in files)
                        {
                            switch (Path.GetExtension(file).ToLower())
                            {
                                case ".htm":
                                case ".html":
                                case ".dhtml":
                                case ".htmlx":
                                case ".xhtml":
                                case ".txt":
                                case ".md":
                                    long nlen = HttpClientSendFile(Request, file, null, 200, null);
                                    ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{4}]: {3}, {2}", 200, Request.RemoteIP, FileSys.BytesToString(nlen), Path.GetFileName(file), Request.clientID));
                                    return;
                                default: break;
                            };
                        };
                        // ui
                        files = new List<string>(Directory.GetFiles(path, "ui.*htm*", SearchOption.TopDirectoryOnly));
                        foreach (string file in files)
                        {
                            long nlen = HttpClientSendFile(Request, file, null, 200, null);
                            ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{4}]: {3}, {2}", 200, Request.RemoteIP, FileSys.BytesToString(nlen), Path.GetFileName(file), Request.clientID));
                            return;
                        };
                    };
                    if (!_allowGetDirs)
                    {
                        HttpClientSendError(Request.Client, 403);
                        ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 403, Request.RemoteIP, Request.clientID));
                        return;
                    }
                    else
                    {
                        string html = "<html><body>";
                        if (_allowListDirs)
                        {
                            html += String.Format("<a href=\"{0}/\"><b> {0} </b></a><br/>\n\r", "..");
                            string[] dirs = Directory.GetDirectories(path);
                            if (dirs != null) Array.Sort<string>(dirs);
                            foreach (string dir in dirs)
                            {
                                DirectoryInfo di = new DirectoryInfo(dir);
                                if ((di.Attributes & FileAttributes.Hidden) > 0) continue;
                                string sPath = dir.Substring(dir.LastIndexOf(@"\") + 1);
                                html += String.Format("<a href=\"{1}/\"><b>{0}</b></a><br/>\n\r", sPath, UrlEscape(sPath));
                            };
                        };
                        {
                            List<string> disallowExt = new List<string>(AllowNotFileExt);
                            string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                            if (files != null) Array.Sort<string>(files);
                            foreach (string file in files)
                            {
                                FileInfo fi = new FileInfo(file);
                                if (disallowExt.Contains(fi.Extension.ToLower())) continue;
                                if ((fi.Attributes & FileAttributes.Hidden) > 0) continue;
                                if ((!_allowListBigFiles) && (fi.Length > _MaxFileDownloadSize)) continue;
                                string sPath = Path.GetFileName(file);
                                html += String.Format("<a href=\"{1}\">{0}</a> - <span style=\"color:gray;\">{2}</span>, <span style=\"color:silver;\">MDF: {3}</span><br/>\n\r", sPath, UrlEscape(sPath), ToFileSize(fi.Length), fi.LastWriteTime);
                            };
                        };
                        html += "</body></html>";
                        HttpClientSendText(Request.Client, html);
                        ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]: list directory", 200, Request.RemoteIP, Request.clientID));
                        return;
                    };
                };
            };
            HttpClientSendError(Request.Client, 400);
            ApplicationLog.WriteDatedLn(String.Format("Response {0} to {1}[{2}]", 400, Request.RemoteIP, Request.clientID));
        }
    }
}
