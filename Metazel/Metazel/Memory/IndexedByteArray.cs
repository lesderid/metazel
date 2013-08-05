using System;

namespace Metazel
{
	internal class IndexedByteArray : IMemoryProvider
	{
		private readonly int _baseAddress;
		private readonly byte[] _underlyingArray;

		public IndexedByteArray(byte[] array, int baseAddress)
		{
			_underlyingArray = array;
			_baseAddress = baseAddress;
		}

		#region IMemoryProvider Members

		public byte? this[int address]
		{
			get { return _underlyingArray[_baseAddress + address]; }
			set
			{
				if (value != null)
					_underlyingArray[_baseAddress + address] = (byte) value;
				else
					throw new ArgumentNullException("address");
			}
		}

		#endregion
	}
}