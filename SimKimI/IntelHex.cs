using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimKimI
{
    public class IntelHex
    {
        protected int lineAddress;
        protected int currentAddress;
        protected List<byte> bytes;
        protected List<String> output;
        protected int position;

        public IntelHex()
        {
            Clear();
        }

        public void Clear()
        {
            lineAddress = 0;
            currentAddress = 0x7fffffff;
            bytes = new List<byte>();
            output = new List<String>();
        }

        public List<String> GetOutput()
        {
            return output;
        }

        protected void writeCurrentRecord()
        {
            String line;
            int checksum;
            if (bytes.Count == 0) return;
            checksum = bytes.Count;
            checksum += lineAddress & 0xff;
            checksum += (lineAddress >> 8) & 0xff;
            line = ":";
            line += bytes.Count.ToString("X2");
            line += lineAddress.ToString("X4");
            line += "00";
            foreach (var value in bytes)
            {
                checksum += value;
                line += value.ToString("X2");
            }
            checksum = (0 - checksum) & 0xff;
            line += checksum.ToString("X2");
            output.Add(line);
            bytes.Clear();
        }

        public void Add(int address, byte value)
        {
            if (address != currentAddress)
            {
                writeCurrentRecord();
                lineAddress = address;
            }
            bytes.Add(value);
            currentAddress = address + 1;
            if (bytes.Count >= 16)
            {
                writeCurrentRecord();
                lineAddress = address;
                currentAddress = address;
            }
        }

        public void StartAddress(int startAddr)
        {
            String line;
            int checksum;
            if (bytes.Count > 0)
            {
                writeCurrentRecord();
                lineAddress = currentAddress;
            }
            checksum = 9;
            line = ":040000050000" + startAddr.ToString("X4");
            checksum += startAddr & 0xff;
            checksum += (startAddr >> 8) & 0xff;
            checksum = (0 - checksum) & 0xff;
            line += checksum.ToString("X2");
            output.Add(line);
        }

        public void End()
        {
            if (bytes.Count > 0) writeCurrentRecord();
            output.Add(":00000001FF");
        }

        protected int fromHex(String value, int count)
        {
            int ret;
            ret = 0;
            for (var i = 0; i < count; i++)
            {
                ret <<= 4;
                if (value.Length > 0)
                {
                    if (value[0] >= '0' && value[0] <= '9') ret |= (value[0] - '0');
                    if (value[0] >= 'A' && value[0] <= 'F') ret |= (10 + value[0] - 'A');
                    if (value[0] >= 'a' && value[0] <= 'f') ret |= (10 + value[0] - 'a');
                    value = value.Substring(1);
                }
            }
            return ret;
        }

        public Boolean Save(String filename)
        {
            StreamWriter file;
            file = null;
            try
            {
                file = new StreamWriter(filename);
                foreach (var line in output) file.WriteLine(line);
                file.Close();
            }
            catch
            {
                if (file != null) file.Close();
                return false;
            }
            return true;
        }

        public Boolean Load(String filename)
        {
            StreamReader file;
            String line;
            output = new List<String>();
            try
            {
                file = new StreamReader(filename);
            }
            catch
            {
                return false;
            }
            try
            {
                while (!file.EndOfStream)
                {
                    line = file.ReadLine();
                    output.Add(line);
                }
            }
            catch
            {
                file.Close();
                return false;
            }
            file.Close();
            position = 0;
            return true;
        }

        protected List<int> buildLine(int pos)
        {
            List<int> ret;
            int recordType;
            int address;
            int count;
            ret = new List<int>();
            if (pos >= output.Count) return ret;
            if (output[pos][0] != ':') return ret;
            if (output[pos].Length < 9) return ret;
            recordType = fromHex(output[pos].Substring(7), 2);
            if (recordType != 0) return ret;
            count = fromHex(output[pos].Substring(1),2);
            address = fromHex(output[pos].Substring(3), 4);
            ret.Add(address);
            for (var i = 0; i < count; i++) ret.Add(fromHex(output[pos].Substring(9 + i * 2), 2));
            return ret;
        }

        public List<int> First()
        {
            position = 0;
            return buildLine(position);
        }

        public List<int> Next()
        {
            position++;
            return buildLine(position);
        }
    }
}
