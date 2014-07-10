namespace Metazel.NES
{
	public enum AddressingMode
	{
		Implicit,
		Accumulator,
		Immediate,
		ZeroPage,
		ZeroPageX,
		ZeroPageY,
		Relative,
		Absolute,
		AbsoluteX,
		AbsoluteY,
		Indirect,
		IndexedIndirect, //(Indirect, X)
		IndirectIndexed //(Indirect), X
	}
}