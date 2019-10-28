using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace SimKimI
{
    public class KimI : Memory
    {
        protected const int DIGIT_DELAY = 50000;

        public Cpu65c02 cpu;
        public byte[] memory;
        public int[] Address;
        public int[] Data;
        public int[] keypadRows;
        public Boolean[] BaseMemoryInstalled;
        public Boolean[] BaseRom;
        public Boolean[] ExtendedMemoryInstalled;
        public Boolean[] ExtendedRom;
        public List<String> LoadFiles;
        public Boolean MemoryMapper;
        public Boolean SingleStep;
        public char Riot1IrqMode;
        public char Riot2IrqMode;
        public Boolean UseKeypad;
        public TapeDeck TapeDeck;
        public Teletype TeleType;

        protected int[] AddressTime;
        protected int[] DataTime;
        protected Thread cpuThread;
        protected int phaseCount;
        protected Boolean lastPhase;

        IoChip Io2;
        IoChip Io3;

        public KimI()
        {
            UseKeypad = true;
            memory = new byte[65536];
            Address = new int[4];
            AddressTime = new int[4];
            Data = new int[2];
            DataTime = new int[2];
            keypadRows = new int[3];
            lastPhase = false;
            phaseCount = 0;
            BaseMemoryInstalled = new Boolean[8];
            for (var i = 0; i < 8; i++) BaseMemoryInstalled[i] = false;
            BaseMemoryInstalled[0] = true;
            BaseMemoryInstalled[6] = true;
            BaseMemoryInstalled[7] = true;
            BaseRom = new Boolean[8];
            for (var i = 0; i < 8; i++) BaseRom[i] = false;
            BaseRom[6] = true;
            BaseRom[7] = true;
            ExtendedMemoryInstalled = new Boolean[8];
            for (var i = 0; i < 8; i++) ExtendedMemoryInstalled[i] = false;
            ExtendedRom = new Boolean[8];
            for (var i = 0; i < 8; i++) ExtendedRom[i] = false;
            for (var i = 0; i < 3; i++) keypadRows[i] = 0;
            cpu = new Cpu65c02(this);
            cpu.Frequency = 500000;
            TapeDeck = new TapeDeck(cpu.Frequency);
            TeleType = new Teletype(cpu.Frequency);
            TeleType.Parity = 'N';
            TeleType.DataBits = 8;
            TeleType.StopBits = 1;
            TeleType.Baud = 300;
            Io2 = new IoChip(cpu);
            Io3 = new IoChip(cpu);
            MemoryMapper = false;
            SingleStep = false;
            Riot1IrqMode = ' ';
            Riot2IrqMode = ' ';
            LoadConfiguration();
            Io2.IrqMode = Riot1IrqMode;
            Io3.IrqMode = Riot2IrqMode;
            cpuThread = new Thread(cpu.Run);
            cpuThread.Start();
        }

        public void LoadConfiguration()
        {
            StreamReader file;
            String line;
            int pos;
            LoadFiles = new List<String>();
            file = new StreamReader("KimI.cfg");
            if (file == null) return;
            while (!file.EndOfStream)
            {
                line = file.ReadLine().Trim();
                if (line.ToUpper().StartsWith("LOAD="))
                {
                    line = line.Substring(5).Trim();
                    LoadFiles.Add(line);
                    LoadHex(line);
                }
                else if (line.ToUpper().StartsWith("MAPPER="))
                {
                    line = line.Substring(7).Trim();
                    MemoryMapper = Convert.ToBoolean(line);
                }
                else if (line.ToUpper().StartsWith("USEKEYPAD="))
                {
                    line = line.Substring(10).Trim();
                    UseKeypad = Convert.ToBoolean(line);
                }
                else if (line.ToUpper().StartsWith("BASEMEMORY"))
                {
                    pos = line[10] - '0';
                    line = line.Substring(12).Trim();
                    BaseMemoryInstalled[pos] = Convert.ToBoolean(line);
                }
                else if (line.ToUpper().StartsWith("BASEROM"))
                {
                    pos = line[7] - '0';
                    line = line.Substring(9).Trim();
                    BaseRom[pos] = Convert.ToBoolean(line);
                }
                else if (line.ToUpper().StartsWith("EXTMEMORY"))
                {
                    pos = line[9] - '0';
                    line = line.Substring(11).Trim();
                    ExtendedMemoryInstalled[pos] = Convert.ToBoolean(line);
                }
                else if (line.ToUpper().StartsWith("EXTROM"))
                {
                    pos = line[6] - '0';
                    line = line.Substring(8).Trim();
                    ExtendedRom[pos] = Convert.ToBoolean(line);
                }
                else if (line.ToUpper().StartsWith("RIOT1IRQ="))
                {
                    line = line.Substring(9).Trim();
                    try
                    {
                        Riot1IrqMode = line[0];
                    }
                    catch
                    {
                        Riot1IrqMode = ' ';
                    }
                }
                else if (line.ToUpper().StartsWith("RIOT2IRQ="))
                {
                    line = line.Substring(9).Trim();
                    try
                    {
                        Riot2IrqMode = line[0];
                    }
                    catch
                    {
                        Riot2IrqMode = ' ';
                    }
                }
            }
            file.Close();
        }

        public void SaveConfiguration()
        {
            StreamWriter file;
            file = new StreamWriter("KimI.cfg");
            foreach (var line in LoadFiles) file.WriteLine("Load=" + line);
            for (var i = 1; i < 5; i++) file.WriteLine("BaseMemory" + i.ToString() + "=" + BaseMemoryInstalled[i].ToString());
            for (var i = 1; i < 5; i++) file.WriteLine("BaseROM" + i.ToString() + "=" + BaseRom[i].ToString());
            for (var i = 1; i < 8; i++) file.WriteLine("ExtMemory" + i.ToString() + "=" + ExtendedMemoryInstalled[i].ToString());
            for (var i = 1; i < 8; i++) file.WriteLine("ExtROM" + i.ToString() + "=" + ExtendedRom[i].ToString());
            file.WriteLine("Mapper=" + MemoryMapper.ToString());
            file.WriteLine("Riot1Irq=" + Riot1IrqMode.ToString());
            file.WriteLine("Riot2Irq=" + Riot2IrqMode.ToString());
            file.WriteLine("UseKeypad=" + UseKeypad.ToString());
            file.Close();
        }

        public void SetRiot1Mode(char mode)
        {
            Riot1IrqMode = mode;
            Io2.IrqMode = mode;
            SaveConfiguration();
        }

        public void SetRiot2Mode(char mode)
        {
            Riot2IrqMode = mode;
            Io3.IrqMode = mode;
            SaveConfiguration();
        }

        public void Write(UInt16 address, byte value)
        {
            int k;
            int sk;
            if (address < 0 || address > 65535) return;
            if (!MemoryMapper) address &= 0x1fff;
            else if ((address & 0xfc00) == 0xfc00) address &= 0x1fff;
            k = (address >> 13) & 0x7;
            switch (k)
            {
                case 0:
                    sk = (address >> 10);
                    if (sk == 5)
                    {
                        if (address >= 0x1700 && address <= 0x173f)
                        {
                            Io3.Write(address - 0x1700, value);
                            return;
                        }
                        if (address >= 0x1740 && address <= 0x177f)
                        {
                            Io2.Write(address - 0x1740, value);
                            return;
                        }
                        if (address >= 0x1780 && address <= 0x17ff) memory[address] = value;
                        return;
                    }
                    if (BaseMemoryInstalled[sk] && !BaseRom[sk]) memory[address] = value;
                    break;
                default:
                    if (ExtendedMemoryInstalled[k] && !ExtendedRom[k]) memory[address] = value;
                    break;
            }
        }

        public byte Read(UInt16 address)
        {
            int k;
            int sk;
            if (address < 0 || address > 65535) return 255;
            if (!MemoryMapper) address &= 0x1fff;
            else if ((address & 0xfc00) == 0xfc00) address &= 0x1fff;
            k = (address >> 13) & 0x7;
            switch (k)
            {
                case 0:
                    sk = (address >> 10);
                    if (sk == 5)
                    {
                        if (address >= 0x1700 && address <= 0x173f)
                        {
                            return Io3.Read(address - 0x1700);
                        }
                        if (address >= 0x1740 && address <= 0x177f)
                        {
                            return Io2.Read(address - 0x1740);
                        }
                        if (address >= 0x1780 && address <= 0x17ff) return memory[address];
                        return 0xff;
                    }
                    if (BaseMemoryInstalled[sk]) return memory[address];
                    return 0xff;
                default:
                    if (ExtendedMemoryInstalled[k]) return memory[address];
                    return 0xff;
            }
        }

        public void LoadHex(String filename)
        {
            Boolean flag;
            List<int> bytes;
            UInt16 address;
            IntelHex hex = new IntelHex();
            if (!hex.Load(filename)) return;
            flag = true;
            bytes = hex.First();
            if (bytes.Count < 1) flag = false;
            while (flag)
            {
                address = (UInt16)bytes[0];
                for (var i = 1; i < bytes.Count; i++) memory[address++] = (byte)bytes[i];
                bytes = hex.Next();
                if (bytes.Count < 1) flag = false;
            }
        }

        public void ReloadFiles()
        {
            foreach (var file in LoadFiles) LoadHex(file);
        }

        public void InstComplete()
        {
            if ((cpu.lastPc & 0x1c00) != 0x1c00 && SingleStep) cpu.nmi();
        }

        public void Reset()
        {
            if (Io2 != null) Io2.Reset();
            if (Io3 != null) Io3.Reset();
        }

        public void Cycle()
        {
            int i;
            byte p;
            byte rxBit1;
            byte rxBit2;
            i = (Io2.OutB >> 1) & 0xf;
            switch (i)
            {
                case 0: Io2.SetPinsA((byte)~keypadRows[0]); break;
                case 1: Io2.SetPinsA((byte)~keypadRows[1]); break;
                case 2: Io2.SetPinsA((byte)~keypadRows[2]); break;
                case 3: Io2.SetPinsA((byte)((UseKeypad) ? 0xff : 0xfe)); break;
                case 4: AddressTime[3] = DIGIT_DELAY; Address[3] = Io2.OutA; break;
                case 5: AddressTime[2] = DIGIT_DELAY; Address[2] = Io2.OutA; break;
                case 6: AddressTime[1] = DIGIT_DELAY; Address[1] = Io2.OutA; break;
                case 7: AddressTime[0] = DIGIT_DELAY; Address[0] = Io2.OutA; break;
                case 8: DataTime[1] = DIGIT_DELAY; Data[1] = Io2.OutA; break;
                case 9: DataTime[0] = DIGIT_DELAY; Data[0] = Io2.OutA; break;
            }
            for (i=0; i<4; i++)
                if (AddressTime[i] > 0)
                {
                    if (--AddressTime[i] <= 0) Address[i] = 0;
                }
            for (i = 0; i < 2; i++)
                if (DataTime[i] > 0)
                {
                    if (--DataTime[i] <= 0) Data[i] = 0;
                }
            if ((Io2.PortBDir & 0x80) == 0x80)
            {
                Boolean phase;
                phaseCount++;
                phase = (Io2.OutB & 0x80) == 0x80;
                if (phase != lastPhase)
                {
                    lastPhase = phase;
                    TapeDeck.CurrentBit = (phaseCount < 0xa0);
                    phaseCount = 0;
                }
            }
            else
            {
                p = (byte)(Io2.OutB & 0x7f);
                if (!TapeDeck.CurrentBit) p |= 0x80;
                Io2.SetPinsB(p);
                phaseCount = 0;
            }
            rxBit1 = 1;
            rxBit2 = 1;
            if ((Io2.PortADir & 0x80) == 0x00)
            {
                p = (byte)(Io2.InA & 0x7f);
                if (TeleType.TxBit == 1) p |= 0x80;
                Io2.SetPinsA(p);
                rxBit1 = TeleType.TxBit;
            }
            if ((Io2.PortBDir & 0x01) == 0x01)
            {
                rxBit2 = (byte)((Io2.OutB) & 0x01);
            }
            TeleType.RxBit = (byte)(rxBit1 & rxBit2);
            Io2.Cycle();
            Io3.Cycle();
            TapeDeck.Cycle();
            TeleType.Cycle();
        }
    }
}
