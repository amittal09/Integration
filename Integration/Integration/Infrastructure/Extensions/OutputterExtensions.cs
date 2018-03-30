using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Vertica.Integration.Infrastructure.Extensions
{
	public static class OutputterExtensions
	{
		public static string ReadLine(this TextWriter outputter, string message = "Enter value: ")
		{
			if (outputter == null)
			{
				throw new ArgumentNullException("outputter");
			}
			if (!string.IsNullOrWhiteSpace(message))
			{
				outputter.Write(message);
			}
			if (!Environment.UserInteractive)
			{
				return null;
			}
			return Console.ReadLine();
		}

		public static string ReadPassword(this TextWriter outputter, string message = "Enter password: ", char mask = '*')
		{
			if (outputter == null)
			{
				throw new ArgumentNullException("outputter");
			}
			if (!string.IsNullOrWhiteSpace(message))
			{
				outputter.Write(message);
			}
			if (!Environment.UserInteractive)
			{
				return null;
			}
			int[] numArray = new int[] { 0, 27, 9, 10 };
			Stack<char> chrs = new Stack<char>();
			while (true)
			{
				char keyChar = Console.ReadKey(true).KeyChar;
				char chr = keyChar;
				char chr1 = keyChar;
				if (chr == '\r')
				{
					break;
				}
				if (chr1 == '\b')
				{
					if (chrs.Count > 0)
					{
						outputter.Write("\b \b");
						chrs.Pop();
					}
				}
				else if (chr1 == '\u007F')
				{
					while (chrs.Count > 0)
					{
						outputter.Write("\b \b");
						chrs.Pop();
					}
				}
				else if (numArray.Count<int>((int x) => chr1 == (char)x) <= 0)
				{
					chrs.Push(chr1);
					outputter.Write(mask);
				}
			}
			outputter.WriteLine();
			return new string(chrs.Reverse<char>().ToArray<char>());
		}

		public static void RepeatUntilEscapeKeyIsHit(this TextWriter outputter, Action repeat)
		{
			if (outputter == null)
			{
				throw new ArgumentNullException("outputter");
			}
			if (repeat == null)
			{
				throw new ArgumentNullException("repeat");
			}
			do
			{
				repeat();
				outputter.WriteLine("Press ESCAPE to break or any key to continue");
				outputter.WriteLine();
			}
			while (OutputterExtensions.WaitingForEscape());
		}

		private static bool WaitingForEscape()
		{
			if (!Environment.UserInteractive)
			{
				return true;
			}
			return Console.ReadKey(true).Key != ConsoleKey.Escape;
		}

		public static void WaitUntilEscapeKeyIsHit(this TextWriter outputter, string message = "Press ESCAPE to continue...")
		{
			if (outputter == null)
			{
				throw new ArgumentNullException("outputter");
			}
			do
			{
				if (string.IsNullOrWhiteSpace(message))
				{
					continue;
				}
				outputter.WriteLine(message);
				outputter.WriteLine();
			}
			while (OutputterExtensions.WaitingForEscape());
		}
	}
}