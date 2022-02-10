using System;
using System.Net.Sockets;

namespace UnitySocketManager
{
	internal class ServerToClientSocket : Connection
	{
		protected override int MaxMessageSize => Socket.SendBufferSize;

		private readonly Socket Socket;

		internal ServerToClientSocket(Socket s) => Socket = s;

		protected override void SendRaw(byte[] data) => Socket.Send(data);

		internal override void Start(Action<Message> OnMessage) => Socket.ReceiveMessages(MessageReceiver, OnMessage);
	}
}
