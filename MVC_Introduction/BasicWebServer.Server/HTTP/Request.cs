using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BasicWebServer.Server.HTTP
{
    public class Request
    {
        private static Dictionary<string, Session> Sessions = new Dictionary<string, Session>();
        public Method Method { get; private set; }

        public string Url { get; private set; }

        public HeaderCollection Headers { get; private set; }

        public CookieCollection Cookies { get; private set; }

        public string Body { get; private set; }

        public Session Session { get; private set; }

        public IReadOnlyDictionary<string, string> Form { get; private set; }

        public static Request Parse(string request)  // To parse the request string to an HTTP request we need to first separate each line
                                                     // and get the first one
        {
            var lines = request.Split("\r\n");       // array with the method and the URL as strings

            var startLine = lines.First().Split(" ");

            var method = ParseMethod(startLine[0]);  // parse the given method string to an HTTP method

            var url = startLine[1];

            var headers = ParseHeaders(lines.Skip(1)); // Take the headers, starting from the second request line

            var cookies = ParseCookies(headers);

            var session = GetSession(cookies);

            int countHeaders = headers.Count;

            var bodyLines = lines.Skip(countHeaders + 2).ToArray();

            var body = string.Join("\r\n", bodyLines);

            var form = ParseForm(headers, body);

            return new Request
            {
                Method = method,
                Headers = headers,
                Cookies = cookies,
                Url = url,
                Body = body,
                Session = session,
                Form = form
            };
        }

        private static Session GetSession(CookieCollection cookies)
        {
            var sessionId = cookies.Contains(Session.SessionCookieName)
                ? cookies[Session.SessionCookieName]
                : Guid.NewGuid().ToString();

            if (!Sessions.ContainsKey(sessionId))
            {
                Sessions[sessionId] = new Session(sessionId);
            }

            return Sessions[sessionId];
        }

        private static CookieCollection ParseCookies(HeaderCollection headers)
        {
            var cookiesCollection = new CookieCollection();

            if (headers.Contains(Header.Cookie))
            {
                var cookieHeader = headers[Header.Cookie];
                var allCookies = cookieHeader.Split(';');

                foreach (var cookieText in allCookies)
                {
                    var cookiesParts = cookieText.Split('=');

                    var cookieName = cookiesParts[0].Trim();
                    var cookieValue = cookiesParts[1].Trim();

                    cookiesCollection.Add(cookieName, cookieValue);
                }
            }

            return cookiesCollection;
        }

        private static Dictionary<string, string> ParseForm(HeaderCollection headers, string body)
        {
            var formCollection = new Dictionary<string, string>();

            if (headers.Contains(Header.ContentType)
                && headers[Header.ContentType] == ContentType.FormUrlEncoded)
            {
                var parseResult = ParseFormData(body);

                foreach (var (name, value) in parseResult)
                {
                    formCollection.Add(name, value);
                }
            }

            return formCollection;
        }

        private static Dictionary<string, string> ParseFormData(string bodyLines)
       => HttpUtility.UrlDecode(bodyLines)
            .Split('&')
            .Select(part => part.Split('='))
            .Where(part => part.Length == 2)
            .ToDictionary(
           part => part[0],
           part => part[1],
           StringComparer.InvariantCultureIgnoreCase);

        private static Method ParseMethod(string method)
        {
            try
            {
                return (Method)Enum.Parse(typeof(Method), method, true);
            }
            catch (Exception)
            {

                throw new InvalidOperationException($"Method '{method}' is not supported");
            }
        }

        private static HeaderCollection ParseHeaders(IEnumerable<string> headerLines)
        {
            var headerCollection = new HeaderCollection();

            foreach (var headerLine in headerLines)
            {
                if (headerLine == String.Empty)
                {
                    break;
                }

                var headerParts = headerLine.Split(":", 2);

                if (headerParts.Length != 2)
                {
                    throw new InvalidOperationException("Request headers is not invalid");
                }

                var headerName = headerParts[0];
                var headerValue = headerParts[1];

                headerCollection.Add(headerName, headerValue.Trim());
            }

            return headerCollection;
        }
    }
}
