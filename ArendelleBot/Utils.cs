using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace ArendelleBot {
    static class Fmt {
        public const string Bold = "\x02";
        public const string Italic = "\x1D";
        public const string Underline = "\x1F";
        public const string Color = "\x03";
        public const string Reset = "\x0F";
    }

    class Utils {
        public static void GetHtmlTitleAsync(string URL, Action<string> callback) {
            if(callback == null) return;
            try {
                var req = WebRequest.CreateHttp(URL);
                req.Method = WebRequestMethods.Http.Head;
                string[] ctype;
                using(var resp = req.GetResponse() as HttpWebResponse)
                    ctype = resp.ContentType.Split(';');
                if(ctype[0] != "text/html") return;

                req = WebRequest.CreateHttp(URL);
                req.Method = WebRequestMethods.Http.Get;
                using(var resp = req.GetResponse() as HttpWebResponse)
                using(var stream = resp.GetResponseStream()) {
                    var encd = Encoding.GetEncoding(resp.CharacterSet);
                    var reader = new StreamReader(stream, encd);
                    var data = reader.ReadToEnd();
                    var title = new Regex(@"<title>([^<]*)</title>").Match(data);
                    if(title.Success) callback(WebUtility.HtmlDecode(title.Groups[1].Value));
                }
            } catch(Exception ex) {
                Console.WriteLine("ERR({0}) = {1}", nameof(GetHtmlTitleAsync), ex.Message);
            }
        }

        public static string[] SimpleParse(string args) {
            return args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
