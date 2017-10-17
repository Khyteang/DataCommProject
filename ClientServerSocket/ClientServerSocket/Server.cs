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

		private static void SetupServer(int port)
		{
			Console.WriteLine("Setting up server...");
			_serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			_serverSocket.Listen(10);
			_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

		}

		private static void AcceptCallback(IAsyncResult ar)
		{
			Socket socket = _serverSocket.EndAccept(ar);
			_clientSocket.Add(socket);
			Console.WriteLine("Client connects...");
			socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
			_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

		}

		private static void ReceiveCallback(IAsyncResult ar)
		{
			Socket socket = (Socket)ar.AsyncState;
			int received = socket.EndReceive(ar);
			byte[] dataBuf = new byte[received];
			Array.Copy(_buffer, dataBuf, received);

			string text = Encoding.ASCII.GetString(dataBuf);
			Console.WriteLine("Text received: " + text);

			var req = text.Split(',');
			var method = req[0];
			var filename = req[1];
			var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
			if (method.ToUpper() == "GET")
			{
				string textFromFile = File.ReadAllText(path + "/" + filename);
				byte[] result = Encoding.ASCII.GetBytes(textFromFile);
				socket.BeginSend(result, 0, result.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
			}
			else if (method.ToUpper() == "PUT")
			{

			}
			else
			{

			}
		}

		private static void SendCallback(IAsyncResult ar)
		{
			Socket socket = (Socket)ar.AsyncState;
			socket.EndSend(ar);
		}
	}
}
