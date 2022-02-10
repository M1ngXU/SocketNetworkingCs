using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnitySocketManager;

namespace LocalRelayServer
{
	internal class FakeClient : ILogger
	{
		string ILogger.Name => "Fake-Client";

		private readonly Socket Socket;

		internal FakeClient(IPEndPoint endPoint, ULongId id, Action<byte[]> OnReceive)
		{
			Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			Socket.Connect(endPoint);
			Send(id.GetBytes());
			this.Log(id + " connected to the server");
			Task.Run(() =>
			{
				while (Socket.IsConnected())
				{
					byte[] buffer = new byte[1024];
					buffer = buffer[..Socket.Receive(buffer)];
					OnReceive(buffer);
					this.Log(id + " <== Relay: " + buffer.Length + " bytes");
				}
				Socket.Close();
				this.Log(id + " closed Socket.");
			});
		}

		internal void Send(byte[] content) => Socket.Send(content);
	}
}
