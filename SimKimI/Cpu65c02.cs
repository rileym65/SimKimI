using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SimKimI
{
    public class Cpu65c02
    {
        public const int FLAG_C = 1;
        public const int FLAG_Z = 2;
        public const int FLAG_I = 4;
        public const int FLAG_D = 8;
        public const int FLAG_B = 16;
        public const int FLAG_V = 64;
        public const int FLAG_N = 128;

        public Memory memory;
        public long cycles { get; set; }
        public Boolean halted { get; set; }
        public UInt16 pc;
        public UInt16 lastPc;
        public byte ac;
        public byte x;
        public byte y;
        public byte sp;
        public byte flags;
        public byte inst;
        public byte am;
        public byte opcode;
        public UInt16 ea;
        public Boolean irqPending;
        public Boolean nmiPending;
        public Boolean ResetPending;
        public Boolean Debug;
        public Boolean NextStep;
        protected Boolean terminateFlag;
        public long cycleRunOff;
        public long missedCycles;
        public String DebugOutput;
        protected String AddressString;
        public int Frequency;

        public Cpu65c02(Memory m)
        {
            memory = m;
            cycles = 0;
            halted = true;
            Debug = false;
            NextStep = false;
            reset();
        }

        protected void WriteMem(UInt16 address, byte value)
        {
            cycles++;
            memory.Write(address, value);
        }

        protected byte ReadMem(UInt16 address)
        {
            cycles++;
            return memory.Read(address);
        }

        protected void Push(byte value)
        {
            cycles++;
            WriteMem((UInt16)(0x100 + sp), value);
            sp--;
        }

        protected byte Pop()
        {
            sp++;
            cycles += 2;
            return ReadMem((UInt16)(0x100 + sp));
        }

        protected byte Fetch()
        {
            return ReadMem(pc++);
        }

        protected UInt16 ImmediateAddress()
        {
            UInt16 ret;
            ret = pc;
            pc++;
            if (Debug) AddressString = "#$" + ReadMem(ret).ToString("X2");
            return ret;
        }

        protected UInt16 AbsoluteAddress()
        {
            UInt16 addr;
            addr = Fetch();
            addr |= (UInt16)((UInt16)Fetch() << 8);
            if (Debug) AddressString = "$" + addr.ToString("X4");
            return addr;
        }

        protected UInt16 ZeroPageAddress()
        {
            UInt16 addr;
            addr = Fetch();
            if (Debug) AddressString = "$" + addr.ToString("X2");
            return addr;
        }

        protected UInt16 ZeroPageIndexedXAddress()
        {
            UInt16 addr;
            addr = Fetch();
            if (Debug) AddressString = "$" + addr.ToString("X2") + ",X";
            addr = (UInt16)((addr + x) & 0xff);
            cycles++;
            return addr;
        }

        protected UInt16 ZeroPageIndexedYAddress()
        {
            UInt16 addr;
            addr = Fetch();
            if (Debug) AddressString = "$" + addr.ToString("X2") + ",Y";
            addr = (UInt16)((addr + y) & 0xff);
            cycles++;
            return addr;
        }

        protected UInt16 AbsoluteIndexedXAddress()
        {
            UInt16 addr;
            addr = Fetch();
            addr |= (UInt16)((UInt16)Fetch() << 8);
            if (Debug) AddressString = "$" + addr.ToString("X4") + ",X";
            addr += x;
            return addr;
        }

        protected UInt16 AbsoluteIndexedYAddress()
        {
            UInt16 addr;
            addr = Fetch();
            addr |= (UInt16)((UInt16)Fetch() << 8);
            if (Debug) AddressString = "$" + addr.ToString("X4") + ",Y";
            addr += y;
            return addr;
        }

        protected UInt16 AbsoluteIndirectAddress()
        {
            UInt16 addr;
            UInt16 ret;
            addr = Fetch();
            addr |= (UInt16)((UInt16)Fetch() << 8);
            if (Debug) AddressString = "($" + addr.ToString("X4") + ")";
            ret = ReadMem(addr++);
            ret |= (UInt16)((UInt16)ReadMem(addr) << 8);
            return ret;
        }

        protected UInt16 AbsoluteIndirectIndexedAddress()
        {
            UInt16 addr;
            UInt16 ret;
            addr = Fetch();
            addr |= (UInt16)((UInt16)Fetch() << 8);
            if (Debug) AddressString = "($" + addr.ToString("X4") + ",X)";
            addr += x;
            ret = ReadMem(addr++);
            ret |= (UInt16)((UInt16)ReadMem(addr) << 8);
            return ret;
        }

        protected UInt16 FetchRelative()
        {
            UInt16 addr;
            addr = Fetch();
            if (Debug) AddressString = "$" + addr.ToString("X2");
            if (addr > 127) addr |= 0xff00;
            return (UInt16)(pc + addr);
        }

        protected UInt16 IndexedIndirectXAddress()
        {
            UInt16 addr;
            UInt16 addr2;
            addr = Fetch();
            if (Debug) AddressString = "($" + addr.ToString("X2") + ",X)";
            addr = (UInt16)((addr + x) & 0xff);
            addr2 = ReadMem(addr);
            addr = (UInt16)((addr + 1) & 0xff);
            addr2 |= (UInt16)((UInt16)ReadMem(addr) << 8);
            cycles++;
            return addr2;
        }

        protected UInt16 IndirectIndexedYAddress()
        {
            UInt16 addr;
            UInt16 addr2;
            addr = Fetch();
            if (Debug) AddressString = "($" + addr.ToString("X2") + "),Y";
            addr2 = ReadMem(addr);
            addr = (UInt16)((addr + 1) & 0xff);
            addr2 |= (UInt16)((UInt16)ReadMem(addr) << 8);
            addr2 += y;
            return addr2;
        }

        protected UInt16 Indirectaddress()
        {
            UInt16 addr;
            UInt16 addr2;
            addr = Fetch();
            addr2 = ReadMem(addr);
            addr = (UInt16)((addr + 1) & 0xff);
            addr2 |= (UInt16)((UInt16)ReadMem(addr) << 8);
            return addr2;
        }

        protected UInt16 ZeroPageIndirect()
        {
            UInt16 addr;
            UInt16 addr2;
            addr = Fetch();
            if (Debug) AddressString = "($" + addr.ToString("X2") + ")";
            addr2 = ReadMem(addr);
            addr = (UInt16)((addr + 1) & 0xff);
            addr2 |= (UInt16)((UInt16)ReadMem(addr) << 8);
            return addr2;
        }

        protected void logicalFlags(byte value)
        {
            flags = (byte)((flags & 0x7f) | (value & 0x80));
            if (value == 0) flags |= 0x02; else flags &= 0xfd;
        }

        protected void doADC(UInt16 ea)
        {
            UInt16 a1, a2, r;
            a1 = ac;
            a2 = ReadMem(ea);
            r = (UInt16)(a1 + a2);
            if ((flags & 1) != 0) r++;
            if ((flags & 0x08) == 0)
            {
                ac = (byte)(r & 0xff);
                if (r > 0xff) flags |= FLAG_C;
                else flags &= (0xff - FLAG_C);
                if ((a1 & 0x80) == (a2 & 0x80))
                {
                    if ((r & 0x80) != (a1 & 0x80)) flags |= FLAG_V;
                    else flags &= (0xff - FLAG_V);
                }
                else flags &= (0xff - FLAG_V);
                logicalFlags(ac);
            }
            else
            {
            }
        }

        protected byte doSBC(byte src,UInt16 ea,Boolean updateV,int carry)
        {
            UInt16 a1, a2, r;
            a1 = src;
            a2 = ReadMem(ea);
            a2 = (UInt16)((~a2 + carry) & 0xff);
            r = (UInt16)(a1 + a2);
            if ((flags & 0x08) == 0)
            {
                if (r > 0xff) flags |= FLAG_C;
                else flags &= (0xff - FLAG_C);
                if (updateV)
                {
                    if ((a1 & 0x80) == (a2 & 0x80))
                    {
                        if ((r & 0x80) != (a1 & 0x80)) flags |= FLAG_V;
                        else flags &= (0xff - FLAG_V);
                    }
                    else flags &= (0xff - FLAG_V);
                }
                logicalFlags((byte)r);
            }
            else
            {
            }
            return (byte)(r & 0xff);
        }

        protected String convertDisp(UInt16 disp)
        {
            String ret;
            if (disp < 256)
            {
                ret = "+" + (disp).ToString();
            }
            else
            {
                disp = (UInt16)~disp;
                ret = "-" + (disp + 1).ToString();
            }
            return ret;
        }

        protected void group0()
        {
            byte value;
            byte mask;
            byte comp;
            UInt16 disp;
            if (am == 0 && opcode == 0)                          // BRK
            {
                pc++;
                Push((byte)(pc >> 8));
                Push((byte)(pc & 0xff));
                Push((byte)(flags | FLAG_B));
                flags |= FLAG_I;
                pc = (UInt16)(ReadMem(0xfffe) | (ReadMem(0xffff) << 8));
                cycles -= 2;
                if (Debug) DebugOutput += "BRK";
                return;
            }
            if (am == 0 && opcode == 2)                          // RTI
            {
                flags = Pop();
                pc = Pop();
                pc |= (UInt16)((Pop() << 8));
                cycles -= 4;
                if (Debug) DebugOutput += "RTI";
                return;
            }
            if (am == 0 && opcode == 4)                          // BRA
            {
                disp = Fetch();
                if (disp >= 0x80) disp |= 0xff00;
                pc += disp;
                cycles++;
                if (Debug) DebugOutput += "BRA" + disp.ToString("X2");
                return;
            }
            if (am == 2)
            {
                switch (opcode)
                {
                    case 0:                                      // PHP
                        Push(flags);
                        if (Debug) DebugOutput += "PHP";
                        break;
                    case 1:                                      // PLP
                        flags = Pop();
                        if (Debug) DebugOutput += "PLP";
                        break;
                    case 2:                                      // PHA
                        Push(ac);
                        if (Debug) DebugOutput += "PHA";
                        break;
                    case 3:                                      // PLA
                        ac = Pop();
                        logicalFlags(ac);
                        if (Debug) DebugOutput += "PLA";
                        break;
                    case 4:                                      // DEY
                        y--;
                        cycles++;
                        logicalFlags(y);
                        if (Debug) DebugOutput += "DEY";
                        break;
                    case 5:                                      // TAY
                        y = ac;
                        cycles++;
                        logicalFlags(y);
                        if (Debug) DebugOutput += "TAY";
                        break;
                    case 6:                                      // INY
                        y++;
                        cycles++;
                        logicalFlags(y);
                        if (Debug) DebugOutput += "INY";
                        break;
                    case 7:                                      // INX
                        x++;
                        cycles++;
                        logicalFlags(x);
                        if (Debug) DebugOutput += "INX";
                        break;
                }
                return;
            }
            if (am == 4)
            {
                mask = 0;
                disp = Fetch();
                if (disp >= 0x80) disp |= 0xff00;
                switch (opcode >> 1)
                {
                    case 0: mask = 0x80; break;
                    case 1: mask = 0x40; break;
                    case 2: mask = 0x01; break;
                    case 3: mask = 0x02; break;
                }
                comp = (byte)(((opcode & 1) == 1) ? mask : 0x00);
                if (Debug)
                {
                    switch (opcode)
                    {
                        case 0: DebugOutput += "BPL " + convertDisp(disp); break;
                        case 1: DebugOutput += "BMI " + convertDisp(disp); break;
                        case 2: DebugOutput += "BVC " + convertDisp(disp); break;
                        case 3: DebugOutput += "BVS " + convertDisp(disp); break;
                        case 4: DebugOutput += "BCC " + convertDisp(disp); break;
                        case 5: DebugOutput += "BCS " + convertDisp(disp); break;
                        case 6: DebugOutput += "BNE " + convertDisp(disp); break;
                        case 7: DebugOutput += "BEQ " + convertDisp(disp); break;
                    }
                }
                if ((flags & mask) == comp) pc += disp;
                return;
            }
            if ((am == 5  || am == 7) && opcode == 0)            // TRB
            {
                ea = (am == 5) ? ZeroPageAddress() : AbsoluteAddress();
                value = ReadMem(ea);
                if ((ac & value) == 0) flags |= FLAG_Z; else flags &= (255 - FLAG_Z);
                value &= (byte)(~ac);
                WriteMem(ea, value);
                cycles++;
                if (Debug) DebugOutput += "TRB " + AddressString;
                return;
            }
            if (am == 6)
            {
                cycles++;
                switch (opcode)
                {
                    case 0:                                      // CLC
                        flags &= (255 - FLAG_C);
                        if (Debug) DebugOutput += "CLC";
                        break;
                    case 1:                                      // SEC
                        flags |= FLAG_C;
                        if (Debug) DebugOutput += "SEC";
                        break;
                    case 2:                                      // CLI
                        flags &= (255 - FLAG_I);
                        if (Debug) DebugOutput += "CLI";
                        break;
                    case 3:                                      // SEI
                        flags |= FLAG_I;
                        if (Debug) DebugOutput += "SEI";
                        break;
                    case 4:                                      // TYA
                        ac = y;
                        logicalFlags(ac);
                        if (Debug) DebugOutput += "TYA";
                        break;
                    case 5:                                      // CLV
                        flags &= (255 - FLAG_V);
                        if (Debug) DebugOutput += "CLV";
                        break;
                    case 6:                                      // CLD
                        flags &= (255 - FLAG_D);
                        if (Debug) DebugOutput += "CLD";
                        break;
                    case 7:                                      // SED
                        flags |= FLAG_D;
                        if (Debug) DebugOutput += "SED";
                        break;
                }
                return;
            }
            if (am == 7 && opcode == 4)
            {
                switch (opcode)
                {
                    case 4:                                      // STZ abs
                        ea = AbsoluteAddress();
                        WriteMem(ea, 0);
                        if (Debug) DebugOutput += "STZ " + AddressString;
                        break;
                }
                return;
            }
            switch (am)
            {
                case 0x0: ea = (opcode != 1) ? ImmediateAddress() : AbsoluteAddress(); break;
                case 0x1: ea = ZeroPageAddress(); break;
                case 0x3: ea = AbsoluteAddress(); break;
                case 0x5: ea = ZeroPageIndexedXAddress(); break;
                case 0x7: ea = AbsoluteIndexedXAddress(); break;
                default: ea = 0; break;
            }
            switch (opcode)
            {
                case 0x0:                                               // TSB
                    value = ReadMem(ea);
                    if ((ac & value) == 0) flags |= FLAG_Z; else flags &= (255 - FLAG_Z);
                    value |= ac;
                    WriteMem(ea,value);
                    cycles++;
                    if (Debug) DebugOutput += "TSB " + AddressString;
                    break;
                case 0x1:                                               // BIT
                    if (am == 0)                                           // JSR
                    {
                        pc--;
                        Push((byte)(pc >> 8));
                        Push((byte)(pc & 0xff));
                        pc = ea;
                        cycles--;
                        if (Debug) DebugOutput += "JSR " + AddressString;
                        return;
                    }
                    value = ReadMem(ea);
                    if (am != 0) flags = (byte)((flags & 0x3f) | (value & 0xc0));
                    value &= ac;
                    flags = (byte)((flags & 0xfd) | ((value == 0) ? 0x2 : 0));
                    if (Debug) DebugOutput += "BIT " + AddressString;
                    break;
                case 0x2:                                               // JMP
                    pc = ea;
                    if (Debug) DebugOutput += "JMP " + AddressString;
                    break;
                case 0x3:                                               // JMP (ABS)
                    if (am == 0)                                           // RTS
                    {
                        pc = Pop();
                        pc |= (UInt16)(Pop() << 8);
                        pc++;
                        cycles--;
                        if (Debug) DebugOutput += "RTS";
                        return;
                    }
                    if (am == 1 || am == 5)                                // STZ zp
                    {
                        WriteMem(ea, 0);
                        if (Debug) DebugOutput += "STZ " + AddressString;
                        return;
                    }
                    pc = (UInt16)(ReadMem(ea) | (ReadMem((UInt16)(ea+1)) << 8));
                    if (am == 7) cycles++;
                    if (Debug) DebugOutput += "JMP " + AddressString;
                    break;
                case 0x4:                                               // STY
                    WriteMem(ea, y);
                    if (Debug) DebugOutput += "STY " + AddressString;
                    break;
                case 0x5:                                               // LDY
                    y = ReadMem(ea);
                    logicalFlags(y);
                    if (Debug) DebugOutput += "LDY " + AddressString;
                    break;
                case 0x6:                                               // CPY
                    doSBC(y, ea, false,1);
                    if (Debug) DebugOutput += "CPY " + AddressString;
                    break;
                case 0x7:                                               // CPX
                    doSBC(x, ea, false,1);
                    if (Debug) DebugOutput += "CPX " + AddressString;
                    break;
            }
        }

        protected void group1()
        {
            byte value;
            switch (am)
            {
                case 0x0: ea = IndexedIndirectXAddress(); break;
                case 0x1: ea = ZeroPageAddress(); break;
                case 0x2: ea = ImmediateAddress(); break;
                case 0x3: ea = AbsoluteAddress(); break;
                case 0x4: ea = IndirectIndexedYAddress(); break;
                case 0x5: ea = ZeroPageIndexedXAddress(); break;
                case 0x6: ea = AbsoluteIndexedYAddress(); break;
                case 0x7: ea = AbsoluteIndexedXAddress(); break;
                default: ea = 0; break;
            }
            switch (opcode)
            {
                case 0x0:                                               // ORA
                    ac |= ReadMem(ea);
                    logicalFlags(ac);
                    if (Debug) DebugOutput += "ORA " + AddressString;
                    break;
                case 0x1:                                               // AND
                    ac &= ReadMem(ea);
                    logicalFlags(ac);
                    if (Debug) DebugOutput += "AND " + AddressString;
                    break;
                case 0x2:                                               // EOR
                    ac ^= ReadMem(ea);
                    logicalFlags(ac);
                    if (Debug) DebugOutput += "EOR " + AddressString;
                    break;
                case 0x3:                                               // ADC
                    doADC(ea);
                    if (Debug) DebugOutput += "ADC " + AddressString;
                    break;
                case 0x4:                                               // STA
                    if (am == 0x2)                                         // BIT #
                    {
                        value = ReadMem(ea);
                        value &= ac;
                        flags = (byte)((flags & 0xfd) | ((value == 0) ? 0x2 : 0));
                        if (Debug) DebugOutput += "BIT " + AddressString;
                        return;
                    }
                    WriteMem(ea, ac);
                    if (am == 7 || am == 6 || am == 4) cycles++;
                    if (Debug) DebugOutput += "STA " + AddressString;
                    break;
                case 0x5:                                               // LDA
                    ac = ReadMem(ea);
                    logicalFlags(ac);
                    if (Debug) DebugOutput += "LDA " + AddressString;
                    break;
                case 0x6:                                               // CMP
                    doSBC(ac, ea, false,1);
                    if (Debug) DebugOutput += "CMP " + AddressString;
                    break;
                case 0x7:                                               // SBC
                    ac = doSBC(ac, ea, true,flags & 1);
                    if (Debug) DebugOutput += "SBC " + AddressString;
                    break;
            }
        }

        protected void group2()
        {
            byte value;
            byte carry;
            if (am == 6)
            {
                switch (opcode)
                {
                    case 0:                                      // INC A
                        ac++;
                        logicalFlags(ac);
                        cycles++;
                        if (Debug) DebugOutput += "INC A";
                        break;
                    case 1:                                      // DEC A
                        ac--;
                        logicalFlags(ac);
                        cycles++;
                        if (Debug) DebugOutput += "DEC A";
                        break;
                    case 2:                                      // PHY
                        Push(y);
                        if (Debug) DebugOutput += "PHY";
                        break;
                    case 3:                                      // PLY
                        y = Pop();
                        logicalFlags(y);
                        if (Debug) DebugOutput += "PLY";
                        break;
                    case 4:                                      // TXS
                        sp = x;
                        cycles++;
                        if (Debug) DebugOutput += "TXS";
                        break;
                    case 5:                                      // TSX
                        x = sp;
                        cycles++;
                        logicalFlags(x);
                        if (Debug) DebugOutput += "TSX";
                        break;
                    case 6:                                      // PHX
                        Push(x);
                        if (Debug) DebugOutput += "PHX";
                        break;
                    case 7:                                      // PLX
                        x = Pop();
                        logicalFlags(x);
                        if (Debug) DebugOutput += "PLX";
                        break;
                }
                return;
            }
            if (am == 7 && opcode == 4)
            {
                ea = AbsoluteIndexedXAddress();
                WriteMem(ea, 0);
                cycles++;
                return;
            }
            switch (am)
            {
                case 0x0: ea = ImmediateAddress(); break;
                case 0x1: ea = ZeroPageAddress(); break;
                case 0x3: ea = AbsoluteAddress(); break;
                case 0x4: ea = ZeroPageIndirect(); break;
                case 0x5: ea = (opcode == 4 || opcode == 5) ? ZeroPageIndexedYAddress() : ZeroPageIndexedXAddress();
                    break;
                case 0x7: ea = (opcode == 4 || opcode == 5) ? AbsoluteIndexedYAddress() : AbsoluteIndexedXAddress(); break;
                default: ea = 0; break;
            }
            switch (opcode)
            {
                case 0x0:                                               // ASL
                    if (am == 4)                                               // ORA
                    {
                        ac |= ReadMem(ea);
                        logicalFlags(ac);
                        if (Debug) DebugOutput += "ORA " + AddressString;
                        return;
                    }
                    cycles++;
                    if (am == 7) cycles++;
                    value = (am == 2) ? ac : ReadMem(ea);
                    if (Debug && am == 2) AddressString = "A";
                    if (value >= 0x80) flags |= FLAG_C; else flags &= (255-FLAG_C);
                    value <<= 1;
                    logicalFlags(value);
                    if (am == 2) ac = value; else WriteMem(ea, value);
                    if (Debug) DebugOutput += "ASL " + AddressString;
                    break;
                case 0x1:                                               // ROL
                    if (am == 4)                                               // AND
                    {
                        ac &= ReadMem(ea);
                        logicalFlags(ac);
                        if (Debug) DebugOutput += "AND " + AddressString;
                        return;
                    }
                    cycles++;
                    if (am == 7) cycles++;
                    value = (am == 2) ? ac : ReadMem(ea);
                    if (Debug && am == 2) AddressString = "A";
                    carry = (byte)(flags & 1);
                    if (value >= 0x80) flags |= FLAG_C; else flags &= (255 - FLAG_C);
                    value <<= 1;
                    value |= carry;
                    logicalFlags(value);
                    if (am == 2) ac = value; else WriteMem(ea, value);
                    if (Debug) DebugOutput += "ROL " + AddressString;
                    break;
                case 0x2:                                               // LSR
                    if (am == 4)                                               // EOR
                    {
                        ac ^= ReadMem(ea);
                        logicalFlags(ac);
                        if (Debug) DebugOutput += "EOR " + AddressString;
                        return;
                    }
                    cycles++;
                    if (am == 7) cycles++;
                    value = (am == 2) ? ac : ReadMem(ea);
                    if (Debug && am == 2) AddressString = "A";
                    if ((value & 1) == 0x01) flags |= FLAG_C; else flags &= (255 - FLAG_C);
                    value >>= 1;
                    logicalFlags(value);
                    if (am == 2) ac = value; else WriteMem(ea, value);
                    if (Debug) DebugOutput += "LSR " + AddressString;
                    break;
                case 0x3:                                               // ROR
                    if (am == 4)                                               // ADC
                    {
                        doADC(ea);
                        if (Debug) DebugOutput += "ADC " + AddressString;
                        return;
                    }
                    cycles++;
                    if (am == 7) cycles++;
                    value = (am == 2) ? ac : ReadMem(ea);
                    if (Debug && am == 2) AddressString = "A";
                    carry = (byte)((flags & 1) << 7);
                    if ((value & 1) == 0x01) flags |= FLAG_C; else flags &= (255 - FLAG_C);
                    value >>= 1;
                    value |= carry;
                    logicalFlags(value);
                    if (am == 2) ac = value; else WriteMem(ea, value);
                    if (Debug) DebugOutput += "ROR " + AddressString;
                    break;
                case 0x4:                                               // STX
                    if (am == 2)                                               // TXA
                    {
                        ac = x;
                        logicalFlags(ac);
                        cycles++;
                        if (Debug) DebugOutput += "TXA";
                        return;
                    }
                    if (am == 4)                                               // STA
                    {
                        WriteMem(ea, ac);
                        if (Debug) DebugOutput += "STA " + AddressString;
                        return;
                    }
                    WriteMem(ea, x);
                    if (Debug) DebugOutput += "STX " + AddressString;
                    break;
                case 0x5:                                               // LDX
                    if (am == 2)                                               // TAX
                    {
                        x = ac;
                        logicalFlags(x);
                        cycles++;
                        if (Debug) DebugOutput += "TAX";
                        return;
                    }
                    if (am == 4)                                               // LDA
                    {
                        ac = ReadMem(ea);
                        logicalFlags(ac);
                        if (Debug) DebugOutput += "LDA " + AddressString;
                        return;
                    }
                    x = ReadMem(ea);
                    logicalFlags(x);
                    if (Debug) DebugOutput += "LDX " + AddressString;
                    break;
                case 0x6:                                               // DEC
                    if (am == 2)
                    {
                        x--;                                                   // DEX
                        cycles++;
                        logicalFlags(x);
                        if (Debug) DebugOutput += "DEX";
                        return;
                    }
                    if (am == 4)                                               // CMP
                    {
                        doSBC(ac, ea,false,1);
                        if (Debug) DebugOutput += "CMP " + AddressString;
                        return;
                    }
                    cycles++;
                    if (am == 7) cycles++;
                    value = ReadMem(ea);
                    value = (byte)((value - 1) & 0xff);
                    logicalFlags(value);
                    WriteMem(ea, value);
                    if (Debug) DebugOutput += "DEC " + AddressString;
                    break;
                case 0x7:                                               // INC
                    if (am == 2)                                               // NOP
                    {
                        cycles++;
                        if (Debug) DebugOutput += "NOP";
                        return;
                    }
                    if (am == 4)                                               // SBC
                    {
                        ac = doSBC(ac, ea,true,flags & 1);
                        if (Debug) DebugOutput += "SBC " + AddressString;
                        return;
                    }
                    cycles++;
                    if (am == 7) cycles++;
                    value = ReadMem(ea);
                    value = (byte)((value + 1) & 0xff);
                    logicalFlags(value);
                    WriteMem(ea, value);
                    if (Debug) DebugOutput += "INC " + AddressString;
                    break;
            }
        }

        protected void irqAction(char type)
        {
            UInt16 addr;
            Push((byte)(pc >> 8));
            Push((byte)(pc & 0xff));
            Push(flags);
            flags |= FLAG_I;
            addr = (UInt16)((type == 'N') ? 0xfffa : 0xfffe);
            pc = ReadMem(addr);
            addr++;
            pc |= (UInt16)(ReadMem(addr) << 8);
        }

        public void reset()
        {
            flags |= FLAG_I;
            flags &= (255 - FLAG_D);
            cycles = 0;
            irqPending = false;
            nmiPending = false;
            pc = (UInt16)(ReadMem(0xfffc) | (UInt16)(ReadMem(0xfffd) << 8));
            cycleRunOff = 0;
            ResetPending = false;
            memory.Reset();
        }

        public void Reset()
        {
            ResetPending = true;
        }

        public void irq()
        {
            if ((flags & FLAG_I) != 0x00) return;
            irqPending = true;
        }

        public void nmi()
        {
            nmiPending = true;
        }

        public void cycle()
        {
            long startCycles;
            if (halted) return;
            if (Debug && !NextStep) return;
            memory.Cycle();
            if (cycleRunOff > 0)
            {
                cycleRunOff--;
                return;
            }
            if (ResetPending) reset();
            if (nmiPending)
            {
                irqAction('N');
                nmiPending = false;
                return;
            }
            if (irqPending)
            {
                irqAction('I');
                irqPending = false;
                return;
            }
            lastPc = pc;
            if (Debug) DebugOutput = "[" + pc.ToString("X4") + "]  ";
            inst = Fetch();
            startCycles = cycles;
            am = (byte)((inst >> 2) & 0x7);
            opcode = (byte)((inst >> 5) & 0x7);
            switch (inst & 0x3)
            {
                case 0: group0(); break;
                case 1: group1(); break;
                case 2: group2(); break;
            }
            memory.InstComplete();
            cycleRunOff = cycles - startCycles;
            if (Debug)
            {
                NextStep = false;
                cycleRunOff = 0;
            }
        }

        public void Run()
        {
            long ticks;
            Stopwatch clock;
            clock = new Stopwatch();
            ticks = Stopwatch.Frequency / Frequency;
            terminateFlag = false;
            clock.Start();
            while (!terminateFlag)
            {
                while (clock.ElapsedTicks < ticks) { }
                clock.Restart();
                cycle();
                if (clock.ElapsedTicks > ticks) missedCycles++;
            }
        }

        public void Terminate()
        {
            terminateFlag = true;
        }

    }
}
