using BasicWebServer.Server;
using BasicWebServer.Server.Controllers;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Responses;
using BasicWebServer.Server.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BasicWebServer.Demo
{
    public class Startup
    {
        private const string HtmlForm = @"<form action='/HTML' method='POST'>
Name: <input type='text' name='Name'/>
Age: <input type='number' name ='Age'/>
<input type='submit' value ='Save' />
</form>";

        private const string DownloadForm = @"<form action='/Content' method='POST'>
<input type='submit' value ='Download Sites Content' /> 
</form>";

        private const string FileName = "content.txt";

        private const string LoginForm = @"<form action='/Login' method='POST'>
   Username: <input type='text' name='Username'/>
   Password: <input type='text' name='Password'/>
   <input type='submit' value ='Log In' /> 
</form>";

        private const string Username = "user";
        private const string Password = "user123";
        public static async Task Main()
        {
            //await DownloadSitesAsTextFile(Startup.FileName,
            //    new string[] { "https://code.org/", "https://code.org/" });
            //await DownloadSitesAsTextFile(Startup.FileName,
            //new string[] { "https://judge.softuni.org/", "https://softuni.org/" });

            await new  HttpServer(routes => routes
                 .MapGet<HomeController>("/", c => c.Index())
                 .MapGet<HomeController>("/Redirect", c => c.Redirect())
                 .MapGet<HomeController>("/HTML", c => c.Html())
                 .MapPost<HomeController>("/HTML", c => c.HtmlFormPost())
                 .MapGet<HomeController>("/Content", c => c.Content())
                 .MapPost<HomeController>("/Content", c => c.DownloadContent())
                 .MapGet<HomeController>("/Cookies", c => c.Cookies()))
                 //.MapGet<HomeController>("/Session", c => c.Session()))
                 .Start();
            //.MapGet<HomeController>("/Login", new HtmlResponse(Startup.LoginForm))
            //.MapPost("/Login", new HtmlResponse("", Startup.LoginAction)));
           
        }

        private static void LoginAction(Request request, Response response)
        {
            request.Session.Clear();

            var bodyText = "";

            var usernameMatches = request.Form["Username"] == Startup.Username;
            var passwordMatches = request.Form["Password"] == Startup.Password;

            if (usernameMatches && passwordMatches)
            {
                request.Session[Session.SessionUserKey] = "MyUserId";
                response.Cookies.Add(Session.SessionCookieName, request.Session.Id);

                bodyText = "<h3>Logged successfully!</h3>";
            }
            else
            {
                bodyText = Startup.LoginForm;
            }

            response.Body = "";
            response.Body += bodyText;
        }


        //.MapPost("/HTML", new TextResponse("")))                 
        // set the action of our form to "/HTML" and the method to "POST".
        // This means that when the form is submitted, it will send a "POST" request to the "/HTML" path
        private static void DisplaySessionInfoAction(Request request, Response response)
        {
            var sessionExist = request.Session
                .ContainsKey(Session.SessionCurrentDateKey);

            var bodyText = "";

            if (sessionExist)
            {
                var currentDate = request.Session[Session.SessionCurrentDateKey];
                bodyText = $"Stored date: {currentDate}!";
            }
            else
            {
                bodyText = "Current date stored!";
            }

            response.Body = "";
            response.Body += bodyText;
        }
        private static void AddCookiesAction(Request request, Response response)
        {

            var requestHasCookies = request.Cookies
                         .Any(c => c.Name != Session.SessionCookieName);

            var bodyText = "";

            if (requestHasCookies)
            {
                var cookiesText = new StringBuilder();

                cookiesText.AppendLine("<hi>Cookies</h1>");
                cookiesText.Append("<table border='1'><tr><th>Name</th><th>Value</th></tr>");

                foreach (var cookie in request.Cookies)
                {
                    cookiesText.Append("<tr>");
                    cookiesText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Name)}</td>");
                    cookiesText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Value)}</td>");
                    cookiesText.Append("</tr>");
                }
                cookiesText.Append("</table>");

                bodyText = cookiesText.ToString();

            }
            else
            {
                bodyText = "<h1>Cookies set!</h1>";
                response.Cookies.Add("My-Cookie", "My-Value");
                response.Cookies.Add("My-Second-Cookie", "My-Second-Value");
            }

            response.Body = bodyText;

            //if (!requestHasCookies)
            //{
            //    response.Cookies.Add("My-Cookie", "My-Value");
            //    response.Cookies.Add("My-Second-Cookie", "My-Second-Value");
            //}
        }
        private static void AddFormDataAction(Request request, Response response)
        {
            response.Body = "";

            foreach (var (key, value) in request.Form)
            {
                response.Body += $"{key} - {value}";
                response.Body += Environment.NewLine;
            }
        }

     
    }
}
