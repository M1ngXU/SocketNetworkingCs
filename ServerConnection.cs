using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UnitySocketManager
{
	internal class ServerConnection: Connection
	{
		protected override int MaxMessageSize => Socket.SendBufferSize;

		private readonly Socket Socket;

		internal ServerConnection(Socket s) => Socket = s;

		protected override void SendRaw(byte[] data) => Socket.Send(data);

		internal override void Start(Action<Message> OnMessage) => Socket.ReceiveMessages(MessageReceiver, OnMessage);
	}
}
