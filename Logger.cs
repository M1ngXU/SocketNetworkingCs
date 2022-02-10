using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySocketManager
{
	internal interface ILogger
	{
		internal string Name { get; }
	}

	internal static class ILoggerEx
	{
		internal static bool LOG = true;
		internal static readonly DateTime Start = DateTime.Now;

		internal static void Log(this ILogger l, string s)
		{
			if (LOG) Debug.Log(
				DateTime.Now.Subtract(Start).ToString(@"\[hh\.mm\.ss\.ffff\]\ ")
				+ l.Name.ToUpper().PadRight(20) + s
			);
		}

		internal static void Log(this ILogger l, IEnumerable<byte> bytes) =>
			Log(l, BitConverter.ToString(
				bytes
					.Reverse()
					.SkipWhile(b => b == 0)
					.Reverse()
					.ToArray()
			));
	}
}
