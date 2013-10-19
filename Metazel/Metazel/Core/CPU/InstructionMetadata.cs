using System;
using System.Collections.Generic;
using Metazel.Library;

namespace Metazel.NES
{
	public partial class NESCPU
	{
		// ReSharper disable InconsistentNaming -> Ignore opcode names.

		private Dictionary<byte, InstructionMetadata> _instructionData;

		private void InitialiseInstructionMetadata()
		{
			_instructionData = new Dictionary<byte, InstructionMetadata>
			{
				{ 0x78, new InstructionMetadata("SEI", AddressingMode.Implicit, 0, 2, (data, bytes) => { I = true; }) },
				{ 0x58, new InstructionMetadata("CLI", AddressingMode.Implicit, 0, 2, (data, bytes) => { I = false; }) },
				{ 0xF8, new InstructionMetadata("SED", AddressingMode.Implicit, 0, 2, (data, bytes) => { D = true; }) },
				{ 0xD8, new InstructionMetadata("CLD", AddressingMode.Implicit, 0, 2, (data, bytes) => { D = false; }) },
				{ 0x38, new InstructionMetadata("SEC", AddressingMode.Implicit, 0, 2, (data, bytes) => { C = true; }) },
				{ 0x18, new InstructionMetadata("CLC", AddressingMode.Implicit, 0, 2, (data, bytes) => { C = false; }) },
				{ 0xB8, new InstructionMetadata("CLV", AddressingMode.Implicit, 0, 2, (data, bytes) => { V = false; }) },

				{ 0xA9, new InstructionMetadata("LDA", AddressingMode.Immediate, 1, 2, LDA) },
				{ 0xAD, new InstructionMetadata("LDA", AddressingMode.Absolute, 2, 4, LDA) },
				{ 0xA5, new InstructionMetadata("LDA", AddressingMode.ZeroPage, 1, 3, LDA) },
				{ 0xA1, new InstructionMetadata("LDA", AddressingMode.IndexedIndirect, 1, 6, LDA) },
				{ 0xB1, new InstructionMetadata("LDA", AddressingMode.IndirectIndexed, 1, 5, LDA) },
				{ 0xB9, new InstructionMetadata("LDA", AddressingMode.AbsoluteY, 2, 4, LDA) },
				{ 0xB5, new InstructionMetadata("LDA", AddressingMode.ZeroPageX, 1, 4, LDA) },
				{ 0xBD, new InstructionMetadata("LDA", AddressingMode.AbsoluteX, 2, 4, LDA) },
				{ 0xA2, new InstructionMetadata("LDX", AddressingMode.Immediate, 1, 2, LDX) },
				{ 0xAE, new InstructionMetadata("LDX", AddressingMode.Absolute, 2, 4, LDX) },
				{ 0xA6, new InstructionMetadata("LDX", AddressingMode.ZeroPage, 1, 3, LDX) },
				{ 0xB6, new InstructionMetadata("LDX", AddressingMode.ZeroPageY, 1, 4, LDX) },
				{ 0xBE, new InstructionMetadata("LDX", AddressingMode.AbsoluteY, 2, 4, LDX) },
				{ 0xA0, new InstructionMetadata("LDY", AddressingMode.Immediate, 1, 2, LDY) },
				{ 0xAC, new InstructionMetadata("LDY", AddressingMode.Absolute, 2, 4, LDY) },
				{ 0xA4, new InstructionMetadata("LDY", AddressingMode.ZeroPage, 1, 3, LDY) },
				{ 0xB4, new InstructionMetadata("LDY", AddressingMode.ZeroPageX, 1, 4, LDY) },
				{ 0xBC, new InstructionMetadata("LDY", AddressingMode.AbsoluteX, 2, 4, LDY) },

				{ 0x4C, new InstructionMetadata("JMP", AddressingMode.Absolute, 2, 3, JMP) },
				{ 0x6C, new InstructionMetadata("JMP", AddressingMode.Indirect, 2, 5, JMP) },
				{ 0x20, new InstructionMetadata("JSR", AddressingMode.Absolute, 2, 6, JSR) },
				{ 0x60, new InstructionMetadata("RTS", AddressingMode.Implicit, 0, 6, RTS) },
				{ 0x40, new InstructionMetadata("RTI", AddressingMode.Implicit, 0, 6, RTI) },

				{ 0x85, new InstructionMetadata("STA", AddressingMode.ZeroPage, 1, 3, STA) },
				{ 0x8D, new InstructionMetadata("STA", AddressingMode.Absolute, 2, 4, STA) },
				{ 0x81, new InstructionMetadata("STA", AddressingMode.IndexedIndirect, 1, 6, STA) },
				{ 0x91, new InstructionMetadata("STA", AddressingMode.IndirectIndexed, 1, 6, STA) },
				{ 0x99, new InstructionMetadata("STA", AddressingMode.AbsoluteY, 2, 5, STA) },
				{ 0x9D, new InstructionMetadata("STA", AddressingMode.AbsoluteX, 2, 5, STA) },
				{ 0x95, new InstructionMetadata("STA", AddressingMode.ZeroPageX, 1, 4, STA) },
				{ 0x86, new InstructionMetadata("STX", AddressingMode.ZeroPage, 1, 3, STX) },
				{ 0x8E, new InstructionMetadata("STX", AddressingMode.Absolute, 2, 4, STX) },
				{ 0x96, new InstructionMetadata("STX", AddressingMode.ZeroPageY, 1, 4, STX) },
				{ 0x84, new InstructionMetadata("STY", AddressingMode.ZeroPage, 1, 3, STY) },
				{ 0x8C, new InstructionMetadata("STY", AddressingMode.Absolute, 2, 4, STY) },
				{ 0x94, new InstructionMetadata("STY", AddressingMode.ZeroPageX, 1, 4, STY) },

				{ 0xEA, new InstructionMetadata("NOP", AddressingMode.Implicit, 0, 2, NOP) },
				{ 0x1A, new InstructionMetadata("NOP", AddressingMode.Implicit, 0, 2, NOP) },
				{ 0x3A, new InstructionMetadata("NOP", AddressingMode.Implicit, 0, 2, NOP) },
				{ 0x5A, new InstructionMetadata("NOP", AddressingMode.Implicit, 0, 2, NOP) },
				{ 0x7A, new InstructionMetadata("NOP", AddressingMode.Implicit, 0, 2, NOP) },
				{ 0xDA, new InstructionMetadata("NOP", AddressingMode.Implicit, 0, 2, NOP) },
				{ 0xFA, new InstructionMetadata("NOP", AddressingMode.Implicit, 0, 2, NOP) },
				{ 0x04, new InstructionMetadata("NOP", AddressingMode.ZeroPage, 1, 3, NOP) },
				{ 0x44, new InstructionMetadata("NOP", AddressingMode.ZeroPage, 1, 3, NOP) },
				{ 0x64, new InstructionMetadata("NOP", AddressingMode.ZeroPage, 1, 3, NOP) },
				{ 0x0C, new InstructionMetadata("NOP", AddressingMode.Absolute, 2, 4, NOP) },
				{ 0x14, new InstructionMetadata("NOP", AddressingMode.ZeroPageX, 1, 4, NOP) },
				{ 0x34, new InstructionMetadata("NOP", AddressingMode.ZeroPageX, 1, 4, NOP) },
				{ 0x54, new InstructionMetadata("NOP", AddressingMode.ZeroPageX, 1, 4, NOP) },
				{ 0x74, new InstructionMetadata("NOP", AddressingMode.ZeroPageX, 1, 4, NOP) },
				{ 0xD4, new InstructionMetadata("NOP", AddressingMode.ZeroPageX, 1, 4, NOP) },
				{ 0xF4, new InstructionMetadata("NOP", AddressingMode.ZeroPageX, 1, 4, NOP) },
				{ 0x80, new InstructionMetadata("NOP", AddressingMode.Immediate, 1, 2, NOP) },
				{ 0x1C, new InstructionMetadata("NOP", AddressingMode.AbsoluteX, 2, 4, NOP) },
				{ 0x3C, new InstructionMetadata("NOP", AddressingMode.AbsoluteX, 2, 4, NOP) },
				{ 0x5C, new InstructionMetadata("NOP", AddressingMode.AbsoluteX, 2, 4, NOP) },
				{ 0x7C, new InstructionMetadata("NOP", AddressingMode.AbsoluteX, 2, 4, NOP) },
				{ 0xDC, new InstructionMetadata("NOP", AddressingMode.AbsoluteX, 2, 4, NOP) },
				{ 0xFC, new InstructionMetadata("NOP", AddressingMode.AbsoluteX, 2, 4, NOP) },

				{ 0xB0, new InstructionMetadata("BCS", AddressingMode.Relative, 1, 2, (data, bytes) => B(bytes, C, true)) },
				{ 0x90, new InstructionMetadata("BCC", AddressingMode.Relative, 1, 2, (data, bytes) => B(bytes, C, false)) },
				{ 0xF0, new InstructionMetadata("BEQ", AddressingMode.Relative, 1, 2, (data, bytes) => B(bytes, Z, true)) },
				{ 0xD0, new InstructionMetadata("BNE", AddressingMode.Relative, 1, 2, (data, bytes) => B(bytes, Z, false)) },
				{ 0x70, new InstructionMetadata("BVS", AddressingMode.Relative, 1, 2, (data, bytes) => B(bytes, V, true)) },
				{ 0x50, new InstructionMetadata("BVC", AddressingMode.Relative, 1, 2, (data, bytes) => B(bytes, V, false)) },
				{ 0x10, new InstructionMetadata("BPL", AddressingMode.Relative, 1, 2, (data, bytes) => B(bytes, N, false)) },
				{ 0x30, new InstructionMetadata("BMI", AddressingMode.Relative, 1, 2, (data, bytes) => B(bytes, N, true)) },

				{ 0x24, new InstructionMetadata("BIT", AddressingMode.ZeroPage, 1, 3, BIT) },
				{ 0x2C, new InstructionMetadata("BIT", AddressingMode.Absolute, 2, 4, BIT) },

				{ 0x29, new InstructionMetadata("AND", AddressingMode.Immediate, 1, 2, AND) },
				{ 0x21, new InstructionMetadata("AND", AddressingMode.IndexedIndirect, 1, 6, AND) },
				{ 0x25, new InstructionMetadata("AND", AddressingMode.ZeroPage, 1, 3, AND) },
				{ 0x2D, new InstructionMetadata("AND", AddressingMode.Absolute, 2, 4, AND) },
				{ 0x31, new InstructionMetadata("AND", AddressingMode.IndirectIndexed, 1, 5, AND) },
				{ 0x39, new InstructionMetadata("AND", AddressingMode.AbsoluteY, 2, 4, AND) },
				{ 0x35, new InstructionMetadata("AND", AddressingMode.ZeroPageX, 1, 4, AND) },
				{ 0x3D, new InstructionMetadata("AND", AddressingMode.AbsoluteX, 2, 4, AND) },

				{ 0x09, new InstructionMetadata("ORA", AddressingMode.Immediate, 1, 2, ORA) },
				{ 0x01, new InstructionMetadata("ORA", AddressingMode.IndexedIndirect, 1, 6, ORA) },
				{ 0x05, new InstructionMetadata("ORA", AddressingMode.ZeroPage, 1, 3, ORA) },
				{ 0x0D, new InstructionMetadata("ORA", AddressingMode.Absolute, 2, 4, ORA) },
				{ 0x11, new InstructionMetadata("ORA", AddressingMode.IndirectIndexed, 1, 5, ORA) },
				{ 0x19, new InstructionMetadata("ORA", AddressingMode.AbsoluteY, 2, 4, ORA) },
				{ 0x15, new InstructionMetadata("ORA", AddressingMode.ZeroPageX, 1, 4, ORA) },
				{ 0x1D, new InstructionMetadata("ORA", AddressingMode.AbsoluteX, 2, 4, ORA) },

				{ 0x49, new InstructionMetadata("EOR", AddressingMode.Immediate, 1, 2, EOR) },
				{ 0x41, new InstructionMetadata("EOR", AddressingMode.IndexedIndirect, 1, 6, EOR) },
				{ 0x45, new InstructionMetadata("EOR", AddressingMode.ZeroPage, 1, 3, EOR) },
				{ 0x4D, new InstructionMetadata("EOR", AddressingMode.Absolute, 2, 4, EOR) },
				{ 0x51, new InstructionMetadata("EOR", AddressingMode.IndirectIndexed, 1, 5, EOR) },
				{ 0x59, new InstructionMetadata("EOR", AddressingMode.AbsoluteY, 2, 4, EOR) },
				{ 0x55, new InstructionMetadata("EOR", AddressingMode.ZeroPageX, 1, 4, EOR) },
				{ 0x5D, new InstructionMetadata("EOR", AddressingMode.AbsoluteX, 2, 4, EOR) },

				{ 0x69, new InstructionMetadata("ADC", AddressingMode.Immediate, 1, 2, ADC) },
				{ 0x61, new InstructionMetadata("ADC", AddressingMode.IndexedIndirect, 1, 6, ADC) },
				{ 0x65, new InstructionMetadata("ADC", AddressingMode.ZeroPage, 1, 3, ADC) },
				{ 0x6D, new InstructionMetadata("ADC", AddressingMode.Absolute, 2, 4, ADC) },
				{ 0x71, new InstructionMetadata("ADC", AddressingMode.IndirectIndexed, 1, 5, ADC) },
				{ 0x79, new InstructionMetadata("ADC", AddressingMode.AbsoluteY, 2, 4, ADC) },
				{ 0x75, new InstructionMetadata("ADC", AddressingMode.ZeroPageX, 1, 4, ADC) },
				{ 0x7D, new InstructionMetadata("ADC", AddressingMode.AbsoluteX, 2, 4, ADC) },
				{ 0xE9, new InstructionMetadata("SBC", AddressingMode.Immediate, 1, 2, SBC) },
				{ 0xE1, new InstructionMetadata("SBC", AddressingMode.IndexedIndirect, 1, 6, SBC) },
				{ 0xE5, new InstructionMetadata("SBC", AddressingMode.ZeroPage, 1, 3, SBC) },
				{ 0xED, new InstructionMetadata("SBC", AddressingMode.Absolute, 2, 4, SBC) },
				{ 0xF1, new InstructionMetadata("SBC", AddressingMode.IndirectIndexed, 1, 5, SBC) },
				{ 0xF9, new InstructionMetadata("SBC", AddressingMode.AbsoluteY, 2, 4, SBC) },
				{ 0xF5, new InstructionMetadata("SBC", AddressingMode.ZeroPageX, 1, 4, SBC) },
				{ 0xFD, new InstructionMetadata("SBC", AddressingMode.AbsoluteX, 2, 4, SBC) },

				{ 0x4A, new InstructionMetadata("LSR", AddressingMode.Implicit, 0, 2, LSR) },
				{ 0x46, new InstructionMetadata("LSR", AddressingMode.ZeroPage, 1, 5, LSR) },
				{ 0x4E, new InstructionMetadata("LSR", AddressingMode.Absolute, 2, 6, LSR) },
				{ 0x56, new InstructionMetadata("LSR", AddressingMode.ZeroPageX, 1, 6, LSR) },
				{ 0x5E, new InstructionMetadata("LSR", AddressingMode.AbsoluteX, 2, 7, LSR) },
				{ 0x0A, new InstructionMetadata("ASL", AddressingMode.Implicit, 0, 2, ASL) },
				{ 0x06, new InstructionMetadata("ASL", AddressingMode.ZeroPage, 1, 5, ASL) },
				{ 0x0E, new InstructionMetadata("ASL", AddressingMode.Absolute, 2, 6, ASL) },
				{ 0x16, new InstructionMetadata("ASL", AddressingMode.ZeroPageX, 1, 6, ASL) },
				{ 0x1E, new InstructionMetadata("ASL", AddressingMode.AbsoluteX, 2, 7, ASL) },
				{ 0x6A, new InstructionMetadata("ROR", AddressingMode.Implicit, 0, 2, ROR) },
				{ 0x66, new InstructionMetadata("ROR", AddressingMode.ZeroPage, 1, 5, ROR) },
				{ 0x6E, new InstructionMetadata("ROR", AddressingMode.Absolute, 2, 6, ROR) },
				{ 0x76, new InstructionMetadata("ROR", AddressingMode.ZeroPageX, 1, 6, ROR) },
				{ 0x7E, new InstructionMetadata("ROR", AddressingMode.AbsoluteX, 2, 7, ROR) },
				{ 0x2A, new InstructionMetadata("ROL", AddressingMode.Implicit, 0, 2, ROL) },
				{ 0x26, new InstructionMetadata("ROL", AddressingMode.ZeroPage, 1, 5, ROL) },
				{ 0x2E, new InstructionMetadata("ROL", AddressingMode.Absolute, 2, 6, ROL) },
				{ 0x36, new InstructionMetadata("ROL", AddressingMode.ZeroPageX, 1, 6, ROL) },
				{ 0x3E, new InstructionMetadata("ROL", AddressingMode.AbsoluteX, 2, 7, ROL) },

				{ 0xE6, new InstructionMetadata("INC", AddressingMode.ZeroPage, 1, 5, INC) },
				{ 0xEE, new InstructionMetadata("INC", AddressingMode.Absolute, 2, 6, INC) },
				{ 0xF6, new InstructionMetadata("INC", AddressingMode.ZeroPageX, 1, 6, INC) },
				{ 0xFE, new InstructionMetadata("INC", AddressingMode.AbsoluteX, 2, 7, INC) },
				{ 0xE8, new InstructionMetadata("INX", AddressingMode.Implicit, 0, 2, INX) },
				{ 0xC8, new InstructionMetadata("INY", AddressingMode.Implicit, 0, 2, INY) },
				{ 0xC6, new InstructionMetadata("DEC", AddressingMode.ZeroPage, 1, 5, DEC) },
				{ 0xCE, new InstructionMetadata("DEC", AddressingMode.Absolute, 2, 6, DEC) },
				{ 0xD6, new InstructionMetadata("DEC", AddressingMode.ZeroPageX, 1, 6, DEC) },
				{ 0xDE, new InstructionMetadata("DEC", AddressingMode.AbsoluteX, 2, 7, DEC) },
				{ 0xCA, new InstructionMetadata("DEX", AddressingMode.Implicit, 0, 2, DEX) },
				{ 0x88, new InstructionMetadata("DEY", AddressingMode.Implicit, 0, 2, DEY) },

				{ 0xAA, new InstructionMetadata("TAX", AddressingMode.Implicit, 0, 2, TAX) },
				{ 0xA8, new InstructionMetadata("TAY", AddressingMode.Implicit, 0, 2, TAY) },
				{ 0xBA, new InstructionMetadata("TSX", AddressingMode.Implicit, 0, 2, TSX) },
				{ 0x8A, new InstructionMetadata("TXA", AddressingMode.Implicit, 0, 2, TXA) },
				{ 0x9A, new InstructionMetadata("TXS", AddressingMode.Implicit, 0, 2, (data, bytes) => S = X) },
				{ 0x98, new InstructionMetadata("TYA", AddressingMode.Implicit, 0, 2, TYA) },

				{ 0xC9, new InstructionMetadata("CMP", AddressingMode.Immediate, 1, 2, CMP) },
				{ 0xC1, new InstructionMetadata("CMP", AddressingMode.IndexedIndirect, 1, 6, CMP) },
				{ 0xC5, new InstructionMetadata("CMP", AddressingMode.ZeroPage, 1, 3, CMP) },
				{ 0xCD, new InstructionMetadata("CMP", AddressingMode.Absolute, 2, 4, CMP) },
				{ 0xD1, new InstructionMetadata("CMP", AddressingMode.IndirectIndexed, 1, 5, CMP) },
				{ 0xD9, new InstructionMetadata("CMP", AddressingMode.AbsoluteY, 2, 4, CMP) },
				{ 0xD5, new InstructionMetadata("CMP", AddressingMode.ZeroPageX, 1, 4, CMP) },
				{ 0xDD, new InstructionMetadata("CMP", AddressingMode.AbsoluteX, 2, 4, CMP) },
				{ 0xE0, new InstructionMetadata("CPX", AddressingMode.Immediate, 1, 2, CPX) },
				{ 0xE4, new InstructionMetadata("CPX", AddressingMode.ZeroPage, 1, 3, CPX) },
				{ 0xEC, new InstructionMetadata("CPX", AddressingMode.Absolute, 2, 4, CPX) },
				{ 0xC0, new InstructionMetadata("CPY", AddressingMode.Immediate, 1, 2, CPY) },
				{ 0xC4, new InstructionMetadata("CPY", AddressingMode.ZeroPage, 1, 3, CPY) },
				{ 0xCC, new InstructionMetadata("CPY", AddressingMode.Absolute, 2, 4, CPY) },

				{ 0x08, new InstructionMetadata("PHP", AddressingMode.Implicit, 0, 3, (data, bytes) => Push((byte) (P | 0x10))) },
				{ 0x28, new InstructionMetadata("PLP", AddressingMode.Implicit, 0, 4, (data, bytes) => P = (byte) (PopByte() & 0xEF | 0x20)) },
				{ 0x48, new InstructionMetadata("PHA", AddressingMode.Implicit, 0, 3, (data, bytes) => Push(A)) },
				{ 0x68, new InstructionMetadata("PLA", AddressingMode.Implicit, 0, 4, PLA) },

				{ 0xA3, new InstructionMetadata("LAX", AddressingMode.IndexedIndirect, 1, 6, LAX) },
				{ 0xA7, new InstructionMetadata("LAX", AddressingMode.ZeroPage, 1, 3, LAX) },
				{ 0xAF, new InstructionMetadata("LAX", AddressingMode.Absolute, 2, 4, LAX) },
				{ 0xB3, new InstructionMetadata("LAX", AddressingMode.IndirectIndexed, 1, 5, LAX) },
				{ 0xB7, new InstructionMetadata("LAX", AddressingMode.ZeroPageY, 1, 4, LAX) },
				{ 0xBF, new InstructionMetadata("LAX", AddressingMode.AbsoluteY, 2, 4, LAX) },

				{ 0x83, new InstructionMetadata("SAX", AddressingMode.IndexedIndirect, 1, 6, SAX) },
				{ 0x87, new InstructionMetadata("SAX", AddressingMode.ZeroPage, 1, 3, SAX) },
				{ 0x8F, new InstructionMetadata("SAX", AddressingMode.Absolute, 2, 4, SAX) },
				{ 0x93, new InstructionMetadata("SAX", AddressingMode.IndirectIndexed, 1, 5, SAX) },
				{ 0x97, new InstructionMetadata("SAX", AddressingMode.ZeroPageY, 1, 4, SAX) },
				{ 0x9F, new InstructionMetadata("SAX", AddressingMode.AbsoluteY, 2, 4, SAX) },
			
				{ 0xEB, new InstructionMetadata("SBC", AddressingMode.Immediate, 1, 2, SBC) },

				{ 0xC3, new InstructionMetadata("DCP", AddressingMode.IndexedIndirect, 1, 8, DCP) },
				{ 0xC7, new InstructionMetadata("DCP", AddressingMode.ZeroPage, 1, 5, DCP) },
				{ 0xCF, new InstructionMetadata("DCP", AddressingMode.Absolute, 2, 6, DCP) },
				{ 0xD3, new InstructionMetadata("DCP", AddressingMode.IndirectIndexed, 1, 7, DCP) },
				{ 0xD7, new InstructionMetadata("DCP", AddressingMode.ZeroPageX, 1, 6, DCP) },
				{ 0xDB, new InstructionMetadata("DCP", AddressingMode.AbsoluteY, 2, 6, DCP) },
				{ 0xDF, new InstructionMetadata("DCP", AddressingMode.AbsoluteX, 2, 6, DCP) },

				{ 0xE3, new InstructionMetadata("ISB", AddressingMode.IndexedIndirect, 1, 8, ISB) },
				{ 0xE7, new InstructionMetadata("ISB", AddressingMode.ZeroPage, 1, 5, ISB) },
				{ 0xEF, new InstructionMetadata("ISB", AddressingMode.Absolute, 2, 6, ISB) },
				{ 0xF3, new InstructionMetadata("ISB", AddressingMode.IndirectIndexed, 1, 7, ISB) },
				{ 0xF7, new InstructionMetadata("ISB", AddressingMode.ZeroPageX, 1, 6, ISB) },
				{ 0xFB, new InstructionMetadata("ISB", AddressingMode.AbsoluteY, 2, 6, ISB) },
				{ 0xFF, new InstructionMetadata("ISB", AddressingMode.AbsoluteX, 2, 6, ISB) },

				{ 0x03, new InstructionMetadata("SLO", AddressingMode.IndexedIndirect, 1, 8, SLO) },
				{ 0x07, new InstructionMetadata("SLO", AddressingMode.ZeroPage, 1, 5, SLO) },
				{ 0x0F, new InstructionMetadata("SLO", AddressingMode.Absolute, 2, 6, SLO) },
				{ 0x13, new InstructionMetadata("SLO", AddressingMode.IndirectIndexed, 1, 7, SLO) },
				{ 0x17, new InstructionMetadata("SLO", AddressingMode.ZeroPageX, 1, 6, SLO) },
				{ 0x1B, new InstructionMetadata("SLO", AddressingMode.AbsoluteY, 2, 6, SLO) },
				{ 0x1F, new InstructionMetadata("SLO", AddressingMode.AbsoluteX, 2, 6, SLO) },

				{ 0x23, new InstructionMetadata("RLA", AddressingMode.IndexedIndirect, 1, 8, RLA) },
				{ 0x27, new InstructionMetadata("RLA", AddressingMode.ZeroPage, 1, 5, RLA) },
				{ 0x2F, new InstructionMetadata("RLA", AddressingMode.Absolute, 2, 6, RLA) },
				{ 0x33, new InstructionMetadata("RLA", AddressingMode.IndirectIndexed, 1, 7, RLA) },
				{ 0x37, new InstructionMetadata("RLA", AddressingMode.ZeroPageX, 1, 6, RLA) },
				{ 0x3B, new InstructionMetadata("RLA", AddressingMode.AbsoluteY, 2, 6, RLA) },
				{ 0x3F, new InstructionMetadata("RLA", AddressingMode.AbsoluteX, 2, 6, RLA) },

				{ 0x43, new InstructionMetadata("SRE", AddressingMode.IndexedIndirect, 1, 8, SRE) },
				{ 0x47, new InstructionMetadata("SRE", AddressingMode.ZeroPage, 1, 5, SRE) },
				{ 0x4F, new InstructionMetadata("SRE", AddressingMode.Absolute, 2, 6, SRE) },
				{ 0x53, new InstructionMetadata("SRE", AddressingMode.IndirectIndexed, 1, 7, SRE) },
				{ 0x57, new InstructionMetadata("SRE", AddressingMode.ZeroPageX, 1, 6, SRE) },
				{ 0x5B, new InstructionMetadata("SRE", AddressingMode.AbsoluteY, 2, 6, SRE) },
				{ 0x5F, new InstructionMetadata("SRE", AddressingMode.AbsoluteX, 2, 6, SRE) },

				{ 0x63, new InstructionMetadata("RRA", AddressingMode.IndexedIndirect, 1, 8, RRA) },
				{ 0x67, new InstructionMetadata("RRA", AddressingMode.ZeroPage, 1, 5, RRA) },
				{ 0x6F, new InstructionMetadata("RRA", AddressingMode.Absolute, 2, 6, RRA) },
				{ 0x73, new InstructionMetadata("RRA", AddressingMode.IndirectIndexed, 1, 7, RRA) },
				{ 0x77, new InstructionMetadata("RRA", AddressingMode.ZeroPageX, 1, 6, RRA) },
				{ 0x7B, new InstructionMetadata("RRA", AddressingMode.AbsoluteY, 2, 6, RRA) },
				{ 0x7F, new InstructionMetadata("RRA", AddressingMode.AbsoluteX, 2, 6, RRA) },
			};
		}

