using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimKimI
{
    public class Assembler
    {
            //            imp    #      zp     zp,x   zp,y   abs    abs,x  abs,y  (zp,x) (zp),y (zp)   (abs)  Acc    Rel    (abs,x)
        protected const int AM_IMPLIED = 1;
        protected const int AM_IMMEDIATE = 2;
        protected const int AM_ZEROPAGE = 3;
        protected const int AM_ZEROPAGEX = 4;
        protected const int AM_ZEROPAGEY = 5;
        protected const int AM_ABSOLUTE = 6;
        protected const int AM_ABSOLUTEX = 7;
        protected const int AM_ABSOLUTEY = 8;
        protected const int AM_ZEROPAGEINDIRECTX = 9;
        protected const int AM_ZEROPAGEINDIRECTY = 10;
        protected const int AM_ZEROPAGEINDIRECT = 11;
        protected const int AM_ABSOLUTEINDIRECT = 12;
        protected const int AM_ACCUMULATOR = 13;
        protected const int AM_RELATIVE = 14;
        protected const int AM_ABSOLUTEINDIRECTX = 15;

        protected List<List<object>> opcodes;
        protected List<List<object>> labels;
        protected List<String> tokens;
        protected int pass;
        protected int address;
        public String results;
        public int errors;
        public String outputFile;
        public char outputMode;
        public Boolean showSymbolTable;
        public Boolean crossReference;
        public Boolean showMemoryMap;
        protected int lineNumber;
        protected String outputLine;
        protected Boolean labelFound;
        protected String lastLabel;
        protected Boolean dsectMode;
        protected int linesAssembled;
        protected int bytesAssembled;
        protected IntelHex hexOutput;
        protected int startAddress;
        protected byte[] memory;
        protected char[] memoryMap;
        protected int minAddress;
        protected int maxAddress;

        public Assembler()
        {
            init();
            outputMode = 'M';
            hexOutput = new IntelHex();
            showSymbolTable = false;
            crossReference = false;
            showMemoryMap = false;
            memoryMap = new char[65536];
        }

        protected void opcode(String o,int a1,int a2,int a3,int a4,int a5,int a6,int a7,int a8,int a9, int a10,int a11,int a12,int a13,int a14,int a15)
        {
            List<object> temp;
            temp = new List<object>();
            temp.Add(o);
            temp.Add(a1);
            temp.Add(a2);
            temp.Add(a3);
            temp.Add(a4);
            temp.Add(a5);
            temp.Add(a6);
            temp.Add(a7);
            temp.Add(a8);
            temp.Add(a9);
            temp.Add(a10);
            temp.Add(a11);
            temp.Add(a12);
            temp.Add(a13);
            temp.Add(a14);
            temp.Add(a15);
            opcodes.Add(temp);
        }

        protected void init()
        {
            opcodes = new List<List<object>>();
            //            imp    #      zp     zp,x   zp,y   abs    abs,x  abs,y  (zp,x) (zp),y (zp)   (abs)  Acc    Rel    (abs,x)
            opcode("ADC", 0xff, 0x69, 0x65, 0x75, 0xff, 0x6d, 0x7d, 0x79, 0x61, 0x71, 0x72, 0xff, 0xff, 0xff, 0xff);
            opcode("AND", 0xff, 0x29, 0x25, 0x35, 0xff, 0x2d, 0x3d, 0x39, 0x21, 0x31, 0x32, 0xff, 0xff, 0xff, 0xff);
            opcode("ASL", 0xff, 0xff, 0x06, 0x16, 0xff, 0x0e, 0x1e, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0a, 0xff, 0xff);
            opcode("BCC", 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x90, 0xff);
            opcode("BCS", 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xb0, 0xff);
            opcode("BEQ", 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xf0, 0xff);
            opcode("BIT", 0xff, 0x89, 0x24, 0x34, 0xff, 0x2c, 0x3c, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("BMI", 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x30, 0xff);
            opcode("BNE", 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xd0, 0xff);
            opcode("BPL", 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x10, 0xff);
            opcode("BRA", 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x80, 0xff);
            opcode("BRK", 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("BVC", 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x50, 0xff);
            opcode("BVS", 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x70, 0xff);
            opcode("CLC", 0x18, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("CLD", 0xd8, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("CLI", 0x58, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("CLV", 0xb8, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("CMP", 0xff, 0xc9, 0xc5, 0xd5, 0xff, 0xcd, 0xdd, 0xd9, 0xc1, 0xd1, 0xd2, 0xff, 0xff, 0xff, 0xff);
            opcode("CPX", 0xff, 0xe0, 0xe4, 0xff, 0xff, 0xec, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("CPY", 0xff, 0xc0, 0xc4, 0xff, 0xff, 0xcc, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("DEA", 0x3a, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("DEC", 0xff, 0xff, 0xc6, 0xd6, 0xff, 0xce, 0xde, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("DEX", 0xca, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("DEY", 0x88, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("EOR", 0xff, 0x49, 0x45, 0x55, 0xff, 0x4d, 0x5d, 0x59, 0x41, 0x51, 0x52, 0xff, 0xff, 0xff, 0xff);
            opcode("INA", 0x1a, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("INC", 0xff, 0xff, 0xe6, 0xf6, 0xff, 0xee, 0xfe, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("INX", 0xe8, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("INY", 0xc8, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("JMP", 0xff, 0xff, 0xff, 0xff, 0xff, 0x4c, 0xff, 0xff, 0xff, 0xff, 0xff, 0x6c, 0xff, 0xff, 0x7c);
            opcode("JSR", 0xff, 0xff, 0xff, 0xff, 0xff, 0x20, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("LDA", 0xff, 0xa9, 0xa5, 0xb5, 0xff, 0xad, 0xbd, 0xb9, 0xa1, 0xb1, 0xb2, 0xff, 0xff, 0xff, 0xff);
            opcode("LDX", 0xff, 0xa2, 0xa6, 0xff, 0xb6, 0xae, 0xff, 0xbe, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("LDY", 0xff, 0xa0, 0xa4, 0xb4, 0xff, 0xac, 0xbc, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("LSR", 0xff, 0xff, 0x46, 0x56, 0xff, 0x4e, 0x5e, 0xff, 0xff, 0xff, 0xff, 0xff, 0x4A, 0xff, 0xff);
            opcode("NOP", 0xea, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("ORA", 0xff, 0x09, 0x05, 0x15, 0xff, 0x0d, 0x1d, 0x19, 0x01, 0x11, 0x12, 0xff, 0xff, 0xff, 0xff);
            opcode("PHA", 0x48, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("PHX", 0xda, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("PHY", 0x5a, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("PLA", 0x68, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("PLX", 0xfa, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("PLY", 0x7a, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("ROL", 0xff, 0xff, 0x26, 0x36, 0xff, 0x2e, 0x3e, 0xff, 0xff, 0xff, 0xff, 0xff, 0x2a, 0xff, 0xff);
            opcode("ROR", 0xff, 0xff, 0x66, 0x76, 0xff, 0x6e, 0x7e, 0xff, 0xff, 0xff, 0xff, 0xff, 0x6a, 0xff, 0xff);
            opcode("RTI", 0x40, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("RTS", 0x60, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("SBC", 0xff, 0xe9, 0xe5, 0xf5, 0xff, 0xed, 0xfd, 0xf9, 0xe1, 0xf1, 0xf2, 0xff, 0xff, 0xff, 0xff);
            opcode("SEC", 0x38, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("SED", 0xf8, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("SEI", 0x78, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("STA", 0xff, 0xff, 0x85, 0x95, 0xff, 0x8d, 0x9d, 0x99, 0x81, 0x91, 0x92, 0xff, 0xff, 0xff, 0xff);
            opcode("STX", 0xff, 0xff, 0x86, 0xff, 0x96, 0x8e, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("STY", 0xff, 0xff, 0x84, 0x94, 0xff, 0x8c, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("STZ", 0xff, 0xff, 0x64, 0x74, 0xff, 0x9c, 0x9e, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("TAX", 0xaa, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("TAY", 0xa8, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("TRB", 0xff, 0xff, 0x14, 0xff, 0xff, 0x1c, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("TSB", 0xff, 0xff, 0x04, 0xff, 0xff, 0x0c, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("TSX", 0xba, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("TXA", 0x8a, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("TXS", 0x9a, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("TYA", 0x98, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("EQU", 0xaf, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("=", 0xaf, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("ORG", 0xaf, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("DSECT", 0xaf, 0x02, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("DEND", 0xaf, 0x03, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("DS", 0xaf, 0x04, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("DFB", 0xaf, 0x05, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("DFW", 0xaf, 0x06, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
            opcode("EXEC", 0xaf, 0x07, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
        }

        protected void error(String msg)
        {
            if (pass == 2)
            {
                errors++;
                results += msg + "\r\n";
            }
            
        }

        protected List<object> findOpcode(String opcode)
        {
            foreach (var o in opcodes)
            {
                if (((String)o[0]).ToUpper().Equals(opcode.ToUpper())) return o;
            }
            return null;
        }

        protected void sortLabels()
        {
            List<object> temp;
            Boolean flag;
            flag = true;
            while (flag)
            {
                flag = false;
                for (var i = 0; i < labels.Count - 1; i++)
                {
                    if (((String)labels[i][0]).CompareTo(((String)labels[i + 1][0])) > 0)
                    {
                        temp = labels[i];
                        labels[i] = labels[i + 1];
                        labels[i + 1] = temp;
                        flag = true;
                    }
                }
            }
        }

        protected Boolean addLabel(String label, int value)
        {
            List<object> newEntry;
            lastLabel = label;
            foreach (var entry in labels)
            {
                if (label.ToUpper().Equals(((String)entry[0]).ToUpper()))
                {
                    entry[1] = value;
                    return true;
                }
            }
            newEntry = new List<object>();
            newEntry.Add(label);
            newEntry.Add(value);
            newEntry.Add(lineNumber);
            labels.Add(newEntry);
            return false;
        }

        protected int findLabel(String label)
        {
            labelFound = false;
            foreach (var entry in labels)
            {
                if (label.ToUpper().Equals(((String)entry[0]).ToUpper()))
                {
                    labelFound = true;
                    if (pass == 2) entry.Add(lineNumber);
                    return (int)entry[1];
                }
            }
            return -1;
        }

        protected void tokenize(String input,String separators)
        {
            String current;
            tokens = new List<String>();
            if (input.Length < 1) return;
            if (input[0] == 32 || input[0] == 9)
            {
                tokens.Add("");
            }
            input = input.Trim();
            current = "";
            while (input.Length > 0)
            {
                if (separators.IndexOf(input[0]) >= 0)
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current);
                        current = "";
                    }
                    if (input[0] == ';')
                    {
                        input = " ";
                    }
                    else if (input[0] !=' ' && input[0] != '\t') tokens.Add(input[0].ToString());
                }
                else
                {
                    current = current + input[0].ToString();
                }
                input = input.Substring(1);
            }
            if (current.Length > 0) tokens.Add(current);
        }

        protected Boolean isNumeric(String value)
        {
            foreach (var chr in value)
            {
                if (chr < '0') return false;
                if (chr > 'f') return false;
                if (chr > '9' && chr < 'A') return false;
                if (chr > 'F' && chr < 'a') return false;
            }
            return true;
        }

        protected Boolean isDecimal(String value)
        {
            foreach (var chr in value)
            {
                if (chr < '0' && chr != '-') return false;
                if (chr > '9' && chr != '-') return false;
            }
            return true;
        }

        protected String hexToDecimal(String input)
        {
            int value;
            value = 0;
            while (input.Length > 0)
            {
                if (input[0] >= '0' && input[0] <= '9') value = (value << 4) + input[0] - '0';
                if (input[0] >= 'A' && input[0] <= 'F') value = (value << 4) + input[0] - 'A' + 10;
                if (input[0] >= 'a' && input[0] <= 'f') value = (value << 4) + input[0] - 'a' + 10;
                input = input.Substring(1);
            }
            return value.ToString();
        }

        protected String binaryToDecimal(String input)
        {
            int value;
            value = 0;
            while (input.Length > 0)
            {
                if (input[0] == '0') value = (value << 1) + 0;
                if (input[0] == '1') value = (value << 1) + 1;
                input = input.Substring(1);
            }
            return value.ToString();
        }

        protected Boolean isLabel(String input)
        {
            if (input.Length < 1) return false;
            if (input.ToUpper().Equals("A")) return false;
            if (input.ToUpper().Equals("X")) return false;
            if (input.ToUpper().Equals("Y")) return false;
            foreach (var chr in input)
            {
                if (chr >= 'a' && chr <= 'z') return true;
                if (chr >= 'A' && chr <= 'Z') return true;
                if (chr == '_') return true;
            }
            return false;
        }

        protected void reduce()
        {
            int i;
            int v1, v2;
            i = 2;
            // ****************************
            // ***** Data conversions *****
            // ****************************
            while (i < tokens.Count)
            {
                if (tokens[i].Equals("*"))
                {
                    v1 = address;
                    tokens[i] = v1.ToString();
                }
                else if (tokens[i].Equals("'"))
                {
                    if (i + 2 < tokens.Count && tokens[i + 2].Equals("'") && tokens[i + 1].Length == 1)
                    {
                        v1 = tokens[i + 1][0];
                        tokens[i] = v1.ToString();
                        tokens.RemoveAt(i + 1);
                        tokens.RemoveAt(i + 1);
                    }

                }
                else if (isLabel(tokens[i]) && (i == 0 || !tokens[i - 1].Equals("$")))
                {
                    v1 = findLabel(tokens[i]);
                    if (labelFound)
                    {
                        tokens[i] = v1.ToString();
                    }
                    else
                    {
                        v1 = 0x7fff;
                        if (pass == 2) error("Error: Label not defined: " + tokens[i]);
                        tokens[i] = v1.ToString();
                    }
                }
                else if (tokens[i].Equals("$") && i + 1 < tokens.Count)
                {
                    if (isNumeric(tokens[i + 1]))
                    {
                        tokens[i + 1] = hexToDecimal(tokens[i + 1]);
                        tokens.RemoveAt(i);
                    }
                }
                else if (tokens[i].Equals("%") && i + 1 < tokens.Count)
                {
                    if (isNumeric(tokens[i + 1]))
                    {
                        tokens[i + 1] = binaryToDecimal(tokens[i + 1]);
                        tokens.RemoveAt(i);
                    }
                }
                i++;
            }
            // *****************************************
            // ***** Unary operators high priority *****
            // *****************************************
            i = 0;
            while (i < tokens.Count)
            {
                if (tokens[i].Equals("-") && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i + 1]) && (i == 0 || !isDecimal(tokens[i-1])))
                    {
                        tokens[i + 1] = (-Convert.ToInt32(tokens[i+1])).ToString();
                        tokens.RemoveAt(i);
                    }
                }
                else if (tokens[i].Equals("~") && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i + 1]) && (i == 0 || !isDecimal(tokens[i - 1])))
                    {
                        v1 = Convert.ToInt32(tokens[i + 1]);
                        tokens[i + 1] = (v1 ^ 0xffff).ToString();
                        tokens.RemoveAt(i);
                    }
                }
                i++;
            }
            // ***************
            // ***** * / *****
            // ***************
            i = 0;
            while (i < tokens.Count)
            {
                if (tokens[i].Equals("*") && i > 0 && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i - 1]) && isDecimal(tokens[i + 1]))
                    {
                        v1 = Convert.ToInt32(tokens[i - 1]);
                        v2 = Convert.ToInt32(tokens[i + 1]);
                        v1 *= v2;
                        tokens[i - 1] = v1.ToString();
                        tokens.RemoveAt(i);
                        tokens.RemoveAt(i);
                        i--;
                    }
                }
                else if (tokens[i].Equals("/") && i > 0 && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i - 1]) && isDecimal(tokens[i + 1]))
                    {
                        v1 = Convert.ToInt32(tokens[i - 1]);
                        v2 = Convert.ToInt32(tokens[i + 1]);
                        v1 /= v2;
                        tokens[i - 1] = v1.ToString();
                        tokens.RemoveAt(i);
                        tokens.RemoveAt(i);
                        i--;
                    }
                }
                i++;
            }
            // ***************
            // ***** + - *****
            // ***************
            i = 0;
            while (i < tokens.Count)
            {
                if (tokens[i].Equals("+") && i > 0 && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i - 1]) && isDecimal(tokens[i + 1]))
                    {
                        v1 = Convert.ToInt32(tokens[i - 1]);
                        v2 = Convert.ToInt32(tokens[i + 1]);
                        v1 += v2;
                        tokens[i - 1] = v1.ToString();
                        tokens.RemoveAt(i);
                        tokens.RemoveAt(i);
                        i--;
                    }
                }
                else if (tokens[i].Equals("-") && i > 0 && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i - 1]) && isDecimal(tokens[i + 1]))
                    {
                        v1 = Convert.ToInt32(tokens[i - 1]);
                        v2 = Convert.ToInt32(tokens[i + 1]);
                        v1 -= v2;
                        tokens[i - 1] = v1.ToString();
                        tokens.RemoveAt(i);
                        tokens.RemoveAt(i);
                        i--;
                    }
                }
                i++;
            }
            // *******************
            // ***** < > = # *****
            // *******************
            i = 0;
            while (i < tokens.Count)
            {
                if (tokens[i].Equals("<") && i > 0 && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i - 1]) && isDecimal(tokens[i + 1]))
                    {
                        v1 = Convert.ToInt32(tokens[i - 1]);
                        v2 = Convert.ToInt32(tokens[i + 1]);
                        v1 = (v1 < v2) ? -1 : 0;
                        tokens[i - 1] = v1.ToString();
                        tokens.RemoveAt(i);
                        tokens.RemoveAt(i);
                        i--;
                    }
                }
                else if (tokens[i].Equals(">") && i > 0 && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i - 1]) && isDecimal(tokens[i + 1]))
                    {
                        v1 = Convert.ToInt32(tokens[i - 1]);
                        v2 = Convert.ToInt32(tokens[i + 1]);
                        v1 = (v1 > v2) ? -1 : 0;
                        tokens[i - 1] = v1.ToString();
                        tokens.RemoveAt(i);
                        tokens.RemoveAt(i);
                        i--;
                    }
                }
                else if (tokens[i].Equals("=") && i > 0 && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i - 1]) && isDecimal(tokens[i + 1]))
                    {
                        v1 = Convert.ToInt32(tokens[i - 1]);
                        v2 = Convert.ToInt32(tokens[i + 1]);
                        v1 = (v1 == v2) ? -1 : 0;
                        tokens[i - 1] = v1.ToString();
                        tokens.RemoveAt(i);
                        tokens.RemoveAt(i);
                        i--;
                    }
                }
                else if (tokens[i].Equals("#") && i > 0 && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i - 1]) && isDecimal(tokens[i + 1]))
                    {
                        v1 = Convert.ToInt32(tokens[i - 1]);
                        v2 = Convert.ToInt32(tokens[i + 1]);
                        v1 = (v1 != v2) ? -1 : 0;
                        tokens[i - 1] = v1.ToString();
                        tokens.RemoveAt(i);
                        tokens.RemoveAt(i);
                        i--;
                    }
                }
                i++;
            }
            // ****************************************
            // ***** Unary operators low priority *****
            // ****************************************
            i = 0;
            while (i < tokens.Count)
            {
                if (tokens[i].Equals("<") && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i + 1]) && (i == 0 || !isDecimal(tokens[i - 1])))
                    {
                        v1 = Convert.ToInt32(tokens[i + 1]);
                        tokens[i + 1] = ((v1 >> 8) & 0xff).ToString();
                        tokens.RemoveAt(i);
                    }
                }
                else if (tokens[i].Equals(">") && i + 1 < tokens.Count)
                {
                    if (isDecimal(tokens[i + 1]) && (i == 0 || !isDecimal(tokens[i - 1])))
                    {
                        v1 = Convert.ToInt32(tokens[i + 1]);
                        tokens[i + 1] = (v1 & 0xff).ToString();
                        tokens.RemoveAt(i);
                    }
                }
                i++;
            }
        }

        protected void writeByte(byte value)
        {
            outputLine += value.ToString("x2") + " ";
            if (!dsectMode)
            {
                if (pass == 2)
                {
                    if (outputMode == 'F') hexOutput.Add(address, value);
                    if (outputMode == 'M') memory[address] = value;
                }
                if (address < minAddress) minAddress = address;
                if (address > maxAddress) maxAddress = address;
                if (pass == 2)
                {
                    if (memoryMap[address] == '*') error("Error: Overlap in output");
                    memoryMap[address] = '*';
                }
            }
            bytesAssembled++;
            address++;
        }

        protected void assembleOpcode(String opcode, int am, String arg)
        {
            List<object> opcodeEntry;
            int value;
            int op;
            opcodeEntry = findOpcode(opcode);
            if (opcodeEntry == null)
            {
                error("Error: Invalid opcode: " + opcode);
                return;
            }
            try
            {
                value = Convert.ToInt32(arg);
            }
            catch
            {
                value = 0;
            }
            if ((int)opcodeEntry[1] == 0xaf)
            {
                switch ((int)opcodeEntry[2])
                {
                    case 0:                                                    // EQU
                        addLabel(lastLabel, value);
                        break;
                    case 1:                                                    // ORG
                        address = value;
                        break;
                    case 2:                                                    // DSECT
                        dsectMode = true;
                        break;
                    case 3:                                                    // DEND
                        dsectMode = false;
                        break;
                    case 4:                                                    // DS
                        address += value;
                        break;
                    case 7:                                                    // EXEC
                        startAddress = value;
                        break;
                }
                return;
            }
            if (am < 1 || am > 15) am = 1;
            if (am == AM_IMMEDIATE && (int)opcodeEntry[AM_RELATIVE] != 0xff)
            {
                am = AM_RELATIVE;
                if (pass == 2 && value > 127) error("Error: Branch out of range");
                if (pass == 2 && value < -128) error("Error: Branch out of range");
            }
            if (am == AM_ABSOLUTE && (int)opcodeEntry[AM_RELATIVE] != 0xff)
            {
                am = AM_RELATIVE;
                value = value - (address + 2);
                if (pass == 2 && value > 127) error("Error: Branch out of range");
                if (pass == 2 && value < -128) error("Error: Branch out of range");
            }
            if (am == AM_ABSOLUTE && value < 256 && (int)opcodeEntry[AM_ZEROPAGE] != 0xff) am = AM_ZEROPAGE;
            if (am == AM_ABSOLUTEX && value < 256 && (int)opcodeEntry[AM_ZEROPAGEX] != 0xff) am = AM_ZEROPAGEX;
            if (am == AM_ABSOLUTEY && value < 256 && (int)opcodeEntry[AM_ZEROPAGEY] != 0xff) am = AM_ZEROPAGEY;
            if (am == AM_ABSOLUTEINDIRECTX && value < 256 && (int)opcodeEntry[AM_ZEROPAGEINDIRECTX] != 0xff) am = AM_ZEROPAGEINDIRECTX;
            if (am == AM_ABSOLUTEINDIRECT && value < 256 && (int)opcodeEntry[AM_ZEROPAGEINDIRECT] != 0xff) am = AM_ZEROPAGEINDIRECT;
            op = (int)opcodeEntry[am];
            if (op == 0xff)
            {
                error("Error: Invalid address mode");
            }
            writeByte((byte)op);
            if ((am >= 2 && am <= 5) || (am >= 9 && am <=11) || am == 14)
            {
                writeByte((byte)(value & 0xff));
            }
            if ((am >= 6 && am <= 8) || am == 12 || am == 15)
            {
                writeByte((byte)(value & 0xff));
                writeByte((byte)((value >> 8) & 0xff));
            }
        }

        protected void assembleLine(String line)
        {
            String label;
            String opcode;
            Boolean result;
            int addressMode;
            String arg;
            int i;
            int value;
            linesAssembled++;
            outputLine = address.ToString("x4") + " ";
            tokenize(line,"!@#$%^&*()_+`'-=<>,./?[]{}\\| \t;:~");
            reduce();
            if (tokens.Count < 1) return;
            if (tokens[0].Length > 0)
            {
                label = tokens[0];
                result = addLabel(label, address);
                if (pass == 1 && result)
                {
                    errors++;
                    results += "Error: Line " + lineNumber.ToString() + " Multiply defined label: " + label + "\r\n";
                }
            }
            if (tokens.Count < 2) return;
            opcode = tokens[1];
            addressMode = 0;
            arg = "";
            if (opcode.ToUpper().Equals("DFB") || opcode.ToUpper().Equals("DFW"))
            {
                i = 2;
                while (i < tokens.Count)
                {
                    if (isDecimal(tokens[i]))
                    {
                        value = Convert.ToInt32(tokens[i]);
                        writeByte((byte)(value & 0xff));
                        if (opcode.ToUpper().Equals("DFW")) writeByte((byte)((value >> 8) & 0xff));
                        i++;
                    }
                    else if (tokens[i].Equals(",")) i++;
                    else
                    {
                        error("Error: Malformed data line");
                        i++;
                    }
                }
                if (pass == 2)
                {
                    while (outputLine.Length < 15) outputLine += " ";
                    outputLine += line;
                    results += outputLine + "\r\n";
                }
                return;
            }
            else if (tokens.Count == 2)
            {
                addressMode = AM_IMPLIED;
                arg = "";
            }
            else if (tokens.Count == 3)
            {
                if (tokens[2].ToUpper().Equals("A"))
                {
                    addressMode = AM_ACCUMULATOR;
                }
                else
                {
                    arg = tokens[2];
                    addressMode = AM_ABSOLUTE;
                }
            }
            else if (tokens.Count == 4)
            {
                if (tokens[2].Equals("#"))
                {
                    arg = tokens[3];
                    addressMode = AM_IMMEDIATE;
                }
                else
                {
                    error("Error: Syntax error");
                }
            }
            else if (tokens.Count == 5)
            {
                if (tokens[3].Equals(",") && tokens[4].ToUpper().Equals("X"))
                {
                    arg = tokens[2];
                    addressMode = AM_ABSOLUTEX;
                }
                else if (tokens[3].Equals(",") && tokens[4].ToUpper().Equals("Y"))
                {
                    arg = tokens[2];
                    addressMode = AM_ABSOLUTEY;
                }
                else if (tokens[2].Equals("(") && tokens[4].ToUpper().Equals(")"))
                {
                    arg = tokens[3];
                    addressMode = AM_ABSOLUTEINDIRECT;
                }
                else
                {
                    error("Error: Syntax error");
                }
            }
            else if (tokens.Count == 7)
            {
                if (tokens[2].Equals("(") && tokens[4].Equals(",") && tokens[5].ToUpper().Equals("X") && tokens[6].Equals(")"))
                {
                    arg = tokens[3];
                    addressMode = AM_ABSOLUTEINDIRECTX;
                }
                else if (tokens[2].Equals("(") && tokens[4].Equals(")") && tokens[5].Equals(",") && tokens[6].ToUpper().Equals("Y"))
                {
                    arg = tokens[3];
                    addressMode = AM_ZEROPAGEINDIRECTY;
                }
                else
                {
                    error("Error: Syntax error");
                }
            }
            else
            {
                error("Error: Syntax error");
            }
            //            results += "opcode: " + opcode + ", AM: " + addressMode.ToString() + " Arg: " + arg.ToString() + "\r\n";
            assembleOpcode(opcode, addressMode, arg);
            if (pass == 2)
            {
                while (outputLine.Length < 15) outputLine += " ";
                outputLine += line;
                results += outputLine + "\r\n";
            }
        }

        protected String expandTabs(String input)
        {
            String ret;
            ret = "";
            foreach (var c in input)
            {
                if (c == 9)
                {
                    ret += " ";
                    while (ret.Length % 8 != 0) ret += " ";
                }
                else ret += c;
            }
            return ret;
        }

        protected void assemblyPass(int n,String[] lines)
        {
            pass = n;
            address = 0;
            lineNumber = 0;
            dsectMode = false;
            linesAssembled = 0;
            bytesAssembled = 0;
            results += "Pass " + pass.ToString() + ":\r\n";
            foreach (var line in lines)
            {
                lineNumber++;
                assembleLine(expandTabs(line));
            }
        }

        protected void appendMemoryMap()
        {
            int start;
            int end;
            int i;
            results += "\r\n";
            start = (minAddress / 64) * 64;
            end = (maxAddress / 64) * 64;
            if (maxAddress % 64 != 0) end++;
            if (maxAddress > 0x10000-64) maxAddress = 0x10000 - 64;
            while (start <= end)
            {
                results += start.ToString("X4") + ":";
                for (i = 0; i < 64; i++) results += memoryMap[start + i].ToString();
                results += "\r\n";
                start += 64;
            }
        }

        public void Assemble(String[] lines,byte[] mem)
        {
            String line;
            hexOutput.Clear();
            results = "";
            errors = 0;
            startAddress = 0x0000;
            memory = mem;
            for (var i = 0; i < 65536; i++) memoryMap[i] = '.';
            minAddress = 65535;
            maxAddress = 0;
            labels = new List<List<object>>();
            assemblyPass(1,lines);
            assemblyPass(2, lines);
            if (showSymbolTable)
            {
                sortLabels();
                results += "\r\n";
                foreach (var item in labels)
                {
                    line = (String)item[0];
                    while (line.Length < 12) line += " ";
                    line += ((int)item[1]).ToString("X4");
                    line += "  " + ((int)item[2]).ToString("D5") + " ";
                    if (crossReference)
                    {
                        for (var i = 3; i < item.Count; i++)
                        {
                            line += " " + item[i].ToString();
                        }
                    }
                    line += "\r\n";
                    results += line;
                }
            }
            if (showMemoryMap) appendMemoryMap();
            results += "\r\n";
            results += "Lines Assembled: " + linesAssembled.ToString() + "\r\n";
            results += "Bytes Assembled: " + bytesAssembled.ToString() + "\r\n";
            results += "Errors         : " + errors.ToString() + "\r\n";
            if (outputMode == 'F')
            {
                hexOutput.StartAddress(startAddress);
                hexOutput.End();
                hexOutput.Save(outputFile);
            }
        }
    }
}
