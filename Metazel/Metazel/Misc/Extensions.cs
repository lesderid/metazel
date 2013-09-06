using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Metazel
{
	internal static class Extensions
	{
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte SetBit(this byte input, int bit, bool value)
		{
			return (byte) (value ? input | 1 << bit : input & ~(1 << bit));
		}

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool GetBit(this byte input, int bit)
		{
			return (input | 1 << bit) == input;
		}

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte GetBits(this byte input, int start, int end)
		{
			return (byte) ((byte) (input << 7 - end) >> 7 - end + start);
		}
	}
}