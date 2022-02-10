using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitySocketManager
{
	internal class Message
	{
		internal bool IsCallback;
		internal bool IsResponse;
		internal byte Id;
		internal byte[] Content;
		internal bool CompletesOther;
		internal bool ExpectNext;

		private const byte SP_COMPLETION = 0b0010_0000;
		private const byte SP_AWAITING_NEXT = 0b0001_0000;
		private const byte SP_CALLBACK = 0b0000_1000;
		private const byte SP_RESPONSE = 0b0000_0100;
		private const byte SP_LENGTH = 0b0000_0011;

		internal Message(byte[] buf, Func<byte[], int> read)
		{
			int content_length_length = buf[0] & SP_LENGTH;

			// set flags - if any of them are true a id exists
			if ((CompletesOther = (buf[0] & SP_COMPLETION) == SP_COMPLETION)
				|| (ExpectNext = (buf[0] & SP_AWAITING_NEXT) == SP_AWAITING_NEXT)
				|| (IsCallback = (buf[0] & SP_CALLBACK) == SP_CALLBACK)
				|| (IsResponse = (buf[0] & SP_RESPONSE) == SP_RESPONSE))
			{
				buf = new byte[1];
				read(buf);
				Id = buf[0];
			}

			if (content_length_length > 0)
			{
				buf = new byte[content_length_length];
				read(buf);

				int content_length = buf.Select(a => (int)a).Aggregate((a, b) => (a << 8) | b);
				if (content_length > 0)
				{
					Content = new byte[content_length];
					read(Content);
				}
			}
		}

		internal Message(
			byte[] Content,
			byte Id = 0,
			bool IsCallback = false,
			bool IsResponse = false,
			bool CompletesOther = false,
			bool ExpectNext = false
		)
		{
			this.IsCallback = IsCallback;
			this.IsResponse = IsResponse;
			this.Id = Id;
			this.Content = Content;
			this.CompletesOther = CompletesOther;
			this.ExpectNext = ExpectNext;
		}

		internal void SetResponse(byte[] content, bool isCallback = false)
		{
			IsCallback = isCallback;
			Content = content;
			IsResponse = true;
		}

		internal byte[][] GetSendableChunks(int max_size)
		{
			List<byte[]> chunks = new List<byte[]>();
			int offset = 0;
			do
			{
				int remaining_length = Content.Length - offset;
				int next_package_length = Math.Min(Math.Min(remaining_length, 8 * ushort.MaxValue), max_size - 5);

				List<byte> buf = new List<byte> { 0 };

				if (IsCallback) buf[0] |= SP_CALLBACK;
				if (IsResponse) buf[0] |= SP_RESPONSE;
				if (next_package_length < remaining_length) buf[0] |= SP_AWAITING_NEXT;
				if (offset != 0) buf[0] |= SP_COMPLETION;

				if (buf[0] != 0) buf.Add(Id);

				buf[0] |= 0b11;
				bool b = false;
				for (int i = 2 * 8; i >= 0; i -= 8)
				{
					int mask = 0xFF << i;
					if ((mask & next_package_length) != 0)
					{
						buf.Add((byte)((mask & next_package_length) >> i));
						b = true;
					}
					else if (!b)
					{
						buf[0]--;
					}
				}

				buf.AddRange(Content.Skip(offset).Take(next_package_length));

				chunks.Add(buf.ToArray());
				offset += next_package_length;
			} while (offset < Content.Length);
			return chunks.ToArray();
		}
	}
}
