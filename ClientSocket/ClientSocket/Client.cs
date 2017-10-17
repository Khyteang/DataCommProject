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

			Console.Title = "Client";
			ConnectServer(serverIP, serverPort);
			SendRequest(serverIP, method.ToUpper(), filename);
			Console.ReadLine();
		}

		private static void SendRequest(string serverIP, string method, string filename)
		{
			//"127.0.0.1"
			var request = $"{method} / HTTP/1.1\r\n" +
				$"Host: {serverIP}\r\n" +
				"Connection: keep-alive\r\n" +
				"Accept: text/html\r\n" +
				"User-Agent: CSharpTests\r\n\r\n";
			//var request = $"{method},{filename}";
			_clientSocket.Send(Encoding.ASCII.GetBytes(request));

			byte[] receivedBuf = new byte[50240];
			var blob = _clientSocket.Receive(receivedBuf);
			string text = Encoding.ASCII.GetString(receivedBuf);
			var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
			Console.WriteLine(text);
			File.WriteAllText(path+'/'+filename, text);
			/*
			while(true)
			{
				_clientSocket.RemoteEndPoint
			}
			*/
		}

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
