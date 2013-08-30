namespace Metazel
{
	// ReSharper disable InconsistentNaming
	public enum Interrupt : ushort
	{
		NMI = 0xFFFA,
		RESET = 0xFFFC,
		IRQ = 0xFFFE
	}
}