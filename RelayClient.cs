using System;

namespace UnitySocketManager
{
	internal class RelayClient : Connection, ILogger
	{
		string ILogger.Name => "Relay-Client";

		protected override int MaxMessageSize => int.MaxValue;

		private readonly RelayListener RelayListener;
		private readonly ULongId ULongId;

		internal RelayClient(ULongId id)
		{
			RelayListener = new RelayListener(id);
			ULongId = id;
		}

		internal override void Start(Action<Message> OnMessage)
		{
			RelayListener.Start(b =>
			{
				this.Log(ULongId + " <== Relay: " + b.Length + " bytes");
				int offset = 1;
				Message m = MessageReceiver.HandleMessage(b[0], buf =>
				{
					int copied = buf.Length - (offset > b.Length ? offset - b.Length : 0);
					Array.Copy(b, offset, buf, 0, copied);
					offset += copied;
					return copied;
				});
				if (m != null) OnMessage(m);
			}).GetAwaiter().GetResult();
			#region
			/*
			_ = Task.Run(async () =>
			{
				Socket Server = new Socket(SocketType.Stream, ProtocolType.Tcp);
				Server.Bind(ip);
				Server.Listen(1);

				this.Log("Awaiting a client to connect ...");

				Socket Client = Server.Accept();

				byte[] b = new byte[8];
				Client.Receive(b);
				ULongId id = new ULongId(b);

				this.Log(id + " client connected");

				RelayListener rl = new RelayListener(id);

				await rl.Start(b =>
				{
					this.Log(id + " <== Relay: " + b.Length + " bytes");
					Client.Send(b);
				});

				while (Client.IsConnected())
				{
					byte[] buffer = new byte[1024];
					buffer = buffer[..Client.Receive(buffer)];
					this.Log(id + " ==> Relay: " + buffer.Length + " bytes");
					await rl.SendBinary(buffer);
				}
				this.Log(id + " lost connection");
				Client.Close();
			});
			*/
			#endregion
		}

		protected override void SendRaw(byte[] data) => RelayListener.SendBinary(data);
	}
}
