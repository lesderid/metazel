namespace Metazel
{
	class NESPPU
	{
		private readonly NESEngine _engine;

		public NESPPU(NESEngine engine)
		{
			_engine = engine;

			
		}

		public MemoryMap Memory
		{
			get { return _engine.PPUMemoryMap; }
		}

		public void DoCycle()
		{
			
		}
	}
}
