using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimKimI
{
    public interface Memory
    {
        void Write(UInt16 address, byte value);
        byte Read(UInt16 address);
        void Cycle();
        void InstComplete();
        void Reset();
    }
}