		private void RRA(InstructionMetadata metadata, byte[] operands)
		{
			var oldCarry = C;

			var address = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.IndexedIndirect:
					address = BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0);
					break;
				case AddressingMode.ZeroPage:
					address = operands[0];
					break;
				case AddressingMode.Absolute:
					address = BitConverter.ToUInt16(operands, 0);
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					break;
				case AddressingMode.ZeroPageX:
					address = (byte) (operands[0] + X);
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						address = (ushort) (absolute + Y);
					}
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						address = (ushort) (absolute + X);
					}
					break;
			}

			Memory[address] >>= 1;
			Memory[address] = Memory[address].SetBit(7, oldCarry);

			Z = Memory[address] == 0;
			N = Memory[address].GetBit(7);

			var value = Memory[address];

			//TODO: Fix addition part.

			var unsignedResult = A + (uint) value + (uint) (C ? 1 : 0);

			C = unsignedResult > byte.MaxValue;
			V = ((value ^ unsignedResult) & (A ^ unsignedResult) & 0x80) != 0;
			
			A = (byte) unsignedResult;

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void SRE(InstructionMetadata metadata, byte[] operands)
		{
			var address = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.IndexedIndirect:
					address = BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0);
					break;
				case AddressingMode.ZeroPage:
					address = operands[0];
					break;
				case AddressingMode.Absolute:
					address = BitConverter.ToUInt16(operands, 0);
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					break;
				case AddressingMode.ZeroPageX:
					address = (byte) (operands[0] + X);
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						address = (ushort) (absolute + Y);
					}
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						address = (ushort) (absolute + X);
					}
					break;
			}

			C = Memory[address].GetBit(0);

			Memory[address] >>= 1;

			A = (byte) (A ^ Memory[address]);

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void RLA(InstructionMetadata metadata, byte[] operands)
		{
			var oldCarry = C;

			var address = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.IndexedIndirect:
					address = BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0);
					break;
				case AddressingMode.ZeroPage:
					address = operands[0];
					break;
				case AddressingMode.Absolute:
					address = BitConverter.ToUInt16(operands, 0);
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					break;
				case AddressingMode.ZeroPageX:
					address = (byte) (operands[0] + X);
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						address = (ushort) (absolute + Y);
					}
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						address = (ushort) (absolute + X);
					}
					break;
			}

			C = Memory[address].GetBit(7);

			Memory[address] <<= 1;
			Memory[address] = Memory[address].SetBit(0, oldCarry);

			A = (byte) (A & Memory[address]);

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void SLO(InstructionMetadata metadata, byte[] operands)
		{
			var address = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.IndexedIndirect:
					address = BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0);
					break;
				case AddressingMode.ZeroPage:
					address = operands[0];
					break;
				case AddressingMode.Absolute:
					address = BitConverter.ToUInt16(operands, 0);
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					break;
				case AddressingMode.ZeroPageX:
					address = (byte) (operands[0] + X);
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						address = (ushort) (absolute + Y);
					}
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						address = (ushort) (absolute + X);
					}
					break;
			}

			C = Memory[address].GetBit(7);

			Memory[address] <<= 1;

			A = (byte) (A | Memory[address]);

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void ISB(InstructionMetadata metadata, byte[] operands)
		{
			var value = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.IndexedIndirect:
					value = ++Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)];
					break;
				case AddressingMode.ZeroPage:
					value = ++Memory[operands[0]];
					break;
				case AddressingMode.Absolute:
					value = ++Memory[BitConverter.ToUInt16(operands, 0)];
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					value = ++Memory[(ushort) address];
					break;
				case AddressingMode.ZeroPageX:
					value = ++Memory[(byte) (operands[0] + X)];
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = ++Memory[(ushort) (absolute + Y)];
					}
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = ++Memory[(ushort) (absolute + X)];
					}
					break;
			}

			var unsignedResult = A + (byte) ~value + (uint) (C ? 1 : 0);

			C = unsignedResult > byte.MaxValue;
			V = (((byte) ~value ^ unsignedResult) & (A ^ unsignedResult) & 0x80) != 0;

			A = (byte) unsignedResult;

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void DCP(InstructionMetadata metadata, byte[] operands)
		{
			var value = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.IndexedIndirect:
					value = --Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)];
					break;
				case AddressingMode.ZeroPage:
					value = --Memory[operands[0]];
					break;
				case AddressingMode.Absolute:
					value = --Memory[BitConverter.ToUInt16(operands, 0)];
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					value = --Memory[(ushort) address];
					break;
				case AddressingMode.ZeroPageX:
					value = --Memory[(byte) (operands[0] + X)];
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = --Memory[(ushort) (absolute + Y)];
					}
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = --Memory[(ushort) (absolute + X)];
					}
					break;
			}

			C = A >= value;
			Z = A == value;
			N = ((byte) (A - value)).GetBit(7);
		}

		private void SAX(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.IndexedIndirect:
					Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)] = (byte) (A & X);
					break;
				case AddressingMode.ZeroPage:
					Memory[operands[0]] = (byte) (A & X);
					break;
				case AddressingMode.Absolute:
					Memory[BitConverter.ToUInt16(operands, 0)] = (byte) (A & X);
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					Memory[(ushort) address] = (byte) (A & X);
					break;
				case AddressingMode.ZeroPageY:
					Memory[(byte) (operands[0] + Y)] = (byte) (A & X);
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						Memory[(ushort) (absolute + Y)] = (byte) (A & X);
					}
					break;
			}
		}

		private void LAX(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.IndexedIndirect:
					A = X = Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)];
					break;
				case AddressingMode.ZeroPage:
					A = X = Memory[operands[0]];
					break;
				case AddressingMode.Absolute:
					A = X = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					A = X = Memory[(ushort) address];
					break;
				case AddressingMode.ZeroPageY:
					A = X = Memory[(byte) (operands[0] + Y)];
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						A = X = Memory[(ushort) (absolute + Y)];
					}
					break;
			}

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void NOP(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.AbsoluteX:
					var absolute = BitConverter.ToUInt16(operands, 0);
					if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					break;
			}
		}

		private void ROL(InstructionMetadata metadata, byte[] operands)
		{
			var oldCarry = C;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.Implicit:
					C = A.GetBit(7);

					A <<= 1;
					A = A.SetBit(0, oldCarry);

					Z = A == 0;
					N = A.GetBit(7);
					break;
				case AddressingMode.ZeroPage:
					C = Memory[operands[0]].GetBit(7);

					Memory[operands[0]] <<= 1;
					Memory[operands[0]] = Memory[operands[0]].SetBit(0, oldCarry);

					Z = Memory[operands[0]] == 0;
					N = Memory[operands[0]].GetBit(7);
					break;
				case AddressingMode.Absolute:
					{
						var address = BitConverter.ToUInt16(operands, 0);

						C = Memory[address].GetBit(7);

						Memory[address] <<= 1;
						Memory[address] = Memory[address].SetBit(0, oldCarry);

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
				case AddressingMode.ZeroPageX:
					C = Memory[(byte) (operands[0] + X)].GetBit(7);

					Memory[(byte) (operands[0] + X)] <<= 1;
					Memory[(byte) (operands[0] + X)] = Memory[(byte) (operands[0] + X)].SetBit(0, oldCarry);

					Z = Memory[(byte) (operands[0] + X)] == 0;
					N = Memory[(byte) (operands[0] + X)].GetBit(7);
					break;
				case AddressingMode.AbsoluteX:
					{
						var address = BitConverter.ToUInt16(operands, 0) + X;

						C = Memory[address].GetBit(7);

						Memory[address] <<= 1;
						Memory[address] = Memory[address].SetBit(0, oldCarry);

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
			}
		}

		private void ROR(InstructionMetadata metadata, byte[] operands)
		{
			var oldCarry = C;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.Implicit:
					C = A.GetBit(0);

					A >>= 1;
					A = A.SetBit(7, oldCarry);

					Z = A == 0;
					N = A.GetBit(7);
					break;
				case AddressingMode.ZeroPage:
					C = Memory[operands[0]].GetBit(0);

					Memory[operands[0]] >>= 1;
					Memory[operands[0]] = Memory[operands[0]].SetBit(7, oldCarry);

					Z = Memory[operands[0]] == 0;
					N = Memory[operands[0]].GetBit(7);
					break;
				case AddressingMode.Absolute:
					{
						var address = BitConverter.ToUInt16(operands, 0);

						C = Memory[address].GetBit(0);

						Memory[address] >>= 1;
						Memory[address] = Memory[address].SetBit(7, oldCarry);

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
				case AddressingMode.ZeroPageX:
					C = Memory[(byte) (operands[0] + X)].GetBit(0);

					Memory[(byte) (operands[0] + X)] >>= 1;
					Memory[(byte) (operands[0] + X)] = Memory[(byte) (operands[0] + X)].SetBit(7, oldCarry);

					Z = Memory[(byte) (operands[0] + X)] == 0;
					N = Memory[(byte) (operands[0] + X)].GetBit(7);
					break;
				case AddressingMode.AbsoluteX:
					{
						var address = BitConverter.ToUInt16(operands, 0) + X;

						C = Memory[address].GetBit(0);

						Memory[address] >>= 1;
						Memory[address] = Memory[address].SetBit(7, oldCarry);

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
			}
		}

		private void ASL(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.Implicit:
					C = A.GetBit(7);

					A <<= 1;

					Z = A == 0;
					N = A.GetBit(7);
					break;
				case AddressingMode.ZeroPage:
					C = Memory[operands[0]].GetBit(7);

					Memory[operands[0]] <<= 1;

					Z = Memory[operands[0]] == 0;
					N = Memory[operands[0]].GetBit(7);
					break;
				case AddressingMode.Absolute:
					{
						var address = BitConverter.ToUInt16(operands, 0);

						C = Memory[address].GetBit(7);

						Memory[address] <<= 1;

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
				case AddressingMode.ZeroPageX:
					C = Memory[(byte) (operands[0] + X)].GetBit(7);

					Memory[(byte) (operands[0] + X)] <<= 1;

					Z = Memory[(byte) (operands[0] + X)] == 0;
					N = Memory[(byte) (operands[0] + X)].GetBit(7);
					break;
				case AddressingMode.AbsoluteX:
					{
						var address = BitConverter.ToUInt16(operands, 0) + X;

						C = Memory[address].GetBit(7);

						Memory[address] <<= 1;

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
			}
		}

		private void LSR(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.Implicit:
					C = A.GetBit(0);

					A >>= 1;

					Z = A == 0;
					N = A.GetBit(7);
					break;
				case AddressingMode.ZeroPage:
					C = Memory[operands[0]].GetBit(0);

					Memory[operands[0]] >>= 1;

					Z = Memory[operands[0]] == 0;
					N = Memory[operands[0]].GetBit(7);
					break;
				case AddressingMode.Absolute:
					{
						var address = BitConverter.ToUInt16(operands, 0);

						C = Memory[address].GetBit(0);

						Memory[address] >>= 1;

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);

						break;
					}
				case AddressingMode.ZeroPageX:
					C = Memory[(byte) (operands[0] + X)].GetBit(0);

					Memory[(byte) (operands[0] + X)] >>= 1;

					Z = Memory[(byte) (operands[0] + X)] == 0;
					N = Memory[(byte) (operands[0] + X)].GetBit(7);
					break;
				case AddressingMode.AbsoluteX:
					{
						var address = BitConverter.ToUInt16(operands, 0) + X;

						C = Memory[address].GetBit(0);

						Memory[address] >>= 1;

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);

						break;
					}
			}

		}

		private void RTI(InstructionMetadata metadata, byte[] operands)
		{
			P = (byte) (PopByte() & 0xEF | 0x20);
			PC = PopUInt16();
		}

		private void TYA(InstructionMetadata metadata, byte[] operands)
		{
			A = Y;

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void TXA(InstructionMetadata metadata, byte[] operands)
		{
			A = X;

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void TSX(InstructionMetadata metadata, byte[] operands)
		{
			X = S;

			Z = X == 0;
			N = X.GetBit(7);
		}

		private void TAY(InstructionMetadata metadata, byte[] operands)
		{
			Y = A;

			Z = Y == 0;
			N = Y.GetBit(7);
		}

		private void TAX(InstructionMetadata metadata, byte[] operands)
		{
			X = A;

			Z = X == 0;
			N = X.GetBit(7);
		}

		private void DEY(InstructionMetadata metadata, byte[] operands)
		{
			Y--;

			Z = Y == 0;
			N = Y.GetBit(7);
		}

		private void DEX(InstructionMetadata metadata, byte[] operands)
		{
			X--;

			Z = X == 0;
			N = X.GetBit(7);
		}

		private void DEC(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.ZeroPage:
					Memory[operands[0]]--;

					Z = Memory[operands[0]] == 0;
					N = Memory[operands[0]].GetBit(7);
					break;
				case AddressingMode.Absolute:
					{
						var address = BitConverter.ToUInt16(operands, 0);

						Memory[address]--;

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
				case AddressingMode.ZeroPageX:
					Memory[(byte) (operands[0] + X)]--;

					Z = Memory[(byte) (operands[0] + X)] == 0;
					N = Memory[(byte) (operands[0] + X)].GetBit(7);
					break;
				case AddressingMode.AbsoluteX:
					{
						var address = BitConverter.ToUInt16(operands, 0) + X;

						Memory[address]--;

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
			}
		}

		private void INY(InstructionMetadata metadata, byte[] operands)
		{
			Y++;

			Z = Y == 0;
			N = Y.GetBit(7);
		}

		private void INX(InstructionMetadata metadata, byte[] operands)
		{
			X++;

			Z = X == 0;
			N = X.GetBit(7);
		}

		private void INC(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.ZeroPage:
					Memory[operands[0]]++;

					Z = Memory[operands[0]] == 0;
					N = Memory[operands[0]].GetBit(7);
					break;
				case AddressingMode.Absolute:
					{
						var address = BitConverter.ToUInt16(operands, 0);

						Memory[address]++;

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
				case AddressingMode.ZeroPageX:
					Memory[(byte) (operands[0] + X)]++;

					Z = Memory[(byte) (operands[0] + X)] == 0;
					N = Memory[(byte) (operands[0] + X)].GetBit(7);
					break;
				case AddressingMode.AbsoluteX:
					{
						var address = BitConverter.ToUInt16(operands, 0) + X;

						Memory[address]++;

						Z = Memory[address] == 0;
						N = Memory[address].GetBit(7);
						break;
					}
			}
		}

		private void ADC(InstructionMetadata metadata, byte[] operands)
		{
			var value = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					value = operands[0];
					break;
				case AddressingMode.IndexedIndirect:
					value = Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)];
					break;
				case AddressingMode.ZeroPage:
					value = Memory[operands[0]];
					break;
				case AddressingMode.Absolute:
					value = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					value = Memory[(ushort) address];
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = Memory[(ushort) (absolute + Y)];
						break;
					}
				case AddressingMode.ZeroPageX:
					value = Memory[(byte) (operands[0] + X)];
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = Memory[(ushort) (absolute + X)];
						break;
					}
			}

			var unsignedResult = A + (uint) value + (uint) (C ? 1 : 0);

			C = unsignedResult > byte.MaxValue;
			V = ((value ^ unsignedResult) & (A ^ unsignedResult) & 0x80) != 0;
			
			A = (byte) unsignedResult;

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void SBC(InstructionMetadata metadata, byte[] operands)
		{
			var value = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					value = operands[0];
					break;
				case AddressingMode.IndexedIndirect:
					value = Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)];
					break;
				case AddressingMode.ZeroPage:
					value = Memory[operands[0]];
					break;
				case AddressingMode.Absolute:
					value = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					value = Memory[(ushort) address];
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = Memory[(ushort) (absolute + Y)];
						break;
					}
				case AddressingMode.ZeroPageX:
					value = Memory[(byte) (operands[0] + X)];
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = Memory[(ushort) (absolute + X)];
						break;
					}
			}

			var unsignedResult = A + (byte)~value + (uint) (C ? 1 : 0);

			C = unsignedResult > byte.MaxValue;
			V = (((byte) ~value ^ unsignedResult) & (A ^ unsignedResult) & 0x80) != 0;

			A = (byte) unsignedResult;

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void CPY(InstructionMetadata metadata, byte[] operands)
		{
			var value = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					value = operands[0];
					break;
				case AddressingMode.ZeroPage:
					value = Memory[operands[0]];
					break;
				case AddressingMode.Absolute:
					value = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
			}

			C = Y >= value;
			Z = Y == value;
			N = ((byte) (Y - value)).GetBit(7);
		}

		private void CPX(InstructionMetadata metadata, byte[] operands)
		{
			var value = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					value = operands[0];
					break;
				case AddressingMode.ZeroPage:
					value = Memory[operands[0]];
					break;
				case AddressingMode.Absolute:
					value = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
			}

			C = X >= value;
			Z = X == value;
			N = ((byte) (X - value)).GetBit(7);
		}

		private void CMP(InstructionMetadata metadata, byte[] operands)
		{
			var value = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					value = operands[0];
					break;
				case AddressingMode.IndexedIndirect:
					value = Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)];
					break;
				case AddressingMode.ZeroPage:
					value = Memory[operands[0]];
					break;
				case AddressingMode.Absolute:
					value = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					value = Memory[(ushort) address];
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = Memory[(ushort) (absolute + Y)];
						break;
					}
				case AddressingMode.ZeroPageX:
					value = Memory[(byte) (operands[0] + X)];
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						value = Memory[(ushort) (absolute + X)];
						break;
					}
			}

			C = A >= value;
			Z = A == value;
			N = ((byte) (A - value)).GetBit(7);
		}

		private void EOR(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					A = (byte) (A ^ operands[0]);
					break;
				case AddressingMode.IndexedIndirect:
					A = (byte) (A ^ Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)]);
					break;
				case AddressingMode.ZeroPage:
					A = (byte) (A ^ Memory[operands[0]]);
					break;
				case AddressingMode.Absolute:
					A = (byte) (A ^ Memory[BitConverter.ToUInt16(operands, 0)]);
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					A = (byte) (A ^ Memory[(ushort) address]);
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						A = (byte) (A ^ Memory[(ushort) (absolute + Y)]);
						break;
					}
				case AddressingMode.ZeroPageX:
					A = (byte) (A ^ Memory[(byte) (operands[0] + X)]);
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						A = (byte) (A ^ Memory[(ushort) (absolute + X)]);
						break;
					}
			}

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void ORA(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					A = (byte) (A | operands[0]);
					break;
				case AddressingMode.IndexedIndirect:
					A = (byte) (A | Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)]);
					break;
				case AddressingMode.ZeroPage:
					A = (byte) (A | Memory[operands[0]]);
					break;
				case AddressingMode.Absolute:
					A = (byte) (A | Memory[BitConverter.ToUInt16(operands, 0)]);
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					A = (byte) (A | Memory[(ushort) address]);
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						A = (byte) (A | Memory[(ushort) (absolute + Y)]);
						break;
					}
				case AddressingMode.ZeroPageX:
					A = (byte) (A | Memory[(byte) (operands[0] + X)]);
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						A = (byte) (A | Memory[(ushort) (absolute + X)]);
						break;
					}
			}

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void AND(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					A = (byte) (A & operands[0]);
					break;
				case AddressingMode.IndexedIndirect:
					A = (byte) (A & Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)]);
					break;
				case AddressingMode.ZeroPage:
					A = (byte) (A & Memory[operands[0]]);
					break;
				case AddressingMode.Absolute:
					A = (byte) (A & Memory[BitConverter.ToUInt16(operands, 0)]);
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					A = (byte) (A & Memory[(ushort) address]);
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						A = (byte) (A & Memory[(ushort) (absolute + Y)]);
						break;
					}
				case AddressingMode.ZeroPageX:
					A = (byte) (A & Memory[(byte) (operands[0] + X)]);
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						A = (byte) (A & Memory[(ushort) (absolute + X)]);
						break;
					}
			}

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void PLA(InstructionMetadata metadata, byte[] operands)
		{
			A = PopByte();

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void RTS(InstructionMetadata metadata, byte[] operands)
		{
			PC = (ushort) (PopUInt16() + 1);
		}

		private void BIT(InstructionMetadata metadata, byte[] operands)
		{
			byte value = 0;

			switch (metadata.AddressingMode)
			{
				case AddressingMode.ZeroPage:
					value = Memory[operands[0]];
					break;
				case AddressingMode.Absolute:
					value = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
			}

			Z = (A & value) == 0;
			V = value.GetBit(6);
			N = value.GetBit(7);
		}

		private void B(IList<byte> operands, bool flag, bool state)
		{
			if (flag == state)
			{
				var target = (ushort) (PC + (sbyte) operands[0]);

				if (PC >> 8 << 8 != target >> 8 << 8)
					_currentInstruction.CyclesLeft++;

				PC = target;

				_currentInstruction.CyclesLeft++;
			}
		}

		private void JMP(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.Absolute:
					PC = BitConverter.ToUInt16(operands, 0);
					break;
				case AddressingMode.Indirect:
					var indirect = BitConverter.ToUInt16(operands, 0);
					PC = BitConverter.ToUInt16(new[] { Memory[indirect], (indirect & 0xFF) == 0xFF ? Memory[indirect - 0xFF] : Memory[indirect + 1] }, 0);
					break;
			}
		}

		private void JSR(InstructionMetadata metadata, byte[] operands)
		{
			Push((ushort) (PC - 1));

			PC = BitConverter.ToUInt16(operands, 0);
		}

		private void STA(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.ZeroPage:
					Memory[operands[0]] = A;
					break;
				case AddressingMode.Absolute:
					Memory[BitConverter.ToUInt16(operands, 0)] = A;
					break;
				case AddressingMode.IndexedIndirect:
					Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)] = A;
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					Memory[(ushort) address] = A;
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						Memory[(ushort) (absolute + Y)] = A;
						break;
					}
				case AddressingMode.ZeroPageX:
					Memory[(byte) (operands[0] + X)] = A;
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						Memory[(ushort) (absolute + X)] = A;
						break;
					}
			}
		}

		private void STX(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.ZeroPage:
					Memory[operands[0]] = X;
					break;
				case AddressingMode.Absolute:
					Memory[BitConverter.ToUInt16(operands, 0)] = X;
					break;
				case AddressingMode.ZeroPageY:
					Memory[(byte) (operands[0] + Y)] = X;
					break;
			}
		}

		private void STY(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.ZeroPage:
					Memory[operands[0]] = Y;
					break;
				case AddressingMode.Absolute:
					Memory[BitConverter.ToUInt16(operands, 0)] = Y;
					break;
				case AddressingMode.ZeroPageX:
					Memory[(byte) (operands[0] + X)] = Y;
					break;
			}
		}

		private void LDA(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					A = operands[0];
					break;
				case AddressingMode.Absolute:
					A = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
				case AddressingMode.ZeroPage:
					A = Memory[operands[0]];
					break;
				case AddressingMode.IndexedIndirect:
					A = Memory[BitConverter.ToUInt16(new[] { Memory[(byte) (operands[0] + X)], Memory[(byte) (operands[0] + X + 1)] }, 0)];
					break;
				case AddressingMode.IndirectIndexed:
					var baseIndirect = BitConverter.ToUInt16(new[] { Memory[operands[0]], Memory[(byte) (operands[0] + 1)] }, 0);
					var address = baseIndirect + Y;
					if (baseIndirect >> 8 << 8 != address >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					A = Memory[(ushort) address];
					break;
				case AddressingMode.AbsoluteY:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						A = Memory[(ushort) (absolute + Y)];
					}
					break;
				case AddressingMode.ZeroPageX:
					A = Memory[(byte) (operands[0] + X)];
					break;
				case AddressingMode.AbsoluteX:
					{
						var absolute = BitConverter.ToUInt16(operands, 0);
						if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
							_currentInstruction.CyclesLeft++;
						A = Memory[(ushort) (absolute + X)];
					}
					break;
			}

			Z = A == 0;
			N = A.GetBit(7);
		}

		private void LDX(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					X = operands[0];
					break;
				case AddressingMode.Absolute:
					X = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
				case AddressingMode.ZeroPage:
					X = Memory[operands[0]];
					break;
				case AddressingMode.ZeroPageY:
					X = Memory[(byte) (operands[0] + Y)];
					break;
				case AddressingMode.AbsoluteY:
					var absolute = BitConverter.ToUInt16(operands, 0);
					if (absolute >> 8 << 8 != (ushort) (absolute + Y) >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					X = Memory[(ushort) (absolute + Y)];
					break;
			}

			Z = X == 0;
			N = X.GetBit(7);
		}

		private void LDY(InstructionMetadata metadata, byte[] operands)
		{
			switch (metadata.AddressingMode)
			{
				case AddressingMode.Immediate:
					Y = operands[0];
					break;
				case AddressingMode.Absolute:
					Y = Memory[BitConverter.ToUInt16(operands, 0)];
					break;
				case AddressingMode.ZeroPage:
					Y = Memory[operands[0]];
					break;
				case AddressingMode.ZeroPageX:
					Y = Memory[(byte) (operands[0] + X)];
					break;
				case AddressingMode.AbsoluteX:
					var absolute = BitConverter.ToUInt16(operands, 0);
					if (absolute >> 8 << 8 != (ushort) (absolute + X) >> 8 << 8)
						_currentInstruction.CyclesLeft++;
					Y = Memory[(ushort) (absolute + X)];
					break;
			}

			Z = Y == 0;
			N = Y.GetBit(7);
		}
	}

	internal class InstructionMetadata
	{
		public readonly Action<InstructionMetadata, byte[]> Action;
		public readonly AddressingMode AddressingMode;
		public readonly int CycleCount;
		public readonly string Name;
		public readonly ushort OperandSize;

		public InstructionMetadata(string name, AddressingMode addressingMode, ushort operandSize, int cycleCount, Action<InstructionMetadata, byte[]> action)
		{
			Name = name;
			AddressingMode = addressingMode;
			OperandSize = operandSize;
			CycleCount = cycleCount;
			Action = action;
		}
	}
}