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

		public static class Colors {
			public const string White = Color + "0";
			public const string Black = Color + "1";
			public const string Blue = Color + "2";
			public const string Green = Color + "3";
			public const string Red = Color + "4";
			public const string Brown = Color + "5";
			public const string Purple = Color + "6";
			public const string Orange = Color + "7";
			public const string Yellow = Color + "8";
			public const string Lime = Color + "9";
			public const string Greenish = Color + "10";
			public const string Cyan = Color + "11";
			public const string GrayBlue = Color + "12";
			public const string RedPurple = Color + "13";
			public const string DarkGray = Color + "14";
			public const string LightGray = Color + "15";

			public static string Colored(string Clr, string Msg) {
				return Clr + Msg + Reset;
			}
		}
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
