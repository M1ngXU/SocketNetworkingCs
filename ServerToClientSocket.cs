using System;
using System.Net.Sockets;

namespace UnitySocketManager
{
	public class ServerToClientSocket : Connection
	{
		protected override int MaxMessageSize => Socket.SendBufferSize;

		private readonly Socket Socket;

		public ServerToClientSocket(Socket s) => Socket = s;

		protected override void SendRaw(byte[] data) => Socket.Send(data);

		public override void Start(Action<Message> OnMessage) => Socket.ReceiveMessages(MessageReceiver, OnMessage);
	}
}
