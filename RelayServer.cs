using System;
using System.Linq;

namespace UnitySocketManager
{
	public class RelayServer : Connection, ILogger
	{
		private readonly RelayListener RelayListener;
		private readonly ULongId ULongId;

		string ILogger.Name => "Relay-Server";

		public RelayServer(ULongId id, RelayListener rl)
		{
			ULongId = id;
			RelayListener = rl;
		}

		protected override void SendRaw(byte[] data) => RelayListener.SendTo(ULongId, data);

		public override void Start(Action<Message> OnMessage)
		{
			RelayListener.Listeners.Add(b =>
			{
				if (!Enumerable.SequenceEqual(b.Take(ULongId.GetBytes().Length), ULongId.GetBytes())) return;
				b = b.SkipWhile((_, i) => i < ULongId.GetBytes().Length).ToArray();
				this.Log("Server <== Relay: " + b.Length + " bytes");
				int offset = 1;
				Message m = MessageReceiver.HandleMessage(b[0], buf =>
				{
					int copied = buf.Length - (offset > b.Length ? offset - b.Length : 0);
					Array.Copy(b, offset, buf, 0, copied);
					offset += copied;
					return copied;
				});
				if (m != null) OnMessage(m);
			});
		}
	}
}
