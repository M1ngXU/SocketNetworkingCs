using System;
using System.Threading;

namespace UnitySocketManager
{
	internal abstract class Connection
	{
		protected virtual int MaxMessageSize { get; } = int.MaxValue;
		protected MessageReceiver MessageReceiver = new MessageReceiver();

		protected abstract void SendRaw(byte[] data);

		internal abstract void Start(Action<Message> OnMessage);

		/// <summary>
		/// Sends a message.
		/// </summary>
		internal void Send(Message m)
		{
			foreach (byte[] data in m.GetSendableChunks(MaxMessageSize))
			{
				SendRaw(data);
			}
		}
		/// <summary>
		/// Sends the message & calls the function when a response is received
		/// </summary>
		internal void Send(Message m, Action<Message> a)
		{
			m.IsCallback = true;
			MessageReceiver.AddListener(m.Id, a);
			Send(m);
		}
		/// <summary>
		/// Sends a message and returns the Message that is responded, blocking with a timeout.
		/// </summary>
		internal Message BlockUntilCallback(Message m, int timeout = -1)
		{
			Message r = null;
			CancellationTokenSource cts = new CancellationTokenSource();
			Send(m, n =>
			{
				r = n;
				cts.Cancel();
			});
			cts.Token.WaitHandle.WaitOne(timeout);
			return r;
		}
	}
}
