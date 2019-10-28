using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimKimI
{
    public class IoChip
    {
        public byte OutA { get; protected set; }
        public byte InA { get; protected set; }
        public byte PortADir { get; protected set; }
        public byte OutB { get; protected set; }
        public byte InB { get; protected set; }
        public byte PortBDir { get; protected set; }
        public char IrqMode { get; set; }
        protected byte timer;
        protected Boolean timerIrq;
        protected Boolean timerRunning;
        protected Boolean countExpired;
        protected int divider;
        protected int tickCount;
        protected Cpu65c02 cpu;

        public IoChip(Cpu65c02 c)
        {
            cpu = c;
            OutA = 0xff;
            OutB = 0xff;
            InA = 0xff;
            InB = 0xff;
            PortADir = 0;
            PortBDir = 0;
            timerRunning = false;
            timerIrq = false;
            countExpired = false;
            timer = 0;
        }

        public void Reset()
        {
            timerRunning = false;
            timerIrq = false;
            countExpired = false;
            PortADir = 0;
            PortBDir = 0;
            OutA = 0xff;
            OutB = 0xff;
            InA = 0xff;
            InB = 0xff;
        }

        public void Write(int address, byte value)
        {
            switch (address)
            {
                case 0:
                    OutA = (byte)((0xff ^ PortADir) | (value & PortADir));
                    break;
                case 1:
                    PortADir = value;
                    break;
                case 2:
                    OutB = (byte)((0xff ^ PortBDir) | (value & PortBDir));
                    break;
                case 3:
                    PortBDir = value;
                    break;
                case 4:
                    timer = value;
                    timerRunning = true;
                    timerIrq = false;
                    divider = 1;
                    tickCount = 1;
                    countExpired = false;
                    break;
                case 5:
                    timer = value;
                    timerRunning = true;
                    timerIrq = false;
                    divider = 8;
                    tickCount = 8;
                    countExpired = false;
                    break;
                case 6:
                    timer = value;
                    timerRunning = true;
                    timerIrq = false;
                    divider = 64;
                    tickCount = 64;
                    countExpired = false;
                    break;
                case 7:
                    timer = value;
                    timerRunning = true;
                    timerIrq = false;
                    divider = 1024;
                    tickCount = 1024;
                    countExpired = false;
                    break;
                case 12:
                    timer = value;
                    timerRunning = true;
                    timerIrq = true;
                    divider = 1;
                    tickCount = 1;
                    countExpired = false;
                    break;
                case 13:
                    timer = value;
                    timerRunning = true;
                    timerIrq = true;
                    divider = 8;
                    tickCount = 8;
                    countExpired = false;
                    break;
                case 14:
                    timer = value;
                    timerRunning = true;
                    timerIrq = true;
                    divider = 64;
                    tickCount = 64;
                    countExpired = false;
                    break;
                case 15:
                    timer = value;
                    timerRunning = true;
                    timerIrq = true;
                    divider = 1024;
                    tickCount = 1024;
                    countExpired = false;
                    break;
            }
        }

        public byte Read(int address)
        {
            switch (address)
            {
                case 0:
                    return InA;
                case 1:
                    return PortADir;
                case 2:
                    return InB;
                case 3:
                    return PortBDir;
                case 4:
                    timerIrq = false;
                    if (countExpired) timerRunning = false;
                    return timer;
                case 5:
                    timerIrq = false;
                    if (countExpired) timerRunning = false;
                    if (!timerRunning) return 0x80;
                    return (byte)((countExpired) ? 0x80 : 0x00);
                case 6:
                    timerIrq = false;
                    if (countExpired) timerRunning = false;
                    return timer;
                case 7:
                    timerIrq = false;
                    if (countExpired) timerRunning = false;
                    if (!timerRunning) return 0x80;
                    return (byte)((countExpired) ? 0x80 : 0x00);
                case 12:
                    timerIrq = true;
                    if (countExpired) timerRunning = false;
                    return timer;
                case 13:
                    timerIrq = true;
                    if (countExpired) timerRunning = false;
                    if (!timerRunning) return 0x80;
                    return (byte)((countExpired) ? 0x80 : 0x00);
                case 14:
                    timerIrq = true;
                    if (countExpired) timerRunning = false;
                    return timer;
                case 15:
                    timerIrq = true;
                    if (countExpired) timerRunning = false;
                    if (!timerRunning) return 0x80;
                    return (byte)((countExpired) ? 0x80 : 0x00);
            }
            return 0xff;
        }

        public void SetPinsA(byte value)
        {
            value &= (byte)(~PortADir);
            InA = (byte)((0xff ^ (~PortADir)) | value);
        }

        public void SetPinsB(byte value)
        {
            value &= (byte)(~PortBDir);
            InB = (byte)((0xff ^ (~PortBDir)) | value);
        }

        public void Cycle()
        {
            if (!timerRunning) return;
            tickCount--;
            if (tickCount > 0) return;
            tickCount = divider;
            if (!countExpired)
            {
                timer--;
                if (timer == 0xff)
                {
                    divider = 1;
                    tickCount = 1;
                    countExpired = true;
                    if (timerIrq)
                    {
                        switch (IrqMode)
                        {
                            case 'I': cpu.irq(); break;
                            case 'N': cpu.nmi(); break;
                            case 'R': cpu.Reset(); break;
                        }
                    }
                }
            }
            else
            {
                timer--;
                if (timer == 0x00)
                {
                    timerRunning = false;
                }
                else
                {
                    if (timerIrq)
                    {
                        switch (IrqMode)
                        {
                            case 'I': cpu.irq(); break;
                            case 'N': cpu.nmi(); break;
                            case 'R': cpu.Reset(); break;
                        }
                    }
                }
            }
        }

    }
}
