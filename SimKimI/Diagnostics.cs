using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimKimI
{
    public class Diagnostics : Memory
    {
        protected MainForm mainForm;
        protected byte[] memory;
        protected Cpu65c02 cpu;
        protected int good;
        protected int bad;

        public Diagnostics(MainForm mf)
        {
            mainForm = mf;
            memory = new byte[65536];
            cpu = new Cpu65c02(this);
        }

        public void InstComplete()
        {
        }

        public void Write(UInt16 address, byte value)
        {
            if (address < 0 || address > 65535) return;
            memory[address] = value;
        }

        public byte Read(UInt16 address)
        {
            if (address < 0 || address > 65535) return 255;
            return memory[address];
        }

        protected void Good(String msg)
        {
            good++;
            mainForm.Debug(msg);
        }

        protected void Bad(String msg)
        {
            bad++;
            mainForm.Debug(msg);
        }

        protected void ResetTests()
        {
            mainForm.Debug("");
            mainForm.Debug("Reset Tests");
            mainForm.Debug("-----------");
            cpu.halted = false;
            memory[0xfffc] = 0x34;
            memory[0xfffd] = 0x12;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.cycles = 0;
            cpu.flags = 0xff;
            cpu.reset();
            if (cpu.pc == 0x1234) Good("Good: PC had correct value following reset");
            else Bad("Bad : PC did not have correct value following reset");
            if ((cpu.flags & Cpu65c02.FLAG_I) != 0x00) Good("Good: I flag set following reset");
            else Bad("Bad : I flag not set following reset");
            if ((cpu.flags & Cpu65c02.FLAG_D) == 0x00) Good("Good: D flag cleared following reset");
            else Bad("Bad : D flag not cleared following reset");
        }

        protected void LogicalFlagTests()
        {
            mainForm.Debug("");
            mainForm.Debug("Logical Flag Tests");
            mainForm.Debug("------------------");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xa9;       // LDA #$0
            memory[0x1001] = 0x00;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.cycles = 0;
            cpu.flags = 0xfd;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag reset when loading 0");
            else Bad("Bad : N flag was not reset when loading 0");
            if ((cpu.flags & 0x02) == 0x02) Good("Good: Z flag set when loading 0");
            else Bad("Bad : Z flag was not set when loading 0");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag unaffected when loading 0");
            else Bad("Bad : V flag was affected when loading 0");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag unaffected when loading 0");
            else Bad("Bad : C flag was affected when loading 0");
            cpu.reset();
            memory[0x1000] = 0xa9;       // LDA #1
            memory[0x1001] = 0x01;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.cycles = 0;
            cpu.flags = 0xff;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag reset when loading +1");
            else Bad("Bad : N flag was not reset when loading +1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag reset when loading +1");
            else Bad("Bad : Z flag was not reset when loading +1");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag unaffected when loading +1");
            else Bad("Bad : V flag was affected when loading +1");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag unaffected when loading +1");
            else Bad("Bad : C flag was affected when loading +1");
            cpu.reset();
            memory[0x1000] = 0xa9;       // LDA #-1
            memory[0x1001] = 0xff;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.cycles = 0;
            cpu.flags = 0x02;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set when loading -1");
            else Bad("Bad : N flag was not set when loading -1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag reset when loading -1");
            else Bad("Bad : Z flag was not reset when loading -1");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag unaffected when loading -1");
            else Bad("Bad : V flag was affected when loading -1");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag unaffected when loading -1");
            else Bad("Bad : C flag was affected when loading -1");
        }

        protected void ADCtests()
        {
            mainForm.Debug("");
            mainForm.Debug("ADC - Binary");
            mainForm.Debug("------------");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x69;       // ADC #$05
            memory[0x1001] = 0x05;
            cpu.pc = 0x1000;
            cpu.ac = 0x06;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x0b) Good("Good: AC was $0b after ADC #$05");
            else Bad("Bad : AC was not $0b after ADC #$05");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ADC #$05");
            else Bad("Bad : PC was not incremented by 2 after ADC #$05");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after ADC #$05");
            else Bad("Bad : Cycles not incremented by 2 after ADC #$05");
            cpu.reset();
            memory[0x1000] = 0x65;       // ADC $05
            memory[0x1001] = 0x05;
            memory[0x0005] = 0x04;
            cpu.pc = 0x1000;
            cpu.ac = 0x03;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x07) Good("Good: AC was $07 after ADC $05");
            else Bad("Bad : AC was not $07 after ADC $05");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ADC $05");
            else Bad("Bad : PC was not incremented by 2 after ADC $05");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after ADC $05");
            else Bad("Bad : Cycles not incremented by 3 after ADC $05");
            cpu.reset();
            memory[0x1000] = 0x75;       // ADC $05,X
            memory[0x1001] = 0x05;
            memory[0x0007] = 0x12;
            cpu.pc = 0x1000;
            cpu.ac = 0x23;
            cpu.x = 2;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x35) Good("Good: AC was $35 after ADC $05,X");
            else Bad("Bad : AC was not $35 after ADC $05,X");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ADC $05,X");
            else Bad("Bad : PC was not incremented by 2 after ADC $05,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after ADC $05,X");
            else Bad("Bad : Cycles not incremented by 4 after ADC $05,X");
            cpu.reset();
            memory[0x1000] = 0x6d;       // ADC $4567
            memory[0x1001] = 0x67;
            memory[0x1002] = 0x45;
            memory[0x4567] = 0x1a;
            cpu.pc = 0x1000;
            cpu.ac = 0x07;
            cpu.x = 2;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x21) Good("Good: AC was $21 after ADC $4567");
            else Bad("Bad : AC was not $21 after ADC $4567");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ADC $4567");
            else Bad("Bad : PC was not incremented by 3 after ADC $4567");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after ADC $4567");
            else Bad("Bad : Cycles not incremented by 4 after ADC $4567");
            cpu.reset();
            memory[0x1000] = 0x7d;       // ADC $4567,X
            memory[0x1001] = 0x67;
            memory[0x1002] = 0x45;
            memory[0x4569] = 0x34;
            cpu.pc = 0x1000;
            cpu.ac = 0x45;
            cpu.x = 2;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x79) Good("Good: AC was $79 after ADC $4567,X");
            else Bad("Bad : AC was not $79 after ADC $4567,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ADC $4567,X");
            else Bad("Bad : PC was not incremented by 3 after ADC $4567,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after ADC $4567,X");
            else Bad("Bad : Cycles not incremented by 4 after ADC $4567,X");
            cpu.reset();
            memory[0x1000] = 0x79;       // ADC $4567,Y
            memory[0x1001] = 0x67;
            memory[0x1002] = 0x45;
            memory[0x456a] = 0x45;
            cpu.pc = 0x1000;
            cpu.ac = 0x67;
            cpu.y = 3;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xac) Good("Good: AC was $ac after ADC $4567,Y");
            else Bad("Bad : AC was not $ac after ADC $4567,Y");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ADC $4567,Y");
            else Bad("Bad : PC was not incremented by 3 after ADC $4567,Y");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after ADC $4567,Y");
            else Bad("Bad : Cycles not incremented by 4 after ADC $4567,Y");
            cpu.reset();
            memory[0x1000] = 0x61;       // ADC ($fa,X)
            memory[0x1001] = 0xfa;
            memory[0x00ff] = 0x40;
            memory[0x0000] = 0x50;
            memory[0x5040] = 0xaa;
            cpu.pc = 0x1000;
            cpu.ac = 0x23;
            cpu.x = 5;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xcd) Good("Good: AC was $cd after ADC ($fa,X)");
            else Bad("Bad : AC was not $cd after ADC ($fa,X)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ADC ($fa,X)");
            else Bad("Bad : PC was not incremented by 2 after ADC ($fa,X)");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after ADC ($fa,X)");
            else Bad("Bad : Cycles not incremented by 6 after ADC ($fa,X)");
            cpu.reset();
            memory[0x1000] = 0x71;       // ADC ($ff),Y
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x40;
            memory[0x0000] = 0x50;
            memory[0x5043] = 0x52;
            cpu.pc = 0x1000;
            cpu.ac = 0x27;
            cpu.y = 3;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x79) Good("Good: AC was $79 after ADC ($ff),Y");
            else Bad("Bad : AC was not $79 after ADC ($ff),Y");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ADC ($ff),Y");
            else Bad("Bad : PC was not incremented by 2 after ADC ($ff),Y");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after ADC ($ff),Y");
            else Bad("Bad : Cycles not incremented by 5 after ADC ($ff),Y");
            cpu.reset();
            memory[0x1000] = 0x72;       // ADC ($ff)
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x40;
            memory[0x0000] = 0x51;
            memory[0x5140] = 0x6a;
            cpu.pc = 0x1000;
            cpu.ac = 0x13;
            cpu.y = 3;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x7d) Good("Good: AC was $7d after ADC ($ff)");
            else Bad("Bad : AC was not $7d after ADC ($ff)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ADC ($ff)");
            else Bad("Bad : PC was not incremented by 2 after ADC ($ff)");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after ADC ($ff)");
            else Bad("Bad : Cycles not incremented by 5 after ADC ($ff)");
            cpu.reset();
            memory[0x1000] = 0x69;       // ADC #$01
            memory[0x1001] = 0x01;
            cpu.pc = 0x1000;
            cpu.ac = 0x01;
            cpu.flags = 1;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x03) Good("Good: 1 + 1 + cf produced 3");
            else Bad("Bad : 1 + 1 + cf did not produce 3");
            cpu.reset();
            memory[0x1000] = 0x69;       // ADC #$01
            memory[0x1001] = 0x01;
            cpu.pc = 0x1000;
            cpu.ac = 0xff;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x00) Good("Good: +1 + -1 produced 0");
            else Bad("Bad : +1 + -1 did not produce 0");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared doing +1 + -1");
            else Bad("Bad : N flag was not cleared when doing +1 + -1");
            if ((cpu.flags & 0x02) == 0x02) Good("Good: Z flag set when doing +1 + -1");
            else Bad("Bad : Z flag was not set when doing +1 + -1");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag cleared doing +1 + -1");
            else Bad("Bad : V flag was not cleared doing +1 + -1");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set doing +1 + -1");
            else Bad("Bad : C flag was not set doing +1 + -1");
            cpu.reset();
            memory[0x1000] = 0x69;       // ADC #$7f
            memory[0x1001] = 0x7f;
            cpu.pc = 0x1000;
            cpu.ac = 0x01;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x80) Good("Good: 127+1 produced 128");
            else Bad("Bad : 127+1 did not produce 128");
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set doing 127+1");
            else Bad("Bad : N flag was not set when doing 127+1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when doing 127+1");
            else Bad("Bad : Z flag was not cleared when doing 127+1");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag set doing 127+1");
            else Bad("Bad : V flag was not set doing 127+1");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared doing 127+1");
            else Bad("Bad : C flag was not cleared doing 127+1");
        }

        protected void ANDtests()
        {
            mainForm.Debug("");
            mainForm.Debug("AND");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x29;       // AND #$23
            memory[0x1001] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0x6e;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x22) Good("Good: AC was $22 after AND #$23");
            else Bad("Bad : AC was not $22 after AND #$23");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after AND #$23");
            else Bad("Bad : PC was not incremented by two after AND #$23");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by two after AND #$23");
            else Bad("Bad : Cycles not incremented by two after AND #$23");
            cpu.reset();
            memory[0x1000] = 0x25;       // AND $23
            memory[0x1001] = 0x23;
            memory[0x0023] = 0x3c;
            cpu.pc = 0x1000;
            cpu.ac = 0x99;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x18) Good("Good: AC was $18 after AND $23");
            else Bad("Bad : AC was not $18 after AND $23");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after AND $23");
            else Bad("Bad : PC was not incremented by two after AND $23");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after AND $23");
            else Bad("Bad : Cycles not incremented by 3 after AND $23");
            cpu.reset();
            memory[0x1000] = 0x35;       // AND $23,X
            memory[0x1001] = 0x23;
            memory[0x0029] = 0x66;
            cpu.pc = 0x1000;
            cpu.ac = 0x55;
            cpu.x = 6;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x44) Good("Good: AC was $44 after AND $23,X");
            else Bad("Bad : AC was not $4 after AND $23,X");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after AND $23,X");
            else Bad("Bad : PC was not incremented by two after AND $23,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after AND $23,X");
            else Bad("Bad : Cycles not incremented by 4 after AND $23,X");
            cpu.reset();
            memory[0x1000] = 0x2d;       // AND $2345
            memory[0x1001] = 0x45;
            memory[0x1002] = 0x23;
            memory[0x2345] = 0xc3;
            cpu.pc = 0x1000;
            cpu.ac = 0x66;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x42) Good("Good: AC was $42 after AND $2345");
            else Bad("Bad : AC was not $42 after AND $2345");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after AND $2345");
            else Bad("Bad : PC was not incremented by 3 after AND $2345");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after AND $2345");
            else Bad("Bad : Cycles not incremented by 4 after AND $2345");
            cpu.reset();
            memory[0x1000] = 0x3d;       // AND $4523,X
            memory[0x1001] = 0x23;
            memory[0x1002] = 0x45;
            memory[0x4528] = 0x7e;
            cpu.pc = 0x1000;
            cpu.ac = 0x9a;
            cpu.x = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x1a) Good("Good: AC was $1a after AND $4523,X");
            else Bad("Bad : AC was not $1a after AND $4523,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after AND $4523,X");
            else Bad("Bad : PC was not incremented by 3 after AND $4523,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after AND $4523,X");
            else Bad("Bad : Cycles not incremented by 4 after AND $4523,X");
            cpu.reset();
            memory[0x1000] = 0x39;       // AND $4523,Y
            memory[0x1001] = 0x23;
            memory[0x1002] = 0x45;
            memory[0x4525] = 0xaa;
            cpu.pc = 0x1000;
            cpu.ac = 0x81;
            cpu.y = 2;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x80) Good("Good: AC was $80 after AND $4523,Y");
            else Bad("Bad : AC was not $80 after AND $4523,Y");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after AND $4523,Y");
            else Bad("Bad : PC was not incremented by 3 after AND $4523,Y");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after AND $4523,Y");
            else Bad("Bad : Cycles not incremented by 4 after AND $4523,Y");
            cpu.reset();
            memory[0x1000] = 0x21;       // AND ($fa,X)
            memory[0x1001] = 0xfa;
            memory[0x00ff] = 0x87;
            memory[0x0000] = 0x47;
            memory[0x4787] = 0x55;
            cpu.pc = 0x1000;
            cpu.ac = 0xbc;
            cpu.x = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x14) Good("Good: AC was $14 after AND ($fa,X)");
            else Bad("Bad : AC was not $14 after AND ($fa,X)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after AND ($fa,X)");
            else Bad("Bad : PC was not incremented by 2 after AND ($fa,X)");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after AND ($fa,X)");
            else Bad("Bad : Cycles not incremented by 6 after AND ($fa,X)");
            cpu.reset();
            memory[0x1000] = 0x31;       // AND ($ff),Y
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x71;
            memory[0x0000] = 0x92;
            memory[0x9275] = 0xe7;
            cpu.pc = 0x1000;
            cpu.ac = 0x7e;
            cpu.y = 4;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x66) Good("Good: AC was $66 after AND ($ff),Y");
            else Bad("Bad : AC was not $6 after AND ($ff),Y");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after AND ($ff),Y");
            else Bad("Bad : PC was not incremented by 2 after AND ($ff),Y");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after AND ($ff),Y");
            else Bad("Bad : Cycles not incremented by 5 after AND ($ff),Y");
            cpu.reset();
            memory[0x1000] = 0x32;       // AND ($ff)
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x71;
            memory[0x0000] = 0x92;
            memory[0x9271] = 0x3f;
            cpu.pc = 0x1000;
            cpu.ac = 0xf3;
            cpu.y = 4;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x33) Good("Good: AC was $33 after AND ($ff)");
            else Bad("Bad : AC was not $33 after AND ($ff)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after AND ($ff)");
            else Bad("Bad : PC was not incremented by 2 after AND ($ff)");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after AND ($ff)");
            else Bad("Bad : Cycles not incremented by 5 after AND ($ff)");
            cpu.reset();
            memory[0x1000] = 0x29;       // AND #$0
            memory[0x1001] = 0xf0;
            cpu.pc = 0x1000;
            cpu.ac = 0x0f;
            cpu.cycles = 0;
            cpu.flags = 0xfd;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag reset when result 0");
            else Bad("Bad : N flag was not reset when result 0");
            if ((cpu.flags & 0x02) == 0x02) Good("Good: Z flag set when result 0");
            else Bad("Bad : Z flag was not set when result 0");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag unaffected when result 0");
            else Bad("Bad : V flag was affected when result 0");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag unaffected when result 0");
            else Bad("Bad : C flag was affected when result 0");
            cpu.reset();
            memory[0x1000] = 0x29;       // AND #1
            memory[0x1001] = 0x01;
            cpu.pc = 0x1000;
            cpu.ac = 0xff;
            cpu.cycles = 0;
            cpu.flags = 0xff;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag reset when result +1");
            else Bad("Bad : N flag was not reset when result +1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag reset when result +1");
            else Bad("Bad : Z flag was not reset when result +1");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag unaffected when result +1");
            else Bad("Bad : V flag was affected when result +1");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag unaffected when result +1");
            else Bad("Bad : C flag was affected when result +1");
            cpu.reset();
            memory[0x1000] = 0x29;       // AND #-1
            memory[0x1001] = 0xff;
            cpu.pc = 0x1000;
            cpu.ac = 0xff;
            cpu.cycles = 0;
            cpu.flags = 0x02;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set when result -1");
            else Bad("Bad : N flag was not set when result -1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag reset when result -1");
            else Bad("Bad : Z flag was not reset when result -1");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag unaffected when result -1");
            else Bad("Bad : V flag was affected when result -1");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag unaffected when result -1");
            else Bad("Bad : C flag was affected when result -1");
        }

        protected void ASLtests()
        {
            mainForm.Debug("");
            mainForm.Debug("ASL");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x0a;       // ASL A
            cpu.pc = 0x1000;
            cpu.ac = 0x21;
            cpu.flags = 1;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x42) Good("Good: AC was $42 after ASL A");
            else Bad("Bad : AC was not $42 after ASL A");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after ASL A");
            else Bad("Bad : PC was not incremented by 1 after ASL A");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after ASL A");
            else Bad("Bad : Cycles not incremented by 2 after ASL A");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared when shifting $21");
            else Bad("Bad : C flag was not cleared when shifting $21");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when shifting $21");
            else Bad("Bad : N flag was not cleared when shifting $21");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when shifting $21");
            else Bad("Bad : Z flag was not cleared when shifting $21");
            cpu.reset();
            memory[0x1000] = 0x06;       // ASL $23
            memory[0x1001] = 0x23;
            memory[0x0023] = 0x61;
            cpu.pc = 0x1000;
            cpu.ac = 0x21;
            cpu.flags = 1;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x21) Good("Good: AC was unaffected after ASL $23 with $61");
            else Bad("Bad : AC was affected after ASL $23 with $61");
            if (memory[0x0023] == 0xc2) Good("Good: memory was $c2 after ASL $23 with $61");
            else Bad("Bad : memory was not $c2 after ASL $23 with $61");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ASL $23 with $61");
            else Bad("Bad : PC was not incremented by 2 after ASL $23 with $61");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after ASL $23 with $61");
            else Bad("Bad : Cycles not incremented by 5 after ASL $23 with $61");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared when shifting $61");
            else Bad("Bad : C flag was not cleared when shifting $61");
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set when shifting $61");
            else Bad("Bad : N flag was not set when shifting $61");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when shifting $61");
            else Bad("Bad : Z flag was not cleared when shifting $61");
            cpu.reset();
            memory[0x1000] = 0x16;       // ASL $fa,X
            memory[0x1001] = 0xfa;
            memory[0x0005] = 0x80;
            cpu.pc = 0x1000;
            cpu.ac = 0x21;
            cpu.x = 11;
            cpu.flags = 1;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x21) Good("Good: AC was unaffected after ASL $fa,X with $80");
            else Bad("Bad : AC was affected after ASL $fa,X with $80");
            if (memory[0x0005] == 0x00) Good("Good: memory was $00 after ASL $fa,X with $80");
            else Bad("Bad : memory was not $00 after ASL $fa,X with $80");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ASL $fa,X with $80");
            else Bad("Bad : PC was not incremented by 2 after ASL $fa,X with $80");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after ASL $fa,X with $80");
            else Bad("Bad : Cycles not incremented by 6 after ASL $fa,X with $80");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set when shifting $80");
            else Bad("Bad : C flag was not set when shifting $80");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when shifting $80");
            else Bad("Bad : N flag was not cleared when shifting $80");
            if ((cpu.flags & 0x02) == 0x02) Good("Good: Z flag set when shifting $80");
            else Bad("Bad : Z flag was not set when shifting $80");
            cpu.reset();
            memory[0x1000] = 0x0e;       // ASL $a123
            memory[0x1001] = 0x23;
            memory[0x1002] = 0xa1;
            memory[0xa123] = 0x35;
            cpu.pc = 0x1000;
            cpu.ac = 0x21;
            cpu.x = 11;
            cpu.flags = 1;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x21) Good("Good: AC was unaffected after ASL $a123 with $35");
            else Bad("Bad : AC was affected after ASL $a123 with $35");
            if (memory[0xa123] == 0x6a) Good("Good: memory was $6a after ASL $a123 with $35");
            else Bad("Bad : memory was not $6a after ASL $a123 with $35");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ASL $a123 with $35");
            else Bad("Bad : PC was not incremented by 3 after ASL $a123 with $35");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after ASL $a123 with $35");
            else Bad("Bad : Cycles not incremented by 6 after ASL $a123 with $35");
            cpu.reset();
            memory[0x1000] = 0x1e;       // ASL $a123,X
            memory[0x1001] = 0x23;
            memory[0x1002] = 0xa1;
            memory[0xa134] = 0x68;
            cpu.pc = 0x1000;
            cpu.ac = 0x21;
            cpu.x = 0x11;
            cpu.flags = 1;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x21) Good("Good: AC was unaffected after ASL $a123,X with $68");
            else Bad("Bad : AC was affected after ASL $a123,X with $68");
            if (memory[0xa134] == 0xd0) Good("Good: memory was $d0 after ASL $a123,X with $68");
            else Bad("Bad : memory was not $d0 after ASL $a123,X with $68");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ASL $a123,X with $68");
            else Bad("Bad : PC was not incremented by 3 after ASL $a123,X with $68");
            if (cpu.cycles == 7) Good("Good: Cycles incremented by 7 after ASL $a123,X with $68");
            else Bad("Bad : Cycles not incremented by 7 after ASL $a123,X with $68");
        }

        protected void BCCtests()
        {
            mainForm.Debug("");
            mainForm.Debug("BCC");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x90;       // BCC #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0xfe;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1008) Good("Good: PC is correct after BCC #$6 on not set");
            else Bad("Bad : PC is not correct after BCC #$6 on not set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BCC #$6");
            else Bad("Bad : Cycles not incremented by 2 after BCC #$6");
            cpu.reset();
            memory[0x1000] = 0x90;       // BCC #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x01;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1002) Good("Good: PC is correct after BCC #$6 on set");
            else Bad("Bad : PC is not correct after BCC #$6 on set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BCC #$6");
            else Bad("Bad : Cycles not incremented by 2 after BCC #$6");
            cpu.reset();
            memory[0x1000] = 0x90;       // BCC #-$6
            memory[0x1001] = 0xfd;
            cpu.pc = 0x1000;
            cpu.flags = 0xfe;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x0fff) Good("Good: PC is correct after BCC #$6 on not set and negative displacement");
            else Bad("Bad : PC is not correct after BCC #$6 on not set and negative displacement");
        }

        protected void BCStests()
        {
            mainForm.Debug("");
            mainForm.Debug("BCS");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xb0;       // BCS #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x01;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1008) Good("Good: PC is correct after BCS #$6 on set");
            else Bad("Bad : PC is not correct after BCS #$6 on set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BCS #$6");
            else Bad("Bad : Cycles not incremented by 2 after BCS #$6");
            cpu.reset();
            memory[0x1000] = 0xb0;       // BCS #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0xfe;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1002) Good("Good: PC is correct after BCS #$6 on not set");
            else Bad("Bad : PC is not correct after BCS #$6 on not set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BCS #$6");
            else Bad("Bad : Cycles not incremented by 2 after BCS #$6");
            cpu.reset();
            memory[0x1000] = 0xb0;       // BCS #-$6
            memory[0x1001] = 0xfd;
            cpu.pc = 0x1000;
            cpu.flags = 0x01;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x0fff) Good("Good: PC is correct after BCS #$6 on set and negative displacement");
            else Bad("Bad : PC is not correct after BCS #$6 on set and negative displacement");
        }

        protected void BEQtests()
        {
            mainForm.Debug("");
            mainForm.Debug("BEQ");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xf0;       // BEQ #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x02;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1008) Good("Good: PC is correct after BEQ #$6 on set");
            else Bad("Bad : PC is not correct after BEQ #$6 on set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BEQ #$6");
            else Bad("Bad : Cycles not incremented by 2 after BEQ #$6");
            cpu.reset();
            memory[0x1000] = 0xf0;       // BEQ #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0xfd;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1002) Good("Good: PC is correct after BEQ #$6 on not set");
            else Bad("Bad : PC is not correct after BEQ #$6 on not set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BEQ #$6");
            else Bad("Bad : Cycles not incremented by 2 after BEQ #$6");
            cpu.reset();
            memory[0x1000] = 0xf0;       // BEQ #-$6
            memory[0x1001] = 0xfd;
            cpu.pc = 0x1000;
            cpu.flags = 0x02;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x0fff) Good("Good: PC is correct after BEQ #$6 on set and negative displacement");
            else Bad("Bad : PC is not correct after BEQ #$6 on set and negative displacement");
        }

        protected void BITtests()
        {
            mainForm.Debug("");
            mainForm.Debug("BIT");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x89;       // BIT #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x05) Good("Good: AC was unaltered by BIT #$6");
            else Bad("Bad : AC was altered by BIT #$6");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after BIT #$6");
            else Bad("Bad : PC not incremented by 2 after BIT #$6");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BIT #$6");
            else Bad("Bad : Cycles not incremented by 2 after BIT #$6");
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0) Good("Good: N was not affected by BIT #$6");
            else Bad("Bad : N was affected by BIT #$6");
            if ((cpu.flags & Cpu65c02.FLAG_V) != 0) Good("Good: V was not affected by BIT #$6");
            else Bad("Bad : V was affected by BIT #$6");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0) Good("Good: Z was cleared by BIT #$6 with AC=$5");
            else Bad("Bad : Z was not cleared by BIT #$6 with AC=$5");
            cpu.reset();
            memory[0x1000] = 0x24;       // BIT $6
            memory[0x1001] = 0x06;
            memory[0x0006] = 0x83;
            cpu.pc = 0x1000;
            cpu.ac = 0x09;
            cpu.flags = 0x7f;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x09) Good("Good: AC was unaltered by BIT $6");
            else Bad("Bad : AC was altered by BIT $6");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after BIT $6");
            else Bad("Bad : PC not incremented by 2 after BIT $6");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after BIT $6");
            else Bad("Bad : Cycles not incremented by 3 after BIT $6");
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0) Good("Good: N was set by BIT $6 with $83");
            else Bad("Bad : N was not set by BIT $6 with $83");
            if ((cpu.flags & Cpu65c02.FLAG_V) == 0) Good("Good: V was cleared by BIT $6 with $83");
            else Bad("Bad : V was not cleared by BIT $6 with $83");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0) Good("Good: Z was cleared by BIT $6 with AC=$9 and $83");
            else Bad("Bad : Z was not cleared by BIT $6 with AC=$9 and $83");
            cpu.reset();
            memory[0x1000] = 0x34;       // BIT $fa,X
            memory[0x1001] = 0xfa;
            memory[0x0004] = 0x77;
            cpu.pc = 0x1000;
            cpu.x = 10;
            cpu.ac = 0x06;
            cpu.flags = 0xbf;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x06) Good("Good: AC was unaltered by BIT $fa,X");
            else Bad("Bad : AC was altered by BIT $fa,X");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after BIT $fa,X");
            else Bad("Bad : PC not incremented by 2 after BIT $fa,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after BIT $fa,X");
            else Bad("Bad : Cycles not incremented by 4 after BIT $fa,X");
            if ((cpu.flags & Cpu65c02.FLAG_N) == 0) Good("Good: N was cleared by BIT $fa,X with $77");
            else Bad("Bad : N was not cleared by BIT $fa,X with $77");
            if ((cpu.flags & Cpu65c02.FLAG_V) != 0) Good("Good: V was set by BIT $fa,X with $77");
            else Bad("Bad : V was not set by BIT $fa,X with $77");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0) Good("Good: Z was cleared by BIT $fa,X with AC=$6 and $77");
            else Bad("Bad : Z was not cleared by BIT $fa,X with AC=$6 and $77");
            cpu.reset();
            memory[0x1000] = 0x2c;       // BIT $1290
            memory[0x1001] = 0x90;
            memory[0x1002] = 0x12;
            memory[0x1290] = 0x3c;
            cpu.pc = 0x1000;
            cpu.x = 10;
            cpu.ac = 0x04;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x04) Good("Good: AC was unaltered by BIT $1290");
            else Bad("Bad : AC was altered by BIT $1290");
            if (cpu.pc == 0x1003) Good("Good: PC incremented by 3 after BIT $1290");
            else Bad("Bad : PC not incremented by 3 after BIT $1290");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after BIT $1290");
            else Bad("Bad : Cycles not incremented by 4 after BIT $1290");
            if ((cpu.flags & Cpu65c02.FLAG_N) == 0) Good("Good: N was cleared by BIT $1290 with $3c");
            else Bad("Bad : N was not cleared by BIT $1290 with $3c");
            if ((cpu.flags & Cpu65c02.FLAG_V) == 0) Good("Good: V was cleared by BIT $1290 with $3c");
            else Bad("Bad : V was not cleared by BIT $1290 with $3c");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0) Good("Good: Z was cleared by BIT $1290 with AC=$4 and $3c");
            else Bad("Bad : Z was not cleared by BIT $1290 with AC=$4 and $3c");
            cpu.reset();
            memory[0x1000] = 0x3c;       // BIT $1290,X
            memory[0x1001] = 0x90;
            memory[0x1002] = 0x12;
            memory[0x12a1] = 0xf0;
            cpu.pc = 0x1000;
            cpu.x = 0x11;
            cpu.ac = 0x0f;
            cpu.flags = 0x30;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x0f) Good("Good: AC was unaltered by BIT $1290,X");
            else Bad("Bad : AC was altered by BIT $1290,X");
            if (cpu.pc == 0x1003) Good("Good: PC incremented by 3 after BIT $1290,X");
            else Bad("Bad : PC not incremented by 3 after BIT $1290,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after BIT $1290,X");
            else Bad("Bad : Cycles not incremented by 4 after BIT $1290,X");
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0) Good("Good: N was set by BIT $1290,X with $f0");
            else Bad("Bad : N was not set by BIT $1290,X with $f0");
            if ((cpu.flags & Cpu65c02.FLAG_V) != 0) Good("Good: V was set by BIT $1290,X with $f0");
            else Bad("Bad : V was not set by BIT $1290,X with $f0");
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0) Good("Good: Z was set by BIT $1290,X with AC=$0f and $f0");
            else Bad("Bad : Z was not set by BIT $1290,X with AC=$0f and $f0");
        }

        protected void BMItests()
        {
            mainForm.Debug("");
            mainForm.Debug("BMI");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x30;       // BMI #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x80;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1008) Good("Good: PC is correct after BMI #$6 on negative");
            else Bad("Bad : PC is not correct after BMI #$6 on negative");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BMI #$6");
            else Bad("Bad : Cycles not incremented by 2 after BMI #$6");
            cpu.reset();
            memory[0x1000] = 0x30;       // BMI #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1002) Good("Good: PC is correct after BMI #$6 on positive");
            else Bad("Bad : PC is not correct after BMI #$6 on positive");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BMI #$6");
            else Bad("Bad : Cycles not incremented by 2 after BMI #$6");
            cpu.reset();
            memory[0x1000] = 0x30;       // BMI #-$6
            memory[0x1001] = 0xfd;
            cpu.pc = 0x1000;
            cpu.flags = 0x80;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x0fff) Good("Good: PC is correct after BMI #$6 on negative and negative displacement");
            else Bad("Bad : PC is not correct after BMI #$6 on negative and negative displacement");
        }

        protected void BNEtests()
        {
            mainForm.Debug("");
            mainForm.Debug("BNE");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xd0;       // BNE #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0xfd;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1008) Good("Good: PC is correct after BNE #$6 on not set");
            else Bad("Bad : PC is not correct after BNE #$6 on not set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BNE #$6");
            else Bad("Bad : Cycles not incremented by 2 after BNE #$6");
            cpu.reset();
            memory[0x1000] = 0xd0;       // BNE #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x02;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1002) Good("Good: PC is correct after BNE #$6 on set");
            else Bad("Bad : PC is not correct after BNE #$6 on set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BNE #$6");
            else Bad("Bad : Cycles not incremented by 2 after BNE #$6");
            cpu.reset();
            memory[0x1000] = 0xd0;       // BNE #-$6
            memory[0x1001] = 0xfd;
            cpu.pc = 0x1000;
            cpu.flags = 0xfd;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x0fff) Good("Good: PC is correct after BNE #$6 on not set and negative displacement");
            else Bad("Bad : PC is not correct after BNE #$6 on not set and negative displacement");
        }

        protected void BPLtests()
        {
            mainForm.Debug("");
            mainForm.Debug("BPL");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x10;       // BPL #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1008) Good("Good: PC is correct after BPL #$6 on positive");
            else Bad("Bad : PC is not correct after BPL #$6 on positive");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BPL #$6");
            else Bad("Bad : Cycles not incremented by 2 after BPL #$6");
            cpu.reset();
            memory[0x1000] = 0x10;       // BPL #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x80;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1002) Good("Good: PC is correct after BPL #$6 on negative");
            else Bad("Bad : PC is not correct after BPL #$6 on negative");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BPL #$6");
            else Bad("Bad : Cycles not incremented by 2 after BPL #$6");
            cpu.reset();
            memory[0x1000] = 0x10;       // BPL #-$6
            memory[0x1001] = 0xfd;
            cpu.pc = 0x1000;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x0fff) Good("Good: PC is correct after BPL #$6 on positive and negative displacement");
            else Bad("Bad : PC is not correct after BPL #$6 on positive and negative displacement");
        }

        protected void BRAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("BRA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x80;       // BRA $5
            memory[0x1001] = 0x05;
            cpu.pc = 0x1000;
            cpu.x = 0x31;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1007) Good("Good: PC was correct after BRA $5");
            else Bad("Bad : PC was not correct after BRA $5");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after BRA");
            else Bad("Bad : Cycles not incremented by 3 after BRA");
            cpu.reset();
            memory[0x1000] = 0x80;       // BRA $-5
            memory[0x1001] = 0xfb;
            cpu.pc = 0x1000;
            cpu.x = 0x31;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x0ffd) Good("Good: PC was correct after BRA $-5");
            else Bad("Bad : PC was not correct after BRA $-5");
        }

        protected void BRKtests()
        {
            mainForm.Debug("");
            mainForm.Debug("BRK");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x00;       // BRK
            memory[0xfffe] = 0x20;
            memory[0xffff] = 0x34;
            cpu.pc = 0x1000;
            cpu.ac = 0x06;
            cpu.flags = 0;
            cpu.sp = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.sp == 0xfc) Good("Good: S was correct after BRK");
            else Bad("Bad : S was not correct after BRK");
            if (cpu.pc == 0x3420) Good("Good: PC was correct after BRK");
            else Bad("Bad : PC was not correct after BRK");
            if (memory[0x1fd] == 0x10) Good("Good: Correct flags with B set on stack after BRK");
            else Bad("Bad : Incorrect flags wer on stack after BRK");
            if (memory[0x1fe] == 0x02) Good("Good: Correct low P on stack after BRK");
            else Bad("Bad : Incorrect low P on stack after BRK");
            if (memory[0x1ff] == 0x010) Good("Good: Correct high P on stack after BRK");
            else Bad("Bad : Incorrect high P on stack after BRK");
            if ((cpu.flags & Cpu65c02.FLAG_I) != 0x00) Good("Good: I flag was set after BRK");
            else Bad("Bad : I flag was not set after BRK");
            if (cpu.cycles == 7) Good("Good: Cycles incremented by 7 after BRK");
            else Bad("Bad : Cycles not incremented by 7 after BRK");
        }

        protected void BVCtests()
        {
            mainForm.Debug("");
            mainForm.Debug("BVC");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x50;       // BVC #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0xbf;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1008) Good("Good: PC is correct after BVC #$6 on not set");
            else Bad("Bad : PC is not correct after BVC #$6 on not set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BVC #$6");
            else Bad("Bad : Cycles not incremented by 2 after BVC #$6");
            cpu.reset();
            memory[0x1000] = 0x50;       // BVC #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x40;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1002) Good("Good: PC is correct after BVC #$6 on set");
            else Bad("Bad : PC is not correct after BVC #$6 on set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BVC #$6");
            else Bad("Bad : Cycles not incremented by 2 after BVC #$6");
            cpu.reset();
            memory[0x1000] = 0x50;       // BVC #-$6
            memory[0x1001] = 0xfd;
            cpu.pc = 0x1000;
            cpu.flags = 0xbf;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x0fff) Good("Good: PC is correct after BVC #$6 on not set and negative displacement");
            else Bad("Bad : PC is not correct after BVC #$6 on not set and negative displacement");
        }

        protected void BVStests()
        {
            mainForm.Debug("");
            mainForm.Debug("BVS");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x70;       // BVS #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0x40;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1008) Good("Good: PC is correct after BVS #$6 on set");
            else Bad("Bad : PC is not correct after BVS #$6 on positive");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BVS #$6");
            else Bad("Bad : Cycles not incremented by 2 after BVS #$6");
            cpu.reset();
            memory[0x1000] = 0x70;       // BVS #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.flags = 0xbf;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1002) Good("Good: PC is correct after BVS #$6 on not set");
            else Bad("Bad : PC is not correct after BVS #$6 on not set");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after BVS #$6");
            else Bad("Bad : Cycles not incremented by 2 after BVS #$6");
            cpu.reset();
            memory[0x1000] = 0x70;       // BVS #-$6
            memory[0x1001] = 0xfd;
            cpu.pc = 0x1000;
            cpu.flags = 0x40;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x0fff) Good("Good: PC is correct after BVS #$6 on set and negative displacement");
            else Bad("Bad : PC is not correct after BVS #$6 on set and negative displacement");
        }

        protected void CLCtests()
        {
            mainForm.Debug("");
            mainForm.Debug("CLC");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x18;       // CLC
            cpu.pc = 0x1000;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_C) == 0x00) Good("Good: C cleared after CLC");
            else Bad("Bad : C not cleared after CLC");
            if (cpu.flags == 0xfe) Good("Good: No other flags cleared after CLC");
            else Bad("Bad : Other flags were cleared after CLC");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after CLC");
            else Bad("Bad : PC was not incremented by 1 after CLC");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after CLC");
            else Bad("Bad : Cycles not incremented by 2 after CLC");
        }

        protected void CLDtests()
        {
            mainForm.Debug("");
            mainForm.Debug("CLD");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xd8;       // CLD
            cpu.pc = 0x1000;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_D) == 0x00) Good("Good: D cleared after CLD");
            else Bad("Bad : D not cleared after CLD");
            if (cpu.flags == 0xf7) Good("Good: No other flags cleared after CLD");
            else Bad("Bad : Other flags were cleared after CLD");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after CLD");
            else Bad("Bad : PC was not incremented by 1 after CLD");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after CLD");
            else Bad("Bad : Cycles not incremented by 2 after CLD");
        }

        protected void CLItests()
        {
            mainForm.Debug("");
            mainForm.Debug("CLI");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x58;       // CLI
            cpu.pc = 0x1000;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_I) == 0x00) Good("Good: I cleared after CLI");
            else Bad("Bad : I not cleared after CLI");
            if (cpu.flags == 0xfb) Good("Good: No other flags cleared after CLI");
            else Bad("Bad : Other flags were cleared after CLI");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after CLI");
            else Bad("Bad : PC was not incremented by 1 after CLI");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after CLI");
            else Bad("Bad : Cycles not incremented by 2 after CLI");
        }

        protected void CLVtests()
        {
            mainForm.Debug("");
            mainForm.Debug("CLV");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xb8;       // CLV
            cpu.pc = 0x1000;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_V) == 0x00) Good("Good: V cleared after CLV");
            else Bad("Bad : V not cleared after CLV");
            if (cpu.flags == 0xbf) Good("Good: No other flags cleared after CLV");
            else Bad("Bad : Other flags were cleared after CLV");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after CLV");
            else Bad("Bad : PC was not incremented by 1 after CLV");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after CLV");
            else Bad("Bad : Cycles not incremented by 2 after CLV");
        }

        protected void CPXtests()
        {
            mainForm.Debug("");
            mainForm.Debug("CPX");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xe0;       // CPX #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.x = 0x06;
            cpu.y = 0;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.x == 0x06) Good("Good: X was unaltered by CPX #$6");
            else Bad("Bad : X was altered by CPX #$6");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after CPX #$6");
            else Bad("Bad : PC not incremented by 2 after CPX #$6");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after CPX #$6");
            else Bad("Bad : Cycles not incremented by 2 after CPX #$6");
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set by CPX #$6 when Y=$6");
            else Bad("Bad : Z Flag not set by CPX #$6 when Y=$6");
            cpu.reset();
            memory[0x1000] = 0xe4;       // CPX $9
            memory[0x1001] = 0x09;
            memory[0x0009] = 0x1c;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.x = 0x1c;
            cpu.y = 0;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.x == 0x1c) Good("Good: X was unaltered by CPX $9");
            else Bad("Bad : X was altered by CPX $9");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after CPX $9");
            else Bad("Bad : PC not incremented by 2 after CPX $9");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after CPX $9");
            else Bad("Bad : Cycles not incremented by 3 after CPX $9");
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set by CPX $9 when Y=$1c");
            else Bad("Bad : Z Flag not set by CPX $9 when Y=$1c");
            cpu.reset();
            memory[0x1000] = 0xec;       // CPX $9876
            memory[0x1001] = 0x76;
            memory[0x1002] = 0x98;
            memory[0x9876] = 0xbe;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.x = 0xbe;
            cpu.y = 0;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.x == 0xbe) Good("Good: X was unaltered by CPX $9876");
            else Bad("Bad : X was altered by CPX $9876");
            if (cpu.pc == 0x1003) Good("Good: PC incremented by 3 after CPX $9876");
            else Bad("Bad : PC not incremented by 3 after CPX $9876");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after CPX $9876");
            else Bad("Bad : Cycles not incremented by 4 after CPX $9876");
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set by CPX $9876 when Y=$be");
            else Bad("Bad : Z Flag not set by CPX $9876 when Y=$be");
            cpu.reset();
            memory[0x1000] = 0xe0;       // cpy #5
            memory[0x1001] = 0x05;
            cpu.pc = 0x1000;
            cpu.x = 0x07;
            cpu.y = 0;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared with 7-5");
            else Bad("Bad : N flag was not cleared with 7-5");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared with 7-5");
            else Bad("Bad : Z flag was not cleared with 7-5");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag cleared with 7-5");
            else Bad("Bad : V flag was not cleared with 7-5");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set with 7-5");
            else Bad("Bad : C flag was not set with 7-5");
            cpu.reset();
            memory[0x1000] = 0xe0;       // CPX #7
            memory[0x1001] = 0x07;
            cpu.pc = 0x1000;
            cpu.x = 0x05;
            cpu.y = 0;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set with 5-7");
            else Bad("Bad : N flag was not set with 5-7");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared with 5-7");
            else Bad("Bad : Z flag was not cleared with 5-7");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag cleared with 5-7");
            else Bad("Bad : V flag was not cleared with 5-7");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared with 5-7");
            else Bad("Bad : C flag was not cleared with 5-7");
            cpu.reset();
            memory[0x1000] = 0xe0;       // CPX #1
            memory[0x1001] = 0x01;
            cpu.pc = 0x1000;
            cpu.x = 0x80;
            cpu.y = 0;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared with -128-1");
            else Bad("Bad : N flag was not cleared with -128-1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared with -128-1");
            else Bad("Bad : Z flag was not cleared with -128-1");
            if ((cpu.flags & 0x40) != 0x40) Good("Good: V flag unchanged with -128-1");
            else Bad("Bad : V flag was changed with -128-1");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set with -128-1");
            else Bad("Bad : C flag was not set with -128-1");
            cpu.reset();
            memory[0x1000] = 0xe0;       // CPX #4
            memory[0x1001] = 0x04;
            cpu.pc = 0x1000;
            cpu.x = 0x04;
            cpu.y = 0;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared with 4-4");
            else Bad("Bad : N flag was not cleared with 4-4");
            if ((cpu.flags & 0x02) != 0x00) Good("Good: Z flag set with 4-4");
            else Bad("Bad : Z flag was not cleared with 4-4");
            if ((cpu.flags & 0x40) != 0x40) Good("Good: V flag unchanged with 4-4");
            else Bad("Bad : V flag was changed with 4-4");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set with 4-4");
            else Bad("Bad : C flag was not set with 4-4");
        }

        protected void CPYtests()
        {
            mainForm.Debug("");
            mainForm.Debug("CPY");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xc0;       // CPY #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0x06;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.y == 0x06) Good("Good: Y was unaltered by CPY #$6");
            else Bad("Bad : Y was altered by CPY #$6");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after CPY #$6");
            else Bad("Bad : PC not incremented by 2 after CPY #$6");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after CPY #$6");
            else Bad("Bad : Cycles not incremented by 2 after CPY #$6");
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set by CPY #$6 when Y=$6");
            else Bad("Bad : Z Flag not set by CPY #$6 when Y=$6");
            cpu.reset();
            memory[0x1000] = 0xc4;       // CPY $9
            memory[0x1001] = 0x09;
            memory[0x0009] = 0x1c;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0x1c;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.y == 0x1c) Good("Good: Y was unaltered by CPY $9");
            else Bad("Bad : Y was altered by CPY $9");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after CPY $9");
            else Bad("Bad : PC not incremented by 2 after CPY $9");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after CPY $9");
            else Bad("Bad : Cycles not incremented by 3 after CPY $9");
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set by CPY $9 when Y=$1c");
            else Bad("Bad : Z Flag not set by CPY $9 when Y=$1c");
            cpu.reset();
            memory[0x1000] = 0xcc;       // CPY $9876
            memory[0x1001] = 0x76;
            memory[0x1002] = 0x98;
            memory[0x9876] = 0xbe;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0xbe;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.y == 0xbe) Good("Good: Y was unaltered by CPY $9876");
            else Bad("Bad : Y was altered by CPY $9876");
            if (cpu.pc == 0x1003) Good("Good: PC incremented by 3 after CPY $9876");
            else Bad("Bad : PC not incremented by 3 after CPY $9876");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after CPY $9876");
            else Bad("Bad : Cycles not incremented by 4 after CPY $9876");
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set by CPY $9876 when Y=$be");
            else Bad("Bad : Z Flag not set by CPY $9876 when Y=$be");
            cpu.reset();
            memory[0x1000] = 0xc0;       // cpy #5
            memory[0x1001] = 0x05;
            cpu.pc = 0x1000;
            cpu.y = 0x07;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared with 7-5");
            else Bad("Bad : N flag was not cleared with 7-5");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared with 7-5");
            else Bad("Bad : Z flag was not cleared with 7-5");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag cleared with 7-5");
            else Bad("Bad : V flag was not cleared with 7-5");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set with 7-5");
            else Bad("Bad : C flag was not set with 7-5");
            cpu.reset();
            memory[0x1000] = 0xc0;       // CPY #7
            memory[0x1001] = 0x07;
            cpu.pc = 0x1000;
            cpu.y = 0x05;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set with 5-7");
            else Bad("Bad : N flag was not set with 5-7");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared with 5-7");
            else Bad("Bad : Z flag was not cleared with 5-7");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag cleared with 5-7");
            else Bad("Bad : V flag was not cleared with 5-7");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared with 5-7");
            else Bad("Bad : C flag was not cleared with 5-7");
            cpu.reset();
            memory[0x1000] = 0xc0;       // CPY #1
            memory[0x1001] = 0x01;
            cpu.pc = 0x1000;
            cpu.y = 0x80;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared with -128-1");
            else Bad("Bad : N flag was not cleared with -128-1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared with -128-1");
            else Bad("Bad : Z flag was not cleared with -128-1");
            if ((cpu.flags & 0x40) != 0x40) Good("Good: V flag unchanged with -128-1");
            else Bad("Bad : V flag was changed with -128-1");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set with -128-1");
            else Bad("Bad : C flag was not set with -128-1");
            cpu.reset();
            memory[0x1000] = 0xc0;       // CPY #4
            memory[0x1001] = 0x04;
            cpu.pc = 0x1000;
            cpu.y = 0x04;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared with 4-4");
            else Bad("Bad : N flag was not cleared with 4-4");
            if ((cpu.flags & 0x02) != 0x00) Good("Good: Z flag set with 4-4");
            else Bad("Bad : Z flag was not cleared with 4-4");
            if ((cpu.flags & 0x40) != 0x40) Good("Good: V flag unchanged with 4-4");
            else Bad("Bad : V flag was changed with 4-4");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set with 4-4");
            else Bad("Bad : C flag was not set with 4-4");
        }

        protected void DECtests()
        {
            mainForm.Debug("");
            mainForm.Debug("DEC");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xc6;       // DEC $21
            memory[0x1001] = 0x21;
            memory[0x0021] = 0x80;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after DEC $21");
            else Bad("Bad : AC was affected after DEC $21");
            if (memory[0x0021] == 0x7f) Good("Good: Memory was correct after DEC $21");
            else Bad("Bad : Memory was not correct after DEC $21");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after DEC $21");
            else Bad("Bad : Incorrect flags were cleared after DEC $21");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after DEC $21");
            else Bad("Bad : PC was not incremented by 2 after DEC $21");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after DEC $21");
            else Bad("Bad : Cycles not incremented by 5 after DEC $21");
            cpu.reset();
            memory[0x1000] = 0xd6;       // DEC $21,X
            memory[0x1001] = 0x21;
            memory[0x0025] = 0x84;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 4;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after DEC $21,X");
            else Bad("Bad : AC was affected after DEC $21,X");
            if (memory[0x0025] == 0x83) Good("Good: Memory was correct after DEC $21,X");
            else Bad("Bad : Memory was not correct after DEC $21,X");
            if (cpu.flags == 0x80) Good("Good: Only flag N was set after DEC $21,X with $84");
            else Bad("Bad : Incorrect flags were cleared after DEC $21,X with $84");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after DEC $21,X");
            else Bad("Bad : PC was not incremented by 2 after DEC $21,X");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after DEC $21,X");
            else Bad("Bad : Cycles not incremented by 6 after DEC $21,X");
            cpu.reset();
            memory[0x1000] = 0xce;       // DEC $2188
            memory[0x1001] = 0x88;
            memory[0x1002] = 0x21;
            memory[0x2188] = 0x04;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 4;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after DEC $2188");
            else Bad("Bad : AC was affected after DEC $2188");
            if (memory[0x2188] == 0x03) Good("Good: Memory was correct after DEC $2188");
            else Bad("Bad : Memory was not correct after DEC $2188");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after DEC $2188");
            else Bad("Bad : PC was not incremented by 3 after DEC $2188");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after DEC $2188");
            else Bad("Bad : Cycles not incremented by 6 after DEC $2188");
            cpu.reset();
            memory[0x1000] = 0xde;       // DEC $2188,X
            memory[0x1001] = 0x88;
            memory[0x1002] = 0x21;
            memory[0x2198] = 0x73;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0x10;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after DEC $2188,X");
            else Bad("Bad : AC was affected after DEC $2188,X");
            if (memory[0x2198] == 0x72) Good("Good: Memory was correct after DEC $2188,X");
            else Bad("Bad : Memory was not correct after DEC $2188,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after DEC $2188,X");
            else Bad("Bad : PC was not incremented by 3 after DEC $2188,X");
            if (cpu.cycles == 7) Good("Good: Cycles incremented by 7 after DEC $2188,X");
            else Bad("Bad : Cycles not incremented by 7 after DEC $2188,X");
            cpu.reset();
            memory[0x1000] = 0xc6;       // DEC $21
            memory[0x1001] = 0x21;
            memory[0x0021] = 0x01;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.flags == 0x02) Good("Good: Z flag was set when decrementing $01");
            else Bad("Bad : Z flag was not set when decrementing $01");
        }

        protected void DEAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("DEA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x3a;       // DEA
            cpu.pc = 0x1000;
            cpu.ac = 0x72;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x71) Good("Good: A was correct after DEA");
            else Bad("Bad : A was not correct after DEA");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after DEA with $31");
            else Bad("Bad : Incorrect flags were cleared after DEA with $31");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after DEA");
            else Bad("Bad : PC was not incremented by 1 after DEA");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after DEA");
            else Bad("Bad : Cycles not incremented by 2 after DEA");
            cpu.reset();
            memory[0x1000] = 0x3a;       // DEA
            cpu.pc = 0x1000;
            cpu.ac = 0x00;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set decrementing $00");
            else Bad("Bad : N flag not set decrementing $7f");
            cpu.reset();
            memory[0x1000] = 0x3a;       // DEA
            cpu.pc = 0x1000;
            cpu.ac = 0x01;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set decrementing $01");
            else Bad("Bad : Z flag not set decrementing $01");
        }

        protected void DEXtests()
        {
            mainForm.Debug("");
            mainForm.Debug("DEX");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xca;       // DEX
            cpu.pc = 0x1000;
            cpu.x = 0x31;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.x == 0x30) Good("Good: X was correct after DEX");
            else Bad("Bad : X was not correct after DEX");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after DEX with $31");
            else Bad("Bad : Incorrect flags were cleared after DEX with $31");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after DEX");
            else Bad("Bad : PC was not incremented by 1 after DEX");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after DEX");
            else Bad("Bad : Cycles not incremented by 2 after DEX");
            cpu.reset();
            memory[0x1000] = 0xca;       // DEX
            cpu.pc = 0x1000;
            cpu.x = 0x00;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set decrementing $00");
            else Bad("Bad : N flag not set decrementing $7f");
            cpu.reset();
            memory[0x1000] = 0xca;       // DEX
            cpu.pc = 0x1000;
            cpu.x = 0x01;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set decrementing $01");
            else Bad("Bad : Z flag not set decrementing $01");
        }
      
        protected void DEYtests()
        {
            mainForm.Debug("");
            mainForm.Debug("DEY");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x88;       // DEY
            cpu.pc = 0x1000;
            cpu.y = 0x31;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.y == 0x30) Good("Good: Y was correct after DEY");
            else Bad("Bad : Y was not correct after DEY");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after DEY with $31");
            else Bad("Bad : Incorrect flags were cleared after DEY with $31");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after DEY");
            else Bad("Bad : PC was not incremented by 1 after DEY");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after DEY");
            else Bad("Bad : Cycles not incremented by 2 after DEY");
            cpu.reset();
            memory[0x1000] = 0x88;       // DEY
            cpu.pc = 0x1000;
            cpu.y = 0x00;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set decrementing $00");
            else Bad("Bad : N flag not set decrementing $7f");
            cpu.reset();
            memory[0x1000] = 0x88;       // DEY
            cpu.pc = 0x1000;
            cpu.y = 0x01;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set decrementing $01");
            else Bad("Bad : Z flag not set decrementing $01");
        }

        protected void EORtests()
        {
            mainForm.Debug("");
            mainForm.Debug("EOR");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x49;       // EOR #$23
            memory[0x1001] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xdc) Good("Good: AC was $dc after EOR #$23");
            else Bad("Bad : AC was not $dc after EOR #$23");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after EOR #$23");
            else Bad("Bad : PC was not incremented by two after EOR #$23");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by two after EOR #$23");
            else Bad("Bad : Cycles not incremented by two after EOR #$23");
            cpu.reset();
            memory[0x1000] = 0x45;       // EOR $23
            memory[0x1001] = 0x23;
            memory[0x0023] = 0xcc;
            cpu.pc = 0x1000;
            cpu.ac = 0xa5;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x69) Good("Good: AC was $69 after EOR $23");
            else Bad("Bad : AC was not $69 after EOR $23");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after EOR $23");
            else Bad("Bad : PC was not incremented by two after EOR $23");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after EOR $23");
            else Bad("Bad : Cycles not incremented by 3 after EOR $23");
            cpu.reset();
            memory[0x1000] = 0x55;       // EOR $23,X
            memory[0x1001] = 0x23;
            memory[0x0029] = 0x66;
            cpu.pc = 0x1000;
            cpu.ac = 0x33;
            cpu.x = 6;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x55) Good("Good: AC was $55 after EOR $23,X");
            else Bad("Bad : AC was not $55 after EOR $23,X");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after EOR $23,X");
            else Bad("Bad : PC was not incremented by two after EOR $23,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after EOR $23,X");
            else Bad("Bad : Cycles not incremented by 4 after EOR $23,X");
            cpu.reset();
            memory[0x1000] = 0x4d;       // EOR $2345
            memory[0x1001] = 0x45;
            memory[0x1002] = 0x23;
            memory[0x2345] = 0x99;
            cpu.pc = 0x1000;
            cpu.ac = 0x88;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x11) Good("Good: AC was $11 after EOR $2345");
            else Bad("Bad : AC was not $11 after EOR $2345");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after EOR $2345");
            else Bad("Bad : PC was not incremented by 3 after EOR $2345");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after EOR $2345");
            else Bad("Bad : Cycles not incremented by 4 after EOR $2345");
            cpu.reset();
            memory[0x1000] = 0x5d;       // EOR $4523,X
            memory[0x1001] = 0x23;
            memory[0x1002] = 0x45;
            memory[0x4528] = 0x7e;
            cpu.pc = 0x1000;
            cpu.ac = 0x9a;
            cpu.x = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xe4) Good("Good: AC was $e4 after EOR $4523,X");
            else Bad("Bad : AC was not $e4 after EOR $4523,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after EOR $4523,X");
            else Bad("Bad : PC was not incremented by 3 after EOR $4523,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after EOR $4523,X");
            else Bad("Bad : Cycles not incremented by 4 after EOR $4523,X");
            cpu.reset();
            memory[0x1000] = 0x59;       // EOR $4523,Y
            memory[0x1001] = 0x23;
            memory[0x1002] = 0x45;
            memory[0x4525] = 0xaa;
            cpu.pc = 0x1000;
            cpu.ac = 0x81;
            cpu.y = 2;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x2b) Good("Good: AC was $2b after EOR $4523,Y");
            else Bad("Bad : AC was not $2b after EOR $4523,Y");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after EOR $4523,Y");
            else Bad("Bad : PC was not incremented by 3 after EOR $4523,Y");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after EOR $4523,Y");
            else Bad("Bad : Cycles not incremented by 4 after EOR $4523,Y");
            cpu.reset();
            memory[0x1000] = 0x41;       // EOR ($fa,X)
            memory[0x1001] = 0xfa;
            memory[0x00ff] = 0x87;
            memory[0x0000] = 0x47;
            memory[0x4787] = 0x55;
            cpu.pc = 0x1000;
            cpu.ac = 0xbc;
            cpu.x = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xe9) Good("Good: AC was $e9 after EOR ($fa,X)");
            else Bad("Bad : AC was not $e9 after EOR ($fa,X)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after EOR ($fa,X)");
            else Bad("Bad : PC was not incremented by 2 after EOR ($fa,X)");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after EOR ($fa,X)");
            else Bad("Bad : Cycles not incremented by 6 after EOR ($fa,X)");
            cpu.reset();
            memory[0x1000] = 0x51;       // EOR ($ff),Y
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x71;
            memory[0x0000] = 0x92;
            memory[0x9275] = 0xe7;
            cpu.pc = 0x1000;
            cpu.ac = 0x7e;
            cpu.y = 4;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x99) Good("Good: AC was $99 after EOR ($ff),Y");
            else Bad("Bad : AC was not $99 after EOR ($ff),Y");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after EOR ($ff),Y");
            else Bad("Bad : PC was not incremented by 2 after EOR ($ff),Y");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after EOR ($ff),Y");
            else Bad("Bad : Cycles not incremented by 5 after EOR ($ff),Y");
            cpu.reset();
            memory[0x1000] = 0x52;       // EOR ($ff)
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x71;
            memory[0x0000] = 0x92;
            memory[0x9271] = 0x3f;
            cpu.pc = 0x1000;
            cpu.ac = 0xf3;
            cpu.y = 4;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xcc) Good("Good: AC was $cc after EOR ($ff)");
            else Bad("Bad : AC was not $cc after EOR ($ff)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after EOR ($ff)");
            else Bad("Bad : PC was not incremented by 2 after EOR ($ff)");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after EOR ($ff)");
            else Bad("Bad : Cycles not incremented by 5 after EOR ($ff)");
            cpu.reset();
            memory[0x1000] = 0x49;       // EOR #$0
            memory[0x1001] = 0xf0;
            cpu.pc = 0x1000;
            cpu.ac = 0xf0;
            cpu.cycles = 0;
            cpu.flags = 0xfd;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag reset when result 0");
            else Bad("Bad : N flag was not reset when result 0");
            if ((cpu.flags & 0x02) == 0x02) Good("Good: Z flag set when result 0");
            else Bad("Bad : Z flag was not set when result 0");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag unaffected when result 0");
            else Bad("Bad : V flag was affected when result 0");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag unaffected when result 0");
            else Bad("Bad : C flag was affected when result 0");
            cpu.reset();
            memory[0x1000] = 0x49;       // EOR #1
            memory[0x1001] = 0xfe;
            cpu.pc = 0x1000;
            cpu.ac = 0xff;
            cpu.cycles = 0;
            cpu.flags = 0xff;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag reset when result +1");
            else Bad("Bad : N flag was not reset when result +1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag reset when result +1");
            else Bad("Bad : Z flag was not reset when result +1");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag unaffected when result +1");
            else Bad("Bad : V flag was affected when result +1");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag unaffected when result +1");
            else Bad("Bad : C flag was affected when result +1");
            cpu.reset();
            memory[0x1000] = 0x49;       // EOR #-1
            memory[0x1001] = 0xaa;
            cpu.pc = 0x1000;
            cpu.ac = 0x55;
            cpu.cycles = 0;
            cpu.flags = 0x02;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set when result -1");
            else Bad("Bad : N flag was not set when result -1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag reset when result -1");
            else Bad("Bad : Z flag was not reset when result -1");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag unaffected when result -1");
            else Bad("Bad : V flag was affected when result -1");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag unaffected when result -1");
            else Bad("Bad : C flag was affected when result -1");
        }

        protected void INCtests()
        {
            mainForm.Debug("");
            mainForm.Debug("INC");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xe6;       // INC $31
            memory[0x1001] = 0x31;
            memory[0x0031] = 0x70;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after INC $31");
            else Bad("Bad : AC was affected after INC $31");
            if (memory[0x0031] == 0x71) Good("Good: Memory was correct after INC $31");
            else Bad("Bad : Memory was not correct after INC $31");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after INC $31");
            else Bad("Bad : Incorrect flags were cleared after INC $31");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after INC $31");
            else Bad("Bad : PC was not incremented by 2 after INC $31");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after INC $31");
            else Bad("Bad : Cycles not incremented by 5 after INC $31");
            cpu.reset();
            memory[0x1000] = 0xf6;       // INC $31,X
            memory[0x1001] = 0x31;
            memory[0x0033] = 0x7f;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 2;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after INC $31,X");
            else Bad("Bad : AC was affected after INC $31,X");
            if (memory[0x0033] == 0x80) Good("Good: Memory was correct after INC $31,X");
            else Bad("Bad : Memory was not correct after INC $31,X");
            if (cpu.flags == 0x80) Good("Good: Only flag N was set after INC $31,X with $7f");
            else Bad("Bad : N flag not set after INC $31,X with $7f");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after INC $31,X");
            else Bad("Bad : PC was not incremented by 2 after INC $31,X");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after INC $31,X");
            else Bad("Bad : Cycles not incremented by 6 after INC $31,X");
            cpu.reset();
            memory[0x1000] = 0xee;       // INC $3113
            memory[0x1001] = 0x13;
            memory[0x1002] = 0x31;
            memory[0x3113] = 0x27;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 2;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after INC $3113");
            else Bad("Bad : AC was affected after INC $3113");
            if (memory[0x3113] == 0x28) Good("Good: Memory was correct after INC $3113");
            else Bad("Bad : Memory was not correct after INC $3113");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after INC $3113");
            else Bad("Bad : PC was not incremented by 3 after INC $3113");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after INC $3113");
            else Bad("Bad : Cycles not incremented by 6 after INC $3113");
            cpu.reset();
            memory[0x1000] = 0xfe;       // INC $3113,X
            memory[0x1001] = 0x13;
            memory[0x1002] = 0x31;
            memory[0x3124] = 0x72;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0x11;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after INC $3113,X");
            else Bad("Bad : AC was affected after INC $3113,X");
            if (memory[0x3124] == 0x73) Good("Good: Memory was correct after INC $3113,X");
            else Bad("Bad : Memory was not correct after INC $3113,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after INC $3113,X");
            else Bad("Bad : PC was not incremented by 3 after INC $3113,X");
            if (cpu.cycles == 7) Good("Good: Cycles incremented by 7 after INC $3113,X");
            else Bad("Bad : Cycles not incremented by 7 after INC $3113,X");
            cpu.reset();
            memory[0x1000] = 0xe6;       // INC $31
            memory[0x1001] = 0x31;
            memory[0x0031] = 0xff;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.flags == 0x02) Good("Good: Z flag was set after incrementing $ff");
            else Bad("Bad : Z flag was not set after incrementing $ff");
        }

        protected void INAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("INA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x1a;       // INA
            cpu.pc = 0x1000;
            cpu.ac = 0x3c;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x3d) Good("Good: A was correct after INA");
            else Bad("Bad : A was not correct after INA");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after INA with $3c");
            else Bad("Bad : Incorrect flags were cleared after INA with $3c");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after INA");
            else Bad("Bad : PC was not incremented by 1 after INA");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after INA");
            else Bad("Bad : Cycles not incremented by 2 after INA");
            cpu.reset();
            memory[0x1000] = 0x1a;       // INA
            cpu.pc = 0x1000;
            cpu.ac = 0x7f;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set incrementing $7f");
            else Bad("Bad : N flag not set incrementing $7f");
            cpu.reset();
            memory[0x1000] = 0x1a;       // INA
            cpu.pc = 0x1000;
            cpu.ac = 0xff;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set incrementing $ff");
            else Bad("Bad : Z flag not set incrementing $ff");
        }
       
        protected void INXtests()
        {
            mainForm.Debug("");
            mainForm.Debug("INX");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xe8;       // INX
            cpu.pc = 0x1000;
            cpu.x = 0x3a;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.x == 0x3b) Good("Good: X was correct after INX");
            else Bad("Bad : X was not correct after INX");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after INX with $31");
            else Bad("Bad : Incorrect flags were cleared after INX with $31");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after INX");
            else Bad("Bad : PC was not incremented by 1 after INX");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after INX");
            else Bad("Bad : Cycles not incremented by 2 after INX");
            cpu.reset();
            memory[0x1000] = 0xe8;       // INX
            cpu.pc = 0x1000;
            cpu.x = 0x7f;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set incrementing $7f");
            else Bad("Bad : N flag not set incrementing $7f");
            cpu.reset();
            memory[0x1000] = 0xe8;       // INX
            cpu.pc = 0x1000;
            cpu.x = 0xff;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set incrementing $ff");
            else Bad("Bad : Z flag not set incrementing $ff");
        }

        protected void INYtests()
        {
            mainForm.Debug("");
            mainForm.Debug("INY");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xc8;       // INY
            cpu.pc = 0x1000;
            cpu.y = 0x31;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.y == 0x32) Good("Good: Y was correct after INY");
            else Bad("Bad : Y was not correct after INY");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after INY with $31");
            else Bad("Bad : Incorrect flags were cleared after INY with $31");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after INY");
            else Bad("Bad : PC was not incremented by 1 after INY");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after INY");
            else Bad("Bad : Cycles not incremented by 2 after INY");
            cpu.reset();
            memory[0x1000] = 0xc8;       // INY
            cpu.pc = 0x1000;
            cpu.y = 0x7f;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set incrementing $7f");
            else Bad("Bad : N flag not set incrementing $7f");
            cpu.reset();
            memory[0x1000] = 0xc8;       // INY
            cpu.pc = 0x1000;
            cpu.y = 0xff;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set incrementing $ff");
            else Bad("Bad : Z flag not set incrementing $ff");
        }

        protected void JMPtests()
        {
            mainForm.Debug("");
            mainForm.Debug("JMP");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x4c;       // JMP $1234
            memory[0x1001] = 0x34;
            memory[0x1002] = 0x12;
            cpu.pc = 0x1000;
            cpu.ac = 0x06;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1234) Good("Good: PC was correct after JMP $1234");
            else Bad("Bad : PC was not correct after JMP $1234");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after JMP $1234");
            else Bad("Bad : Cycles not incremented by 3 after JMP $1234");
            if (cpu.flags == 0) Good("Good: No flags were set by JMP $1234");
            else Bad("Bad : Flags were set by JMP $1234");
            cpu.reset();
            memory[0x1000] = 0x6c;       // JMP ($1234)
            memory[0x1001] = 0x34;
            memory[0x1002] = 0x12;
            memory[0x1234] = 0x56;
            memory[0x1235] = 0x34;
            cpu.pc = 0x1000;
            cpu.ac = 0x06;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x3456) Good("Good: PC was correct after JMP ($1234)");
            else Bad("Bad : PC was not correct after JMP ($1234)");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after JMP ($1234)");
            else Bad("Bad : Cycles not incremented by 5 after JMP ($1234)");
            if (cpu.flags == 0xff) Good("Good: No flags were cleared by JMP ($1234)");
            else Bad("Bad : Flags were cleared by JMP ($1234)");
            cpu.reset();
            memory[0x1000] = 0x7c;       // JMP ($1234,X)
            memory[0x1001] = 0x34;
            memory[0x1002] = 0x12;
            memory[0x1234] = 0x00;
            memory[0x1235] = 0x00;
            memory[0x1245] = 0x78;
            memory[0x1246] = 0x56;
            cpu.pc = 0x1000;
            cpu.x = 0x11;
            cpu.ac = 0x06;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x5678) Good("Good: PC was correct after JMP ($1234,X)");
            else Bad("Bad : PC was not correct after JMP ($1234,X)");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after JMP ($1234,X)");
            else Bad("Bad : Cycles not incremented by 6 after JMP ($1234,X)");
        }

        protected void JSRtests()
        {
            mainForm.Debug("");
            mainForm.Debug("JSR");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x20;       // JSR $1234
            memory[0x1001] = 0x34;
            memory[0x1002] = 0x12;
            cpu.pc = 0x1000;
            cpu.ac = 0x06;
            cpu.flags = 0;
            cpu.sp = 0xf0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1234) Good("Good: PC was correct after JSR $1234");
            else Bad("Bad : PC was not correct after JSR $1234");
            if (cpu.sp == 0xee) Good("Good: S was correct after JSR $1234");
            else Bad("Bad : S was not correct after JSR $1234");
            if (memory[0x01f0] == 0x10 && memory[0x1ef] == 0x02) Good("Good: Correct address was written to stack");
            else Bad("Bad : Address was not correctly written to stack");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after JSR $1234");
            else Bad("Bad : Cycles not incremented by 6 after JSR $1234");
        }

        protected void LDAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("LDA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xa9;       // LDA #$23
            memory[0x1001] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x23) Good("Good: AC was $23 after LDA #$23");
            else Bad("Bad : AC was not $23 after LDA #$23");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after LDA #$23");
            else Bad("Bad : PC was not incremented by two after LDA #$23");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by two after LDA #$23");
            else Bad("Bad : Cycles not incremented by two after LDA #$23");
            cpu.reset();
            memory[0x1000] = 0xa5;       // LDA $23
            memory[0x1001] = 0x23;
            memory[0x0023] = 0x51;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x51) Good("Good: AC was $51 after LDA $23");
            else Bad("Bad : AC was not $51 after LDA $23");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after LDA $23");
            else Bad("Bad : PC was not incremented by two after LDA $23");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after LDA $23");
            else Bad("Bad : Cycles not incremented by 3 after LDA $23");
            cpu.reset();
            memory[0x1000] = 0xb5;       // LDA $23,X
            memory[0x1001] = 0x23;
            memory[0x0029] = 0x76;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.x = 6;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x76) Good("Good: AC was $76 after LDA $23,X");
            else Bad("Bad : AC was not $76 after LDA $23,X");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after LDA $23,X");
            else Bad("Bad : PC was not incremented by two after LDA $23,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDA $23,X");
            else Bad("Bad : Cycles not incremented by 4 after LDA $23,X");
            cpu.reset();
            memory[0x1000] = 0xad;       // LDA $2345
            memory[0x1001] = 0x45;
            memory[0x1002] = 0x23;
            memory[0x2345] = 0x92;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x92) Good("Good: AC was $92 after LDA $2345");
            else Bad("Bad : AC was not $92 after LDA $2345");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after LDA $2345");
            else Bad("Bad : PC was not incremented by 3 after LDA $2345");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDA $2345");
            else Bad("Bad : Cycles not incremented by 4 after LDA $2345");
            cpu.reset();
            memory[0x1000] = 0xbd;       // LDA $4523,X
            memory[0x1001] = 0x23;
            memory[0x1002] = 0x45;
            memory[0x4528] = 0xab;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.x = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xab) Good("Good: AC was $ab after LDA $4523,X");
            else Bad("Bad : AC was not $ab after LDA $4523,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after LDA $4523,X");
            else Bad("Bad : PC was not incremented by 3 after LDA $4523,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDA $4523,X");
            else Bad("Bad : Cycles not incremented by 4 after LDA $4523,X");
            cpu.reset();
            memory[0x1000] = 0xb9;       // LDA $4523,Y
            memory[0x1001] = 0x23;
            memory[0x1002] = 0x45;
            memory[0x4525] = 0xf2;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.y = 2;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xf2) Good("Good: AC was $f2 after LDA $4523,Y");
            else Bad("Bad : AC was not $f2 after LDA $4523,Y");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after LDA $4523,Y");
            else Bad("Bad : PC was not incremented by 3 after LDA $4523,Y");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDA $4523,Y");
            else Bad("Bad : Cycles not incremented by 4 after LDA $4523,Y");
            cpu.reset();
            memory[0x1000] = 0xa1;       // LDA ($fa,X)
            memory[0x1001] = 0xfa;
            memory[0x00ff] = 0x87;
            memory[0x0000] = 0x47;
            memory[0x4787] = 0x58;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.x = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x58) Good("Good: AC was $58 after LDA ($fa,X)");
            else Bad("Bad : AC was not $58 after LDA ($fa,X)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after LDA ($fa,X)");
            else Bad("Bad : PC was not incremented by 2 after LDA ($fa,X)");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after LDA ($fa,X)");
            else Bad("Bad : Cycles not incremented by 6 after LDA ($fa,X)");
            cpu.reset();
            memory[0x1000] = 0xb1;       // LDA ($ff),Y
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x71;
            memory[0x0000] = 0x92;
            memory[0x9275] = 0xbe;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.y = 4;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xbe) Good("Good: AC was $be after LDA ($ff),Y");
            else Bad("Bad : AC was not $be after LDA ($ff),Y");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after LDA ($ff),Y");
            else Bad("Bad : PC was not incremented by 2 after LDA ($ff),Y");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after LDA ($ff),Y");
            else Bad("Bad : Cycles not incremented by 5 after LDA ($ff),Y");
            cpu.reset();
            memory[0x1000] = 0xb2;       // LDA ($ff)
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x71;
            memory[0x0000] = 0x92;
            memory[0x9271] = 0x0c;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.y = 4;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x0c) Good("Good: AC was $0c after LDA ($ff)");
            else Bad("Bad : AC was not $0c after LDA ($ff)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after LDA ($ff)");
            else Bad("Bad : PC was not incremented by 2 after LDA ($ff)");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after LDA ($ff)");
            else Bad("Bad : Cycles not incremented by 5 after LDA ($ff)");

        }

        protected void LDXtests()
        {
            mainForm.Debug("");
            mainForm.Debug("LDX");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xa2;       // LDX #$17
            memory[0x1001] = 0x17;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0xc3;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after LDX #$17");
            else Bad("Bad : AC was affected after LDX #$17");
            if (cpu.x == 0x17) Good("Good: X was correct after LDX #$17");
            else Bad("Bad : X was not correct after LDX #$17");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after LDX #$17");
            else Bad("Bad : Incorrect flags were cleared after LDX #$17");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after LDX #$17");
            else Bad("Bad : PC was not incremented by 2 after LDX #$17");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after LDX #$17");
            else Bad("Bad : Cycles not incremented by 2 after LDX #$17");
            cpu.reset();
            memory[0x1000] = 0xa6;       // LDX $69
            memory[0x1001] = 0x69;
            memory[0x0069] = 0x49;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0xc3;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after LDX $69");
            else Bad("Bad : AC was affected after LDX $69");
            if (cpu.x == 0x49) Good("Good: X was correct after LDX $69");
            else Bad("Bad : X was not correct after LDX $69");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after LDX $69");
            else Bad("Bad : PC was not incremented by 2 after LDX $69");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after LDX $69");
            else Bad("Bad : Cycles not incremented by 3 after LDX $69");
            cpu.reset();
            memory[0x1000] = 0xb6;       // LDX $69,Y
            memory[0x1001] = 0x69;
            memory[0x006b] = 0x94;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0xc3;
            cpu.y = 2;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after LDX $69,Y");
            else Bad("Bad : AC was affected after LDX $69,Y");
            if (cpu.x == 0x94) Good("Good: X was correct after LDX $69,Y");
            else Bad("Bad : X was not correct after LDX $69,Y");
            if (cpu.flags == 0x80) Good("Good: N flag was set after LDX $69,Y with $94");
            else Bad("Bad : N flag was not set after LDX $69,Y with $94");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after LDX $69,Y");
            else Bad("Bad : PC was not incremented by 2 after LDX $69,Y");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDX $69,Y");
            else Bad("Bad : Cycles not incremented by 4 after LDX $69,Y");
            cpu.reset();
            memory[0x1000] = 0xae;       // LDX $4969
            memory[0x1001] = 0x69;
            memory[0x1002] = 0x49;
            memory[0x4969] = 0xbb;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0xc3;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after LDX $4969");
            else Bad("Bad : AC was affected after LDX $4969");
            if (cpu.x == 0xbb) Good("Good: X was correct after LDX $4969");
            else Bad("Bad : X was not correct after LDX $4969");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after LDX $4969");
            else Bad("Bad : PC was not incremented by 3 after LDX $4969");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDX $4969");
            else Bad("Bad : Cycles not incremented by 4 after LDX $4969");
            cpu.reset();
            memory[0x1000] = 0xbe;       // LDX $4969,Y
            memory[0x1001] = 0x69;
            memory[0x1002] = 0x49;
            memory[0x496e] = 0xca;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0xc3;
            cpu.y = 0x05;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after LDX $4969,Y");
            else Bad("Bad : AC was affected after LDX $4969,Y");
            if (cpu.x == 0xca) Good("Good: X was correct after LDX $4969,Y");
            else Bad("Bad : X was not correct after LDX $4969,Y");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after LDX $4969,Y");
            else Bad("Bad : PC was not incremented by 3 after LDX $4969,Y");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDX $4969,Y");
            else Bad("Bad : Cycles not incremented by 4 after LDX $4969,Y");
        }

        protected void LDYtests()
        {
            mainForm.Debug("");
            mainForm.Debug("LDY");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xa0;       // LDY #$6
            memory[0x1001] = 0x06;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0xb8;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x05) Good("Good: AC was unaltered by LDY #$6");
            else Bad("Bad : AC was altered by LDY #$6");
            if (cpu.y == 0x06) Good("Good: Y was correct after LDY #$6");
            else Bad("Bad : Y was not correct after LDY #$6");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after LDY #$6");
            else Bad("Bad : PC not incremented by 2 after LDY #$6");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after LDY #$6");
            else Bad("Bad : Cycles not incremented by 2 after LDY #$6");
            if ((cpu.flags & Cpu65c02.FLAG_N) == 0x00) Good("Good: N flag cleared by LDY #$6");
            else Bad("Bad : N Flag not cleared by LDY #$6");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0x00) Good("Good: Z flag cleared by LDY #$6");
            else Bad("Bad : Z Flag not cleared by LDY #$6");
            if (cpu.flags == 0x7d) Good("Good: No other flags cleared by LDY #$6");
            else Bad("Bad : Other flags cleared by LDY #$6");
            cpu.reset();
            memory[0x1000] = 0xa4;       // LDY $8
            memory[0x1001] = 0x08;
            memory[0x0008] = 0xa0;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0xb8;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x05) Good("Good: AC was unaltered by LDY $8");
            else Bad("Bad : AC was altered by LDY $8");
            if (cpu.y == 0xa0) Good("Good: Y was correct after LDY $8");
            else Bad("Bad : Y was not correct after LDY $8");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after LDY $8");
            else Bad("Bad : PC not incremented by 2 after LDY $8");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after LDY $8");
            else Bad("Bad : Cycles not incremented by 3 after LDY $8");
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set by LDY $8");
            else Bad("Bad : N Flag not set by LDY $8");
            if (cpu.flags == 0x80) Good("Good: No other flags set by LDY $8");
            else Bad("Bad : Other flags set by LDY $8");
            cpu.reset();
            memory[0x1000] = 0xb4;       // LDY $fa,X
            memory[0x1001] = 0xfa;
            memory[0x0004] = 0x2f;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0xb8;
            cpu.x = 0x0a;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x05) Good("Good: AC was unaltered by LDY $fa,X");
            else Bad("Bad : AC was altered by LDY $fa,X");
            if (cpu.y == 0x2f) Good("Good: Y was correct after LDY $fa,X");
            else Bad("Bad : Y was not correct after LDY $fa,X");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after LDY $fa,X");
            else Bad("Bad : PC not incremented by 2 after LDY $fa,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDY $fa,X");
            else Bad("Bad : Cycles not incremented by 4 after LDY $fa,X");
            cpu.reset();
            memory[0x1000] = 0xac;       // LDY $749a
            memory[0x1001] = 0x9a;
            memory[0x1002] = 0x74;
            memory[0x749a] = 0xdb;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0xb8;
            cpu.x = 0x0a;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x05) Good("Good: AC was unaltered by LDY $749a");
            else Bad("Bad : AC was altered by LDY $749a");
            if (cpu.y == 0xdb) Good("Good: Y was correct after LDY $749a");
            else Bad("Bad : Y was not correct after LDY $749a");
            if (cpu.pc == 0x1003) Good("Good: PC incremented by 3 after LDY $749a");
            else Bad("Bad : PC not incremented by 3 after LDY $749a");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDY $749a");
            else Bad("Bad : Cycles not incremented by 4 after LDY $749a");
            cpu.reset();
            memory[0x1000] = 0xbc;       // LDY $749a,X
            memory[0x1001] = 0x9a;
            memory[0x1002] = 0x74;
            memory[0x74ab] = 0xb7;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0x63;
            cpu.x = 0x11;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x05) Good("Good: AC was unaltered by LDY $749a,X");
            else Bad("Bad : AC was altered by LDY $749a,X");
            if (cpu.y == 0xb7) Good("Good: Y was correct after LDY $749a,X");
            else Bad("Bad : Y was not correct after LDY $749a,X");
            if (cpu.pc == 0x1003) Good("Good: PC incremented by 3 after LDY $749a,X");
            else Bad("Bad : PC not incremented by 3 after LDY $749a,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after LDY $749a,X");
            else Bad("Bad : Cycles not incremented by 4 after LDY $749a,X");
            cpu.reset();
            memory[0x1000] = 0xa0;       // LDY #$0
            memory[0x1001] = 0x00;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0xb8;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z set by LDY #$0");
            else Bad("Bad : Z not set by LDY #$0");
        }

        protected void LSRtests()
        {
            mainForm.Debug("");
            mainForm.Debug("LSR");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x4a;       // LSR A
            cpu.pc = 0x1000;
            cpu.ac = 0xa4;
            cpu.flags = 1;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x52) Good("Good: AC was $52 after LSR A");
            else Bad("Bad : AC was not $52 after LSR A");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after LSR A");
            else Bad("Bad : PC was not incremented by 1 after LSR A");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after LSR A");
            else Bad("Bad : Cycles not incremented by 2 after LSR A");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared when shifting $a4");
            else Bad("Bad : C flag was not cleared when shifting $a4");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when shifting $a4");
            else Bad("Bad : N flag was not cleared when shifting $a4");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when shifting $a4");
            else Bad("Bad : Z flag was not cleared when shifting $a4");
            cpu.reset();
            memory[0x1000] = 0x4a;       // LSR A
            cpu.pc = 0x1000;
            cpu.ac = 0x1;
            cpu.flags = 1;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x00) Good("Good: AC was $00 after LSR A with $01");
            else Bad("Bad : AC was not $00 after LSR A with $01");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set when shifting $01");
            else Bad("Bad : C flag was not set when shifting $01");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when shifting $01");
            else Bad("Bad : N flag was not cleared when shifting $01");
            if ((cpu.flags & 0x02) == 0x02) Good("Good: Z flag set when shifting $01");
            else Bad("Bad : Z flag was not set when shifting $01");
            cpu.reset();
            memory[0x1000] = 0x46;       // LSR $50
            memory[0x1001] = 0x50;
            memory[0x0050] = 0x13;
            cpu.pc = 0x1000;
            cpu.ac = 0xa4;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xa4) Good("Good: AC was unaffected after LSR $50");
            else Bad("Bad : AC was affected after LSR $50");
            if (memory[0x0050] == 0x09) Good("Good: Memory was $09 after LSR $50");
            else Bad("Bad : Memory was not $09 after LSR $50");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after LSR $50");
            else Bad("Bad : PC was not incremented by 2 after LSR $50");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after LSR $50");
            else Bad("Bad : Cycles not incremented by 5 after LSR $50");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set when shifting $13");
            else Bad("Bad : C flag was not set when shifting $13");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when shifting $13");
            else Bad("Bad : N flag was not cleared when shifting $13");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when shifting $13");
            else Bad("Bad : Z flag was not cleared when shifting $13");
            cpu.reset();
            memory[0x1000] = 0x56;       // LSR $50,X
            memory[0x1001] = 0x50;
            memory[0x0055] = 0xa5;
            cpu.pc = 0x1000;
            cpu.ac = 0xa4;
            cpu.x = 5;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xa4) Good("Good: AC was unaffected after LSR $50,X");
            else Bad("Bad : AC was affected after LSR $50,X");
            if (memory[0x0055] == 0x52) Good("Good: Memory was $52 after LSR $50,X");
            else Bad("Bad : Memory was not $52 after LSR $50,X");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after LSR $50,X");
            else Bad("Bad : PC was not incremented by 2 after LSR $50,X");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after LSR $50,X");
            else Bad("Bad : Cycles not incremented by 6 after LSR $50,X");
            cpu.reset();
            memory[0x1000] = 0x4e;       // LSR $5072
            memory[0x1001] = 0x72;
            memory[0x1002] = 0x50;
            memory[0x5072] = 0xc6;
            cpu.pc = 0x1000;
            cpu.ac = 0xa4;
            cpu.x = 5;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xa4) Good("Good: AC was unaffected after LSR $5072");
            else Bad("Bad : AC was affected after LSR $5072");
            if (memory[0x5072] == 0x63) Good("Good: Memory was $63 after LSR $5072");
            else Bad("Bad : Memory was not $63 after LSR $5072");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after LSR $5072");
            else Bad("Bad : PC was not incremented by 3 after LSR $5072");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after LSR $5072");
            else Bad("Bad : Cycles not incremented by 6 after LSR $5072");
            cpu.reset();
            memory[0x1000] = 0x5e;       // LSR $5072,X
            memory[0x1001] = 0x72;
            memory[0x1002] = 0x50;
            memory[0x5077] = 0x99;
            cpu.pc = 0x1000;
            cpu.ac = 0xa4;
            cpu.x = 5;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xa4) Good("Good: AC was unaffected after LSR $5072,X");
            else Bad("Bad : AC was affected after LSR $5072,X");
            if (memory[0x5077] == 0x4c) Good("Good: Memory was $4c after LSR $5072,X");
            else Bad("Bad : Memory was not $4c after LSR $5072,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after LSR $5072,X");
            else Bad("Bad : PC was not incremented by 3 after LSR $5072,X");
            if (cpu.cycles == 7) Good("Good: Cycles incremented by 7 after LSR $5072,X");
            else Bad("Bad : Cycles not incremented by 7 after LSR $5072,X");
        }

        protected void NOPtests()
        {
            mainForm.Debug("");
            mainForm.Debug("NOP");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xea;       // NOP
            cpu.pc = 0x1000;
            cpu.ac = 0x12;
            cpu.x = 0x31;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x12) Good("Good: a was unaffected by NOP");
            else Bad("Bad : X was affected by NOP");
            if (cpu.flags == 0xff) Good("Good: No flags cleared by NOP");
            else Bad("Bad : Flags were cleared by NOP");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after NOP");
            else Bad("Bad : PC was not incremented by 1 after NOP");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after NOP");
            else Bad("Bad : Cycles not incremented by 2 after NOP");
            cpu.reset();
            memory[0x1000] = 0xea;       // NOP
            cpu.pc = 0x1000;
            cpu.ac = 0x12;
            cpu.x = 0x31;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.flags == 0x00) Good("Good: No flags set by NOP");
            else Bad("Bad : Flags were set by NOP");
        }

        protected void ORAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("ORA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x09;       // ORA #$23
            memory[0x1001] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x37) Good("Good: AC was $37 after ORA #$23");
            else Bad("Bad : AC was not $37 after ORA #$23");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after ORA #$23");
            else Bad("Bad : PC was not incremented by two after ORA #$23");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by two after ORA #$23");
            else Bad("Bad : Cycles not incremented by two after ORA #$23");
            cpu.reset();
            memory[0x1000] = 0x05;       // ORA $23
            memory[0x1001] = 0x23;
            memory[0x0023] = 0x14;
            cpu.pc = 0x1000;
            cpu.ac = 0x21;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x35) Good("Good: AC was $35 after ORA $23");
            else Bad("Bad : AC was not $35 after ORA $23");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after ORA $23");
            else Bad("Bad : PC was not incremented by two after ORA $23");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after ORA $23");
            else Bad("Bad : Cycles not incremented by 3 after ORA $23");
            cpu.reset();
            memory[0x1000] = 0x15;       // ORA $23,X
            memory[0x1001] = 0x23;
            memory[0x0029] = 0x81;
            cpu.pc = 0x1000;
            cpu.ac = 0x18;
            cpu.x = 6;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x99) Good("Good: AC was $99 after ORA $23,X");
            else Bad("Bad : AC was not $99 after ORA $23,X");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after ORA $23,X");
            else Bad("Bad : PC was not incremented by two after ORA $23,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after ORA $23,X");
            else Bad("Bad : Cycles not incremented by 4 after ORA $23,X");
            cpu.reset();
            memory[0x1000] = 0x0d;       // ORA $2345
            memory[0x1001] = 0x45;
            memory[0x1002] = 0x23;
            memory[0x2345] = 0x42;
            cpu.pc = 0x1000;
            cpu.ac = 0x24;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x66) Good("Good: AC was $66 after ORA $2345");
            else Bad("Bad : AC was not $66 after ORA $2345");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ORA $2345");
            else Bad("Bad : PC was not incremented by 3 after ORA $2345");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after ORA $2345");
            else Bad("Bad : Cycles not incremented by 4 after ORA $2345");
            cpu.reset();
            memory[0x1000] = 0x1d;       // ORA $4523,X
            memory[0x1001] = 0x23;
            memory[0x1002] = 0x45;
            memory[0x4528] = 0x80;
            cpu.pc = 0x1000;
            cpu.ac = 0x08;
            cpu.x = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x88) Good("Good: AC was $88 after ORA $4523,X");
            else Bad("Bad : AC was not $88 after ORA $4523,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ORA $4523,X");
            else Bad("Bad : PC was not incremented by 3 after ORA $4523,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after ORA $4523,X");
            else Bad("Bad : Cycles not incremented by 4 after ORA $4523,X");
            cpu.reset();
            memory[0x1000] = 0x19;       // ORA $4523,Y
            memory[0x1001] = 0x23;
            memory[0x1002] = 0x45;
            memory[0x4525] = 0xf0;
            cpu.pc = 0x1000;
            cpu.ac = 0x0f;
            cpu.y = 2;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xff) Good("Good: AC was $ff after ORA $4523,Y");
            else Bad("Bad : AC was not $ff after ORA $4523,Y");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ORA $4523,Y");
            else Bad("Bad : PC was not incremented by 3 after ORA $4523,Y");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after ORA $4523,Y");
            else Bad("Bad : Cycles not incremented by 4 after ORA $4523,Y");
            cpu.reset();
            memory[0x1000] = 0x01;       // ORA ($fa,X)
            memory[0x1001] = 0xfa;
            memory[0x00ff] = 0x87;
            memory[0x0000] = 0x47;
            memory[0x4787] = 0x50;
            cpu.pc = 0x1000;
            cpu.ac = 0x03;
            cpu.x = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x53) Good("Good: AC was $53 after ORA ($fa,X)");
            else Bad("Bad : AC was not $53 after ORA ($fa,X)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ORA ($fa,X)");
            else Bad("Bad : PC was not incremented by 2 after ORA ($fa,X)");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after ORA ($fa,X)");
            else Bad("Bad : Cycles not incremented by 6 after ORA ($fa,X)");
            cpu.reset();
            memory[0x1000] = 0x11;       // ORA ($ff),Y
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x71;
            memory[0x0000] = 0x92;
            memory[0x9275] = 0xe0;
            cpu.pc = 0x1000;
            cpu.ac = 0x0b;
            cpu.y = 4;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xeb) Good("Good: AC was $eb after ORA ($ff),Y");
            else Bad("Bad : AC was not $eb after ORA ($ff),Y");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ORA ($ff),Y");
            else Bad("Bad : PC was not incremented by 2 after ORA ($ff),Y");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after ORA ($ff),Y");
            else Bad("Bad : Cycles not incremented by 5 after ORA ($ff),Y");
            cpu.reset();
            memory[0x1000] = 0x12;       // ORA ($ff)
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x71;
            memory[0x0000] = 0x92;
            memory[0x9271] = 0xc0;
            cpu.pc = 0x1000;
            cpu.ac = 0x0a;
            cpu.y = 4;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xca) Good("Good: AC was $ca after ORA ($ff)");
            else Bad("Bad : AC was not $ca after ORA ($ff)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ORA ($ff)");
            else Bad("Bad : PC was not incremented by 2 after ORA ($ff)");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after ORA ($ff)");
            else Bad("Bad : Cycles not incremented by 5 after ORA ($ff)");
            cpu.reset();
            memory[0x1000] = 0x09;       // ORA #$0
            memory[0x1001] = 0x00;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.cycles = 0;
            cpu.flags = 0xfd;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag reset when result 0");
            else Bad("Bad : N flag was not reset when result 0");
            if ((cpu.flags & 0x02) == 0x02) Good("Good: Z flag set when result 0");
            else Bad("Bad : Z flag was not set when result 0");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag unaffected when result 0");
            else Bad("Bad : V flag was affected when result 0");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag unaffected when result 0");
            else Bad("Bad : C flag was affected when result 0");
            cpu.reset();
            memory[0x1000] = 0x09;       // ORA #1
            memory[0x1001] = 0x01;
            cpu.pc = 0x1000;
            cpu.ac = 0;
            cpu.cycles = 0;
            cpu.flags = 0xff;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag reset when result +1");
            else Bad("Bad : N flag was not reset when result +1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag reset when result +1");
            else Bad("Bad : Z flag was not reset when result +1");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag unaffected when result +1");
            else Bad("Bad : V flag was affected when result +1");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag unaffected when result +1");
            else Bad("Bad : C flag was affected when result +1");
            cpu.reset();
            memory[0x1000] = 0x09;       // ORA #-1
            memory[0x1001] = 0x0f;
            cpu.pc = 0x1000;
            cpu.ac = 0xf0;
            cpu.cycles = 0;
            cpu.flags = 0x02;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set when result -1");
            else Bad("Bad : N flag was not set when result -1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag reset when result -1");
            else Bad("Bad : Z flag was not reset when result -1");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag unaffected when result -1");
            else Bad("Bad : V flag was affected when result -1");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag unaffected when result -1");
            else Bad("Bad : C flag was affected when result -1");
        }

        protected void PHAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("PHA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x48;       // PHA
            cpu.pc = 0x1000;
            cpu.sp = 0xf0;
            cpu.ac = 0x9b;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x01f0] == 0x9b) Good("Good: A was correctly written to memory");
            else Bad("Bad : A was not correctly written to memory");
            if (cpu.pc == 0x1001) Good("Good: PC is correct after PHA");
            else Bad("Bad : PC is not correct after PHA");
            if (cpu.sp == 0xef) Good("Good: SP is correct after PHA");
            else Bad("Bad : SP is not correct after PHA");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after PHA");
            else Bad("Bad : Cycles not incremented by 3 after PHA");
        }

        protected void PHPtests()
        {
            mainForm.Debug("");
            mainForm.Debug("PHP");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x08;       // PHP
            cpu.pc = 0x1000;
            cpu.sp = 0xff;
            cpu.flags = 0xa2;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x01ff] == 0xa2) Good("Good: Flags were correctly written to memory");
            else Bad("Bad : Flags were not correctly written to memory");
            if (cpu.pc == 0x1001) Good("Good: PC is correct after PHP");
            else Bad("Bad : PC is not correct after PHP");
            if (cpu.sp == 0xfe) Good("Good: SP is correct after PHP");
            else Bad("Bad : SP is not correct after PHP");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after PHP");
            else Bad("Bad : Cycles not incremented by 3 after PHP");
        }

        protected void PHXtests()
        {
            mainForm.Debug("");
            mainForm.Debug("PHX");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xda;       // PHX
            cpu.pc = 0x1000;
            cpu.sp = 0xff;
            cpu.x = 0x5a;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x01ff] == 0x5a) Good("Good: X was correctly written to memory");
            else Bad("Bad : X was not correctly written to memory");
            if (cpu.pc == 0x1001) Good("Good: PC is correct after PHX");
            else Bad("Bad : PC is not correct after PHX");
            if (cpu.sp == 0xfe) Good("Good: SP is correct after PHX");
            else Bad("Bad : SP is not correct after PHX");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after PHX");
            else Bad("Bad : Cycles not incremented by 3 after PHX");
        }

        protected void PHYtests()
        {
            mainForm.Debug("");
            mainForm.Debug("PHY");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x5a;       // PHY
            cpu.pc = 0x1000;
            cpu.sp = 0xff;
            cpu.y = 0xc7;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x01ff] == 0xc7) Good("Good: Y was correctly written to memory");
            else Bad("Bad : Y was not correctly written to memory");
            if (cpu.pc == 0x1001) Good("Good: PC is correct after PHY");
            else Bad("Bad : PC is not correct after PHY");
            if (cpu.sp == 0xfe) Good("Good: SP is correct after PHY");
            else Bad("Bad : SP is not correct after PHY");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after PHY");
            else Bad("Bad : Cycles not incremented by 3 after PHY");
        }

        protected void PLAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("PLA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x68;       // PLA
            memory[0x01f2] = 0x71;
            cpu.pc = 0x1000;
            cpu.sp = 0xf1;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x71) Good("Good: A was correctly read from memory");
            else Bad("Bad : A was not correctly read from memory");
            if ((cpu.flags & Cpu65c02.FLAG_N) == 0x00) Good("Good: N flag cleared pulling positive value");
            else Bad("Bad : N flag not cleared pulling positive value");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0x00) Good("Good: Z flag cleared pulling non-zero value");
            else Bad("Bad : Z flag not cleared pulling non-zero value");
            if (cpu.flags == 0x7d) Good("Good: No other flags cleared pulling positive non-zero value");
            else Bad("Bad : Other flags cleared pulling positive non-zero value");
            if (cpu.pc == 0x1001) Good("Good: PC is correct after PLA");
            else Bad("Bad : PC is not correct after PLA");
            if (cpu.sp == 0xf2) Good("Good: SP is correct after PLA");
            else Bad("Bad : SP is not correct after PLA");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after PLA");
            else Bad("Bad : Cycles not incremented by 4 after PLA");
            cpu.reset();
            memory[0x1000] = 0x68;       // PLA
            memory[0x01e5] = 0xc1;
            cpu.pc = 0x1000;
            cpu.sp = 0xe4;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set pulling negative value");
            else Bad("Bad : N flag not set pulling negative value");
            cpu.reset();
            memory[0x1000] = 0x68;       // PLA
            memory[0x01e5] = 0x00;
            cpu.pc = 0x1000;
            cpu.sp = 0xe4;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set pulling zero value");
            else Bad("Bad : Z flag not set pulling zero value");
        }

        protected void PLPtests()
        {
            mainForm.Debug("");
            mainForm.Debug("PLP");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x28;       // PLP
            memory[0x01ff] = 0x92;
            cpu.pc = 0x1000;
            cpu.sp = 0xfe;
            cpu.flags = 0xa2;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.flags == 0x92) Good("Good: Flags were correctly read from memory");
            else Bad("Bad : Flags were not correctly read from memory");
            if (cpu.pc == 0x1001) Good("Good: PC is correct after PLP");
            else Bad("Bad : PC is not correct after PLP");
            if (cpu.sp == 0xff) Good("Good: SP is correct after PLP");
            else Bad("Bad : SP is not correct after PLP");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after PLP");
            else Bad("Bad : Cycles not incremented by 4 after PLP");
        }

        protected void PLXtests()
        {
            mainForm.Debug("");
            mainForm.Debug("PLX");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xfa;       // PLX
            memory[0x01bf] = 0x57;
            cpu.pc = 0x1000;
            cpu.sp = 0xbe;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.x == 0x57) Good("Good: X was correctly read from memory");
            else Bad("Bad : X was not correctly read from memory");
            if ((cpu.flags & Cpu65c02.FLAG_N) == 0x00) Good("Good: N flag cleared pulling positive value");
            else Bad("Bad : N flag not cleared pulling positive value");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0x00) Good("Good: Z flag cleared pulling non-zero value");
            else Bad("Bad : Z flag not cleared pulling non-zero value");
            if (cpu.flags == 0x7d) Good("Good: No other flags cleared pulling positive non-zero value");
            else Bad("Bad : Other flags cleared pulling positive non-zero value");
            if (cpu.pc == 0x1001) Good("Good: PC is correct after PLX");
            else Bad("Bad : PC is not correct after PLX");
            if (cpu.sp == 0xbf) Good("Good: SP is correct after PLX");
            else Bad("Bad : SP is not correct after PLX");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after PLX");
            else Bad("Bad : Cycles not incremented by 4 after PLX");
            cpu.reset();
            memory[0x1000] = 0xfa;       // PLX
            memory[0x01e3] = 0xc1;
            cpu.pc = 0x1000;
            cpu.sp = 0xe2;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set pulling negative value");
            else Bad("Bad : N flag not set pulling negative value");
            cpu.reset();
            memory[0x1000] = 0xfa;       // PLX
            memory[0x01e5] = 0x00;
            cpu.pc = 0x1000;
            cpu.sp = 0xe4;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set pulling zero value");
            else Bad("Bad : Z flag not set pulling zero value");
        }

        protected void PLYtests()
        {
            mainForm.Debug("");
            mainForm.Debug("PLY");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x7a;       // PLY
            memory[0x01ef] = 0x13;
            cpu.pc = 0x1000;
            cpu.sp = 0xee;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.y == 0x13) Good("Good: Y was correctly read from memory");
            else Bad("Bad : y was not correctly read from memory");
            if ((cpu.flags & Cpu65c02.FLAG_N) == 0x00) Good("Good: N flag cleared pulling positive value");
            else Bad("Bad : N flag not cleared pulling positive value");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0x00) Good("Good: Z flag cleared pulling non-zero value");
            else Bad("Bad : Z flag not cleared pulling non-zero value");
            if (cpu.flags == 0x7d) Good("Good: No other flags cleared pulling positive non-zero value");
            else Bad("Bad : Other flags cleared pulling positive non-zero value");
            if (cpu.pc == 0x1001) Good("Good: PC is correct after PLY");
            else Bad("Bad : PC is not correct after PLY");
            if (cpu.sp == 0xef) Good("Good: SP is correct after PLY");
            else Bad("Bad : SP is not correct after PLY");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after PLY");
            else Bad("Bad : Cycles not incremented by 4 after PLY");
            cpu.reset();
            memory[0x1000] = 0x7a;       // PLY
            memory[0x01e3] = 0xc1;
            cpu.pc = 0x1000;
            cpu.sp = 0xe2;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set pulling negative value");
            else Bad("Bad : N flag not set pulling negative value");
            cpu.reset();
            memory[0x1000] = 0x7a;       // PLY
            memory[0x01e5] = 0x00;
            cpu.pc = 0x1000;
            cpu.sp = 0xe4;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set pulling zero value");
            else Bad("Bad : Z flag not set pulling zero value");
        }

        protected void ROLtests()
        {
            mainForm.Debug("");
            mainForm.Debug("ROL");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x2a;       // ROL A
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x68) Good("Good: AC was $68 after ROL A");
            else Bad("Bad : AC was not $68 after ROL A");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after ROL A");
            else Bad("Bad : PC was not incremented by 1 after ROL A");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after ROL A");
            else Bad("Bad : Cycles not incremented by 2 after ROL A");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared when rotating $34");
            else Bad("Bad : C flag was not cleared when rotating $34");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when rotating $34");
            else Bad("Bad : N flag was not cleared when rotating $34");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when rotating $34");
            else Bad("Bad : Z flag was not cleared when rotating $34");
            cpu.reset();
            memory[0x1000] = 0x26;       // ROL $25
            memory[0x1001] = 0x25;
            memory[0x0025] = 0x82;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after ROL $25");
            else Bad("Bad : AC was affected after ROL $25");
            if (memory[0x0025] == 0x05) Good("Good: Memory was $05 after ROL $25 with $82 and carry set");
            else Bad("Bad : Memory was not $05 after ROL $25 with $82 and carry set");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ROL $25");
            else Bad("Bad : PC was not incremented by 2 after ROL $25");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after ROL $25");
            else Bad("Bad : Cycles not incremented by 5 after ROL $25");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set when rotating $82");
            else Bad("Bad : C flag was not set when rotating $82");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when rotating $82");
            else Bad("Bad : N flag was not cleared when rotating $82");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when rotating $82");
            else Bad("Bad : Z flag was not cleared when rotating $82");
            cpu.reset();
            memory[0x1000] = 0x36;       // ROL $25,X
            memory[0x1001] = 0x25;
            memory[0x0029] = 0x75;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 4;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after ROL $25,X");
            else Bad("Bad : AC was affected after ROL $25,X");
            if (memory[0x0029] == 0xea) Good("Good: Memory was $ea after ROL $25,X with $75");
            else Bad("Bad : Memory was not $ea after ROL $25,X with $75");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ROL $25,X");
            else Bad("Bad : PC was not incremented by 2 after ROL $25,X");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after ROL $25,X");
            else Bad("Bad : Cycles not incremented by 6 after ROL $25,X");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared when rotating $75");
            else Bad("Bad : C flag was not cleared when rotating $75");
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set when rotating $75");
            else Bad("Bad : N flag was not set when rotating $75");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when rotating $75");
            else Bad("Bad : Z flag was not cleared when rotating $75");
            cpu.reset();
            memory[0x1000] = 0x2e;       // ROL $2599
            memory[0x1001] = 0x99;
            memory[0x1002] = 0x25;
            memory[0x2599] = 0x80;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 4;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after ROL $2599");
            else Bad("Bad : AC was affected after ROL $2599");
            if (memory[0x2599] == 0x00) Good("Good: Memory was $00 after ROL $2599 with $80");
            else Bad("Bad : Memory was not $00 after ROL $2599 with $80");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ROL $2599");
            else Bad("Bad : PC was not incremented by 3 after ROL $2599");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after ROL $2599");
            else Bad("Bad : Cycles not incremented by 6 after ROL $2599");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set when rotating $80");
            else Bad("Bad : C flag was not set when rotating $80");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when rotating $80");
            else Bad("Bad : N flag was not cleared when rotating $80");
            if ((cpu.flags & 0x02) == 0x02) Good("Good: Z flag set when rotating $80");
            else Bad("Bad : Z flag was not set when rotating $80");
            cpu.reset();
            memory[0x1000] = 0x3e;       // ROL $2599,X
            memory[0x1001] = 0x99;
            memory[0x1002] = 0x25;
            memory[0x259d] = 0x3c;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 4;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after ROL $2599,X");
            else Bad("Bad : AC was affected after ROL $2599,X");
            if (memory[0x259d] == 0x78) Good("Good: Memory was $78 after ROL $2599,X with $3c");
            else Bad("Bad : Memory was not $78 after ROL $2599,X with $3c");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ROL $2599,X");
            else Bad("Bad : PC was not incremented by 3 after ROL $2599,X");
            if (cpu.cycles == 7) Good("Good: Cycles incremented by 7 after ROL $2599,X");
            else Bad("Bad : Cycles not incremented by 7 after ROL $2599,X");
        }

        protected void RORtests()
        {
            mainForm.Debug("");
            mainForm.Debug("ROR");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x6a;       // ROR A
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x1a) Good("Good: AC was $1a after ROR A");
            else Bad("Bad : AC was not $1a after ROR A");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after ROR A");
            else Bad("Bad : PC was not incremented by 1 after ROR A");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after ROR A");
            else Bad("Bad : Cycles not incremented by 2 after ROR A");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared when rotating $34");
            else Bad("Bad : C flag was not cleared when rotating $34");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when rotating $34");
            else Bad("Bad : N flag was not cleared when rotating $34");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when rotating $34");
            else Bad("Bad : Z flag was not cleared when rotating $34");
            cpu.reset();
            memory[0x1000] = 0x66;       // ROR $60
            memory[0x1001] = 0x60;
            memory[0x0060] = 0xc1;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was $34 after ROR $60");
            else Bad("Bad : AC was not $34 after ROR $60");
            if (memory[0x0060] == 0x60) Good("Good: Memory was $60 after ROR $60 with $c1");
            else Bad("Bad : Memory was not $60 after ROR $60 with $c1");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ROR $60");
            else Bad("Bad : PC was not incremented by 2 after ROR $60");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after ROR $60");
            else Bad("Bad : Cycles not incremented by 5 after ROR $60");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set when rotating $c1");
            else Bad("Bad : C flag was not set when rotating $c1");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when rotating $c1");
            else Bad("Bad : N flag was not cleared when rotating $c1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when rotating $c1");
            else Bad("Bad : Z flag was not cleared when rotating $c1");
            cpu.reset();
            memory[0x1000] = 0x76;       // ROR $60,X
            memory[0x1001] = 0x60;
            memory[0x0067] = 0x22;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 7;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was $34 after ROR $60,X");
            else Bad("Bad : AC was not $34 after ROR $60,X");
            if (memory[0x0067] == 0x91) Good("Good: Memory was $91 after ROR $60,X with $22,cf set");
            else Bad("Bad : Memory was not $91 after ROR $60,X with $22,cf set");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after ROR $60,X");
            else Bad("Bad : PC was not incremented by 2 after ROR $60,X");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after ROR $60,X");
            else Bad("Bad : Cycles not incremented by 6 after ROR $60,X");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared when rotating $22");
            else Bad("Bad : C flag was not cleared when rotating $22");
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set when rotating $22,cf set");
            else Bad("Bad : N flag was not set when rotating $22,cf set");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared when rotating $22");
            else Bad("Bad : Z flag was not cleared when rotating $22");
            cpu.reset();
            memory[0x1000] = 0x6e;       // ROR $6077
            memory[0x1001] = 0x77;
            memory[0x1002] = 0x60;
            memory[0x6077] = 0x01;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 7;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was $34 after ROR $6077");
            else Bad("Bad : AC was not $34 after ROR $6077");
            if (memory[0x6077] == 0x00) Good("Good: Memory was $00 after ROR $6077 with $01");
            else Bad("Bad : Memory was not $00 after ROR $6077 with $01");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ROR $6077");
            else Bad("Bad : PC was not incremented by 3 after ROR $6077");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after ROR $6077");
            else Bad("Bad : Cycles not incremented by 6 after ROR $6077");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set when rotating $01");
            else Bad("Bad : C flag was not set when rotating $01");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared when rotating $01");
            else Bad("Bad : N flag was not cleared when rotating $01");
            if ((cpu.flags & 0x02) == 0x02) Good("Good: Z flag set when rotating $01");
            else Bad("Bad : Z flag was not set when rotating $01");
            cpu.reset();
            memory[0x1000] = 0x7e;       // ROR $6077,X
            memory[0x1001] = 0x77;
            memory[0x1002] = 0x60;
            memory[0x6079] = 0x18;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 2;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was $34 after ROR $6077,X");
            else Bad("Bad : AC was not $34 after ROR $6077,X");
            if (memory[0x6079] == 0x0c) Good("Good: Memory was $0c after ROR $6077,X with $18");
            else Bad("Bad : Memory was not $0c after ROR $6077,X with $18");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after ROR $6077,X");
            else Bad("Bad : PC was not incremented by 3 after ROR $6077,X");
            if (cpu.cycles == 7) Good("Good: Cycles incremented by 7 after ROR $6077,X");
            else Bad("Bad : Cycles not incremented by 7 after ROR $6077,X");
        }

        protected void RTItests()
        {
            mainForm.Debug("");
            mainForm.Debug("RTI");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x2000] = 0x40;       // RTI
            memory[0x01ff] = 0x10;
            memory[0x01fe] = 0x02;
            memory[0x01fd] = 0xc0;
            cpu.pc = 0x2000;
            cpu.ac = 0x06;
            cpu.flags = 0xb7;
            cpu.sp = 0xfc;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1002) Good("Good: PC was correct after RTI");
            else Bad("Bad : PC was not correct after RTI");
            if (cpu.sp == 0xff) Good("Good: S was correct after RTI");
            else Bad("Bad : S was not correct after RTI");
            if (cpu.flags == 0xc0) Good("Good: flags were correct after RTI");
            else Bad("Bad : flags were not correct after RTI");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after RTI $1234");
            else Bad("Bad : Cycles not incremented by 6 after RTI $1234");
        }

        protected void RTStests()
        {
            mainForm.Debug("");
            mainForm.Debug("RTS");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x60;       // RTS
            memory[0x01ab] = 0x10;
            memory[0x01aa] = 0x02;
            cpu.pc = 0x1000;
            cpu.ac = 0x06;
            cpu.flags = 0;
            cpu.sp = 0xa9;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.pc == 0x1003) Good("Good: PC was correct after RTS");
            else Bad("Bad : PC was not correct after RTS");
            if (cpu.sp == 0xab) Good("Good: S was correct after RTS");
            else Bad("Bad : S was not correct after RTS");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after RTS $1234");
            else Bad("Bad : Cycles not incremented by 6 after RTS $1234");
        }

        protected void SBCtests()
        {
            mainForm.Debug("");
            mainForm.Debug("SBC - Binary");
            mainForm.Debug("------------");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xe9;       // SBC #$05
            memory[0x1001] = 0x05;
            cpu.pc = 0x1000;
            cpu.ac = 0x0c;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x07) Good("Good: AC was $07 after SBC #$05");
            else Bad("Bad : AC was not $07 after SBC #$05");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after SBC #$05");
            else Bad("Bad : PC was not incremented by 2 after SBC #$05");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after SBC #$05");
            else Bad("Bad : Cycles not incremented by 2 after SBC #$05");
            cpu.reset();
            memory[0x1000] = 0xe5;       // SBC $05
            memory[0x1001] = 0x05;
            memory[0x0005] = 0x07;
            cpu.pc = 0x1000;
            cpu.ac = 0x10;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x09) Good("Good: AC was $09 after SBC $05");
            else Bad("Bad : AC was not $09 after SBC $05");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after SBC $05");
            else Bad("Bad : PC was not incremented by 2 after SBC $05");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after SBC $05");
            else Bad("Bad : Cycles not incremented by 3 after SBC $05");
            cpu.reset();
            memory[0x1000] = 0xf5;       // SBC $05,X
            memory[0x1001] = 0x05;
            memory[0x0008] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0x57;
            cpu.x = 3;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x34) Good("Good: AC was $34 after SBC $05,X");
            else Bad("Bad : AC was not $34 after SBC $05,X");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after SBC $05,X");
            else Bad("Bad : PC was not incremented by 2 after SBC $05,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after SBC $05,X");
            else Bad("Bad : Cycles not incremented by 4 after SBC $05,X");
            cpu.reset();
            memory[0x1000] = 0xed;       // SBC $3210
            memory[0x1001] = 0x10;
            memory[0x1002] = 0x32;
            memory[0x3210] = 0x11;
            cpu.pc = 0x1000;
            cpu.ac = 0xab;
            cpu.x = 3;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x9a) Good("Good: AC was $9a after SBC $3210");
            else Bad("Bad : AC was not $9a after SBC $3210");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after SBC $3210");
            else Bad("Bad : PC was not incremented by 3 after SBC $3210");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after SBC $3210");
            else Bad("Bad : Cycles not incremented by 4 after SBC $3210");
            cpu.reset();
            memory[0x1000] = 0xfd;       // SBC $3210,X
            memory[0x1001] = 0x10;
            memory[0x1002] = 0x32;
            memory[0x3213] = 0x41;
            cpu.pc = 0x1000;
            cpu.ac = 0xf3;
            cpu.x = 3;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xb2) Good("Good: AC was $b2 after SBC $3210,X");
            else Bad("Bad : AC was not $b2 after SBC $3210,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after SBC $3210,X");
            else Bad("Bad : PC was not incremented by 3 after SBC $3210,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after SBC $3210,X");
            else Bad("Bad : Cycles not incremented by 4 after SBC $3210,X");
            cpu.reset();
            memory[0x1000] = 0xf9;       // SBC $3210,Y
            memory[0x1001] = 0x10;
            memory[0x1002] = 0x32;
            memory[0x3214] = 0xaa;
            cpu.pc = 0x1000;
            cpu.ac = 0xfe;
            cpu.y = 4;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x54) Good("Good: AC was $54 after SBC $3210,Y");
            else Bad("Bad : AC was not $54 after SBC $3210,Y");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after SBC $3210,Y");
            else Bad("Bad : PC was not incremented by 3 after SBC $3210,Y");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after SBC $3210,Y");
            else Bad("Bad : Cycles not incremented by 4 after SBC $3210,Y");
            cpu.reset();
            memory[0x1000] = 0xe1;       // SBC ($fa,X)
            memory[0x1001] = 0xfa;
            memory[0x00ff] = 0x32;
            memory[0x0000] = 0x77;
            memory[0x7732] = 0x22;
            cpu.pc = 0x1000;
            cpu.ac = 0xdc;
            cpu.x = 5;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0xba) Good("Good: AC was $ba after SBC ($fa,X)");
            else Bad("Bad : AC was not $ba after SBC ($fa,X)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after SBC ($fa,X)");
            else Bad("Bad : PC was not incremented by 2 after SBC ($fa,X)");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after SBC ($fa,X)");
            else Bad("Bad : Cycles not incremented by 6 after SBC ($fa,X)");
            cpu.reset();
            memory[0x1000] = 0xf1;       // SBC ($ff),Y
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x32;
            memory[0x0000] = 0x77;
            memory[0x7734] = 0x54;
            cpu.pc = 0x1000;
            cpu.ac = 0xaa;
            cpu.y = 2;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x56) Good("Good: AC was $56 after SBC ($ff),Y");
            else Bad("Bad : AC was not $56 after SBC ($ff),Y");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after SBC ($ff),Y");
            else Bad("Bad : PC was not incremented by 2 after SBC ($ff),Y");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after SBC ($ff),Y");
            else Bad("Bad : Cycles not incremented by 5 after SBC ($ff),Y");
            cpu.reset();
            memory[0x1000] = 0xf2;       // SBC ($ff)
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x45;
            memory[0x0000] = 0x77;
            memory[0x7745] = 0x12;
            cpu.pc = 0x1000;
            cpu.ac = 0x98;
            cpu.y = 2;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x86) Good("Good: AC was $86 after SBC ($ff)");
            else Bad("Bad : AC was not $86 after SBC ($ff)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after SBC ($ff)");
            else Bad("Bad : PC was not incremented by 2 after SBC ($ff)");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after SBC ($ff)");
            else Bad("Bad : Cycles not incremented by 5 after SBC ($ff)");
            cpu.reset();
            memory[0x1000] = 0xe9;       // SBC #$05
            memory[0x1001] = 0x05;
            cpu.pc = 0x1000;
            cpu.ac = 0x0e;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x08) Good("Good: AC was $08 after 14 - 5 with borrow");
            else Bad("Bad : AC was not $08 after 14 - 5 with borrow");
            cpu.reset();
            memory[0x1000] = 0xe9;       // SBC #5
            memory[0x1001] = 0x05;
            cpu.pc = 0x1000;
            cpu.ac = 0x07;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared with 7-5");
            else Bad("Bad : N flag was not cleared with 7-5");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared with 7-5");
            else Bad("Bad : Z flag was not cleared with 7-5");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag cleared with 7-5");
            else Bad("Bad : V flag was not cleared with 7-5");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set with 7-5");
            else Bad("Bad : C flag was not set with 7-5");
            cpu.reset();
            memory[0x1000] = 0xe9;       // SBC #7
            memory[0x1001] = 0x07;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if (cpu.ac == 0xfe) Good("Good: AC was -2 after 5-7");
            else Bad("Bad : AC was not -2 after 5-7");
            if ((cpu.flags & 0x80) == 0x80) Good("Good: N flag set with 5-7");
            else Bad("Bad : N flag was not set with 5-7");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared with 5-7");
            else Bad("Bad : Z flag was not cleared with 5-7");
            if ((cpu.flags & 0x40) == 0x00) Good("Good: V flag cleared with 5-7");
            else Bad("Bad : V flag was not cleared with 5-7");
            if ((cpu.flags & 0x01) == 0x00) Good("Good: C flag cleared with 5-7");
            else Bad("Bad : C flag was not cleared with 5-7");
            cpu.reset();
            memory[0x1000] = 0xe9;       // SBC #1
            memory[0x1001] = 0x01;
            cpu.pc = 0x1000;
            cpu.ac = 0x80;
            cpu.cycles = 0;
            cpu.flags = Cpu65c02.FLAG_C;
            cpu.cycle();
            if (cpu.ac == 0x7f) Good("Good: AC was +127 after -128-1");
            else Bad("Bad : AC was not +127 after -128-1");
            if ((cpu.flags & 0x80) == 0x00) Good("Good: N flag cleared with -128-1");
            else Bad("Bad : N flag was not cleared with -128-1");
            if ((cpu.flags & 0x02) == 0x00) Good("Good: Z flag cleared with -128-1");
            else Bad("Bad : Z flag was not cleared with -128-1");
            if ((cpu.flags & 0x40) == 0x40) Good("Good: V flag set with -128-1");
            else Bad("Bad : V flag was not set with -128-1");
            if ((cpu.flags & 0x01) == 0x01) Good("Good: C flag set with -128-1");
            else Bad("Bad : C flag was not set with -128-1");
        }

        protected void SECtests()
        {
            mainForm.Debug("");
            mainForm.Debug("SEC");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x38;       // SEC
            cpu.pc = 0x1000;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_C) != 0x00) Good("Good: C set after SEC");
            else Bad("Bad : C not set after SEC");
            if (cpu.flags == 0x01) Good("Good: No other flags set after SEC");
            else Bad("Bad : Other flags were set after SEC");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after SEC");
            else Bad("Bad : PC was not incremented by 1 after SEC");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after SEC");
            else Bad("Bad : Cycles not incremented by 2 after SEC");
        }

        protected void SEDtests()
        {
            mainForm.Debug("");
            mainForm.Debug("SED");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xf8;       // SED
            cpu.pc = 0x1000;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_D) != 0x00) Good("Good: D set after SED");
            else Bad("Bad : D not set after SED");
            if (cpu.flags == 0x08) Good("Good: No other flags set after SED");
            else Bad("Bad : Other flags were set after SED");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after SED");
            else Bad("Bad : PC was not incremented by 1 after SED");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after SED");
            else Bad("Bad : Cycles not incremented by 2 after SED");
        }

        protected void SEItests()
        {
            mainForm.Debug("");
            mainForm.Debug("SEI");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x78;       // SEI
            cpu.pc = 0x1000;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_I) != 0x00) Good("Good: I set after SEI");
            else Bad("Bad : I not set after SEI");
            if (cpu.flags == 0x04) Good("Good: No other flags set after SEI");
            else Bad("Bad : Other flags were set after SEI");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after SEI");
            else Bad("Bad : PC was not incremented by 1 after SEI");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after SEI");
            else Bad("Bad : Cycles not incremented by 2 after SEI");
        }

        protected void STAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("STA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x85;       // STA $23
            memory[0x1001] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0xf5;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x0023] == 0xf5) Good("Good: memory was $f5 after STA $23");
            else Bad("Bad : memory was not $f5 after STA $23");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after STA $23");
            else Bad("Bad : PC was not incremented by two after STA $23");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after STA $23");
            else Bad("Bad : Cycles not incremented by 3 after STA $23");
            cpu.reset();
            memory[0x1000] = 0x95;       // STA $23,X
            memory[0x1001] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0xea;
            cpu.x = 2;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x0025] == 0xea) Good("Good: memory was $ea after STA $23,X");
            else Bad("Bad : memory was not $ea after STA $23,X");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after STA $23,X");
            else Bad("Bad : PC was not incremented by 2 after STA $23,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after STA $23,X");
            else Bad("Bad : Cycles not incremented by 4 after STA $23,X");
            cpu.reset();
            memory[0x1000] = 0x8d;       // STA $2345
            memory[0x1001] = 0x45;
            memory[0x1002] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0x95;
            cpu.x = 2;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x2345] == 0x95) Good("Good: memory was $95 after STA $2345");
            else Bad("Bad : memory was not $95 after STA $2345");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after STA $2345");
            else Bad("Bad : PC was not incremented by 3 after STA $2345");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after STA $2345");
            else Bad("Bad : Cycles not incremented by 4 after STA $2345");
            cpu.reset();
            memory[0x1000] = 0x9d;       // STA $2345,X
            memory[0x1001] = 0x45;
            memory[0x1002] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0x79;
            cpu.x = 2;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x2347] == 0x79) Good("Good: memory was $79 after STA $2345,X");
            else Bad("Bad : memory was not $79 after STA $2345,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after STA $2345,X");
            else Bad("Bad : PC was not incremented by 3 after STA $2345,X");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after STA $2345,X");
            else Bad("Bad : Cycles not incremented by 5 after STA $2345,X");
            cpu.reset();
            memory[0x1000] = 0x99;       // STA $2345,Y
            memory[0x1001] = 0x45;
            memory[0x1002] = 0x23;
            cpu.pc = 0x1000;
            cpu.ac = 0x27;
            cpu.y = 4;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x2349] == 0x27) Good("Good: memory was $27 after STA $2345,Y");
            else Bad("Bad : memory was not $27 after STA $2345,Y");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after STA $2345,Y");
            else Bad("Bad : PC was not incremented by 3 after STA $2345,Y");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after STA $2345,Y");
            else Bad("Bad : Cycles not incremented by 5 after STA $2345,Y");
            cpu.reset();
            memory[0x1000] = 0x81;       // STA ($fa,X)
            memory[0x1001] = 0xfa;
            memory[0x00ff] = 0x23;
            memory[0x0000] = 0x78;
            cpu.pc = 0x1000;
            cpu.ac = 0x06;
            cpu.x = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x7823] == 0x06) Good("Good: memory was $06 after STA ($fa,X)");
            else Bad("Bad : memory was not $06 after STA ($fa,X)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after STA ($fa,X)");
            else Bad("Bad : PC was not incremented by 2 after STA ($fa,X)");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after STA ($fa,X)");
            else Bad("Bad : Cycles not incremented by 6 after STA ($fa,X)");
            cpu.reset();
            memory[0x1000] = 0x91;       // STA ($ff),Y
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x23;
            memory[0x0000] = 0x78;
            cpu.pc = 0x1000;
            cpu.ac = 0x69;
            cpu.y = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x7828] == 0x69) Good("Good: memory was $69 after STA ($ff),Y");
            else Bad("Bad : memory was not $69 after STA ($ff),Y");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after STA ($ff),Y");
            else Bad("Bad : PC was not incremented by 2 after STA ($ff),Y");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after STA ($ff),Y");
            else Bad("Bad : Cycles not incremented by 6 after STA ($ff),Y");
            cpu.reset();
            memory[0x1000] = 0x92;       // STA ($ff)
            memory[0x1001] = 0xff;
            memory[0x00ff] = 0x45;
            memory[0x0000] = 0x78;
            cpu.pc = 0x1000;
            cpu.ac = 0xa7;
            cpu.y = 5;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x7845] == 0xa7) Good("Good: memory was $a7 after STA ($ff)");
            else Bad("Bad : memory was not $a7 after STA ($ff)");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after STA ($ff)");
            else Bad("Bad : PC was not incremented by 2 after STA ($ff)");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after STA ($ff)");
            else Bad("Bad : Cycles not incremented by 5 after STA ($ff)");
        }
 
        protected void STXtests()
        {
            mainForm.Debug("");
            mainForm.Debug("STX");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x86;       // STX $14
            memory[0x1001] = 0x14;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0xc3;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x0014] == 0xc3) Good("Good: Memory was correct after STX $14");
            else Bad("Bad : Memory was incorrect affected after STX $14");
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after STX $14");
            else Bad("Bad : AC was affected after STX $14");
            if (cpu.x == 0xc3) Good("Good: X was unaffected after STX $14");
            else Bad("Bad : X was affected after STX $14");
            if (cpu.flags == 0x00) Good("Good: No flags were set after STX $14");
            else Bad("Bad : Flags were set after STX $14");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after STX $14");
            else Bad("Bad : PC was not incremented by 2 after STX $14");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after STX $14");
            else Bad("Bad : Cycles not incremented by 3 after STX $14");
            cpu.reset();
            memory[0x1000] = 0x96;       // STX $14,Y
            memory[0x1001] = 0x14;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0xeb;
            cpu.y = 0x02;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x0016] == 0xeb) Good("Good: Memory was correct after STX $14,Y");
            else Bad("Bad : Memory was incorrect after STX $14,Y");
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after STX $14,Y");
            else Bad("Bad : AC was affected after STX $14,Y");
            if (cpu.x == 0xeb) Good("Good: X was unaffected after STX $14,Y");
            else Bad("Bad : X was affected after STX $14,Y");
            if (cpu.flags == 0xff) Good("Good: No flags were cleared after STX $14,Y");
            else Bad("Bad : Flags were cleared after STX $14,Y");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after STX $14,Y");
            else Bad("Bad : PC was not incremented by 2 after STX $14,Y");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after STX $14,Y");
            else Bad("Bad : Cycles not incremented by 4 after STX $14,Y");
            cpu.reset();
            memory[0x1000] = 0x8e;       // STX $1492
            memory[0x1001] = 0x92;
            memory[0x1002] = 0x14;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0xc6;
            cpu.y = 0x02;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x1492] == 0xc6) Good("Good: Memory was correct after STX $1492");
            else Bad("Bad : Memory was incorrect after STX $1492");
            if (cpu.ac == 0x34) Good("Good: AC was unaffected after STX $1492");
            else Bad("Bad : AC was affected after STX $1492");
            if (cpu.x == 0xc6) Good("Good: X was unaffected after STX $1492");
            else Bad("Bad : X was affected after STX $1492");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after STX $1492");
            else Bad("Bad : PC was not incremented by 3 after STX $1492");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after STX $1492");
            else Bad("Bad : Cycles not incremented by 4 after STX $1492");
        }

        protected void STYtests()
        {
            mainForm.Debug("");
            mainForm.Debug("STY");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x84;       // STY $6
            memory[0x1001] = 0x06;
            memory[0x0006] = 0x00;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0xb8;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x05) Good("Good: AC was unaltered by STY $6");
            else Bad("Bad : AC was altered by STY $6");
            if (memory[0x0006] == 0xb8) Good("Good: Memory was correct after STY $6");
            else Bad("Bad : Memory was not correct after STY $6");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after STY $6");
            else Bad("Bad : PC not incremented by 2 after STY $6");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after STY $6");
            else Bad("Bad : Cycles not incremented by 3 after STY $6");
            if (cpu.flags == 0xff) Good("Good: No flags were cleared by STY $6");
            else Bad("Bad : Flags were cleared by STY $6");
            cpu.reset();
            memory[0x1000] = 0x94;       // STY $fa,X
            memory[0x1001] = 0xfa;
            memory[0x0004] = 0x00;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0xa4;
            cpu.x = 0x0a;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x05) Good("Good: AC was unaltered by STY $fa,X");
            else Bad("Bad : AC was altered by STY $fa,X");
            if (memory[0x0004] == 0xa4) Good("Good: Memory was correct after STY $fa,X");
            else Bad("Bad : Memory was not correct after STY $fa,X");
            if (cpu.pc == 0x1002) Good("Good: PC incremented by 2 after STY $fa,X");
            else Bad("Bad : PC not incremented by 2 after STY $fa,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after STY $fa,X");
            else Bad("Bad : Cycles not incremented by 4 after STY $fa,X");
            if (cpu.flags == 0x00) Good("Good: No flags were set by STY $fa,X");
            else Bad("Bad : Flags were set by STY $fa,X");
            cpu.reset();
            memory[0x1000] = 0x8c;       // STY $8521
            memory[0x1001] = 0x21;
            memory[0x1002] = 0x85;
            memory[0x8521] = 0x00;
            cpu.pc = 0x1000;
            cpu.ac = 0x05;
            cpu.y = 0x8e;
            cpu.x = 0x0a;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x05) Good("Good: AC was unaltered by STY $8521");
            else Bad("Bad : AC was altered by STY $8521");
            if (memory[0x8521] == 0x8e) Good("Good: Memory was correct after STY $8521");
            else Bad("Bad : Memory was not correct after STY $8521");
            if (cpu.pc == 0x1003) Good("Good: PC incremented by 3 after STY $8521");
            else Bad("Bad : PC not incremented by 3 after STY $8521");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after STY $8521");
            else Bad("Bad : Cycles not incremented by 4 after STY $8521");
        }
        
        protected void STZtests()
        {
            mainForm.Debug("");
            mainForm.Debug("STZ");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x64;       // STZ $14
            memory[0x1001] = 0x14;
            memory[0x0014] = 0xff;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0xc3;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x0014] == 0x00) Good("Good: Memory was $0 after STZ $14");
            else Bad("Bad : Memory was not $0 after STZ $14");
            if (cpu.flags == 0x00) Good("Good: No flags were set after STZ $14");
            else Bad("Bad : Flags were set after STZ $14");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after STZ $14");
            else Bad("Bad : PC was not incremented by 2 after STZ $14");
            if (cpu.cycles == 3) Good("Good: Cycles incremented by 3 after STZ $14");
            else Bad("Bad : Cycles not incremented by 3 after STZ $14");
            cpu.reset();
            memory[0x1000] = 0x74;       // STZ $14,X
            memory[0x1001] = 0x14;
            for (var i=0; i<256; i++) memory[i] = 0xff;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0x12;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x0026] == 0x00) Good("Good: Memory was $0 after STZ $14,X");
            else Bad("Bad : Memory was not $0 after STZ $14,X");
            if (cpu.flags == 0xff) Good("Good: No flags were cleared after STZ $14,X");
            else Bad("Bad : Flags were set after STZ $14,X");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after STZ $14,X");
            else Bad("Bad : PC was not incremented by 2 after STZ $14,X");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after STZ $14,X");
            else Bad("Bad : Cycles not incremented by 4 after STZ $14,X");
            for (var i = 0; i < 65536; i++) memory[i] = 0xff;
            cpu.reset();
            memory[0x1000] = 0x9c;       // STZ $1499
            memory[0x1001] = 0x99;
            memory[0x1002] = 0x14;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0x12;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x1499] == 0x00) Good("Good: Memory was $0 after STZ $1499");
            else Bad("Bad : Memory was not $0 after STZ $1499");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after STZ $1499");
            else Bad("Bad : PC was not incremented by 3 after STZ $1499");
            if (cpu.cycles == 4) Good("Good: Cycles incremented by 4 after STZ $1499");
            else Bad("Bad : Cycles not incremented by 4 after STZ $1499");
            for (var i = 0; i < 65536; i++) memory[i] = 0xff;
            cpu.reset();
            memory[0x1000] = 0x9e;       // STZ $1400,X
            memory[0x1001] = 0x00;
            memory[0x1002] = 0x14;
            cpu.pc = 0x1000;
            cpu.ac = 0x34;
            cpu.x = 0x12;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (memory[0x1412] == 0x00) Good("Good: Memory was $0 after STZ $1400,X");
            else Bad("Bad : Memory was not $0 after STZ $1400,X");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after STZ $1400,X");
            else Bad("Bad : PC was not incremented by 3 after STZ $1400,X");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after STZ $1400,X");
            else Bad("Bad : Cycles not incremented by 5 after STZ $1400,X");
        }

        protected void TAXtests()
        {
            mainForm.Debug("");
            mainForm.Debug("TAX");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xaa;       // TAX
            cpu.pc = 0x1000;
            cpu.ac = 0x12;
            cpu.x = 0x31;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.x == 0x12) Good("Good: X was correct after TAX");
            else Bad("Bad : X was not correct after TAX");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after TAX with $12");
            else Bad("Bad : Incorrect flags were cleared after TAX with $12");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after TAX");
            else Bad("Bad : PC was not incremented by 1 after TAX");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after TAX");
            else Bad("Bad : Cycles not incremented by 2 after TAX");
            cpu.reset();
            memory[0x1000] = 0xaa;       // TAX
            cpu.pc = 0x1000;
            cpu.ac = 0xaa;
            cpu.x = 0x00;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set transferring $aa");
            else Bad("Bad : N flag not set transferring $aa");
            cpu.reset();
            memory[0x1000] = 0xaa;       // TAX
            cpu.pc = 0x1000;
            cpu.ac = 0x00;
            cpu.x = 0x01;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set transferring $00");
            else Bad("Bad : Z flag not set transferring $00");
        }
      
        protected void TAYtests()
        {
            mainForm.Debug("");
            mainForm.Debug("TAY");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xa8;       // TAY
            cpu.pc = 0x1000;
            cpu.ac = 0x12;
            cpu.y = 0x31;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.y == 0x12) Good("Good: Y was correct after TAY");
            else Bad("Bad : Y was not correct after TAY");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after TAY with $12");
            else Bad("Bad : Incorrect flags were cleared after TAY with $12");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after TAY");
            else Bad("Bad : PC was not incremented by 1 after TAY");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after TAY");
            else Bad("Bad : Cycles not incremented by 2 after TAY");
            cpu.reset();
            memory[0x1000] = 0xa8;       // TAY
            cpu.pc = 0x1000;
            cpu.ac = 0xaa;
            cpu.y = 0x00;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set transferring $aa");
            else Bad("Bad : N flag not set transferring $aa");
            cpu.reset();
            memory[0x1000] = 0xa8;       // TAY
            cpu.pc = 0x1000;
            cpu.ac = 0x00;
            cpu.y = 0x01;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set transferring $00");
            else Bad("Bad : Z flag not set transferring $00");
        }
        
        protected void TRBtests()
        {
            mainForm.Debug("");
            mainForm.Debug("TRB");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x14;       // TRB $23
            memory[0x1001] = 0x23;
            memory[0x0023] = 0x81;
            cpu.pc = 0x1000;
            cpu.ac = 0x3c;
            cpu.cycles = 0;
            cpu.flags = 0;
            cpu.cycle();
            if (cpu.ac == 0x3c) Good("Good: AC was unaffected by TRB $23");
            else Bad("Bad : AC was affected by TRB $23");
            if (memory[0x0023] == 0x81) Good("Good: Memory had correct value after TRB $23");
            else Bad("Bad : Memory did not have correct value after TRB $23");
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set after TRB $23 with $81 and $3c");
            else Bad("Bad : Z flag not set after TRB $23 with $81 and $3c");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by 2 after TRB $23");
            else Bad("Bad : PC was not incremented by 2 after TRB #$23");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after TRB $23");
            else Bad("Bad : Cycles not incremented by 5 after TRB #$23");
            cpu.reset();
            memory[0x1000] = 0x1c;       // TRB $2355
            memory[0x1001] = 0x55;
            memory[0x1002] = 0x23;
            memory[0x2355] = 0x99;
            cpu.pc = 0x1000;
            cpu.ac = 0x35;
            cpu.cycles = 0;
            cpu.flags = 0xff;
            cpu.cycle();
            if (cpu.ac == 0x35) Good("Good: AC was unaffected by TRB $2355");
            else Bad("Bad : AC was affected by TRB $2355");
            if (memory[0x2355] == 0x88) Good("Good: Memory had correct value after TRB $2355");
            else Bad("Bad : Memory did not have correct value after TRB $2355");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0x00) Good("Good: Z flag cleared after TRB $2355 with $99 and $35");
            else Bad("Bad : Z flag not cleared after TRB $2355 with $99 and $35");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after TRB $2355");
            else Bad("Bad : PC was not incremented by 3 after TRB #$2355");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after TRB $2355");
            else Bad("Bad : Cycles not incremented by 6 after TRB #$2355");
        }

        protected void TSBtests()
        {
            mainForm.Debug("");
            mainForm.Debug("TSB");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x04;       // TSB $23
            memory[0x1001] = 0x23;
            memory[0x0023] = 0x81;
            cpu.pc = 0x1000;
            cpu.ac = 0x3c;
            cpu.cycles = 0;
            cpu.flags = 0;
            cpu.cycle();
            if (cpu.ac == 0x3c) Good("Good: AC was unaffected by TSB $23");
            else Bad("Bad : AC was affected by TSB $23");
            if (memory[0x0023] == 0xbd) Good("Good: Memory had correct value after TSB $23");
            else Bad("Bad : Memory did not have correct value after TSB $23");
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set after TSB $23 with $81 and $3c");
            else Bad("Bad : Z flag not set after TSB $23 with $81 and $3c");
            if (cpu.pc == 0x1002) Good("Good: PC was incremented by two after TSB $23");
            else Bad("Bad : PC was not incremented by two after TSB #$23");
            if (cpu.cycles == 5) Good("Good: Cycles incremented by 5 after TSB $23");
            else Bad("Bad : Cycles not incremented by 5 after TSB #$23");
            cpu.reset();
            memory[0x1000] = 0x0c;       // TSB $2355
            memory[0x1001] = 0x55;
            memory[0x1002] = 0x23;
            memory[0x2355] = 0x99;
            cpu.pc = 0x1000;
            cpu.ac = 0x35;
            cpu.cycles = 0;
            cpu.flags = 0xff;
            cpu.cycle();
            if (cpu.ac == 0x35) Good("Good: AC was unaffected by TSB $2355");
            else Bad("Bad : AC was affected by TSB $2355");
            if (memory[0x2355] == 0xbd) Good("Good: Memory had correct value after TSB $2355");
            else Bad("Bad : Memory did not have correct value after TSB $2355");
            if ((cpu.flags & Cpu65c02.FLAG_Z) == 0x00) Good("Good: Z flag cleared after TSB $2355 with $99 and $35");
            else Bad("Bad : Z flag not cleared after TSB $2355 with $99 and $35");
            if (cpu.pc == 0x1003) Good("Good: PC was incremented by 3 after TSB $2355");
            else Bad("Bad : PC was not incremented by 3 after TSB #$2355");
            if (cpu.cycles == 6) Good("Good: Cycles incremented by 6 after TSB $2355");
            else Bad("Bad : Cycles not incremented by 6 after TSB #$2355");
        }

        protected void TSXtests()
        {
            mainForm.Debug("");
            mainForm.Debug("TSX");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0xba;       // TSX
            cpu.pc = 0x1000;
            cpu.x = 0x12;
            cpu.sp = 0x3f;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.x == 0x3f) Good("Good: X was correct after TSX");
            else Bad("Bad : X was not correct after TSX");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after TSX with $3f");
            else Bad("Bad : Incorrect flags were cleared after TSX with $3f");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after TSX");
            else Bad("Bad : PC was not incremented by 1 after TSX");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after TSX");
            else Bad("Bad : Cycles not incremented by 2 after TSX");
            cpu.reset();
            memory[0x1000] = 0xba;       // TSX
            cpu.pc = 0x1000;
            cpu.x = 0x00;
            cpu.sp = 0xaa;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set transferring $aa");
            else Bad("Bad : N flag not set transferring $aa");
            cpu.reset();
            memory[0x1000] = 0xba;       // TSX
            cpu.pc = 0x1000;
            cpu.x = 0x01;
            cpu.sp = 0x00;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set transferring $00");
            else Bad("Bad : Z flag not set transferring $00");
        }

        protected void TXStests()
        {
            mainForm.Debug("");
            mainForm.Debug("TXS");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x9a;       // TXS
            cpu.pc = 0x1000;
            cpu.sp = 0xee;
            cpu.x = 0x36;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.sp == 0x36) Good("Good: X was correctly transferred to S");
            else Bad("Bad : X was not correctly transferred to S");
            if (cpu.flags == 0xff) Good("Good: No flags cleared on TXS");
            else Bad("Bad : Flags were cleared on TXS");
            if (cpu.pc == 0x1001) Good("Good: PC is correct after TXS");
            else Bad("Bad : PC is not correct after TXS");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after TXS");
            else Bad("Bad : Cycles not incremented by 2 after TXS");
            cpu.reset();
            memory[0x1000] = 0x9a;       // TXS
            cpu.pc = 0x1000;
            cpu.sp = 0xee;
            cpu.x = 0x8a;
            cpu.flags = 0x00;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.flags == 0x00) Good("Good: No flags set on TXS");
            else Bad("Bad : Flags were set on TXS");
        }

        protected void TXAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("TXA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x8a;       // TXA
            cpu.pc = 0x1000;
            cpu.ac = 0x12;
            cpu.x = 0x3c;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x3c) Good("Good: A was correct after TXA");
            else Bad("Bad : A was not correct after TXA");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after TXA with $31");
            else Bad("Bad : Incorrect flags were cleared after TXA with $31");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after TXA");
            else Bad("Bad : PC was not incremented by 1 after TXA");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after TXA");
            else Bad("Bad : Cycles not incremented by 2 after TXA");
            cpu.reset();
            memory[0x1000] = 0x8a;       // TXA
            cpu.pc = 0x1000;
            cpu.ac = 0x00;
            cpu.x = 0xaa;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set transferring $aa");
            else Bad("Bad : N flag not set transferring $aa");
            cpu.reset();
            memory[0x1000] = 0x8a;       // TXA
            cpu.pc = 0x1000;
            cpu.ac = 0x01;
            cpu.x = 0x00;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set transferring $00");
            else Bad("Bad : Z flag not set transferring $00");
        }

        protected void TYAtests()
        {
            mainForm.Debug("");
            mainForm.Debug("TYA");
            mainForm.Debug("---");
            cpu.halted = false;
            cpu.reset();
            memory[0x1000] = 0x98;       // TYA
            cpu.pc = 0x1000;
            cpu.ac = 0x12;
            cpu.y = 0x31;
            cpu.flags = 0xff;
            cpu.cycles = 0;
            cpu.cycle();
            if (cpu.ac == 0x31) Good("Good: A was correct after TYA");
            else Bad("Bad : A was not correct after TYA");
            if (cpu.flags == 0x7d) Good("Good: Only flags N and Z were cleared after TYA with $31");
            else Bad("Bad : Incorrect flags were cleared after TYA with $31");
            if (cpu.pc == 0x1001) Good("Good: PC was incremented by 1 after TYA");
            else Bad("Bad : PC was not incremented by 1 after TYA");
            if (cpu.cycles == 2) Good("Good: Cycles incremented by 2 after TYA");
            else Bad("Bad : Cycles not incremented by 2 after TYA");
            cpu.reset();
            memory[0x1000] = 0x98;       // TYA
            cpu.pc = 0x1000;
            cpu.ac = 0x00;
            cpu.y = 0xaa;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_N) != 0x00) Good("Good: N flag set transferring $aa");
            else Bad("Bad : N flag not set transferring $aa");
            cpu.reset();
            memory[0x1000] = 0x98;       // TYA
            cpu.pc = 0x1000;
            cpu.ac = 0x01;
            cpu.y = 0x00;
            cpu.flags = 0;
            cpu.cycles = 0;
            cpu.cycle();
            if ((cpu.flags & Cpu65c02.FLAG_Z) != 0x00) Good("Good: Z flag set transferring $00");
            else Bad("Bad : Z flag not set transferring $00");
        }

        public void Cycle()
        {
        }

        public void Reset()
        {
        }

        public void Run()
        {
            mainForm.Debug("Starting CPU Diagnostics");
            ResetTests();
            LogicalFlagTests();
            ADCtests();
            ANDtests();
            ASLtests();
            BCCtests();
            BCStests();
            BEQtests();
            BITtests();
            BMItests();
            BNEtests();
            BPLtests();
            BRAtests();
            BRKtests();
            BVCtests();
            BVStests();
            CLCtests();
            CLDtests();
            CLItests();
            CLVtests();
            CPXtests();
            CPYtests();
            DEAtests();
            DECtests();
            DEXtests();
            DEYtests();
            EORtests();
            INAtests();
            INCtests();
            INXtests();
            INYtests();
            JMPtests();
            JSRtests();
            LDAtests();
            LDXtests();
            LDYtests();
            LSRtests();
            NOPtests();
            ORAtests();
            PHAtests();
            PHPtests();
            PHXtests();
            PHYtests();
            PLAtests();
            PLPtests();
            PLXtests();
            PLYtests();
            ROLtests();
            RORtests();
            RTItests();
            RTStests();
            SBCtests();
            SECtests();
            SEDtests();
            SEItests();
            STAtests();
            STXtests();
            STYtests();
            STZtests();
            TAXtests();
            TAYtests();
            TRBtests();
            TSBtests();
            TSXtests();
            TXAtests();
            TXStests();
            TYAtests();
            mainForm.Debug("");
            mainForm.Debug("Good Tests: " + good.ToString());
            mainForm.Debug("Bad Tests : " + bad.ToString());
        }
    }
}
