//
// C# (.Net Framework)
// IncominHttpServer
// https://github.com/dkxce/CSharpBaseUsageClasses
// en,ru,1251,utf-8
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace IncominHttpServer
{
    /// <summary>
    ///     Server State
    /// </summary>
    public enum ServerState
    {
        ssStopped = 0,
        ssStarting = 1,
        ssRunning = 2,
        ssStopping = 3
    }

    /// <summary>
    ///     Базовые методы серверов
    /// </summary>
    public interface IServer
    {
        void Start();
        void Stop();
        ServerState GetState();
        int GetServerPort();
        Exception GetLastError();
    }

    /// <summary>
    ///     Базовый класс для TCP серверов
    /// </summary>
    public abstract class TTCPServer : IServer, IDisposable
    {
        protected Thread mainThread = null;
        protected TcpListener mainListener = null;

        /// <summary>
        ///     Listen IP Address
        /// </summary>
        protected IPAddress ListenIP = IPAddress.Any;
        /// <summary>
        ///     Listen IP Address
        /// </summary>
        public IPAddress ServerIP { get { return ListenIP; } }

        /// <summary>
        ///     Listen Port Number
        /// </summary>
        protected int ListenPort = 5000;
        /// <summary>
        ///     Listen Port Number
        /// </summary>
        public int ServerPort { get { return ListenPort; } set { if (isRunning) throw new Exception("Server is running"); ListenPort = value; } }
        /// <summary>
        ///     Listen Port Number
        /// </summary>
        /// <returns></returns>
        public int GetServerPort() { return ListenPort; }

        /// <summary>
        ///     Server is running or not
        /// </summary>
        protected bool isRunning = false;
        /// <summary>
        ///     Server is running or not
        /// </summary>
        public bool Running { get { return isRunning; } }
        /// <summary>
        ///     Get Servers State
        /// </summary>
        /// <returns></returns>
        public ServerState GetState() { if (isRunning) return ServerState.ssRunning; else return ServerState.ssStopped; }

        /// <summary>
        ///     Last Thread Error
        /// </summary>
        protected Exception LastError = null;
        /// <summary>
        ///     Last Thread Error
        /// </summary>
        public Exception GetLastError() { return LastError; }

        /// <summary>
        ///     Last Error Time
        /// </summary>
        protected DateTime LastErrTime = DateTime.MaxValue;
        /// <summary>
        ///     Last Error Time
        /// </summary>
        public DateTime LastErrorTime { get { return LastErrTime; } }

        /// <summary>
        ///     Get Total Error Count
        /// </summary>
        protected uint ErrorsCounter = 0;
        /// <summary>
        ///     Get Total Error Count
        /// </summary>
        public uint GetErrorsCount { get { return ErrorsCounter; } }

        /// <summary>
        ///     Total Clients Counter
        /// </summary>
        protected ulong counter = 0;
        /// <summary>
        ///     Total Clients Counter
        /// </summary>
        public ulong ClientsCounter { get { return counter; } }

        /// <summary>
        ///     Connected clients counter
        /// </summary>
        protected ulong alive = 0;
        /// <summary>
        ///     Connected clients counter
        /// </summary>
        public ulong ClientsAlive { get { return alive; } }

        /// <summary>
        ///     Client Read Timeout in seconds, default 10
        /// </summary>
        protected int readTimeout = 10; // 10 sec
        /// <summary>
        ///     Client Read Timeout in seconds, default 10
        /// </summary>
        public int ReadTimeout { get { return readTimeout; } set { readTimeout = value; } }

        public TTCPServer() { }
        public TTCPServer(int Port) { this.ListenPort = Port; }
        public TTCPServer(IPAddress IP, int Port) { this.ListenIP = IP; this.ListenPort = Port; }

        /// <summary>
        ///     Start Server
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        ///     Stop Server
        /// </summary>
        public virtual void Stop() { }

        /// <summary>
        ///     Stop Server and Dipose
        /// </summary>
        public virtual void Dispose() { this.Stop(); }

        /// <summary>
        ///     Accept TCP Client
        /// </summary>
        /// <param name="client"></param>
        /// <returns>true - connect, false - ignore</returns>
        protected virtual bool AcceptClient(TcpClient client) { return true; }

        /// <summary>
        ///     Get Client Connection
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="clientID">Client Number</param>
        protected virtual void GetClient(TcpClient Client, ulong clientID) { }

        /// <summary>
        ///     On Error
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="clientID">Client Number</param>
        /// <param name="error"></param>
        protected virtual void onError(TcpClient Client, ulong clientID, Exception error) { throw error; }

        /// <summary>
        ///     Is Connection Alive?
        /// </summary>
        /// <param name="Client"></param>
        /// <returns></returns>
        public static bool IsConnected(TcpClient Client)
        {
            if (!Client.Connected) return false;
            if (Client.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];
                try
                {
                    if (Client.Client.Receive(buff, SocketFlags.Peek) == 0)
                        return false;
                }
                catch
                {
                    return false;
                };
            };
            return true;
        }

        /// <summary>
        ///     Get Client HTTP Headers
        /// </summary>
        /// <param name="Header">client header</param>
        /// <returns>header list</returns>
        public static IDictionary<string, string> GetClientHeaders(string Header)
        {
            if (String.IsNullOrEmpty(Header)) return null;

            Dictionary<string, string> clHeaders = new Dictionary<string, string>();
            Regex rx = new Regex(@"([\w-]+): (.*)", RegexOptions.IgnoreCase);
            try
            {
                MatchCollection mc = rx.Matches(Header);
                foreach (Match mx in mc)
                {
                    string val = mx.Groups[2].Value.Trim();
                    if (!clHeaders.ContainsKey(mx.Groups[1].Value))
                        clHeaders.Add(mx.Groups[1].Value, val);
                    else
                        clHeaders[mx.Groups[1].Value] += val;
                };
            }
            catch { };
            return clHeaders;
        }

        /// <summary>
        ///     Get client HTTP query
        /// </summary>
        /// <param name="Header">header</param>
        /// <param name="host">server:ip</param>
        /// <param name="page">server page</param>
        /// <param name="sParameters">string params</param>
        /// <param name="lParameters">list params</param>
        /// <returns>query string</returns>
        public static string GetClientQuery(string Header, out string host, out string page, out string sParameters, out IDictionary<string, string> lParameters)
        {
            host = null;
            Regex rx = new Regex(@"Host: (.*)", RegexOptions.IgnoreCase);
            Match mx = rx.Match(Header);
            if (mx.Success) host = mx.Groups[1].Value.Trim();

            page = null;
            lParameters = new Dictionary<string, string>();
            sParameters = null;
            if (String.IsNullOrEmpty(Header)) return null;

            string query = "";
            rx = new Regex("^(?:PUT|POST|GET|HEAD) (.*) HTTP", RegexOptions.IgnoreCase); // "^(?:PUT|POST|GET) (.*) HTTP" or @"^(?:PUT|POST|GET) (\/?[\w\.?=%&=\-@/$,]*) HTTP"
            mx = rx.Match(Header);
            if (mx.Success) query = mx.Groups[1].Value; // UrlUnescape(mx.Groups[1].Value);
            if (query != null)
            {
                rx = new Regex(@"^(?<page>[\[\]+!_\(\)\s\w\.=%=\-@/$,]*)?", RegexOptions.None);
                mx = rx.Match(query);
                if (mx.Success) page = UrlUnescape(mx.Groups["page"].Value);

                rx = new Regex(@"(?:[\?&](.*))", RegexOptions.None);
                mx = rx.Match(query);
                if (mx.Success) sParameters = mx.Groups[1].Value;

                rx = new Regex(@"([\?&]((?<name>[^&=]+)=(?<value>[^&=]+)))", RegexOptions.None);
                MatchCollection mc = rx.Matches(query);
                if (mc.Count > 0)
                {
                    //lParameters = new Dictionary<string, string>();
                    foreach (Match m in mc)
                    {
                        string n = UrlUnescape(m.Groups["name"].Value);
                        string v = UrlUnescape(m.Groups["value"].Value);
                        if (lParameters.ContainsKey(n))
                            lParameters[n] += "," + v;
                        else
                            lParameters.Add(n, v);
                    };
                };
            };
            return query;
        }

        /// <summary>
        ///     Get client GET/POST Query Parameters
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public static IDictionary<string, string> GetClientParams(string query)
        {
            if (String.IsNullOrEmpty(query)) return null;
            Dictionary<string, string> lParameters = new Dictionary<string, string>();

            Regex rx = new Regex(@"([\?&]*((?<name>[^&=]+)=(?<value>[^&=]+)))", RegexOptions.None);
            MatchCollection mc = rx.Matches(query);
            if (mc.Count > 0)
            {
                lParameters = new Dictionary<string, string>();
                foreach (Match m in mc)
                {
                    string n = UrlUnescape(m.Groups["name"].Value);
                    string v = UrlUnescape(m.Groups["value"].Value);
                    if (lParameters.ContainsKey(n))
                        lParameters[n] += "," + v;
                    else
                        lParameters.Add(n, v);
                };
            };
            return lParameters;
        }

        /// <summary>
        ///     Dictionary has cas-ignored key
        /// </summary>
        /// <param name="dict">dictionary</param>
        /// <param name="key">key to find</param>
        /// <returns>has key</returns>
        public static bool DictHasKeyIgnoreCase(IDictionary<string, string> dict, string key)
        {
            if (dict == null) return false;
            if (dict.Count == 0) return false;
            foreach (string k in dict.Keys)
                if (string.Compare(k, key, true) == 0)
                    return true;
            return false;
        }

        /// <summary>
        ///     Get Dictionary Value by Key ignoring case
        /// </summary>
        /// <param name="dict">dictionary</param>
        /// <param name="key">key to find</param>
        /// <returns>key value</returns>
        public static string DictGetKeyIgnoreCase(IDictionary<string, string> dict, string key)
        {
            if (dict == null) return null;
            if (dict.Count == 0) return null;
            foreach (string k in dict.Keys)
                if (string.Compare(k, key, true) == 0)
                    return dict[k];
            return null;
        }

        /// <summary>
        ///     Encode string to base64
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        ///     Decode base64 to string
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        ///     Code string with key
        /// </summary>
        /// <param name="clearText"></param>
        /// <param name="EncryptionKey"></param>
        /// <returns></returns>
        public static string CodeInString(string clearText, string EncryptionKey)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (System.Security.Cryptography.SymmetricAlgorithm encryptor = System.Security.Cryptography.SymmetricAlgorithm.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        /// <summary>
        ///     decode string with key
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="EncryptionKey"></param>
        /// <returns></returns>
        public static string CodeOutString(string cipherText, string EncryptionKey)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (System.Security.Cryptography.SymmetricAlgorithm encryptor = System.Security.Cryptography.SymmetricAlgorithm.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string ToFileSize(double value)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (value <= (Math.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(value /
                        Math.Pow(1024, i)) +
                        " " + suffixes[i];
                };
            };
            return ThreeNonZeroDigits(value / Math.Pow(1024, suffixes.Length - 1)) + " " + suffixes[suffixes.Length - 1];
        }

        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }

        protected static char Get1251Char(byte b)
        {
            return (System.Text.Encoding.GetEncoding(1251).GetString(new byte[] { b }, 0, 1))[0];
        }

        public static string UrlEscape(string str)
        {
            return System.Uri.EscapeDataString(str.Replace("+", "%2B"));
        }

        public static string UrlUnescape(string str)
        {
            return System.Uri.UnescapeDataString(str).Replace("%2B", "+");
        }

        /// <summary>
        ///     Получение папки, из которой запущено приложение
        /// </summary>
        /// <returns>Полный путь к папки с \ на конце</returns>
        public static string GetCurrentDir()
        {
            string fname = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            fname = fname.Replace("file:///", "");
            fname = fname.Replace("/", @"\");
            fname = fname.Substring(0, fname.LastIndexOf(@"\") + 1);
            return fname;
        }

        public static string GetMimeType(string fileExt)
        {
            // https://snipp.ru/handbk/mime-list
            // https://docs.w3cub.com/http/basics_of_http/mime_types/complete_list_of_mime_types.html
            switch (fileExt)
            {
                case ".aac": return "audio/aac";
                case ".abw": return "application/x-abiword";
                case ".arc": return "application/x-freearc";
                case ".azw": return "application/vnd.amazon.ebook";
                case ".bin": return "application/octet-stream";
                case ".bz": return "application/x-bzip";
                case ".bz2": return "BZip2 archive 	application/x-bzip2";
                case ".csh": return "application/x-csh";
                case ".eot": return "application/vnd.ms-fontobject";
                case ".epub": return "application/epub+zip";
                case ".gz": return "application/gzip";
                case ".ico": return "image/vnd.microsoft.icon";
                case ".ics": return "text/calendar";
                case ".jar": return "application/java-archive";
                case ".mid": return "audio/midi audio/x-midi";
                case ".midi": return "audio/midi audio/x-midi";
                case ".mjs": return "text/javascript";
                case ".mpkg": return "application/vnd.apple.installer+xml";
                case ".odp": return "application/vnd.oasis.opendocument.presentation";
                case ".ods": return "application/vnd.oasis.opendocument.spreadsheet";
                case ".odt": return "application/vnd.oasis.opendocument.text";
                case ".oga": return "audio/ogg";
                case ".ogv": return "video/ogg";
                case ".ogx": return "application/ogg";
                case ".opus": return "audio/opus";
                case ".otf": return "font/otf";
                case ".ppt": return "application/vnd.ms-powerpoint";
                case ".pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".rtf": return "application/rtf";
                case ".sh": return "application/x-sh";
                case ".swf": return "application/x-shockwave-flash";
                case ".tar": return "application/x-tar";
                case ".ts": return "video/mp2t";
                case ".ttf": return "font/ttf";
                case ".vsd": return "application/vnd.visio";
                case ".weba": return "WEBM audio 	audio/webm";
                case ".webm": return "video/webm";
                case ".webp": return "image/webp";
                case ".woff": return "font/woff";
                //case ".woff2": return "application/font-woff2";
                case ".woff2": return "font/woff2";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".xul": return "application/vnd.mozilla.xul+xml";
                case ".3gp": return "video/3gpp";
                case ".3g2": return "video/3gpp2";
                case ".7z": return "application/x-7z-compressed";
                case ".pdf": return "application/pdf";
                case ".djvu": return "image/vnd.djvu";
                case ".zip": return "application/zip";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                //case ".docx": return "application/msword";
                case ".mp3": return "audio/mpeg";
                case ".m3u": return "audio/x-mpegurl";
                case ".wav": return "audio/wav";
                //case ".wav": return "audio/x-wav";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".psd": return "image/vnd.adobe.photoshop";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".svg": return "image/svg+xml";
                //case ".svg": return "image/svg";
                case ".tif": return "image/tiff";
                case ".tiff": return "image/tiff";
                case ".css": return "text/css";
                case ".csv": return "text/csv";
                case ".htm": return "text/html; charset=utf-8";
                case ".html": return "text/html; charset=utf-8";
                case ".htmlx": return "text/html; charset=utf-8";
                case ".dhtml": return "text/html; charset=utf-8";
                case ".xhtml": return "application/xhtml+xml; charset=utf-8";
                //case ".xhtml": return "text/html";
                //case ".js": return "text/javascript";
                case ".js": return "application/javascript";
                case ".jsonld": return "application/ld+json";
                case ".json": return "application/json";
                case ".txt": return "text/plain";
                case ".md": return "text/plain";
                case ".php": return "application/x-httpd-php";
                //case ".php":  return "text/php";
                //case ".xml": return "application/xml";
                case ".xml": return "text/xml; charset=utf-8";
                case ".mpg": return "video/mpeg";
                case ".mpeg": return "video/mpeg";
                case ".mp4": return "video/mp4";
                case ".ogg": return "video/ogg";
                case ".avi": return "video/x-msvideo";
                case ".rar": return "application/x-rar-compresse";
                default: return "application/octet-stream";
            };
        }

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);

        public static string GetMacAddressByIP(string ip)
        {
            IPAddress ip_adr = IPAddress.Parse(ip);
            byte[] ab = new byte[6];
            int len = ab.Length;
            int r = SendARP(ip_adr.GetHashCode(), 0, ab, ref len);
            return BitConverter.ToString(ab, 0, 6);
        }

        // GET AVAILABLE PORT //
        public static int GetAvailablePort(int startPortNum)
        {
            if (startPortNum < 0) return -1;
            if (startPortNum > ushort.MaxValue) return -1;

            List<int> busyPorts = new List<int>();
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            foreach (System.Net.IPEndPoint ipep in ipGlobalProperties.GetActiveTcpListeners()) // .GetActiveTcpConnections();
                busyPorts.Add(ipep.Port); // netstat -ano |find /i "listening"            

            int nextPort = startPortNum;
            while (nextPort <= ushort.MaxValue)
            {
                if (busyPorts.IndexOf(nextPort) < 0) // port is free
                    return nextPort;
                else // port is busy
                    nextPort++;
            };

            return -1;
        }

        // LOCAL IPs
        public static List<string> GetLocalIPs()
        {
            List<string> res = new List<string>();
            res.Add("127.0.0.1");
            try
            {
                IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress[] addr = ipEntry.AddressList;
                for (int i = 0; i < addr.Length; i++)
                    res.Add(addr[i].ToString());
            }
            catch { };
            return res;
        }
    }

    /// <summary>
    ///     Простейший однопоточный TCP-сервер
    /// </summary>
    public class SingledTCPServer : TTCPServer
    {
        /// <summary>
        ///     Close connection when GetClient method completes
        /// </summary>
        protected bool _closeOnGetClientCompleted = true;

        /// <summary>
        ///     Close connection when GetClient method completes
        /// </summary>
        public bool CloseConnectionOnGetClientCompleted { get { return _closeOnGetClientCompleted; } set { _closeOnGetClientCompleted = value; } }

        public SingledTCPServer() { }
        public SingledTCPServer(int Port) { this.ListenPort = Port; }
        public SingledTCPServer(IPAddress IP, int Port) { this.ListenIP = IP; this.ListenPort = Port; }
        ~SingledTCPServer() { this.Dispose(); }

        public override void Start()
        {
            if (isRunning) throw new Exception("Server Already Running!");

            try
            {
                mainListener = new TcpListener(this.ListenIP, this.ListenPort);
                mainListener.Start();
            }
            catch (Exception ex)
            {
                LastError = ex;
                ErrorsCounter++;
                throw ex;
            };

            mainThread = new Thread(MainThread);
            mainThread.Start();
        }

        public override void Stop()
        {
            if (!isRunning) return;

            isRunning = false;

            if (mainListener != null) mainListener.Stop();
            mainListener = null;

            mainThread.Join();
            mainThread = null;
        }

        private void MainThread()
        {
            isRunning = true;
            while (isRunning)
            {
                try
                {
                    TcpClient client = mainListener.AcceptTcpClient();
                    if (client == null) continue;

                    if (!AcceptClient(client))
                    {
                        client.Client.Close();
                        client.Close();
                        continue;
                    };

                    ulong id = 0;
                    try
                    {
                        alive++;
                        client.GetStream().ReadTimeout = this.readTimeout * 1000;
                        GetClient(client, id = this.counter++);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ErrorsCounter++;
                            LastError = ex;
                            onError(client, id, ex);
                        }
                        catch { };
                    };
                    if (_closeOnGetClientCompleted)
                        CloseClient(client, id);
                }
                catch (Exception ex)
                {
                    LastError = ex;
                    ErrorsCounter++;
                };
                Thread.Sleep(1);
            };
        }

        protected override void GetClient(TcpClient Client, ulong clientID)
        {
            //
            // do something
            //

            if (!this._closeOnGetClientCompleted) CloseClient(Client, clientID);
        }

        protected void CloseClient(TcpClient Client, ulong clientID)
        {
            try
            {
                alive--;
                Client.Client.Close();
                Client.Close();
            }
            catch { };
        }
    }

    /// <summary>
    ///     Простейший однопоточный TCP-сервер, который принимает текст
    /// </summary>
    public class SingledTextTCPServer : SingledTCPServer
    {
        public SingledTextTCPServer() : base() { }
        public SingledTextTCPServer(int Port) : base(Port) { }
        public SingledTextTCPServer(IPAddress IP, int Port) : base(IP, Port) { }
        ~SingledTextTCPServer() { this.Dispose(); }

        protected bool _OnlyHTTP = false;
        public virtual bool OnlyHTTPClients { get { return _OnlyHTTP; } set { _OnlyHTTP = value; } }
        protected uint _MaxHeaderSize = 4096;
        public uint MaxClientHeaderSize { get { return _MaxHeaderSize; } set { _MaxHeaderSize = value; } }
        protected uint _MaxBodySize = 65536;
        public uint MaxClientBodySize { get { return _MaxBodySize; } set { _MaxBodySize = value; } }
        protected Encoding _responseEnc = Encoding.GetEncoding(1251);
        public Encoding ResponseEncoding { get { return _responseEnc; } set { _responseEnc = value; } }
        protected Encoding _requestEnc = Encoding.GetEncoding(1251);
        public Encoding RequestEncoding { get { return _requestEnc; } set { _requestEnc = value; } }

        protected override void GetClient(TcpClient Client, ulong clientID)
        {
            Regex CR = new Regex(@"Content-Length: (\d+)", RegexOptions.IgnoreCase);

            string Request = "";
            string Header = null;
            List<byte> Body = new List<byte>();

            int bRead = -1;
            int posCRLF = -1;
            int receivedBytes = 0;
            int contentLength = 0;

            // Get Header
            //while ((Client.Available > 0) && ((bRead = Client.GetStream().ReadByte()) >= 0)) // doesn't work correct
            while ((bRead = Client.GetStream().ReadByte()) >= 0)
            {
                receivedBytes++;
                Body.Add((byte)bRead);

                // Check GET or POST
                if (_OnlyHTTP && (receivedBytes == 1))
                    if ((bRead != 0x47) && (bRead != 0x50))
                    {
                        if (!this._closeOnGetClientCompleted) CloseClient(Client, clientID);
                        return;
                    };

                Request += (char)bRead; // standard symbol
                if (bRead == 0x0A) posCRLF = Request.IndexOf("\r\n\r\n"); // get body start index
                if (posCRLF >= 0 || Request.Length > _MaxHeaderSize) { break; }; // GET ONLY                    
            };

            bool valid = (posCRLF > 0);
            if ((!valid) && _OnlyHTTP)
            {
                if (!this._closeOnGetClientCompleted) CloseClient(Client, clientID);
                return;
            };

            // Get Body
            if (valid)
            {
                Body.Clear();
                Header = Request;
                Match mx = CR.Match(Request);
                if (mx.Success) contentLength = int.Parse(mx.Groups[1].Value);
                int total2read = posCRLF + 4 + contentLength;
                while ((receivedBytes < total2read) && ((bRead = Client.GetStream().ReadByte()) >= 0))
                {
                    receivedBytes++;
                    Body.Add((byte)bRead);

                    string rcvd = _requestEnc.GetString(new byte[] { (byte)bRead }, 0, 1);
                    Request += rcvd;
                    if (Request.Length > _MaxBodySize) { break; };
                };
            };

            GetClientRequest(Client, clientID, Request, Header, Body.ToArray());
        }

        /// <summary>
        ///     Get Client with Request Text Data
        /// </summary>
        /// <param name="Client">socket</param>
        /// <param name="clientID">number</param>
        /// <param name="Request">Request</param>
        /// <param name="Header">Header</param>        
        /// <param name="Body">Body</param>
        /// <param name="BodyData">Body</param>
        protected virtual void GetClientRequest(TcpClient Client, ulong clientID, string Request, string Header, byte[] Body)
        {
            string proto = "tcp://" + Client.Client.RemoteEndPoint.ToString() + "/text/";

            //
            // do something
            //

            if (!this._closeOnGetClientCompleted) CloseClient(Client, clientID);
        }
    }

    /// <summary>
    ///     Простейший HTTP-сервер
    /// </summary>
    public class SingledHttpServer : SingledTextTCPServer
    {
        public SingledHttpServer() : base(80) { this._closeOnGetClientCompleted = true; this._OnlyHTTP = true; }
        public SingledHttpServer(int Port) : base(Port) { this._closeOnGetClientCompleted = true; this._OnlyHTTP = true; }
        public SingledHttpServer(IPAddress IP, int Port) : base(IP, Port) { this._closeOnGetClientCompleted = true; this._OnlyHTTP = true; }
        ~SingledHttpServer() { this.Dispose(); }

        protected Mutex _h_mutex = new Mutex();
        protected Dictionary<string, string> _headers = new Dictionary<string, string>();
        public Dictionary<string, string> Headers
        {
            get
            {
                _h_mutex.WaitOne();
                Dictionary<string, string> res = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in _headers)
                    res.Add(kvp.Key, kvp.Value);
                _h_mutex.ReleaseMutex();
                return res;
            }
            set
            {
                _h_mutex.WaitOne();
                _headers.Clear();
                foreach (KeyValuePair<string, string> kvp in value)
                    _headers.Add(kvp.Key, kvp.Value);
                _h_mutex.ReleaseMutex();
            }
        }

        public virtual void HttpClientSendError(TcpClient Client, int Code, Dictionary<string, string> dopHeaders)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            // Код простой HTML-странички
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 " + CodeStr + "\r\n";
            this._h_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._h_mutex.ReleaseMutex();
            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    Str += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            Str += "Content-type: text/html\r\nContent-Length: " + Html.Length.ToString() + "\r\n\r\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.GetEncoding(1251).GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            Client.GetStream().Flush();
            Client.Client.Close();
            Client.Close();
        }
        public virtual void HttpClientSendError(TcpClient Client, int Code)
        {
            HttpClientSendError(Client, Code, null);
        }
        public virtual void HttpClientSendText(TcpClient Client, string Text, Dictionary<string, string> dopHeaders)
        {
            // Код простой HTML-странички
            string body = "<html><body>" + Text + "</body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string header = "HTTP/1.1 200\r\n";

            this._h_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                header += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._h_mutex.ReleaseMutex();

            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    header += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);

            byte[] bData = _responseEnc.GetBytes(body);
            if (!DictHasKeyIgnoreCase(dopHeaders, "Content-type"))
                header += "Content-type: text/html\r\n";
            if (!DictHasKeyIgnoreCase(dopHeaders, "Content-Length"))
                header += "Content-Length: " + bData.Length.ToString() + "\r\n";
            header += "\r\n";

            List<byte> response = new List<byte>();
            response.AddRange(Encoding.GetEncoding(1251).GetBytes(header));
            response.AddRange(bData);

            Client.GetStream().Write(response.ToArray(), 0, response.Count);
            Client.GetStream().Flush();
            Client.Client.Close();
            Client.Close();
        }
        public virtual void HttpClientSendText(TcpClient Client, string Text)
        {
            HttpClientSendText(Client, Text, null);
        }

        protected override void GetClientRequest(TcpClient Client, ulong clientID, string Request, string Header, byte[] Body)
        {
            IDictionary<string, string> clHeaders = GetClientHeaders(Header);
            string page, host, inline;
            IDictionary<string, string> parameters;
            string query = GetClientQuery(Header, out host, out page, out inline, out parameters);
            HttpClientSendError(Client, 501);
            if (!this._closeOnGetClientCompleted) CloseClient(Client, clientID);
        }
    }

    /// <summary>
    ///     Многопоточный TCP-сервер
    /// </summary>
    public class ThreadedTCPServer : TTCPServer
    {
        private class ClientTCPSInfo
        {
            public ulong id;
            public TcpClient client;
            public Thread thread;

            public ClientTCPSInfo(TcpClient client, Thread thread)
            {
                this.client = client;
                this.thread = thread;
            }
        }

        /// <summary>
        ///     Working Mode
        /// </summary>
        public enum Mode : byte
        {
            /// <summary>
            ///     Allow all connections
            /// </summary>
            NoRules = 0,
            /// <summary>
            ///     Allow only specified connections
            /// </summary>
            AllowWhiteList = 1,
            /// <summary>
            ///     Allow all but black list
            /// </summary>
            DenyBlackList = 2
        }

        /// <summary>
        ///     Started
        /// </summary>
        private DateTime _started = DateTime.MinValue;
        /// <summary>
        ///     Started
        /// </summary>
        public DateTime Started { get { return isRunning ? _started : DateTime.MinValue; } }

        /// <summary>
        ///     Started
        /// </summary>
        private DateTime _stopped = DateTime.MaxValue;
        /// <summary>
        ///     Started
        /// </summary>
        public DateTime Stopped { get { return isRunning ? DateTime.MaxValue : _stopped; } }

        /// <summary>
        ///     Server IP Mode
        /// </summary>
        private Mode ipmode = Mode.NoRules;
        /// <summary>
        ///     Server Access by IP Rules Mode
        /// </summary>
        public Mode ListenIPMode { get { return ipmode; } set { ipmode = value; } }

        private Mutex iplistmutex = new Mutex();
        /// <summary>
        ///     IP white list (supporting *: 192.168.10.*) (Regex, if starts from ^ or ends with $)
        /// </summary>
        private List<string> ipwhitelist = new List<string>(new string[] { "127.0.0.1", "192.168.*.*", @"^10.0.0?[0-9]?\d.\d{1,3}$" });
        /// <summary>
        ///     IP white List (supporting *: 192.168.10.*) (Regex, if starts from ^ or ends with $)
        /// </summary>
        public string[] ListenIPAllow
        {
            get
            {
                iplistmutex.WaitOne();
                string[] res = ipwhitelist.ToArray();
                iplistmutex.ReleaseMutex();
                return res;
            }
            set
            {
                iplistmutex.WaitOne();
                ipwhitelist.Clear();
                if (value != null)
                    ipwhitelist.AddRange(value);
                iplistmutex.ReleaseMutex();
            }
        }

        /// <summary>
        ///     IP black list (supporting *: 192.168.10.*) (Regex, if starts from ^ or ends with $)
        /// </summary>
        private List<string> ipblacklist = new List<string>();
        /// <summary>
        ///     IP black list (supporting *: 192.168.10.*) (Regex, if starts from ^ or ends with $)
        /// </summary>
        public string[] ListenIPDeny
        {
            get
            {
                iplistmutex.WaitOne();
                string[] res = ipblacklist.ToArray();
                iplistmutex.ReleaseMutex();
                return res;
            }
            set
            {
                iplistmutex.WaitOne();
                ipblacklist.Clear();
                if (value != null)
                    ipblacklist.AddRange(value);
                iplistmutex.ReleaseMutex();
            }
        }

        /// <summary>
        ///     Server Mac Mode
        /// </summary>
        private Mode macmode = Mode.NoRules;
        /// <summary>
        ///     Server Access by IP Rules Mode
        /// </summary>
        public Mode ListenMacMode { get { return macmode; } set { macmode = value; } }

        private Mutex maclistmutex = new Mutex();
        /// <summary>
        ///     Mac Address White List (XX-XX-XX-XX-XX-XX) (supporting *: XX-XX-XX-XX-*-XX) (Regex, if starts from ^ or ends with $)
        /// </summary>
        private List<string> macwhitelist = new List<string>();
        /// <summary>
        ///     Mac Address White List (XX-XX-XX-XX-XX-XX) (supporting *: XX-XX-XX-XX-*-XX) (Regex, if starts from ^ or ends with $)
        /// </summary>
        public string[] ListenMacAllow
        {
            get
            {
                maclistmutex.WaitOne();
                string[] res = macwhitelist.ToArray();
                maclistmutex.ReleaseMutex();
                return res;
            }
            set
            {
                maclistmutex.WaitOne();
                macwhitelist.Clear();
                if (value != null)
                    macwhitelist.AddRange(value);
                if (macwhitelist.Count > 0)
                    for (int i = 0; i < macwhitelist.Count; i++)
                        macwhitelist[i] = macwhitelist[i].ToUpper();
                maclistmutex.ReleaseMutex();
            }
        }
        /// <summary>
        ///     Mac Address Black List (XX-XX-XX-XX-XX-XX) (supporting *: XX-XX-XX-XX-*-XX) (Regex, if starts from ^ or ends with $)
        /// </summary>
        private List<string> macblacklist = new List<string>();
        /// <summary>
        ///     Mac Address Black List (XX-XX-XX-XX-XX-XX) (supporting *: XX-XX-XX-XX-*-XX) (Regex, if starts from ^ or ends with $)
        /// </summary>
        public string[] ListenMacDeny
        {
            get
            {
                maclistmutex.WaitOne();
                string[] res = macblacklist.ToArray();
                maclistmutex.ReleaseMutex();
                return res;
            }
            set
            {
                maclistmutex.WaitOne();
                macblacklist.Clear();
                if (value != null)
                    macblacklist.AddRange(value);
                if (macblacklist.Count > 0)
                    for (int i = 0; i < macblacklist.Count; i++)
                        macblacklist[i] = macblacklist[i].ToUpper();
                maclistmutex.ReleaseMutex();
            }
        }


        /// <summary>
        ///     Max Clients Count
        /// </summary>
        private ushort maxClients = 50;
        /// <summary>
        ///     Max connected clients count
        /// </summary>
        public ushort MaxClients { get { return maxClients; } set { maxClients = value; } }

        /// <summary>
        ///     Abort client connection on stop
        /// </summary>
        private bool abortOnStop = false;
        /// <summary>
        ///     Abort client connections on stop
        /// </summary>
        public bool AbortOnStop { get { return abortOnStop; } set { abortOnStop = value; } }

        /// <summary>
        ///     Mutex for client dictionary
        /// </summary>
        private Mutex stack = new Mutex();
        /// <summary>
        ///     Client dictionary
        /// </summary>
        private Dictionary<ulong, ClientTCPSInfo> clients = new Dictionary<ulong, ClientTCPSInfo>();
        /// <summary>
        ///     Currect connected clients
        /// </summary>
        public KeyValuePair<ulong, TcpClient>[] Clients
        {
            get
            {
                this.stack.WaitOne();
                List<KeyValuePair<ulong, TcpClient>> res = new List<KeyValuePair<ulong, TcpClient>>();
                foreach (KeyValuePair<ulong, ClientTCPSInfo> kvp in this.clients)
                    res.Add(new KeyValuePair<ulong, TcpClient>(kvp.Key, kvp.Value.client));
                this.stack.ReleaseMutex();
                return res.ToArray();
            }
        }

        public ThreadedTCPServer() { }
        public ThreadedTCPServer(int Port) { this.ListenPort = Port; }
        public ThreadedTCPServer(IPAddress IP, int Port) { this.ListenIP = IP; this.ListenPort = Port; }
        ~ThreadedTCPServer() { Dispose(); }

        private bool AllowedByIPRules(TcpClient client)
        {
            if (ipmode != Mode.NoRules)
            {
                string remoteIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                iplistmutex.WaitOne();
                if (ipmode == Mode.AllowWhiteList)
                {
                    if ((ipwhitelist != null) && (ipwhitelist.Count > 0))
                    {
                        foreach (string ip in ipwhitelist)
                            if ((ip.Contains("*") || ip.StartsWith("^") || ip.EndsWith("$")))
                            {
                                string nip = ip.StartsWith("^") || ip.EndsWith("$") ? ip : ip.Replace(".", @"\.").Replace("*", @"\d{1,3}");
                                Regex ex = new Regex(nip, RegexOptions.None);
                                if (ex.Match(remoteIP).Success)
                                    return true;
                            };
                    };
                    if ((ipwhitelist == null) || (ipwhitelist.Count == 0) || (!ipwhitelist.Contains(remoteIP)))
                    {
                        iplistmutex.ReleaseMutex();
                        return false;
                    };
                }
                else
                {
                    if ((ipblacklist != null) && (ipblacklist.Count > 0) && ipblacklist.Contains(remoteIP))
                    {
                        iplistmutex.ReleaseMutex();
                        return false;
                    };
                    if ((ipblacklist != null) && (ipblacklist.Count > 0))
                    {
                        foreach (string ip in ipblacklist)
                            if ((ip.Contains("*") || ip.StartsWith("^") || ip.EndsWith("$")))
                            {
                                string nip = ip.StartsWith("^") || ip.EndsWith("$") ? ip : ip.Replace(".", @"\.").Replace("*", @"\d{1,3}");
                                Regex ex = new Regex(nip, RegexOptions.None);
                                if (ex.Match(remoteIP).Success)
                                    return false;
                            };
                    };
                };
                iplistmutex.ReleaseMutex();
            };
            return true;
        }

        private bool AllowedByMacRules(TcpClient client)
        {
            if (macmode != Mode.NoRules)
            {
                string remoteMac = GetMacAddressByIP(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString().ToUpper());
                maclistmutex.WaitOne();
                if (macmode == Mode.AllowWhiteList)
                {
                    if ((macwhitelist != null) && (macwhitelist.Count > 0))
                    {
                        foreach (string mac in macwhitelist)
                            if ((mac.Contains("*") || mac.StartsWith("^") || mac.EndsWith("$")))
                            {
                                string nmac = mac.StartsWith("^") || mac.EndsWith("$") ? mac : mac.Replace(".", @"\.").Replace("*", @"\w{2}}");
                                Regex ex = new Regex(nmac, RegexOptions.None);
                                if (ex.Match(remoteMac).Success)
                                    return true;
                            };
                    };
                    if ((macwhitelist == null) || (macwhitelist.Count == 0) || (!macwhitelist.Contains(remoteMac)))
                    {
                        maclistmutex.ReleaseMutex();
                        return false;
                    };
                }
                else
                {
                    if ((macblacklist != null) && (macblacklist.Count > 0) && macblacklist.Contains(remoteMac))
                    {
                        maclistmutex.ReleaseMutex();
                        return false;
                    };
                    if ((macblacklist != null) && (macblacklist.Count > 0))
                    {
                        foreach (string mac in macblacklist)
                            if ((mac.Contains("*") || mac.StartsWith("^") || mac.EndsWith("$")))
                            {
                                string nmac = mac.StartsWith("^") || mac.EndsWith("$") ? mac : mac.Replace(".", @"\.").Replace("*", @"\w{2}}");
                                Regex ex = new Regex(nmac, RegexOptions.None);
                                if (ex.Match(remoteMac).Success)
                                    return false;
                            };
                    };
                };
                maclistmutex.ReleaseMutex();
            };
            return true;
        }

        /// <summary>
        ///     Calls at server main listening thread not for each client
        ///     -- connection will be closed after return --
        /// </summary>
        /// <param name="Client"></param>
        protected virtual void GetBlockedClient(TcpClient Client)
        {
            // do nothing
            // close after return
        }

        private void MainThread()
        {
            _started = DateTime.Now;
            isRunning = true;
            while (isRunning)
            {
                try
                {
                    bool allowed = false;
                    TcpClient client = mainListener.AcceptTcpClient();
                    if (!AcceptClient(client))
                    {
                        client.Client.Close();
                        client.Close();
                        continue;
                    };

                    allowed = AllowedByIPRules(client) && AllowedByMacRules(client);
                    if (!allowed)
                    {
                        try
                        {
                            GetBlockedClient(client);
                            client.Client.Close();
                            client.Close();
                        }
                        catch { };
                        continue;
                    };

                    if (this.maxClients < 2) // single-threaded
                    {
                        RunClientThread(new ClientTCPSInfo(client, null));
                    }
                    else // multi-threaded
                    {
                        while ((this.alive >= this.maxClients) && isRunning) // wait for any closed thread
                            System.Threading.Thread.Sleep(5);
                        if (isRunning)
                        {
                            Thread thr = new Thread(RunThreaded);
                            thr.Start(new ClientTCPSInfo(client, thr));
                        };
                    };
                }
                catch { };
                Thread.Sleep(1);
            };
        }

        private void RunThreaded(object client)
        {
            RunClientThread((ClientTCPSInfo)client);
        }

        private void RunClientThread(ClientTCPSInfo Client)
        {
            this.alive++;
            Client.id = this.counter++;
            this.stack.WaitOne();
            this.clients.Add(Client.id, Client);
            this.stack.ReleaseMutex();
            try
            {
                Client.client.GetStream().ReadTimeout = this.readTimeout * 1000;
                GetClient(Client.client, Client.id);
            }
            catch (Exception ex)
            {
                LastError = ex;
                LastErrTime = DateTime.Now;
                ErrorsCounter++;
                onError(Client.client, Client.id, ex);
            }
            finally
            {
                try { Client.client.GetStream().Flush(); }
                catch { };
                try
                {
                    Client.client.Client.Close();
                    Client.client.Close();
                }
                catch { };
            };

            this.stack.WaitOne();
            if (this.clients.ContainsKey(Client.id))
                this.clients.Remove(Client.id);
            this.stack.ReleaseMutex();
            this.alive--;
        }

        /// <summary>
        ///  Start Server
        /// </summary>
        public override void Start()
        {
            if (isRunning) throw new Exception("Server Already Running!");

            try
            {
                mainListener = new TcpListener(this.ListenIP, this.ListenPort);
                mainListener.Start();
            }
            catch (Exception ex)
            {
                LastError = ex;
                LastErrTime = DateTime.Now;
                ErrorsCounter++;
                throw ex;
            };

            mainThread = new Thread(MainThread);
            mainThread.Start();
        }

        /// <summary>
        ///     Stop Server
        /// </summary>
        public override void Stop()
        {
            if (!isRunning) return;

            isRunning = false;

            if (this.abortOnStop)
            {
                this.stack.WaitOne();
                try
                {
                    foreach (KeyValuePair<ulong, ClientTCPSInfo> kvp in this.clients)
                    {
                        try { if (kvp.Value.thread != null) kvp.Value.thread.Abort(); }
                        catch { };
                        try { kvp.Value.client.Client.Close(); }
                        catch { };
                        try { kvp.Value.client.Close(); }
                        catch { };
                    };
                    this.clients.Clear();
                }
                catch { };
                this.stack.ReleaseMutex();
            };

            _stopped = DateTime.Now;

            if (mainListener != null) mainListener.Stop();
            mainListener = null;

            mainThread.Join();
            mainThread = null;
        }

        /// <summary>
        ///     Get Client, threaded
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="id"></param>
        protected override void GetClient(TcpClient Client, ulong id)
        {
            // loop something  
            // connection will be close after return
        }
    }

    /// <summary>
    ///     Многопоточный HTTP-сервер
    /// </summary>
    public class ThreadedHttpServer : ThreadedTCPServer
    {
        public ThreadedHttpServer() : base(80) { Init(); }
        public ThreadedHttpServer(int Port) : base(Port) { Init(); }
        public ThreadedHttpServer(IPAddress IP, int Port) : base(IP, Port) { Init(); }
        ~ThreadedHttpServer() { this.Dispose(); }

        protected bool _AddContentDisposition = false;
        protected bool _ContentDispositionInline = false;

        protected void Init()
        {
            _headers.Add("Server-Name", _serverName);
            //_headers.Add("Connection", "close"); 
        }

        /// <summary>
        ///     Skip Files Extentions to Browse and download
        /// </summary>
        public string[] AllowNotFileExt = new string[] { ".exe", ".dll", ".cmd", ".bat", ".lib", ".crypt", };

        /// <summary>
        ///     Server Name
        /// </summary>
        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                _headers_mutex.WaitOne();
                if (_headers.ContainsKey("Server-Name"))
                    _headers["Server-Name"] = _serverName;
                else
                    _headers.Add("Server-Name", _serverName);
                _headers_mutex.ReleaseMutex();
            }
        }
        protected string _serverName = "dkxce HttpServer v0.1A";

        /// <summary>
        ///     Allow to connect only HTTP clients
        /// </summary>
        public virtual bool OnlyHTTPClients { get { return _OnlyHTTP; } set { _OnlyHTTP = value; } }
        protected bool _OnlyHTTP = true;

        /// <summary>
        ///     Max Http Header Client Size
        /// </summary>
        public uint MaxClientHeaderSize { get { return _MaxHeaderSize; } set { _MaxHeaderSize = value; } }
        protected uint _MaxHeaderSize = 4096;

        /// <summary>
        ///     Max Http Body Client Size
        /// </summary>
        public uint MaxClientBodySize { get { return _MaxBodySize; } set { _MaxBodySize = value; } }
        protected uint _MaxBodySize = 65536;

        /// <summary>
        ///     Max File Size to Download
        /// </summary>
        public long AllowBrowseDownloadMaxSize { get { return _MaxFileDownloadSize; } set { _MaxFileDownloadSize = value; } }
        protected long _MaxFileDownloadSize = 1024 * 1024 * 40; // 40 MB        

        /// <summary>
        ///    Default Server Response Encoding
        /// </summary>
        public Encoding ResponseEncoding { get { return _responseEnc; } set { _responseEnc = value; } }
        protected Encoding _responseEnc = Encoding.GetEncoding(1251);

        /// <summary>
        ///     Default Client Request Encoding
        /// </summary>
        public Encoding RequestEncoding { get { return _requestEnc; } set { _requestEnc = value; } }
        protected Encoding _requestEnc = Encoding.GetEncoding(1251);

        /// <summary>
        ///     Send Error to Denied IP Clients
        /// </summary>
        public bool ListenIPDeniedSendError { get { return _sendlockedError; } set { _sendlockedError = value; } }
        protected bool _sendlockedError = false;

        /// <summary>
        ///     Error Code for Denied IP Clients
        /// </summary>
        public int ListenIPDeniedErrorCode { get { return _sendlockedErrCode; } set { _sendlockedErrCode = value; } }
        protected int _sendlockedErrCode = 423; // Locked        

        /// <summary>
        ///     Server Response Main Headers
        /// </summary>
        public Dictionary<string, string> Headers
        {
            get
            {
                _headers_mutex.WaitOne();
                Dictionary<string, string> res = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in _headers)
                    res.Add(kvp.Key, kvp.Value);
                _headers_mutex.ReleaseMutex();
                return res;
            }
            set
            {
                _headers_mutex.WaitOne();
                _headers.Clear();
                foreach (KeyValuePair<string, string> kvp in value)
                    _headers.Add(kvp.Key, kvp.Value);
                if (_headers.ContainsKey("Server-Name"))
                    _headers["Server-Name"] = _serverName;
                else
                    _headers.Add("Server-Name", _serverName);
                _headers_mutex.ReleaseMutex();
            }
        }
        protected Dictionary<string, string> _headers = new Dictionary<string, string>();
        protected Mutex _headers_mutex = new Mutex();

        /// <summary>
        ///     Home Directory For File Listing
        /// </summary>
        public string HomeDirectory { get { return _baseDir; } set { _baseDir = value; } }
        protected string _baseDir = null;

        /// <summary>
        ///     Allow File Download
        /// </summary>
        public bool AllowBrowseDownloads { get { return _allowGetFiles; } set { _allowGetFiles = value; } }
        protected bool _allowGetFiles = false;

        /// <summary>
        ///     Allow Browse Directory for Files
        /// </summary>
        public bool AllowBrowseFiles { get { return _allowGetDirs; } set { if (value) _allowGetFiles = true; _allowGetDirs = value; } }
        protected bool _allowGetDirs = false;

        /// <summary>
        ///     Allow Range Downloads (Continuos)
        /// </summary>
        public bool AllowRangeDownloads { get { return _allowRangeDownloads; } set { _allowRangeDownloads = value; } }
        protected bool _allowRangeDownloads = true;

        /// <summary>
        ///     Allow Browse Directory for Big Files (over AllowBrowseDownloadMaxSize)
        /// </summary>
        public bool AllowBrowseBigFiles { get { return _allowGetDirs; } set { if (value) _allowGetFiles = true; _allowGetDirs = value; } }
        protected bool _allowListBigFiles = true;

        /// <summary>
        ///     Allow Browse Directory for Directories
        /// </summary>
        public bool AllowBrowseDirectories { get { return _allowListDirs; } set { if (value) _allowGetFiles = true; _allowListDirs = value; } }
        protected bool _allowListDirs = false;

        /// <summary>
        ///     User:password for Authorizated users
        /// </summary>
        public Dictionary<string, string> AuthentificationCredintals = new Dictionary<string, string>();
        /// <summary>
        ///     Server Requires Authorization
        /// </summary>
        public bool AuthentificationRequired { get { return _authRequired; } set { _authRequired = value; } }
        private bool _authRequired = false;
        /// <summary>
        ///     Server Remote Users Authorization
        /// </summary>
        public bool AuthentificateRemoteUsers { get { return _authRemoteUsers; } set { _authRemoteUsers = value; } }
        private bool _authRemoteUsers = false;

        /// <summary>
        ///     Get Client, threaded
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="clientID"></param>
        protected override void GetClient(TcpClient Client, ulong clientID)
        {
            Regex CR = new Regex(@"Content-Length: (\d+)", RegexOptions.IgnoreCase);

            string Request = "";
            string Header = null;
            List<byte> Body = new List<byte>();

            int bRead = -1;
            int posCRLF = -1;
            int receivedBytes = 0;
            int contentLength = 0;

            try
            {
                // Get Header
                //while ((Client.Available > 0) && ((bRead = Client.GetStream().ReadByte()) >= 0)) // doesn't work correct
                while ((bRead = Client.GetStream().ReadByte()) >= 0)
                {
                    receivedBytes++;
                    Body.Add((byte)bRead);

                    // Check GET or POST
                    if (_OnlyHTTP && (receivedBytes == 1))
                        if ((bRead != 0x47) && (bRead != 0x50))
                        {
                            onBadClient(Client, clientID, Body.ToArray());
                            return;
                        };

                    Request += (char)bRead; // standard symbol
                    if (bRead == 0x0A) posCRLF = Request.IndexOf("\r\n\r\n"); // get body start index
                    if (posCRLF >= 0 || Request.Length > _MaxHeaderSize) { break; }; // GET ONLY                    
                };

                if (Request.Length > _MaxHeaderSize)
                {
                    // Header too long
                    HttpClientSendError(Client, 414, "414 Header Too Long");
                    return;
                };

                bool valid = (posCRLF > 0);
                if ((!valid) && _OnlyHTTP)
                {
                    onBadClient(Client, clientID, Body.ToArray());
                    return;
                };

                if ((_authRequired || (_authRemoteUsers && (!GetLocalIPs().Contains(((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString())))) && (AuthentificationCredintals.Count > 0))
                {
                    bool accept = false;
                    string sa = "Authorization:";
                    if (Request.IndexOf(sa) > 0)
                    {
                        int iofcl = Request.IndexOf(sa);
                        sa = Request.Substring(iofcl + sa.Length, Request.IndexOf("\r", iofcl + sa.Length) - iofcl - sa.Length).Trim();
                        if (sa.StartsWith("Basic"))
                        {
                            sa = Base64Decode(sa.Substring(6));
                            string[] up = sa.Split(new char[] { ':' }, 2);
                            if (AuthentificationCredintals.ContainsKey(up[0]) && AuthentificationCredintals[up[0]] == up[1])
                                accept = true;
                        };
                    };
                    if (!accept)
                    {
                        Dictionary<string, string> dh = new Dictionary<string, string>();
                        dh.Add("WWW-Authenticate", "Basic realm=\"Authentification required\"");
                        HttpClientSendError(Client, 401, dh); // 401 Unauthorized
                        return;
                    };
                };

                // Get Body
                if (valid)
                {
                    Body.Clear();
                    Header = Request;
                    Match mx = CR.Match(Request);
                    if (mx.Success) contentLength = int.Parse(mx.Groups[1].Value);
                    int total2read = posCRLF + 4 + contentLength;
                    while ((receivedBytes < total2read) && ((bRead = Client.GetStream().ReadByte()) >= 0))
                    {
                        receivedBytes++;
                        Body.Add((byte)bRead);

                        string rcvd = _requestEnc.GetString(new byte[] { (byte)bRead }, 0, 1);
                        Request += rcvd;
                        if (Request.Length > _MaxBodySize)
                        {
                            // Body too long
                            HttpClientSendError(Client, 413, "413 Payload Too Large");
                            return;
                        };
                    };
                };

                GetClientRequest(Client, clientID, Request, Header, Body.ToArray());
            }
            catch (Exception ex)
            {
                LastError = ex;
                LastErrTime = DateTime.Now;
                ErrorsCounter++;
                onError(Client, clientID, ex);
            };
        }

        /// <summary>
        ///     Send Response with Error Code To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Code">Status Code</param>
        /// <param name="dopHeaders">Response Headers</param>
        protected virtual void HttpClientSendError(TcpClient Client, int Code, Dictionary<string, string> dopHeaders)
        {
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            string body = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            HttpClientSendData(Client, _responseEnc.GetBytes(body), dopHeaders, Code, "text/html");
        }
        /// <summary>
        ///     Send Response with Error Code To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Code">Status Code</param>
        protected virtual void HttpClientSendError(TcpClient Client, int Code)
        {
            HttpClientSendError(Client, Code, (Dictionary<string, string>)null);
        }
        /// <summary>
        ///     Send Response with Error Code To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Code">Status Code</param>
        /// <param name="Text">Body Text</param>
        protected virtual void HttpClientSendError(TcpClient Client, int Code, string Text)
        {
            HttpClientSendData(Client, _responseEnc.GetBytes(Text), null, Code, "text/html");
        }
        /// <summary>
        ///     Send Response Text Status 200 To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Text">Body Text</param>
        /// <param name="dopHeaders">Response Headers</param>
        protected virtual void HttpClientSendText(TcpClient Client, string Text, IDictionary<string, string> dopHeaders)
        {
            string body = "<html><body>" + Text + "</body></html>";
            HttpClientSendData(Client, _responseEnc.GetBytes(body), dopHeaders, 200, "text/html");
        }
        /// <summary>
        ///     Send Response Text Status 200 To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Text">Body Text</param>
        protected virtual void HttpClientSendText(TcpClient Client, string Text)
        {
            HttpClientSendText(Client, Text, null);
        }

        /// <summary>
        ///     Send Response Data Status 200 To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="body"></param>
        protected virtual void HttpClientSendData(TcpClient Client, byte[] body)
        {
            HttpClientSendData(Client, body, null, 200, "text/html");
        }
        /// <summary>
        ///     Send Response Data Status 200 To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="body"></param>
        /// <param name="dopHeaders">Response Headers</param>
        protected virtual void HttpClientSendData(TcpClient Client, byte[] body, IDictionary<string, string> dopHeaders)
        {
            HttpClientSendData(Client, body, dopHeaders, 200, "text/html");
        }
        /// <summary>
        ///     Send Response Data To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="body"></param>
        /// <param name="dopHeaders">Response Headers</param>
        /// <param name="ResponseCode">Status Code</param>
        protected virtual void HttpClientSendData(TcpClient Client, byte[] body, IDictionary<string, string> dopHeaders, int ResponseCode)
        {
            HttpClientSendData(Client, body, dopHeaders, ResponseCode, "text/html");
        }
        /// <summary>
        ///     Send Response Data To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="body"></param>
        /// <param name="dopHeaders">Response Headers</param>
        /// <param name="ContentType">Content Type</param>
        protected virtual void HttpClientSendData(TcpClient Client, byte[] body, IDictionary<string, string> dopHeaders, string ContentType)
        {
            HttpClientSendData(Client, body, dopHeaders, 200, ContentType);
        }
        /// <summary>
        ///     Send Response Data To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="body"></param>
        /// <param name="dopHeaders">Response Headers</param>
        /// <param name="ResponseCode">Status Code</param>
        /// <param name="ContentType">Content Type</param>
        protected virtual void HttpClientSendData(TcpClient Client, byte[] body, IDictionary<string, string> dopHeaders, int ResponseCode, string ContentType)
        {
            string header = "HTTP/1.1 " + ResponseCode.ToString() + "\r\n";

            string val = null;
            if ((val = DictGetKeyIgnoreCase(dopHeaders, "Status")) != null) header = "HTTP/1.1 " + val + "\r\n";

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
                header += "Content-Length: " + body.Length.ToString() + "\r\n";
            header += "\r\n";

            List<byte> response = new List<byte>();
            response.AddRange(Encoding.GetEncoding(1251).GetBytes(header));
            response.AddRange(body);

            Client.GetStream().Write(response.ToArray(), 0, response.Count);
            Client.GetStream().Flush();
            //Client.Client.Close();
            //Client.Close();
        }

        internal Stream HttpClientGetOutputStreamPrepareMainHeaders(ClientRequest Request, Dictionary<string, string> dopHeaders, int ResponseCode, out string header)
        {
            header = "HTTP/1.1 " + ResponseCode.ToString() + "\r\n";
            this._headers_mutex.WaitOne();
            foreach (KeyValuePair<string, string> kvp in this._headers)
                header += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            this._headers_mutex.ReleaseMutex();
            // Dop Headers
            if (dopHeaders != null)
                foreach (KeyValuePair<string, string> kvp in dopHeaders)
                    header += String.Format("{0}: {1}\r\n", kvp.Key, kvp.Value);
            return Request.Client.GetStream();
        }

        /// <summary>
        ///     Send File To Client
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="fileName">File Full Path</param>
        /// <param name="dopHeaders">Response Headers</param>
        /// <param name="ResponseCode">Status Code</param>
        /// <param name="ContentType">Content Type</param>
        protected virtual long HttpClientSendFile(ClientRequest Request, string fileName, Dictionary<string, string> dopHeaders, int ResponseCode, string ContentType)
        {
            int rFrom = -1;
            int rTo = -1;
            // https://developer.mozilla.org/ru/docs/Web/HTTP/Headers/Range
            if (_allowRangeDownloads && Request.Headers.ContainsKey("Range"))
            {
                string ran = Request.Headers["Range"];
                ran = (new Regex(@"bytes\=", RegexOptions.IgnoreCase)).Replace(ran, "");
                if (!string.IsNullOrEmpty(ran))
                {
                    if (ran.IndexOf(",") >= 0)
                    {
                        HttpClientSendError(Request.Client, 416);
                        return 0;
                    };
                    Regex rx = new Regex(@"(?<from>\d*)\-(?<to>\d*)", RegexOptions.None);
                    Match mx = rx.Match(ran);
                    if (!mx.Success)
                    {
                        HttpClientSendError(Request.Client, 416);
                        return 0;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(mx.Groups["from"].Value)) int.TryParse(mx.Groups["from"].Value, out rFrom);
                        if (!string.IsNullOrEmpty(mx.Groups["to"].Value)) int.TryParse(mx.Groups["to"].Value, out rTo);
                    };
                };
            };

            FileInfo fi = new FileInfo(fileName);
            int fileLen = (int)fi.Length;
            if ((rTo > 0) && (rFrom < 0)) { rFrom = fileLen - rTo; rTo = fileLen - 1; };
            if (rFrom == -1) rFrom = 0;
            if (rTo <= 0) rTo = fileLen - 1;
            fileLen = rTo - rFrom + 1;
            if (rTo < rFrom) { rFrom = 0; rTo = fileLen - 1; };

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

            string ext = fi.Extension.ToLower();
            if (String.IsNullOrEmpty(ContentType))
                ContentType = GetMimeType(ext);
            if (!DictHasKeyIgnoreCase(dopHeaders, "Date"))
                header += "Date: " + DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT" + "\r\n";
            if (!DictHasKeyIgnoreCase(dopHeaders, "Last-Modified"))
                header += "Last-Modified: " + fi.LastWriteTimeUtc.ToString("ddd, dd MMM yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) + " GMT" + "\r\n";
            if (!DictHasKeyIgnoreCase(dopHeaders, "Content-type"))
                header += "Content-type: " + ContentType + "\r\n";
            if (!DictHasKeyIgnoreCase(dopHeaders, "Content-Length"))
                header += "Content-Length: " + fileLen.ToString() + "\r\n";
            if (_AddContentDisposition)
            {
                if ((!ext.StartsWith(".htm")) && (!ext.StartsWith(".dht")) && (!ext.StartsWith(".xht")) && (!ext.StartsWith(".js")) && (!ext.StartsWith(".mjs")) && (!ext.StartsWith(".cs")))
                {
                    if (!DictHasKeyIgnoreCase(dopHeaders, "Content-Disposition"))
                        header += "Content-Disposition: " + (_ContentDispositionInline ? "inline" : "attachment") + "; filename=\"" + Path.GetFileName(fileName) + "\"\r\n";
                };
            };
            header += "\r\n";

            List<byte> response = new List<byte>();
            response.AddRange(Encoding.GetEncoding(1251).GetBytes(header));
            Request.Client.GetStream().Write(response.ToArray(), 0, response.Count);

            int writed = 0;
            if (Request.Method != "HEAD")
            {
                // copy
                byte[] buff = new byte[65536];
                int bRead = 0;
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (rFrom > 0) fs.Position = rFrom;
                while (fs.Position <= rTo)
                {
                    bRead = fs.Read(buff, 0, buff.Length);
                    writed += bRead;
                    if (writed > fileLen) bRead = writed - fileLen;
                    if (bRead > 0)
                        Request.Client.GetStream().Write(buff, 0, bRead);
                };
                fs.Close();
            };

            Request.Client.GetStream().Flush();
            //Client.Client.Close();
            //Client.Close();
            return (long)writed;
        }

        /// <summary>
        ///     Get HTTP Client Request, threaded
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="clientID"></param>
        /// <param name="Request"></param>
        /// <param name="Header"></param>
        /// <param name="Body"></param>
        protected virtual void GetClientRequest(TcpClient Client, ulong clientID, string Request, string Header, byte[] Body)
        {
            IDictionary<string, string> clHeaders = GetClientHeaders(Header);
            string page, host, inline, query;
            IDictionary<string, string> parameters;
            ClientRequest cl = ClientRequest.FromServer(this);

            try
            {
                query = GetClientQuery(Header, out host, out page, out inline, out parameters);
                cl.Client = Client;
                cl.clientID = clientID;
                cl.OriginRequest = Request;
                cl.OriginHeader = Header;
                cl.BodyData = Body;
                cl.Query = query;
                cl.Page = page;
                cl.Host = host;
                cl.QueryParams = parameters;
                cl.QueryInline = inline;
                cl.Headers = clHeaders;
                int fs = Header.IndexOf(" ");
                if (fs > 0) cl.Method = Header.Substring(0, fs);
            }
            catch
            {

            };

            GetClientRequest(cl);
            // connection will be close after return   
        }
        /// <summary>
        ///     Get HTTP Client Request, threaded
        /// </summary>
        /// <param name="Request"></param>
        protected virtual void GetClientRequest(ClientRequest Request)
        {
            HttpClientSendError(Request.Client, 501);
            // connection will be close after return   
        }
        /// <summary>
        ///     Call on error in GetClient & GetClientRequest
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="id"></param>
        /// <param name="error"></param>
        protected override void onError(TcpClient Client, ulong id, Exception error)
        {
            // no base.onError - no throw exception
        }
        /// <summary>
        ///     Call on Bad Client (Invalid HTTP Request)
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="id"></param>
        /// <param name="Request"></param>
        protected virtual void onBadClient(TcpClient Client, ulong id, byte[] Request)
        {
            // do nothing
        }

        /// <summary>
        ///     Pass File or Browse Folder(s) to HTTP Client by Request
        /// </summary>
        /// <param name="Request"></param>
        protected virtual void PassFileToClientByRequest(ClientRequest Request)
        {
            PassFileToClientByRequest(Request, _baseDir, null);
        }
        /// <summary>
        ///     Pass File or Browse Folder(s) to HTTP Client by Request
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="HomeDirectory">Home Directory with files to Browse</param>
        protected virtual void PassFileToClientByRequest(ClientRequest Request, string HomeDirectory)
        {
            PassFileToClientByRequest(Request, HomeDirectory, null);
        }

        /// <summary>
        ///     Pass File or Browse Folder(s) to HTTP Client by Request
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="HomeDirectory">Home Directory with files to Browse</param>
        /// <param name="subPath">Sub Path to Extract from URL Path</param>
        protected virtual void PassFileToClientByRequest(ClientRequest Request, string HomeDirectory, string subPath)
        {
            if (String.IsNullOrEmpty(HomeDirectory)) { HttpClientSendError(Request.Client, 403); return; };
            if (!_allowGetFiles) { HttpClientSendError(Request.Client, 403); return; };
            if (String.IsNullOrEmpty(Request.Query)) { HttpClientSendError(Request.Client, 400); return; };
            if (String.IsNullOrEmpty(Request.Page)) { HttpClientSendError(Request.Client, 403); return; };
            if ((Request.QueryParams != null) && (Request.QueryParams.Count > 0)) { HttpClientSendError(Request.Client, 400); return; };

            string path = Request.Page;
            if (!String.IsNullOrEmpty(subPath))
            {
                int i = path.IndexOf(subPath);
                if (i >= 0) path = path.Remove(i, subPath.Length);
            };
            path = path.Replace("/", @"\");
            if (path.IndexOf("/./") >= 0) { HttpClientSendError(Request.Client, 400); return; };
            if (path.IndexOf("/../") >= 0) { HttpClientSendError(Request.Client, 400); return; };
            if (path.IndexOf("/.../") >= 0) { HttpClientSendError(Request.Client, 400); return; };
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
                        return;
                    }
                    else
                    {
                        if (fi.Length > _MaxFileDownloadSize)
                            HttpClientSendError(Request.Client, 509, String.Format("509 File is too big - {0}, limit - {1}", ToFileSize(fi.Length), ToFileSize(_MaxFileDownloadSize)));
                        else
                            HttpClientSendFile(Request, path, null, 200, null);
                        return;
                    };
                };
            }
            else if (!String.IsNullOrEmpty(dName))
            {
                if (!Directory.Exists(path))
                {
                    HttpClientSendError(Request.Client, 404);
                    return;
                }
                else
                {
                    // load default file
                    {
                        List<string> files = new List<string>(Directory.GetFiles(path, "index.*", SearchOption.TopDirectoryOnly));
                        foreach (string file in files)
                        {
                            string fExt = Path.GetExtension(file);
                            if (fExt == ".html") { HttpClientSendFile(Request, file, null, 200, null); return; };
                            if (fExt == ".dhtml") { HttpClientSendFile(Request, file, null, 200, null); return; };
                            if (fExt == ".htmlx") { HttpClientSendFile(Request, file, null, 200, null); return; };
                            if (fExt == ".xhtml") { HttpClientSendFile(Request, file, null, 200, null); return; };
                            if (fExt == ".txt") { HttpClientSendFile(Request, file, null, 200, null); return; };
                        };
                    };
                    if (!_allowGetDirs)
                    {
                        HttpClientSendError(Request.Client, 403);
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
                        return;
                    };
                };
            };
            HttpClientSendError(Request.Client, 400);
        }


        /// <summary>
        ///     Calls on Blocked by IP Client
        /// </summary>
        /// <param name="Client"></param>
        protected override void GetBlockedClient(TcpClient Client)
        {
            if (_sendlockedError)
            {
                if ((_sendlockedErrCode == 0) || ((_sendlockedErrCode == 423)))
                    HttpClientSendError(Client, 423, "423 Locked");
                else
                    HttpClientSendError(Client, _sendlockedErrCode);
            };
        }

        public class ClientRequest
        {
            private ThreadedHttpServer server;

            /// <summary>
            ///     Client
            /// </summary>
            public TcpClient Client;
            /// <summary>
            ///     CLient ID
            /// </summary>
            public ulong clientID;
            /// <summary>
            ///     Original HTTP Request Client Request
            /// </summary>
            public string OriginRequest;
            /// <summary>
            ///     Original Http Request Client Header
            /// </summary>
            public string OriginHeader;
            /// <summary>
            ///     Http Client Request Body Data
            /// </summary>
            public byte[] BodyData;
            /// <summary>
            ///     Http Client Request Full Query
            /// </summary>
            public string Query;
            /// <summary>
            ///     Http Client Request Query Path/Page
            /// </summary>
            public string Page;
            /// <summary>
            ///     Http Client Request Host
            /// </summary>
            public string Host;
            /// <summary>
            ///     Http Client Request Query/Get Parameters
            /// </summary>
            public string QueryInline;
            /// <summary>
            ///     Http Client Request Headers
            /// </summary>
            public IDictionary<string, string> Headers;
            /// <summary>
            ///     Http Client Request Query/Get Parameters
            /// </summary>
            public IDictionary<string, string> QueryParams;
            /// <summary>
            ///     Http Method
            /// </summary>
            public string Method = "GET";
            /// <summary>
            ///     Is WebSocket Client
            /// </summary>
            public bool IsWebSocket = false;

            internal ClientRequest() { }
            internal static ClientRequest FromServer(ThreadedHttpServer server)
            {
                ClientRequest res = new ClientRequest();
                res.server = server;
                return res;
            }

            /// <summary>
            ///     Http Client Request Accept Header
            /// </summary>
            public string Accept { get { return DictGetKeyIgnoreCase(Headers, "Accept"); } }
            /// <summary>
            ///     Http Client Request Accept-Language Header
            /// </summary>
            public string AcceptLanguage { get { return DictGetKeyIgnoreCase(Headers, "Accept-Language"); } }
            /// <summary>
            ///     Http Client Request Accept-Language Header
            /// </summary>
            public string AcceptEncoding { get { return DictGetKeyIgnoreCase(Headers, "Accept-Encoding"); } }
            /// <summary>
            ///     Http Client Request Authorization Header
            /// </summary>
            public string Authorization { get { return DictGetKeyIgnoreCase(Headers, "Authorization"); } }
            /// <summary>
            ///     Http Client Request Body as Text
            /// </summary>
            public string BodyText { get { if ((BodyData == null) || (BodyData.Length == 0)) return null; else return (server == null ? Encoding.ASCII.GetString(BodyData) : server._requestEnc.GetString(BodyData)); } }
            /// <summary>
            ///     Http Client Request Cache-Control Header
            /// </summary>
            public string CacheControl { get { return DictGetKeyIgnoreCase(Headers, "Cache-Control"); } }
            /// <summary>
            ///     Http Client Request Cookie Header
            /// </summary>
            public string Cookie { get { return DictGetKeyIgnoreCase(Headers, "Cookie"); } }
            /// <summary>
            ///     Http Client Request Content-Encoding Header
            /// </summary>
            public string ContentEncoding { get { return DictGetKeyIgnoreCase(Headers, "Content-Encoding"); } }
            /// <summary>
            ///     Http Client Request Content-Length Header
            /// </summary>
            public string ContentLength { get { return DictGetKeyIgnoreCase(Headers, "Content-Length"); } }
            /// <summary>
            ///     Http Client Request Content-Type Header
            /// </summary>
            public string ContentType { get { return DictGetKeyIgnoreCase(Headers, "Content-Type"); } }
            /// <summary>
            ///     Http Client Request Query/Get Parameters
            /// </summary>
            public IDictionary<string, string> GetParams { get { return QueryParams; } }
            /// <summary>
            ///     Http Client Request Origin Header
            /// </summary>
            public string Origin { get { return DictGetKeyIgnoreCase(Headers, "Origin"); } }
            /// <summary>
            ///     Http Client Request Post Data
            /// </summary>
            public string PostData { get { if ((BodyData == null) || (BodyData.Length == 0)) return null; else return (server == null ? UrlUnescape(Encoding.ASCII.GetString(BodyData)) : UrlUnescape(server._requestEnc.GetString(BodyData))); } }
            /// <summary>
            ///     Http Client Request Post Parameters
            /// </summary>
            public IDictionary<string, string> PostParams { get { if ((BodyData == null) || (BodyData.Length == 0)) return new Dictionary<string, string>(); else return GetClientParams(server._requestEnc.GetString(BodyData)); } }
            /// <summary>
            ///     Http Client Request Referer Header
            /// </summary>
            public string Referer { get { return DictGetKeyIgnoreCase(Headers, "Referer"); } }
            /// <summary>
            ///     Http Client Remote IP Address
            /// </summary>
            public string RemoteIP { get { return ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString(); } }

            /// <summary>
            ///     Http Client Remote Mac Address
            /// </summary>
            public string RemoteMac { get { return TTCPServer.GetMacAddressByIP(RemoteIP); } }

            /// <summary>
            ///     Is Client Is Local
            /// </summary>
            public bool IsLocal { get { return GetLocalIPs().Contains(RemoteIP); } }

            /// <summary>
            ///     Http Client Request User-Agent Header
            /// </summary>
            public string UserAgent { get { return DictGetKeyIgnoreCase(Headers, "User-Agent"); } }
            /// <summary>
            ///     Http Client Authentificated User
            /// </summary>
            public string User
            {
                get
                {
                    string auth = DictGetKeyIgnoreCase(Headers, "Authorization");
                    if (String.IsNullOrEmpty(auth)) return null;
                    if (auth.StartsWith("Basic"))
                    {
                        string sp = Base64Decode(auth.Substring(6));
                        string[] up = sp.Split(new char[] { ':' }, 2);
                        return up[0];
                    };
                    return "Unknown";
                }
            }
            /// <summary>
            ///     Http Client Request Query/Get or Post Parameters
            /// </summary>
            /// <param name="value">parameter name</param>
            /// <returns></returns>
            public string this[string value]
            {
                get
                {
                    string res = null;
                    res = DictGetKeyIgnoreCase(QueryParams, value);
                    if (!String.IsNullOrEmpty(res)) return res;
                    res = DictGetKeyIgnoreCase(PostParams, value);
                    return res;
                }
            }

            /// <summary>
            ///     Get Header Parameter, Ignore Case
            /// </summary>
            /// <param name="value">parameter name</param>
            /// <returns></returns>
            public string GetHeaderParam(string value) { return DictGetKeyIgnoreCase(Headers, value); }
            /// <summary>
            ///     Get Query/Get Parameter, Ignore Case
            /// </summary>
            /// <param name="value">parameter name</param>
            /// <returns></returns>
            public string GetQueryParam(string value) { return DictGetKeyIgnoreCase(QueryParams, value); }
            /// <summary>
            ///     Get Post Parameter, Ignore Case
            /// </summary>
            /// <param name="value">parameter name</param>
            /// <returns></returns>
            public string GetPostParam(string value) { return DictGetKeyIgnoreCase(PostParams, value); }

            public string Dump()
            {
                return $"QUERY FROM IP: {this.RemoteIP}\r\n" + this.OriginHeader + this.PostData;
            }
        }
    }

    /// <summary>
    ///     Многопоточный HTTP-сервер с готовыми методами
    ///     Multithread HTTP Server with ready-on methods
    ///     -- ШАБЛОН --
    ///     -- TEMPLATE --
    /// </summary>
    public class HttpExampleServer : ThreadedHttpServer
    {
        public HttpExampleServer() : base(80) { }
        public HttpExampleServer(int Port) : base(Port) { }
        public HttpExampleServer(IPAddress IP, int Port) : base(IP, Port) { }
        ~HttpExampleServer() { this.Dispose(); }

        /// <summary>
        ///     Get HTTP Client Request, threaded (thread per client)
        ///     -- connection to client will be closed after return --
        ///     -- do not call base method if response any data --
        /// </summary>
        /// <param name="Request"></param>
        protected override void GetClientRequest(ClientRequest Request)
        {
            // Stop Server Immideatly
            if (Request.Query == "/exit")
            {
                Dictionary<string, string> rH = new Dictionary<string, string>();
                rH.Add("Refresh", "5; url=/");
                HttpClientSendData(Request.Client, _responseEnc.GetBytes("STOPPING SERVER..."), rH, 201, "text/html");
                this.Stop();
                Environment.Exit(0);
                return;
            };

            //Web Socket
            if (Request.Query == "/socket/")
            {
                return;
            };

            // Empty Request
            if (Request.Query == "/")
            {
                HttpClientSendData(Request.Client, _responseEnc.GetBytes("Server is running"), null, 200);
                return;
            };

            // Test Browsing Files
            if ((Request.QueryParams == null) || (Request.QueryParams.Count == 0))
            {
                this.AllowBrowseDownloads = true;
                this.AllowBrowseFiles = true;
                this.AllowBrowseDirectories = true;
                if (Request.Query.StartsWith("/disk_C/")) { PassFileToClientByRequest(Request, @"C:\", "/disk_C/"); return; };
                return;
            };

            // call base method if no response date to client
            base.GetClientRequest(Request); // returns status 501 
        }
    }
}
