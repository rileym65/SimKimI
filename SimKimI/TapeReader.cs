using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimKimI
{
    public class TapeReader
    {
        protected String filename;
        public Boolean Mounted { get; private set; }
        public Boolean Running { get; private set; }
        protected List<byte> tape;
        protected int position;

        public TapeReader()
        {
            Mounted = false;
            Running = false;
            tape = new List<byte>();
            position = 0;
        }

        public void Mount(String f)
        {
            StreamReader file;
            String line;
            filename = f;
            if (filename.Length > 0) Mounted = true;
            else return;
            tape = new List<byte>();
            try
            {
                file = new StreamReader(filename);
            }
            catch
            {
                Mounted = false;
                return;
            }
            while (!file.EndOfStream)
            {
                line = file.ReadLine();
                foreach (var chr in line) tape.Add((byte)chr);
                tape.Add(13); tape.Add(10);
            }
            file.Close();
            position = 0;
        }

        public void Start()
        {
            Running = true;
        }

        public void Stop()
        {
            Running = false;
        }

        public void Unmount()
        {
            if (Running || !Mounted) return;
            Mounted = false;
        }

        public int Next()
        {
            if (position >= tape.Count) return -1;
            return tape[position++];
        }
    }
}
