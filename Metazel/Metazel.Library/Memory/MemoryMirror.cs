using System;

namespace Metazel.Library
{
	public class MemoryMirror : IMemoryProvider
	{
		private readonly int _baseIndex;
		private readonly MemoryMap _memoryMap;
		private readonly int _size;

		public MemoryMirror(MemoryMap memoryMap, int baseIndex, int size)
		{
			_memoryMap = memoryMap;
			_baseIndex = baseIndex;
			_size = size;
		}

		#region IMemoryProvider Members

		public byte this[int address]
		{
			get
			{
				if (address >= _size)
					throw new ArgumentOutOfRangeException("address");

				return _memoryMap[_baseIndex + address];
			}
			set
			{
				if (address > _size)
					throw new ArgumentOutOfRangeException("address");

				_memoryMap[_baseIndex + address] = value;
			}
		}

		#endregion
	}
}