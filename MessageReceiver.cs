using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitySocketManager
{
	internal class MessageReceiver
	{
		private readonly Action<Message>[] CallbackMessages = new Action<Message>[byte.MaxValue];
		private readonly Message[] OpenMessages = new Message[byte.MaxValue];

		internal MessageReceiver() { }

		internal Message HandleMessage(byte header, Func<byte[], int> get_next)
		{
			Message m = new Message(new byte[] { header }, get_next);

			// start of a longer package
			if (m.ExpectNext && !m.CompletesOther) OpenMessages[m.Id] = m;
			if (m.CompletesOther)
			{
				m.Content = OpenMessages[m.Id].Content.Concat(m.Content).ToArray();
				OpenMessages[m.Id] = m;
				if (!m.ExpectNext) OpenMessages[m.Id] = null;
			}

			if (!m.ExpectNext)
			{
				if (m.IsResponse)
				{
					CallbackMessages[m.Id](m);
				}
				else
				{
					return m;
				}
			}
			return null;
		}

		internal void AddListener(byte id, Action<Message> a) => CallbackMessages[id] = a;
	}
}
