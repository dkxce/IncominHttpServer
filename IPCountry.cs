//
// C# (.Net Framework)
// IncominHttpServer
// https://github.com/dkxce/IncominHttpServer
// en,ru,1251,utf-8
//

using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Net
{
    public static class IPCountry
    {
        public static bool shuffled = true;

        public static string GetCountryByIP(out string service)
        {
            return GetCountryByIP(GetPublicIP(out _), out service);
        }
        
        public static string GetCountryByIP(string ip, out string service)
        {
            service = "";
            if (string.IsNullOrEmpty(ip)) return null;

            string url;
            string res;

            int[] arr = new int[] { 1, 2, 3, 4 };
            if(shuffled) ShuffleArray(arr);
            int cnt = 0;
            while (cnt < arr.Length)
            {
                int index = arr[cnt++];

                if (index == 1)
                {
                    // api.iplocation.net
                    // https://api.iplocation.net/ // country_code2
                    service = "iplocation.net";
                    url = $"https://api.iplocation.net/?ip={ip}";
                    res = GetHttpResponseText(url);
                    if (!string.IsNullOrEmpty(res))
                    {
                        string c = null, f = "";
                        try { c = (new Regex(@"country_code2(?:[\'\""\s\:])+(\w+)", RegexOptions.None)).Match(res).Groups[1].Value; } catch { };
                        try { f = (new Regex(@"country_name(?:[\'\""\s\:])+([\w\s]+)", RegexOptions.None)).Match(res).Groups[1].Value; service = $"{service}, you're from `{f}`"; } catch { };
                        if (!string.IsNullOrEmpty(c)) return c.ToUpper();
                    };
                };

                if (index == 2)
                {
                    // ipapi.com
                    // https://ipapi.com/ // countryCode
                    service = "ipapi.com";
                    url = $"http://ip-api.com/json/{ip}";
                    res = GetHttpResponseText(url);
                    if (!string.IsNullOrEmpty(res))
                    {
                        string c = null, f = "";
                        try { c = (new Regex(@"countryCode(?:[\'\""\s\:])+(\w+)", RegexOptions.None)).Match(res).Groups[1].Value; } catch { };
                        try { f = (new Regex(@"regionName(?:[\'\""\s\:])+([\w\s]+)", RegexOptions.None)).Match(res).Groups[1].Value; service = $"{service}, you're from `{f}`"; } catch { };
                        if (!string.IsNullOrEmpty(c)) return c.ToUpper();
                    };
                };

                if (index == 3)
                {
                    // https://about.ip2c.org/#about
                    service = "ip2c.org";
                    url = $"https://ip2c.org/{ip}";
                    res = GetHttpResponseText(url);
                    if (!string.IsNullOrEmpty(res))
                    {
                        string c = null;
                        try { c = res.Split(';')[1]; } catch { };
                        if (!string.IsNullOrEmpty(c)) return c.ToUpper();
                    };
                };

                if (index == 4)
                {
                    // www.geoplugin.net
                    service = "geoplugin.net";
                    url = $"http://www.geoplugin.net/csv.gp?ip={ip}";
                    res = GetHttpResponseText(url);
                    if (!string.IsNullOrEmpty(res))
                    {
                        string c = null;
                        try { c = res.Substring(res.IndexOf("geoplugin_countryCode,") + 22, 2); } catch { };
                        if (!string.IsNullOrEmpty(c)) return c.ToUpper();
                    };
                };
            };

            return null;
        }

        public static string GetPublicIP(out string service)
        {
            service = "";
            string ip;

            int[] arr = new int[] { 1, 2, 3, 4, 5, 6, 7 };
            if (shuffled) ShuffleArray(arr);
            int cnt = 0;
            while (cnt < arr.Length)
            {
                int index = arr[cnt++];
                if (index == 1)
                {
                    service = "icanhazip.com";
                    ip = GetHttpResponseText("http://icanhazip.com/");
                    if (!string.IsNullOrEmpty(ip)) return ip.Trim();
                };
                if (index == 2)
                {
                    service = "ipinfo.io";
                    ip = GetHttpResponseText("http://ipinfo.io/ip");
                    if (!string.IsNullOrEmpty(ip)) return ip.Trim();
                };
                if (index == 3)
                {
                    service = "api.ipify.org";
                    ip = GetHttpResponseText("https://api.ipify.org");
                    if (!string.IsNullOrEmpty(ip)) return ip.Trim();
                };
                if (index == 4)
                {
                    service = "checkip.amazonaws.com";
                    ip = GetHttpResponseText("http://checkip.amazonaws.com/");
                    if (!string.IsNullOrEmpty(ip)) return ip.Trim();
                };
                if (index == 5)
                {
                    service = "ipv4.icanhazip.com";
                    ip = GetHttpResponseText("https://ipv4.icanhazip.com");
                    if (!string.IsNullOrEmpty(ip)) return ip.Trim();
                };
                if (index == 6)
                {
                    service = "wtfismyip.com";
                    ip = GetHttpResponseText("https://wtfismyip.com/text");
                    if (!string.IsNullOrEmpty(ip)) return ip.Trim();
                };
                if (index == 7)
                {
                    service = "checkip.dyndns.org";
                    ip = GetHttpResponseText("http://checkip.dyndns.org");
                    if (!string.IsNullOrEmpty(ip)) try { return ip.Split(':')[1].Substring(1).Split('<')[0]; } catch { };
                };
            };
            return null;
        }

        private static void ShuffleArray(int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                Random random = new Random();
                int randomIndex = random.Next(0, i + 1);
                int temp = array[i];
                array[i] = array[randomIndex];
                array[randomIndex] = temp;
            };
        }

        private static string GetHttpResponseText(string url)
        {
            try
            {
                HttpWebRequest wreq = (HttpWebRequest)HttpWebRequest.Create(url);
                wreq.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                wreq.Timeout = 3500;
                string response = "";
                int result = 0;
                try
                {
                    HttpWebResponse wres = (HttpWebResponse)wreq.GetResponse();
                    Encoding enc = Encoding.UTF8;
                    using (StreamReader streamReader = new StreamReader(wres.GetResponseStream(), enc))
                        response = streamReader.ReadToEnd();
                    result = (int)wres.StatusCode;
                }
                catch (WebException ex)
                {
                    HttpWebResponse wres = (HttpWebResponse)ex.Response;
                    Encoding enc = Encoding.UTF8;
                    using (StreamReader streamReader = new StreamReader(wres.GetResponseStream(), enc))
                        response = streamReader.ReadToEnd();
                    result = (int)wres.StatusCode;
                }
                catch (Exception ex) { return null; };
                if (result == 200)
                    return response;
                else
                    return null;
            }
            catch { return null; };
        }
    }
}
