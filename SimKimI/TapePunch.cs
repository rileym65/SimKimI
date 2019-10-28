using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimKimI
{
    public class TapePunch
    {
        protected String filename;
        public Boolean Mounted { get; private set; }
        public Boolean Running { get; private set; }
        protected List<byte> tape;

        public TapePunch()
        {
            Mounted = false;
            Running = false;
            tape = new List<byte>();
        }

        public void Punch(byte value)
        {
            if (Mounted && Running) tape.Add(value);
        }

        public void Mount(String f)
        {
            filename = f;
            if (filename.Length > 0) Mounted = true;
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
            StreamWriter file;
            if (Running || !Mounted) return;
            Mounted = false;
            file = new StreamWriter(filename);
            foreach (var value in tape) file.Write(((char)value).ToString());
            file.Close();
        }
    }
}
