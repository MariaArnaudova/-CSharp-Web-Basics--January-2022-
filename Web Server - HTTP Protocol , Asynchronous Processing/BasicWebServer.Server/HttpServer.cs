using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server
{
    public class HttpServer
    {

        private readonly IPAddress ipAddress;
        private readonly int port;
        private readonly TcpListener servreListener;

        private readonly RoutingTable rountingTable;
        public HttpServer(string ipAddress,
            int port,
            Action<IRoutingTable> routingTableConfiguration)
        {
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
            this.servreListener = new TcpListener(this.ipAddress, port);

            routingTableConfiguration(this.rountingTable = new RoutingTable());
        }

        public HttpServer(int port, Action<IRoutingTable> routigTable)
            : this("127.0.0.1", port, routigTable)
        {

        }
        public HttpServer(Action<IRoutingTable> routigTable)
            : this(8080, routigTable)
        {

        }
        public async Task Start()  // Start() method to start the server 
        {
            this.servreListener.Start();

            Console.WriteLine($"Server started on port {port}...");

            Console.WriteLine("Listening for requests...");
            //read the request, parse it to an HTTP request,
            //get the response of that request from the routing table
            //and write the response to the network stream
            while (true)
            {

                var connection = await servreListener.AcceptTcpClientAsync();
                _ = Task.Run(async () =>
                  {
                      var networkStream = connection.GetStream();

                      var requestText = await this.ReadRequest(networkStream);

                      //Console.WriteLine(requestText);

                      //WriteResponce(networkStream, "Hello from the server!");

                      Console.WriteLine(requestText);

                      var request = Request.Parse(requestText);

                      var response = this.rountingTable.MatchRequest(request); // use the MatchRequest(Request request) method of the RoutingTable class to get the response

                      if (response.PreRenderAction != null)                      
                          response.PreRenderAction(request, response);
                     

                      AddSession(request, response);

                      await WriteResponse(networkStream, response);

                      connection.Close();

                 });
            }
        }

        private static void AddSession(Request request, Response response)
        {
            var sessionExist = request.Session
                .ContainsKey(Session.SessionCurrentDateKey);

            if (!sessionExist)
            {
                request.Session[Session.SessionCurrentDateKey]
                    = DateTime.Now.ToString();
                response.Cookies
                    .Add(Session.SessionCookieName, request.Session.Id);                   
            }
        }

        private async Task WriteResponse(NetworkStream networkStream, Response response)  // special method for writing the response in the network stream.
        {
            //            var contentLength = Encoding.UTF8.GetByteCount(message);
            //            var responce = $@"HTTP 1.1 200 OK
            //Content-type: text-plain; charset=UTF-8
            //Content-length: {contentLength}

            //{message}";

            var resposeBytes = Encoding.UTF8.GetBytes(response.ToString());
            await networkStream.WriteAsync(resposeBytes);
        }

        private async Task<string> ReadRequest(NetworkStream networkStream) // provides us with the HTTP request from the browser as a string
        {
            var bufferLegth = 1024;  // buffer for reading will have a length of 1024 bytes and will be a byte array
            var buffer = new byte[bufferLegth];

            var requesrBuilder = new StringBuilder(); // StringBuilder, to which we will append the request strings and which will be returned as a string to the method

            // read bytes from the network stream, parse them into a string
            // and append the string to the StringBuilder.
            do
            {
                var bytesRead = await networkStream.ReadAsync(buffer, 0, bufferLegth);
                requesrBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

            } while (networkStream.DataAvailable);

            return requesrBuilder.ToString();
        }
    }
}
