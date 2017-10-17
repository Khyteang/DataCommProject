using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ClientServerSocket
{
	class Server
	{
		private static byte[] _buffer = new byte[1024];
		private static List<Socket> _clientSocket = new List<Socket>();
		private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private static int SERVER_PORT = 0;

		static void Main(string[] args)
		{
			var serverPort = Convert.ToInt32(args[SERVER_PORT]);
			SetupServer(serverPort);
			Console.ReadLine();
		}

		/**
		 * Setting up server with given port number
		 * Listening for requests
		 **/
		private static void SetupServer(int port)
		{
			Console.WriteLine("Setting up server...");
			_serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			_serverSocket.Listen(10);
			_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
		}

		/**
		 * Once client has been connected, start accepting callback from client
		 **/
		private static void AcceptCallback(IAsyncResult ar)
		{
			Socket socket = _serverSocket.EndAccept(ar);
			_clientSocket.Add(socket);
			Console.WriteLine("Client connects...");
			socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
			_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
		}

		/**
		 * Server receives callback from client
		 **/
		private static void ReceiveCallback(IAsyncResult ar)
		{
			Socket socket = (Socket)ar.AsyncState;
			int received = socket.EndReceive(ar);
			byte[] dataBuf = new byte[received];
			Array.Copy(_buffer, dataBuf, received);

			string text = Encoding.ASCII.GetString(dataBuf);
			Console.WriteLine("Text received: " + text);

			var req = text.Split(
						new[] { "\r\n", "\r", "\n" },
						StringSplitOptions.None
					);

			var parse = req[0].Split(' ');
			var method = parse[0].Trim(' ');
			var filename = parse[1].Trim(' ');
			var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
			var responseByte = new byte[2048];
			if (method.ToUpper() == "GET")
			{
				try
				{
					var textFromFile = File.ReadAllText(path + filename);
					var response = $"HTTP/1.0 200 OK\r\n" +
						$"Data: {DateTime.UtcNow.Date} {DateTime.Now.ToString("HH:mm:ss tt")}\r\n" +
						$"Content-Type: text/html\r\n" +
						$"Content-Length: {textFromFile.Length}\r\n\r\n" +
						$"Body: {textFromFile}";
					responseByte = Encoding.ASCII.GetBytes(response);
				}
				catch (FileNotFoundException ex)
				{
					Console.WriteLine($"ERROR: File {filename} does not exist" + ex);
					var response = $"HTTP/1.0 200 OK\r\n" +
						$"Data: {DateTime.UtcNow.Date} {DateTime.Now.ToString("HH:mm:ss tt")}\r\n\r\n";
					responseByte = Encoding.ASCII.GetBytes(response);
				}
				
			}
			else if (method.ToUpper() == "PUT")
			{
				try
				{
					var body = req[6].Substring(5).Replace("\0", string.Empty);
					Console.WriteLine(body);
					File.WriteAllText(path + '/' + filename, body);
					var response = $"HTTP/1.0 200 OK\r\n" +
						$"Data: {DateTime.UtcNow.Date} {DateTime.Now.ToString("HH:mm:ss tt")}\r\n\r\n";
					responseByte = Encoding.ASCII.GetBytes(response);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"ERROR: Writing {filename}" + ex);
					var response = $"HTTP/1.0 500 Internal Server Error\r\n" +
						$"Data: {DateTime.UtcNow.Date} {DateTime.Now.ToString("HH:mm:ss tt")}\r\n\r\n";
					responseByte = Encoding.ASCII.GetBytes(response);
				}
			}
			else
			{
				Console.WriteLine($"Server cannot handle method {method}");
				var response = $"HTTP/1.0 302 Moved Temporarily\r\n\r\n";
				responseByte = Encoding.ASCII.GetBytes(response);
			}
			socket.BeginSend(responseByte, 0, responseByte.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
		}

		/**
		 * Send callback to client
		 **/
		private static void SendCallback(IAsyncResult ar)
		{
			Socket socket = (Socket)ar.AsyncState;
			socket.EndSend(ar);
		}
	}
}
