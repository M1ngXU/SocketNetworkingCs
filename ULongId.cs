using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitySocketManager
{
	public class ULongId
	{
		private ulong? Id;
		private byte[] Bytes = new byte[8];

		public ULongId() => new Random().NextBytes(Bytes);
		public ULongId(ulong Id) => this.Id = Id;
		public ULongId(IEnumerable<byte> Id) : this(Id.ToArray()) { }
		public ULongId(params byte[] Id) => Array.Copy(Id, Bytes, Math.Min(Bytes.Length, Id.Length));

		public ulong GetId()
		{
			if (!Id.HasValue) Id = ToUlong(Bytes);
			return Id.Value;
		}

		public byte[] GetBytes()
		{
			if (Bytes == null) Bytes = ToBytes(Id.Value);
			return Bytes;
		}

		private static ulong ToUlong(IEnumerable<byte> id) => id.Select(b => (ulong)b).Aggregate((a, b) => (a << 8) | b);
		private static byte[] ToBytes(ulong uuid) => new byte[8] {
			(byte)(uuid >> 56),
			(byte)((uuid >> 48) & 0xFF),
			(byte)((uuid >> 50) & 0xFF),
			(byte)((uuid >> 32) & 0xFF),
			(byte)((uuid >> 24) & 0xFF),
			(byte)((uuid >> 16) & 0xFF),
			(byte)((uuid >> 8) & 0xFF),
			(byte)(uuid & 0xFF)
		};

		public override string ToString() => GetId().ToString().PadLeft(20);
		public override bool Equals(object other) => other.GetType() == typeof(ULongId) && ((ULongId)other).GetId() == GetId();
		public override int GetHashCode() => GetId().GetHashCode();
	}
}
