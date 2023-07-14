//
// C# (.Net Framework)
// IncominHttpServer
// https://github.com/dkxce/IncominHttpServer
// en,ru,1251,utf-8
//

using System;

namespace IncominHttpServer
{
    internal static class ApplicationLog
    {
        public static Form1 form = null;

        private static void Write(string text)
        {
            if (form == null) return;
            form.WriteProcLog(text);
        }

        public static void WriteQuery(string text)
        {
            if (form == null) return;            
            text = text?.Replace("\r", "").Replace("\n", "\r\n") + "\r\n";
            text = string.Format("---- {0} ----\r\n{1}", DateTime.UtcNow.ToString("HH:mm:ss MM.dd.yyyy"), text);
            form.WriteQueryLog(text);
            form.WriteLastQuery(text, false);
        }

        public static void WriteResponse(string text)
        {
            if (form == null) return;
            text = text?.Replace("\r", "").Replace("\n", "\r\n") + "\r\n";
            text = string.Format("---- {0} ----\r\n{1}", DateTime.UtcNow.ToString("HH:mm:ss MM.dd.yyyy"), text);
            form.WriteRespLog(text, false);
        }

        public static void WriteInvoke(string text)
        {
            if (form == null) return;
            text = text?.Replace("\r", "").Replace("\n", "\r\n") + "\r\n";
            text = string.Format("---- {0} ----\r\n{1}", DateTime.UtcNow.ToString("HH:mm:ss MM.dd.yyyy"), text);
            form.WriteInvLog(text, false);
        }

        public static void WriteLn(string text)
        {
            Write(text?.Replace("\r", "").Replace("\n", "\r\n") + "\r\n");
        }

        public static void WriteDatedLn(string text)
        {
            WriteLn(string.Format("{0}: {1}", DateTime.UtcNow.ToString("yyyyMMddHHmmss"), text));
        }
    }
}
