using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BasicWebServer
{
    public class Program
    {
        static void Main(string[] args)
        {

            // 1.	Create the Web Server

            var ipAddress = IPAddress.Parse("127.0.0.1"); // use the IPAddress class to create an IP address instance with the locahost address

            var port = 8080;  // choose a port, on which our console app will work, our app will be on http://localhost:8080. 

            var serverListener = new TcpListener(ipAddress, port); // TCPListener class, which allows us to accept requests from the browser:

            serverListener.Start();

            Console.WriteLine($"Server started on port {port}.");
            Console.WriteLine("Listening for requests ...");

            while (true)
            {
                var connection = serverListener.AcceptTcpClient(); // to make our server wait for the browser to connect to it.
                                                                   // Get the connection from the browser

                var networkStream = connection.GetStream(); // Create a stream, through which data is received or sent to the browser
                                                            // as a byte array

                var content = "Hello from the server!"; // Create a message, which will be sent,

                var contentLength = Encoding.UTF8.GetByteCount(content); //  get its length in bytes (bytes length is often different from the string length)
                                                                         // Write the response    
                string responce = $@"HTTP/1.1 200 OK 
Content-Type: text/plain; charset=UTF-8
Content-Length: {contentLength}

{content}";

                var responseBytes = Encoding.UTF8.GetBytes(responce);  // Use the network stream to send the response bytes to the browser
                networkStream.Write(responseBytes);

                connection.Close();   // close the connection 
            }


        }
    }
}
