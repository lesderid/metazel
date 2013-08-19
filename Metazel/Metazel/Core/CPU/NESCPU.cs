using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Metazel
{
	internal partial class NESCPU
	{
		private readonly NESEngine _engine;
		private readonly StringBuilder _stringBuilder = new StringBuilder();
		private Instruction _currentInstruction;

		public NESCPU(NESEngine engine)
		{
			//TODO: Implement cycle accuracy.

			_engine = engine;

			InitialiseInstructionMetadata();

			var undocumentedOpcodes = Enumerable.Range(0, 255).ToList();
			foreach (var pair in _instructionData)
				undocumentedOpcodes.Remove(pair.Key);

			//TODO: Properly implement RESET interrupt.											  

			PC = (ushort) Memory.GetShort(0xFFFC);

			S = 0xFA;
			P = 0x34;
			A = X = Y = 0;

			for (var i = 0; i < 0x800; i++)
				Memory[i] = 0xFF;

			Memory[0x0008] = 0xF7;
			Memory[0x0009] = 0xEF;
			Memory[0x000A] = 0xDF;
			Memory[0x000F] = 0xBF;

			for (var i = 0x4000; i < 0x4010; i++)
				Memory[i] = 0x00;

			Memory[0x4015] = 0x00;
			Memory[0x4017] = 0x00;

			if (_engine.Cartridge.Name == "nestest.nes") //HACK: NESTEST ONLY!
			{
				//nestest (CPU only) starts at 0xC000 without RESET, so RESET state must be reverted.

				PC = 0xC000;
				P = 0x24;
				S = 0xFD;

				for (var i = 0; i < 0x800; i++)
					Memory[i] = 0x00;
			}
		}

		public MemoryMap Memory
		{
			get { return _engine.CPUMemoryMap; }
		}

		public ushort PC { get; private set; }

		public byte S { get; private set; }
		public byte P { get; private set; }
		public byte A { get; private set; }
		public byte X { get; private set; }
		public byte Y { get; private set; }

		private bool N
		{
			get { return P.GetBit(7); }
			set { P = P.SetBit(7, value); }
		}
		private bool V
		{
			get { return P.GetBit(6); }
			set { P = P.SetBit(6, value); }
		}
		private bool D
		{
			get { return P.GetBit(3); }
			set { P = P.SetBit(3, value); }
		}
		private bool I
		{
			get { return P.GetBit(2); }
			set { P = P.SetBit(2, value); }
		}
		private bool Z
		{
			get { return P.GetBit(1); }
			set { P = P.SetBit(1, value); }
		}
		private bool C
		{
			get { return P.GetBit(0); }
			set { P = P.SetBit(0, value); }
		}

		public int TotalCycleCount { get; private set; }

		public void DoCycle()
		{
			//TODO: Implement this correctly.

			if (_currentInstruction == null || _currentInstruction.CyclesLeft == 0)
			{
				var opcode = Memory[PC];

				InstructionMetadata metadata = null;
				//try
				//{
					metadata = _instructionData[opcode];
				//}
				//catch (Exception)
				//{
				//	File.WriteAllText(_engine.Cartridge.Name + ".log", _stringBuilder.ToString());

				//	Environment.Exit(0);
				//}
				PC++;

				//var undocumentedOpcodes = new[]
				//						  {
				//							  0x00, 0x02, 0x03, 0x04, 0x07, 0x0B, 0x0C, 0x0F, 0x12, 0x13, 0x14, 0x17, 0x1A, 0x1B, 0x1F
				//							  , 0x22, 0x23, 0x27, 0x2B, 0x2F, 0x32, 0x33, 0x34, 0x37, 0x3A, 0x3B, 0x3F, 0x42, 0x43,
				//							  0x44, 0x47, 0x4B, 0x4F, 0x52, 0x53, 0x54, 0x57, 0x5A, 0x5B, 0x5F, 0x62, 0x63, 0x64, 0x67
				//							  , 0x6B, 0x6F, 0x72, 0x73, 0x74, 0x77, 0x7A, 0x7B, 0x7F, 0x80, 0x82, 0x83, 0x87, 0x89,
				//							  0x8B, 0x8F, 0x92, 0x93, 0x97, 0x9B, 0x9C, 0x9E, 0x9F, 0xA3, 0xA7, 0xAB, 0xAF, 0xB2, 0xB3
				//							  , 0xB7, 0xBB, 0xBF, 0xC2, 0xC3, 0xC7, 0xCB, 0xCF, 0xD2, 0xD3, 0xD4, 0xD7, 0xDA, 0xDB,
				//							  0xDF, 0xE2, 0xE3, 0xE7, 0xEB, 0xEF, 0xF2, 0xF3, 0xF4, 0xF7, 0xFA, 0xFB, 0xFF, 0x1C, 0x3C, 0x5C,
				//							  0x7C, 0xDC, 0xFC
				//						  };

				var operands = new byte[metadata.OperandSize];
				for (var i = 0; i < metadata.OperandSize; i++)
					operands[i] = Memory[PC + i];
				PC += metadata.OperandSize;

				_currentInstruction = new Instruction(metadata, operands);

				//var logLine = "";
				//logLine += string.Format("{0:X4}  {1:X2} ", PC - 1 - metadata.OperandSize, opcode);
				//logLine = operands.Aggregate(logLine, (current, t) => current + string.Format("{0:X2} ", t));
				//for (var i = 0; i < 7 - operands.Length * 3; i++)
				//	logLine += ' ';
				//if (undocumentedOpcodes.Contains(opcode))
				//	logLine = logLine.Remove(logLine.Length - 1) + "*";
				//var instructionString = _currentInstruction.ToString(this);
				//logLine += instructionString;
				//for (var i = 0; i < 32 - instructionString.Length; i++)
				//	logLine += ' ';
				//var scanLine = (241 + (TotalCycleCount * 3 - TotalCycleCount * 3 % 341) / 341) % 261;
				//if (scanLine < 241)
				//	scanLine--;
				//logLine += string.Format("A:{0:X2} X:{1:X2} Y:{2:X2} P:{3:X2} SP:{4:X2} CYC:{5,3} SL:{6}", A, X, Y, P, S,
				//						 TotalCycleCount * 3 % 341, scanLine);
				//Console.WriteLine(logLine);
				//_stringBuilder.AppendLine(logLine);

				_currentInstruction.Execute();
			}

			TotalCycleCount++;
			_currentInstruction.CyclesLeft--;
		}

		private void Push(byte value)
		{
			Memory[0x0100 + S--] = value;
		}

		private void Push(ushort value)
		{
			var bytes = BitConverter.GetBytes(value);
			Memory[0x0100 + S--] = bytes[1];
			Memory[0x0100 + S--] = bytes[0];
		}

		private byte PopByte()
		{
			return Memory[0x0100 + ++S];
		}

		private ushort PopUInt16()
		{
			var bytes = new byte[2];
			bytes[0] = Memory[0x0100 + ++S];
			bytes[1] = Memory[0x0100 + ++S];

			return BitConverter.ToUInt16(bytes, 0);
		}
	}
}