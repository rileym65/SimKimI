using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimKimI
{
    public class TapeDeck
    {
        protected const int LENGTH = 100000;
        protected const int BITLENGTH = 500;
        protected const int FASTSPEED = 300;
        public Boolean Moving { get; protected set; }
        public Boolean FastForwarding { get; protected set; }
        public Boolean Rewinding { get; protected set; }
        public Boolean Recording { get; protected set; }
        public Boolean CurrentBit { get; set; }
        public Boolean Mounted { get; protected set; }
        protected int BitPeriod;
        protected int position;
        protected byte currentByte;
        protected byte mask;
        protected int next;
        protected byte[] tape;
        protected String tapeFilename;

        public TapeDeck(int cyclesPerSecond)
        {
            BitPeriod = cyclesPerSecond / BITLENGTH;
            position = 0;
            Moving = false;
            FastForwarding = false;
            Rewinding = false;
            Recording = false;
            tape = new byte[LENGTH];
            tapeFilename = "";
        }

        public int TapeCounter()
        {
            return position / 50;
        }

        public void Play()
        {
            Moving = true;
            FastForwarding = false;
            Rewinding = false;
            Recording = false;
            CurrentBit = false;
            mask = 0;
            next = BitPeriod;
        }

        public void Record()
        {
            Moving = true;
            FastForwarding = false;
            Rewinding = false;
            Recording = true;
            next = BitPeriod;
        }

        public void FastForward()
        {
            Moving = false;
            FastForwarding = true;
            Rewinding = false;
            Recording = false;
            next = BitPeriod;
        }

        public void Rewind()
        {
            Moving = false;
            FastForwarding = false;
            Rewinding = true;
            Recording = false;
            next = BitPeriod;
        }

        public void Stop()
        {
            Moving = false;
            FastForwarding = false;
            Rewinding = false;
            Recording = false;
        }

        public void Cycle()
        {
            if (Recording | Moving)
            {
                if (--next > 0) return;
                next = BitPeriod;
                mask >>= 1;
                if (mask == 0)
                {
                    mask = 0x80;
                    position++;
                    if (position > LENGTH)
                    {
                        position = LENGTH;
                        Stop();
                    }
                    if (Recording)
                    {
                        tape[position] = currentByte;
                        currentByte = 0;
                    }
                    else currentByte = tape[position];
                }
            }
            if (Recording)
            {
                if (CurrentBit) currentByte |= mask;
                return;
            }
            if (Moving)
            {
                CurrentBit = (currentByte & mask) == mask;
                return;
            }
            if (FastForwarding)
            {
                if (--next > 0) return;
                next = FASTSPEED;
                position++;
                if (position > LENGTH)
                {
                    position = LENGTH;
                    Stop();
                }
                return;
            }
            if (Rewinding)
            {
                if (--next > 0) return;
                position--;
                next = FASTSPEED;
                if (position < 0)
                {
                    position = 0;
                    Stop();
                }
                return;
            }
        }

        protected void decodeTape(String line)
        {
            Boolean complete;
            byte value;
            complete = false;
            value = 0;
            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] >= '0' && line[i] <= '9') value = (byte)((value << 4) + (line[i] - '0'));
                if (line[i] >= 'A' && line[i] <= 'F') value = (byte)((value << 4) + (line[i] - 'A' + 10));
                if (line[i] >= 'a' && line[i] <= 'f') value = (byte)((value << 4) + (line[i] - 'a' + 10));
                complete = !complete;
                if (!complete)
                {
                    if (position < tape.Length) tape[position++] = value;
                }
            }
        }

        public void Mount(String filename)
        {
            StreamReader file;
            String line;
            tapeFilename = filename;
            Mounted = true;
            try
            {
                file = new StreamReader(tapeFilename);
            }
            catch
            {
                for (var i = 0; i < tape.Length; i++) tape[i] = 0;
                return;
            }
            position = 0;
            while (!file.EndOfStream)
            {
                line = file.ReadLine();
                decodeTape(line);
            }
            position = 0;
            file.Close();
        }

        public void Eject()
        {
            StreamWriter file;
            String line;
            if (tapeFilename.Length < 1) return;
            file = new StreamWriter(tapeFilename);
            line = "";
            for (var i = 0; i < tape.Length; i++)
            {
                line += tape[i].ToString("X2");
                if (line.Length > 64)
                {
                    file.WriteLine(line);
                    line = "";
                }
            }
            if (line.Length > 0) file.WriteLine(line);
            tapeFilename = "";
            Mounted = false;
        }
    }
}
