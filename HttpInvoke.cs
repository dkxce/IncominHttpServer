//
// C# (.Net Framework)
// IncominHttpServer
// https://github.com/dkxce/IncominHttpServer
// en,ru,1251,utf-8
//

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using static IncominHttpServer.ThreadedHttpServer;

namespace IncominHttpServer
{
    public class HttpInvoke
    {
        public ushort InvokeNumber = 0;

        /// <summary>
        ///     Process Custom Requests
        /// </summary>
        /// <param name="Request"></param>
        /// <returns>Process Next Logic</returns>
        public bool GetClientRequest(ClientRequest Request)
        {
            if (InvokeNumber == 1) return Invoke_1(Request);
            return true;
        }

        /// <summary>
        ///    Invoke 1
        /// </summary>
        /// <param name="Request"></param>
        /// <returns>Process Next Logic</returns>
        public bool Invoke_1(ClientRequest Request)
        {
            bool res = true;
            const string url = "http://localhost:8800/";
            if (!string.IsNullOrEmpty(Request.Authorization) && Request.Authorization.StartsWith("Bearer") && Request.Headers.ContainsKey("Provider"))
            {
                res = false;

                bool wait = false;
                Thread thr = new Thread(new ThreadStart(() => {

                    HttpWebRequest wr = (HttpWebRequest)HttpWebRequest.Create(url);
                    wr.Headers.Add("Authorization", Request.Authorization);
                    wr.Headers.Add("Provider", Request.Headers["Provider"]);
                    wr.ContentType = Request.Headers["Content-Type"];
                    wr.Method = Request.Method.ToUpper().Trim();
                    wr.ContentLength = Request.BodyData.Length;                    
                    try
                    {
                        using (Stream reqstr = wr.GetRequestStream())
                            reqstr.Write(Request.BodyData, 0, Request.BodyData.Length);

                        HttpWebResponse wres = (HttpWebResponse)wr.GetResponse();
                        string response = "";

                        Encoding enc = Encoding.UTF8;
                        string tenc = wres.ContentEncoding;
                        if (!string.IsNullOrEmpty(tenc))
                        {
                            tenc = tenc.Replace("charset=", "");
                            if (!string.IsNullOrEmpty(tenc))
                            {
                                try { enc = Encoding.GetEncoding(tenc); } catch { };
                            };
                        };

                        using (StreamReader streamReader = new StreamReader(wres.GetResponseStream(), enc)) response = streamReader.ReadToEnd();

                        string dump = $"HttpWebResponse (Invoke_1) from {url}\r\n{DateTime.Now}\r\n{url}\r\n\r\nHTTP/{wres.ProtocolVersion} {(int)wres.StatusCode} {wres.StatusCode.ToString()}\r\n";
                        foreach (string key in wres.Headers.AllKeys) dump += $"{key}: {wres.Headers[key]} \r\n";
                        dump += "\r\n" + response;
                        ApplicationLog.WriteInvoke(dump);

                        wres.Close();
                    }
                    catch (Exception ex) {
                        ApplicationLog.WriteLn($"Error: {ex}"); 
                        ApplicationLog.WriteInvoke($"HttpWebResponse (Invoke_1) from {url}\r\nError: {ex}"); 
                    };

                }));
                thr.Start();
                if(wait) while(thr.IsAlive) Thread.Sleep(100);
            };
            return res;
        }
    }
}
