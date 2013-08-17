using System;
using System.Collections.Generic;
using System.Linq;

namespace Metazel
{
	public class MemoryMap
	{
		private readonly List<MemoryMapEntry> _map = new List<MemoryMapEntry>();

		public byte this[int address]
		{
			get
			{
				var entryAddressTuple = Find(address);
				var memoryProvider = entryAddressTuple.Item1.MemoryProvider;
				var relativeAddress = entryAddressTuple.Item2;

				if (memoryProvider is byte[])
					return ((byte[]) memoryProvider)[relativeAddress];
				else if (memoryProvider is IMemoryProvider)
					return ((IMemoryProvider) memoryProvider)[relativeAddress];
				else
				{
					Console.WriteLine("Reading from {0:X4}.", address); //TODO: Throw exception/don't allow other types of providers in Add().

					return 0;
				}
			}

			set
			{
				var entryAddressTuple = Find(address);
				var memoryProvider = entryAddressTuple.Item1.MemoryProvider;
				var relativeAddress = entryAddressTuple.Item2;

				if (memoryProvider is byte[])
					((byte[]) memoryProvider)[relativeAddress] = value;
				else if (memoryProvider is IMemoryProvider)
					((IMemoryProvider) memoryProvider)[address] = value;
				else
					Console.WriteLine("Writing {0:X2} to {1:X4}.", value, address); //TODO: Throw exception/don't allow other types of providers in Add().
			}
		}

		public short GetShort(int address)
		{
			return BitConverter.ToInt16(new[] { this[address], this[address + 1] }, 0);
		}

		public void SetShort(int address, short value)
		{
			var bytes = BitConverter.GetBytes(value);

			this[address] = bytes[0];
			this[address + 1] = bytes[1];
		}

		private Tuple<MemoryMapEntry, int> Find(int address)
		{
			MemoryMapEntry smaller = null;

			foreach (var entry in _map)
			{
				if (entry.Start <= address)
					smaller = entry;
				else
					break;
			}

			if (smaller != null && smaller.Start + smaller.Length - 1 < address)
				smaller = null;

			return smaller == null ? null : new Tuple<MemoryMapEntry, int>(smaller, address - smaller.Start);
		}

		public bool Add(int start, int length, object memory)
		{
			_map.Sort((a, b) => a.Start.CompareTo(b.Start));

			if (_map.Any(entry => entry.Start == start))
				return false;

			MemoryMapEntry smaller = null;
			MemoryMapEntry larger = null;

			foreach (var entry in _map)
			{
				if (entry.Start < start)
					smaller = entry;
				else
				{
					larger = entry;
					break;
				}
			}

			if (smaller != null && smaller.Start + smaller.Length > start)
				return false;

			if (larger != null && larger.Start < start + length)
				return false;

			_map.Add(new MemoryMapEntry(start, length, memory));

			return true;
		}

		public void Clear()
		{
			_map.Clear();
		}

		#region Nested type: MemoryMapEntry

		private class MemoryMapEntry
		{
			public readonly int Length;

			public readonly object MemoryProvider;
			public readonly int Start;

			public MemoryMapEntry(int start, int length, object memoryProvider)
			{
				Start = start;
				Length = length;
				MemoryProvider = memoryProvider;
			}
		}

		#endregion
	}
}