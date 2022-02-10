using System;
using System.Net;
using System.Net.Sockets;

namespace UnitySocketManager
{
	public class SocketClient : Connection, ILogger
	{
		string ILogger.Name => "Socket-Client";

		protected override int MaxMessageSize => Socket.SendBufferSize;

		private readonly IPEndPoint IPEndPoint;
		private readonly ULongId ULongId = new ULongId();
		private readonly Socket Socket;

		public SocketClient(IPEndPoint ip, ULongId id = null)
		{
			IPEndPoint = ip;
			if (id != null) ULongId = id;
			Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
		}

		protected override void SendRaw(byte[] data) => Socket.Send(data);

		public override void Start(Action<Message> OnMessage)
		{
			Socket.Connect(IPEndPoint);
			Socket.Send(ULongId.GetBytes());
			this.Log(ULongId + " Connected to the server and completed handshaking.");

			Socket.ReceiveMessages(MessageReceiver, OnMessage);
		}
	}
}
