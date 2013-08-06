using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metazel
{
    class NESCPU
    {
        private readonly NESEngine _engine;

        public ushort PC { get; private set; }

        public byte S { get; private set; }
        public byte P { get; private set; }
        public byte A { get; private set; }
        public byte X { get; private set; }
        public byte Y { get; private set; }

        public bool N { get { return P.GetBit(7); } private set { P.SetBit(7, value); } }
        public bool V { get { return P.GetBit(6); } private set { P.SetBit(6, value); } }
        public bool D { get { return P.GetBit(3); } private set { P.SetBit(3, value); } }
        public bool I { get { return P.GetBit(2); } private set { P.SetBit(2, value); } }
        public bool Z { get { return P.GetBit(1); } private set { P.SetBit(1, value); } }
        public bool C { get { return P.GetBit(0); } private set { P.SetBit(0, value); } }

        public NESCPU(NESEngine engine)
        {
            _engine = engine;

            //TODO: Properly implement RESET interrupt.

            PC = (ushort)_engine.CPUMemoryMap.GetShort(0xFFFC);

            S = 0xFA;
            P = 0x34;
            A = X = Y = 0;

            for (var i = 0; i < 0x800; i++)
                _engine.CPUMemoryMap[i] = 0xFF;

            _engine.CPUMemoryMap[0x0008] = 0xF7;
            _engine.CPUMemoryMap[0x0009] = 0xEF;
            _engine.CPUMemoryMap[0x000A] = 0xDF;
            _engine.CPUMemoryMap[0x000F] = 0xBF;

            for (var i = 0x4000; i < 0x4010; i++)
                _engine.CPUMemoryMap[i] = 0x00;

            _engine.CPUMemoryMap[0x4015] = 0x00;
            _engine.CPUMemoryMap[0x4017] = 0x00;

            var a = _engine.CPUMemoryMap[PC - 4];

            System.Diagnostics.Debugger.Break();
        }

        public void DoCycle()
        {

        }
    }
}
