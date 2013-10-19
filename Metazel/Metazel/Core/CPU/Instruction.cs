using System;
using System.Collections.Generic;
using Metazel.Library;

namespace Metazel.NES
{
	internal class Instruction
	{
		public int CyclesLeft;

		public Instruction(InstructionMetadata metadata, byte[] operands)
		{
			Metadata = metadata;
			Operands = operands;

			CyclesLeft = metadata.CycleCount;
		}

		public InstructionMetadata Metadata { get; private set; }
		public byte[] Operands { get; private set; }

		public void Execute()
		{
			Metadata.Action(Metadata, Operands);
		}

		public string ToString(NESCPU cpu)
		{
			switch (Metadata.Name)
			{
				case "JMP":
					switch (Metadata.AddressingMode)
					{
						case AddressingMode.Indirect:
							{
								var indirect = BitConverter.ToUInt16(Operands, 0);
								var address = BitConverter.ToUInt16(new[] { cpu.Memory[indirect], cpu.Memory[indirect + 1] }, 0);
								return string.Format("{0} (${1:X4}) = {2:X4}", Metadata.Name, indirect, address);
							}
						case AddressingMode.Absolute:
							return string.Format("{0} ${1:X4}", Metadata.Name, BitConverter.ToUInt16(Operands, 0));
					}
					goto default;
				case "ROL":
				case "ROR":
				case "ASL":
				case "LSR":
					if (Metadata.AddressingMode == AddressingMode.Implicit)
						return string.Format("{0} A", Metadata.Name);

					goto case "STA";
				case "BCS":
				case "BCC":
				case "BEQ":
				case "BNE":
				case "BVS":
				case "BVC":
				case "BPL":
				case "BMI":
					return string.Format("{0} ${1:X4}", Metadata.Name, cpu.PC + (sbyte) Operands[0]);
				case "STA":
				case "STX":
				case "STY":
				case "BIT":
				case "LDA":
				case "LDX":
				case "LDY":
				case "ORA":
				case "AND":
				case "EOR":
				case "ADC":
				case "SBC":
				case "CMP":
				case "CPX":
				case "CPY":
				case "INC":
				case "DEC":
				case "NOP":
				case "LAX":
				case "SAX":
				case "DCP":
				case "ISB":
				case "SLO":
				case "RLA":
				case "SRE":
				case "RRA":
					switch (Metadata.AddressingMode)
					{
						case AddressingMode.ZeroPage:
							return string.Format("{0} ${1:X2} = {2:X2}", Metadata.Name, Operands[0], cpu.Memory[Operands[0]]);
						case AddressingMode.Absolute:
							{
								var address = BitConverter.ToUInt16(Operands, 0);
								return new List<ushort> { 0x2000, 0x2001, 0x2003, 0x2005, 0x2006 }.Contains(address)
									       ? string.Format("{0} ${1:X4} = *PPU Register*", Metadata.Name, address)
									       : string.Format("{0} ${1:X4} = {2:X2}", Metadata.Name, address, cpu.Memory[address]);
							}
						case AddressingMode.IndexedIndirect:
							{
								var indexedValue = Operands[0] + cpu.X;
								var address =
									BitConverter.ToUInt16(new[] { cpu.Memory[(byte) indexedValue], cpu.Memory[(byte) (indexedValue + 1)] }, 0);
								return string.Format("{0} (${1:X2},X) @ {2:X2} = {3:X4} = {4:X2}", Metadata.Name, Operands[0],
								                     (byte) indexedValue, address, cpu.Memory[address]);
							}
						case AddressingMode.IndirectIndexed:
							{
								var baseIndirect = BitConverter.ToUInt16(
									new[] { cpu.Memory[Operands[0]], cpu.Memory[(byte) (Operands[0] + 1)] }, 0);
								var address = (ushort) (baseIndirect + cpu.Y);
								return string.Format("{0} (${1:X2}),Y = {2:X4} @ {3:X4} = {4:X2}", Metadata.Name, Operands[0], baseIndirect,
								                     address, cpu.Memory[address]);
							}
						case AddressingMode.AbsoluteY:
							{
								var address = BitConverter.ToUInt16(Operands, 0);
								return string.Format("{0} ${1:X4},Y @ {2:X4} = {3:X2}", Metadata.Name, address, (ushort) (address + cpu.Y),
								                     cpu.Memory[(ushort) (address + cpu.Y)]);
							}
						case AddressingMode.ZeroPageX:
							return string.Format("{0} ${1:X2},X @ {2:X2} = {3:X2}", Metadata.Name, Operands[0], (byte) (Operands[0] + cpu.X),
							                     cpu.Memory[(byte) (Operands[0] + cpu.X)]);
						case AddressingMode.ZeroPageY:
							return string.Format("{0} ${1:X2},Y @ {2:X2} = {3:X2}", Metadata.Name, Operands[0], (byte) (Operands[0] + cpu.Y),
							                     cpu.Memory[(byte) (Operands[0] + cpu.Y)]);
						case AddressingMode.AbsoluteX:
							{
								var address = BitConverter.ToUInt16(Operands, 0);
								return string.Format("{0} ${1:X4},X @ {2:X4} = {3:X2}", Metadata.Name, address, (ushort) (address + cpu.X),
								                     cpu.Memory[(ushort) (address + cpu.X)]);
							}
					}
					goto default;
				default:
					switch (Operands.Length)
					{
						case 0:
							return string.Format("{0}", Metadata.Name);
						case 1:
							return string.Format("{0} #${1:X2}", Metadata.Name, Operands[0]);
						case 2:
							return string.Format("{0} ${1:X4}", Metadata.Name, BitConverter.ToUInt16(Operands, 0));
						default:
							throw new NotImplementedException();
					}
			}
		}
	}
}