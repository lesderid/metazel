using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Metazel.Library;

namespace Metazel.NES
{
	public partial class NESCPU
	{
		private readonly NESEngine _engine;
		public readonly StringBuilder _stringBuilder = new StringBuilder();
		private Instruction _currentInstruction;

		public NESCPU(NESEngine engine)
		{
			//TODO: Implement cycle accuracy.

			_engine = engine;

			InitialiseInstructionMetadata();

			var undocumentedOpcodes = Enumerable.Range(0, 255).ToList();
			foreach (var pair in InstructionMetadata)
				undocumentedOpcodes.Remove(pair.Key);
		}

		public void Initialize()
		{
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

			if (_currentInstruction == default(Instruction) || _currentInstruction.CyclesLeft == 0)
			{
				if (_interrupts.Count > 0)
					_currentInstruction = DoInterrupt();
				else if (_preParsed)
				{
					_currentInstruction = _preParsedInstructions[PC - 0x8000];

					PC += (ushort)(_currentInstruction.Metadata.OperandSize + 1);
				}
				else
				{
					var opcode = Memory[PC];

					var metadata = InstructionMetadata[opcode];
					PC++;

					var operands = new byte[metadata.OperandSize];
					for (var i = 0; i < metadata.OperandSize; i++)
						operands[i] = Memory[PC + i];
					PC += metadata.OperandSize;

					_currentInstruction = new Instruction(metadata, operands);
				}

				_currentInstruction.Execute();
			}

			TotalCycleCount++;
			_currentInstruction.CyclesLeft--;
		}

		private Instruction DoInterrupt()
		{
			var interruptVector = BitConverter.GetBytes((ushort) _interrupts[0]); //TODO: Choose the correct interrupt based on priority.

			var metadata = new InstructionMetadata("INTERRUPT", AddressingMode.Implicit, 0, 5, (data, bytes) =>
																							   {
																								   Push(PC);
																								   Push(P);

																								   var vector = BitConverter.ToUInt16(bytes, 0);
																								   PC = BitConverter.ToUInt16(new[] { Memory[vector], Memory[vector + 1] }, 0);
																							   });

			_interrupts.Clear();

			return new Instruction(metadata, interruptVector);
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

		private readonly List<Interrupt> _interrupts = new List<Interrupt>();

		public void TriggerInterrupt(Interrupt type)
		{
			_interrupts.Add(type);
		}

		private bool _preParsed;
		private readonly Instruction[] _preParsedInstructions = new Instruction[0x8000];
		public void PreParse()
		{
			if (_engine.Cartridge.ROMMapper != ROMMapper.NROM)
				return;

			for (var i = 0; i < 0x8000; i++)
			{
				var memoryI = i + 0x8000;

				var opcode = Memory[memoryI];

				if (!InstructionMetadata.ContainsKey(opcode)) continue;
				var metadata = InstructionMetadata[opcode];
				memoryI++;

				if (metadata.OperandSize > 2) continue;
				var operands = new byte[metadata.OperandSize];
				for (var j = 0; j < metadata.OperandSize; j++)
				{
					if (memoryI + j > 0xFFFF)
						continue;

					operands[j] = Memory[memoryI + j];
				}
				
				_preParsedInstructions[i] = new Instruction(metadata, operands);
			}

			_preParsed = true;
		}
	}
}