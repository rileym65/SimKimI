using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimKimI
{
    public class Teletype
    {
        public Boolean TypeLock;
        public Boolean PrintLock;
        public byte RxBit { get; set; }
        public byte TxBit { get; set; }
        protected int stopBits;
        protected int bitTime;
        protected int cyclesPerSecond;
        protected char parity;
        protected int dataBits;
        protected int baud;
        protected List<byte> printQueue;
        protected List<byte> sendQueue;
        protected Boolean sending;
        protected Boolean receiving;
        protected int txBuffer;
        protected int txMask;
        protected byte sendParity;
        protected int txTimer;
        protected int rxTimer;
        protected int rxMask;
        protected int rxBuffer;
        
        public Teletype(int cycles)
        {
            cyclesPerSecond = cycles;
            DataBits = 8;
            Parity = 'N';
            StopBits = 1;
            Baud = 300;
            TxBit = 1;
            RxBit = 1;
            TypeLock = false;
            PrintLock = false;
            printQueue = new List<byte>();
            sendQueue = new List<byte>();
            sending = false;
            receiving = false;
        }

        public int Baud
        {
            get { return baud; }
            set
            {
                if (value >= 1 && value <= 19200) baud = value;
                bitTime = cyclesPerSecond / baud;
            }
        }

        public int DataBits
        {
            get { return dataBits; }
            set
            {
                if (value >= 7 && value <= 9) dataBits = value;
            }
        }

        public char Parity
        {
            get { return parity; }
            set
            {
                if (value == 'o' || value == 'O') parity = 'O';
                if (value == 'e' || value == 'E') parity = 'E';
                if (value == 'n' || value == 'N') parity = 'N';
                if (value == 's' || value == 'S') parity = 'S';
                if (value == 'm' || value == 'M') parity = 'M';
            }
        }

        public int StopBits
        {
            get { return stopBits; }
            set { if (value >= 1 && value <= 2) stopBits = value; }
        }

        public void Print(byte value)
        {
            while (PrintLock) ;
            PrintLock = true;
            printQueue.Add(value);
            PrintLock = false;
        }

        public void Send(byte value)
        {
            while (TypeLock) ;
            TypeLock = true;
            sendQueue.Add(value);
            TypeLock = false;
        }

        public byte NextPrintByte()
        {
            byte ret;
            while (PrintLock) ;
            if (printQueue.Count < 1) return 0;
            PrintLock = true;
            ret = printQueue[0];
            printQueue.RemoveAt(0);
            PrintLock = false;
            return ret;
        }

        public byte NextSendByte()
        {
            byte ret;
            while (TypeLock) ;
            if (sendQueue.Count < 1) return 0;
            TypeLock = true;
            ret = sendQueue[0];
            sendQueue.RemoveAt(0);
            TypeLock = false;
            return ret;
        }

        protected void pushSendBit(byte value)
        {
            value = (byte)(value & 1);
            txMask = (txMask == 0) ? 1 : txMask << 1;
            txBuffer <<= 1;
            txBuffer |= value;
            sendParity ^= value;
        }

        protected void buildSendBuffer(byte value)
        {
            txMask = 0;
            txBuffer = 0;
            pushSendBit(0);
            sendParity = 0;
            for (var i = 0; i < dataBits; i++)
            {
                pushSendBit((byte)(value & 1));
                value >>= 1;
            }
            switch (parity)
            {
                case 'O':
                    pushSendBit((byte)((sendParity == 0) ? 1 : 0));
                    break;
                case 'E':
                    pushSendBit((byte)((sendParity == 1) ? 1 : 0));
                    break;
                case 'S':
                    pushSendBit(1);
                    break;
                case 'M':
                    pushSendBit(0);
                    break;
            }
            for (var i = 0; i < stopBits; i++) pushSendBit(1);
        }

        protected void parseByte()
        {
            byte p;
            byte value;
            rxBuffer >>= 1;
            if (stopBits == 2) rxBuffer >>= 1;
            if (parity != 'N')
            {
                p = (byte)(rxBuffer & 1);
                rxBuffer >>= 1;
            }
            value = 0;
            for (var i = 0; i < dataBits; i++)
            {
                value = (byte)((value << 1) | (rxBuffer & 0x01));
                rxBuffer >>= 1;
            }
            Print(value);
        }

        public void Cycle()
        {
            byte value;
            if (sending)
            {
                if (--txTimer == 0)
                {
                    TxBit = (byte)(((txBuffer & txMask) != 0) ? 1 : 0);
                    txMask >>= 1;
                    txTimer = bitTime;
                    if (txMask == 0)
                    {
                        TxBit = 1;
                        sending = false;
                    }
                }
            }
            if (receiving)
            {
                if (--rxTimer == 0)
                {
                    if (RxBit == 1)
                    {
                        rxBuffer |= rxMask;
                    }
                    rxMask >>= 1;
                    if (rxMask == 0)
                    {
                        parseByte();
                        receiving = false;
                    }
                    rxTimer = bitTime;
                }
            }
            if (!sending)
            {
                value = NextSendByte();
                if (value != 0)
                {
                    buildSendBuffer(value);
                    txTimer = bitTime;
                    sending = true;
                    TxBit = 1;
                }
            }
            if (!receiving)
            {
                if (RxBit == 0)
                {
                    rxTimer = bitTime + (bitTime / 2);
                    receiving = true;
                    rxBuffer = 0;
                    rxMask = 1 << (dataBits + stopBits);
                    if (parity == 'N') rxMask >>= 1;
                }
            }
        }
    }
}
