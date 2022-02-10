using System;
using System.Net;
using System.Net.Sockets;

namespace UnitySocketManager
{
	internal class SocketClient : Connection, ILogger
	{
		string ILogger.Name => "Socket-Client";

		protected override int MaxMessageSize => Socket.SendBufferSize;

		private readonly IPEndPoint IPEndPoint;
		private readonly ULongId ULongId = new ULongId();
		private readonly Socket Socket;

		internal SocketClient(IPEndPoint ip, ULongId id = null)
		{
			IPEndPoint = ip;
			if (id != null) ULongId = id;
			Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
		}

		protected override void SendRaw(byte[] data) => Socket.Send(data);

		internal override void Start(Action<Message> OnMessage)
		{
			Socket.Connect(IPEndPoint);
			Socket.Send(ULongId.GetBytes());
			this.Log(ULongId + " Connected to the server and completed handshaking.");

			Socket.ReceiveMessages(MessageReceiver, OnMessage);
		}
	}
}
