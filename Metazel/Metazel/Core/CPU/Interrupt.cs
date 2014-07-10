using System;
using System.Windows;

namespace Metazel.NES
{
    // ReSharper disable InconsistentNaming
    public class Interrupt
    {
        public InterruptType Type { get; private set; }
        public ushort Vector { get; private set; }
        public int CycleCount { get; private set; }
        public bool PushCPUState { get; private set; }
        public bool SetB { get; private set; }
        public bool SetBit5 { get; private set; }
        public bool SetI { get; private set; }

        public Interrupt(InterruptType type)
        {
            Type = type;

            switch (type)
            {
                case InterruptType.NMI:
                    CycleCount = 5;
                    Vector = 0xFFFA;
                    PushCPUState = true;
                    SetB = false;
                    SetBit5 = false;
                    SetI = true;
                    break;
                case InterruptType.RESET:
                    CycleCount = 0;
                    Vector = 0xFFFC;
                    PushCPUState = false;
                    SetB = false;
                    SetBit5 = false;
                    SetI = true;
                    break;
                case InterruptType.IRQ:
                    CycleCount = 0; //Todo: Make IRQ not use a cycle.
                    Vector = 0xFFFE;
                    PushCPUState = true;
                    SetB = false;
                    SetBit5 = false;
                    SetI = true;
                    break;
                case InterruptType.BRK:
                    CycleCount = 6;
                    Vector = 0xFFFE;
                    PushCPUState = true;
                    SetB = true;
                    SetBit5 = true;
                    SetI = true;
                    break;
                default:
                    throw new ArgumentException("Invalid interrupt type!");
            }
        }

        public Instruction GetInstruction(NESCPU cpu)
        {
            var interruptVector = BitConverter.GetBytes(Vector);

            var metadata = new InstructionMetadata(Type.ToString(), AddressingMode.Implicit, 0, CycleCount, (data, vectorBytes) =>
            {
                if(PushCPUState)
                {
                    cpu.Push(cpu.PC);

                    var flags = cpu.P | (SetB ? 0x10 : 0x00) | (SetBit5 ? 0x20 : 0x00);
                    cpu.Push((byte)(cpu.P | flags));
                }

                var vector = BitConverter.ToUInt16(vectorBytes, 0);
                cpu.PC = BitConverter.ToUInt16(new[] { cpu.Memory[vector], cpu.Memory[vector + 1] }, 0);

                if(SetI)
                    cpu.I = true;
            });

            cpu.RemoveInterrupt(this);

            return new Instruction(metadata, interruptVector);
        }
    }

    public enum InterruptType
    {
        RESET,
        BRK,
        IRQ,
        NMI
    }
}