namespace Metazel.NES
{
	public enum NametableLayout
	{
		VerticalLayout,
		HorizontalLayout,
		FourScreenLayout
	}

	//horizontal layout / vertical mirroring:
	//0x2000 (A) | 0x2400 (B)
	//0x2800 (A) | 0x2C00 (B)

	//vertical layout / horizontal mirroring:
	//0x2000 (A) | 0x2400 (A)
	//0x2800 (B) | 0x2C00 (B)
}