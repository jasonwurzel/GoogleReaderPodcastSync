using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using npantarhei.runtime;
using PGK;
using PGK.Extensions;

namespace UnitTests
{
	public class ReplaceStreamContentCore
	{
		private StreamReader _input;
		private StreamWriter _output;
		private List<Replacement> _replacements;
		private int _bufferSize;

		public ReplaceStreamContentCore(StreamReader input, StreamWriter output, int bufferSize, List<Replacement> replacements)
		{
			_input = input;
			_output = output;
			_replacements = replacements;
			_bufferSize = bufferSize;
		}

		/// <summary>
		/// Kernfunktionalität von ReplaceFileContent
		/// </summary>
		public void Replace()
		{
			FlowRuntimeConfiguration config = new FlowRuntimeConfiguration();
			config.AddStreamsFrom(@"
									.in, getBytesToRead
									getBytesToRead, readXBytes
									readXBytes, getLastEndPosition
									getLastEndPosition, replace
									replace, getBytesToWrite
									getBytesToWrite, writeBytes
									writeBytes, deleteWrittenBuffer
									deleteWrittenBuffer, checkEndOfStream
									checkEndOfStream.out0, getBytesToRead
									checkEndOfStream.out1, .out");

			config.AddFunc<StringBuilder, Tuple<int, StringBuilder>>("getBytesToRead", getBytesToRead);
			config.AddFunc<Tuple<int, StringBuilder>, StringBuilder>("readXBytes", readBytes);
			config.AddFunc<StringBuilder, Tuple<int, StringBuilder>>("getLastEndPosition", getLastEndPosition);
			config.AddFunc<Tuple<int, StringBuilder>, Tuple<int, StringBuilder>>("replace", replace);
			config.AddFunc<Tuple<int, StringBuilder>, Tuple<int, StringBuilder>>("getBytesToWrite", getBytesToWrite);
			config.AddFunc<Tuple<int, StringBuilder>, Tuple<int, StringBuilder>>("writeBytes", writeBytes);
			config.AddFunc<Tuple<int, StringBuilder>, StringBuilder>("deleteWrittenBuffer", deleteWrittenBuffer);
			config.AddAction<StringBuilder, StringBuilder>("checkEndOfStream", checkEndOfStream);

			using (FlowRuntime fr = new FlowRuntime(config))
			{
				StringBuilder erg = new StringBuilder();
				fr.Process(".in", erg);
				fr.WaitForResult(_ => erg = (StringBuilder)_.Data);
			}
		}

		private void checkEndOfStream(StringBuilder buffer, Action<StringBuilder> continueIfNoEnd, Action continueIfEnd)
		{
			if (_input.EndOfStream)
			{
				writeBytes(new Tuple<int, StringBuilder>(buffer.Length, buffer));
				continueIfEnd();
			}
			else
				continueIfNoEnd(buffer);
		}

		private Tuple<int, StringBuilder> getBytesToRead(StringBuilder buffer)
		{
			int bytesToRead = _bufferSize - buffer.Length;
			return new Tuple<int, StringBuilder>(bytesToRead, buffer);
		}

		private StringBuilder deleteWrittenBuffer(Tuple<int, StringBuilder> tuple)
		{
			int bytesWritten = tuple.Item1;
			StringBuilder wholeString = tuple.Item2;

			StringBuilder erg = wholeString.Remove(0, bytesWritten);

			return erg;
		}

		private Tuple<int, StringBuilder> writeBytes(Tuple<int, StringBuilder> bytesToWriteAndReplacedString)
		{
			StringBuilder replacedString = bytesToWriteAndReplacedString.Item2;
			int bytesToWrite = bytesToWriteAndReplacedString.Item1;
			if (replacedString.Length < bytesToWrite)
			{
				bytesToWrite = replacedString.Length;
			}
			_output.Write(replacedString.ToString(0, bytesToWrite));

			return new Tuple<int, StringBuilder>(bytesToWrite, replacedString);
		}

		private Tuple<int, StringBuilder> getBytesToWrite(Tuple<int, StringBuilder> lastReplacementAndReplacedString)
		{
			int lastReplacement = lastReplacementAndReplacedString.Item1;
			StringBuilder replacedString = lastReplacementAndReplacedString.Item2;

			var firstOrDefault = _replacements.OrderByDescending(rep => rep.OldValue.Length).FirstOrDefault();
			int bytesToWrite = replacedString.Length;
			if (firstOrDefault != null)
			{
				int biggestReplacement = firstOrDefault.OldValue.Length;

				bytesToWrite = lastReplacement > ((_bufferSize - 1) - biggestReplacement)
					               ? lastReplacement + 1
					               : (_bufferSize - (biggestReplacement));
			}

			return new Tuple<int, StringBuilder>(bytesToWrite, replacedString);
		}

		private Tuple<int, StringBuilder> replace(Tuple<int, StringBuilder> lastReplacementAndString)
		{
			StringBuilder stringToOperateOn = lastReplacementAndString.Item2;
			foreach (var replacement in _replacements)
			{
				stringToOperateOn = stringToOperateOn.Replace(replacement.OldValue, replacement.NewValue);
			}

			return new Tuple<int, StringBuilder>(lastReplacementAndString.Item1, stringToOperateOn);
		}

		private Tuple<int, StringBuilder> getLastEndPosition(StringBuilder arg)
		{
			int index = -1;
			// TODO mke: Performance
			string temp = arg.ToString();
			foreach (var replacement in _replacements)
			{
				int lastIndexOf = temp.LastIndexOf(replacement.OldValue) + replacement.OldValue.Length - 1;
				index = lastIndexOf > index ? lastIndexOf : index;
			}

			return new Tuple<int, StringBuilder>(index, arg);
		}

		private StringBuilder readBytes(Tuple<int, StringBuilder> tuple)
		{
			int countToRead = tuple.Item1;
			StringBuilder erg = tuple.Item2;
			char[] buffer = new char[countToRead];
			int readBytes = _input.ReadBlock(buffer, 0, countToRead);

			


			// Übers Streamende hinausgeschossen?
			if (readBytes < countToRead)
				erg.Append(new string(buffer).Substring(0, readBytes));
			else
				erg.Append(new string(buffer));

			return erg;
		}
	}

	/// <summary>
	/// Zu ersetzendes Objekt
	/// </summary>
	public class Replacement
	{
		private string _oldValue = null;
		private string _newValue = null;
		/// <summary>
		/// Alter Wert
		/// </summary>
		public string OldValue
		{
			get { return _oldValue; }
			set
			{
				_oldValue = value;
				if (!string.IsNullOrEmpty(_oldValue))
				{
					_oldValue = _oldValue.Replace("\\n", "\n");
					_oldValue = _oldValue.Replace("\\r", "\r");
				}
			}
		}
		/// <summary>
		/// Neuer Wert
		/// </summary>
		public string NewValue
		{
			get { return _newValue; }
			set
			{
				_newValue = value;
				if (!string.IsNullOrEmpty(_newValue))
				{
					_newValue = _newValue.Replace("\\n", "\n");
					_newValue = _newValue.Replace("\\r", "\r");
				}
			}
		}
	}

}
