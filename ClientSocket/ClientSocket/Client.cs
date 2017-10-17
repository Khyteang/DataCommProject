using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace ClientSocket
{
	class Client
	{
		private static int SERVER_IP = 0;
		private static int SERVER_PORT = 1;
		private static int METHOD = 2;
		private static int FILENAME = 3;

		private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		static void Main(string[] args)
		{
			var serverIP = args[SERVER_IP];
			var serverPort = Convert.ToInt32(args[SERVER_PORT]);
			var method = args[METHOD];
			var filename = args[FILENAME];

			ConnectServer(serverIP, serverPort);
			SendRequest(serverIP, method.ToUpper(), filename);
			Console.ReadLine();
		}

		/**
		* Create request with the following method and filename  
		**/
		private static string CreateRequest(string serverIP, string method, string filename)
		{
			var request = $"{method} /{filename} HTTP/1.0\r\n" +
					$"Host: {serverIP}\r\n" +
					"Connection: keep-alive\r\n" +
					"Accept: text/html\r\n" +
					"User-Agent: CSharpTests\r\n\r\n";

			//if method is a PUT, read in the file and send in the body of the request
			if (method == "PUT")
			{
				var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
				var textFromFile = File.ReadAllText(path + "/" +filename).Replace("\0", string.Empty);
				request += $"Body: {textFromFile}\r\n";
			}

			return request;
		}

		/**
		 * Build request and receive response
		 **/
		private static void SendRequest(string serverIP, string method, string filename)
		{
			var request = CreateRequest(serverIP, method, filename);
			_clientSocket.Send(Encoding.ASCII.GetBytes(request));

			var receivedBuf = new byte[2048];
			_clientSocket.Receive(receivedBuf);
			var respString = Encoding.ASCII.GetString(receivedBuf);
			HandleResponse(respString, method, filename);
		}

		/**
		 * Once we get a respone from server, handle it properly
		 **/
		private static void HandleResponse(string response, string method, string filename)
		{
			var respArray = response.Split(
						new[] { "\r\n", "\r", "\n" },
						StringSplitOptions.None
					);

			var statusCode = respArray[0].Split(' ')[1];

			if (statusCode == "200" && method == "GET")
			{
				Console.WriteLine(response);
				var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
				var body = respArray[5].Substring(5).Replace("\0", string.Empty);
				File.WriteAllText(path + '/' + filename, body);
			}
			else if (statusCode == "401" && method == "GET")
			{
				Console.WriteLine($"Server cannot find file name {filename}");
			}
			else if (statusCode == "200" && method == "PUT")
			{
				Console.WriteLine($"Server successfully write filename {filename}");
			}
			else if (statusCode == "401" && method == "PUT")
			{
				Console.WriteLine($"Server failed to save filename {filename}");
			}
			else
			{
				Console.WriteLine("Server did not response with an acceptable status code");
			}
		}

		/**
		 * Connecting to server
		 **/
		private static void ConnectServer(string serverIP, int port)
		{
			while (!_clientSocket.Connected)
			{
				try
				{
					_clientSocket.Connect(serverIP, port);
				}
				catch (SocketException)
				{
					Console.WriteLine("Retrying to connect to server...");
				}
			}
			Console.Clear();
			Console.WriteLine("Client Connected...");

		}
	}
}
