using System;

namespace Metazel
{
	internal class Instruction
	{
		private readonly InstructionMetadata _metadata;
		private readonly byte[] _operands;
		public int CyclesLeft;

		public Instruction(InstructionMetadata metadata, byte[] operands)
		{
			_metadata = metadata;
			_operands = operands;

			CyclesLeft = metadata.CycleCount;
		}

		public void Execute()
		{
			_metadata.Action(_metadata, _operands);
		}

		public string ToString(NESCPU cpu)
		{
			switch (_metadata.Name)
			{
				case "JMP":
					switch (_metadata.AddressingMode)
					{
						case AddressingMode.Indirect:
							{
								var indirect = BitConverter.ToUInt16(_operands, 0);
								var address = BitConverter.ToUInt16(new[] { cpu.Memory[indirect], cpu.Memory[indirect + 1] }, 0);
								return string.Format("{0} (${1:X4}) = {2:X4}", _metadata.Name, indirect, address);
							}
						case AddressingMode.Absolute:
							return string.Format("{0} ${1:X4}", _metadata.Name, BitConverter.ToUInt16(_operands, 0));
					}
					goto default;
				case "ROL":
				case "ROR":
				case "ASL":
				case "LSR":
					if (_metadata.AddressingMode == AddressingMode.Implicit)
					{
						return string.Format("{0} A", _metadata.Name);
					}

					goto case "STA";
				case "BCS":
				case "BCC":
				case "BEQ":
				case "BNE":
				case "BVS":
				case "BVC":
				case "BPL":
				case "BMI":
					return string.Format("{0} ${1:X4}", _metadata.Name, cpu.PC + (sbyte) _operands[0]);
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
					switch (_metadata.AddressingMode)
					{
						case AddressingMode.ZeroPage:
							return string.Format("{0} ${1:X2} = {2:X2}", _metadata.Name, _operands[0], cpu.Memory[_operands[0]]);
						case AddressingMode.Absolute:
							{
								var address = BitConverter.ToUInt16(_operands, 0);
								return string.Format("{0} ${1:X4} = {2:X2}", _metadata.Name, address, cpu.Memory[address]);
							}
						case AddressingMode.IndexedIndirect:
							{
								var indexedValue = _operands[0] + cpu.X;
								var address = BitConverter.ToUInt16(new[] { cpu.Memory[(byte) indexedValue], cpu.Memory[(byte) (indexedValue + 1)] }, 0);
								return string.Format("{0} (${1:X2},X) @ {2:X2} = {3:X4} = {4:X2}", _metadata.Name, _operands[0], (byte) indexedValue, address, cpu.Memory[address]);
							}
						case AddressingMode.IndirectIndexed:
							{
								var baseIndirect = BitConverter.ToUInt16(new[] { cpu.Memory[_operands[0]], cpu.Memory[(byte) (_operands[0] + 1)] }, 0);
								var address = (ushort) (baseIndirect + cpu.Y);
								return string.Format("{0} (${1:X2}),Y = {2:X4} @ {3:X4} = {4:X2}", _metadata.Name, _operands[0], baseIndirect, address, cpu.Memory[address]);
							}
						case AddressingMode.AbsoluteY:
							{
								var address = BitConverter.ToUInt16(_operands, 0);
								return string.Format("{0} ${1:X4},Y @ {2:X4} = {3:X2}", _metadata.Name, address, (ushort) (address + cpu.Y), cpu.Memory[(ushort) (address + cpu.Y)]);
							}
						case AddressingMode.ZeroPageX:
							return string.Format("{0} ${1:X2},X @ {2:X2} = {3:X2}", _metadata.Name, _operands[0], (byte) (_operands[0] + cpu.X), cpu.Memory[(byte) (_operands[0] + cpu.X)]);
						case AddressingMode.ZeroPageY:
							return string.Format("{0} ${1:X2},Y @ {2:X2} = {3:X2}", _metadata.Name, _operands[0], (byte) (_operands[0] + cpu.Y), cpu.Memory[(byte) (_operands[0] + cpu.Y)]);
						case AddressingMode.AbsoluteX:
							{
								var address = BitConverter.ToUInt16(_operands, 0);
								return string.Format("{0} ${1:X4},X @ {2:X4} = {3:X2}", _metadata.Name, address, (ushort) (address + cpu.X), cpu.Memory[(ushort) (address + cpu.X)]);
							}
					}
					goto default;
				default:
					switch (_operands.Length)
					{
						case 0:
							return string.Format("{0}", _metadata.Name);
						case 1:
							return string.Format("{0} #${1:X2}", _metadata.Name, _operands[0]);
						case 2:
							return string.Format("{0} ${1:X4}", _metadata.Name, BitConverter.ToUInt16(_operands, 0));
						default:
							throw new NotImplementedException();
					}
			}
		}
	}
}