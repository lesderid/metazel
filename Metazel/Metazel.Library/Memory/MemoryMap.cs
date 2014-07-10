using System;
using System.Collections.Generic;
using System.Linq;

namespace Metazel.Library
{
	public class MemoryMap
	{
		private readonly Type _byteArrayType;
		private readonly Type _memoryProviderType;
		private readonly List<MemoryMapEntry> _map = new List<MemoryMapEntry>();

		private Tuple<int, Type, byte[], IMemoryProvider>[] _entryAddressTuples;
		private bool _entryAddressTuplesDirty = true;

		public MemoryMap()
		{
			_byteArrayType = typeof(byte[]);
			_memoryProviderType = typeof(IMemoryProvider);
		}

		public byte this[int address]
		{
			get
			{
				var entryAddressTuple = Find(address);
				var relativeAddress = entryAddressTuple.Item1;
				var type = entryAddressTuple.Item2;
				var byteArray = entryAddressTuple.Item3;
				var memoryProvider = entryAddressTuple.Item4;

				if (type == _byteArrayType)
					return byteArray[relativeAddress];
				else if (type == _memoryProviderType)
					return memoryProvider[relativeAddress];
				else
				{
					//Console.WriteLine("Reading from {0:X4}.", address); //TODO: Throw exception/don't allow other types of providers in Add().

					return 0;
				}
			}

			set
			{
				var entryAddressTuple = Find(address);
				var relativeAddress = entryAddressTuple.Item1;
				var type = entryAddressTuple.Item2;
				var byteArray = entryAddressTuple.Item3;
				var memoryProvider = entryAddressTuple.Item4;

				if (type == _byteArrayType)
					byteArray[relativeAddress] = value;
				else if (type == _memoryProviderType)
					memoryProvider[relativeAddress] = value;
				//else
				//	Console.WriteLine("Writing {0:X2} to {1:X4}.", value, address); //TODO: Throw exception/don't allow other types of providers in Add().
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

		private Tuple<int, Type, byte[], IMemoryProvider> Find(int address)
		{
			if (!_entryAddressTuplesDirty)
				return _entryAddressTuples[address];

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

			Type type = null;
			byte[] byteArray = null;
			IMemoryProvider memoryProvider = null;

			if (smaller != null && smaller.MemoryProvider != null)
			{
				if (smaller.MemoryProvider is byte[])
				{
					type = _byteArrayType;
					byteArray = (byte[]) smaller.MemoryProvider;
				}
				else if (smaller.MemoryProvider is IMemoryProvider)
				{
					type = _memoryProviderType;
					memoryProvider = (IMemoryProvider) smaller.MemoryProvider;
				}
			}

			return smaller == null ? null : new Tuple<int, Type, byte[], IMemoryProvider>(address - smaller.Start, type, byteArray, memoryProvider);
		}

		public void PopulateTuplesList()
		{
			_entryAddressTuples = new Tuple<int, Type, byte[], IMemoryProvider>[ushort.MaxValue + 1];

			for (var i = 0; i < _entryAddressTuples.Length; i++)
				_entryAddressTuples[i] = Find(i);

			_entryAddressTuplesDirty = false;
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

			_entryAddressTuplesDirty = true;

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