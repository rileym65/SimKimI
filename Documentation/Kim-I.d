[c]	SimKim-I
[c]	by
[c]	Mike Riley
[=]
[h2]	What was the KIM-I?
[=]
	The KIM-I was an early single board computer designed and built by
	MOS Technology, Inc. in 1975.  It was primarily designed for developers
	to get to know the MOS 6502 microprocessor.  But its low cost of $295 
	also made it popular with hobbyists.  The original machine had 1k of
	RAM, expandable to 5k using the onboard memory decoding or up to 56k
	using external decoding.  The computer also contained two RRIOT chips
	which provided 4 I/O ports and two timers.  The timer and IO ports of
	RRIOT1 (6530-002) were used by the onboard os for control of peripherals
	while the second RRIOT (6530-003) were available to the user.
[=]
	This simulation actually uses the 65c02 microprocessor which is an upgrade
	of the 6502 used in the original KIM-I.  For copyright reasons I do not
	provide a copy of the original KIM-I rom, but instead include my own
	version which is a work-alike of the original rom.  I have tested this
	emulator with the actual KIM-I rom and it does function correcty in this
	emulator.
[=]
[h2]	NMI vector
[=]
	In order to use the [ST] button and the single step mode it is required
	to setup the NMI vector first.  These steps should be performed anytime you
	first turn on the computer.  This can be done from keypad mode as
	follows:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	| 1    | XXXX XX    | [AD][1][7][F][A] |
	| 2    | 17FA XX    | [DA][0][0][+]    |
	| 3    | 17FB XX    | [1][C][+]        |
[te]
[i-2]
[=]
[h2]	Keypad Keys
[=]
[i2]
[tb]
	| Key | Function                |
	|-----|-------------------------|
	| GO  | Start program execution |
	| ST  | Stop program execution  |
	| RS  | Reset computer          |
	| AD  | Set Address mode        |
	| DA  | Set Data mode           |
	| PC  | Recall Program Counter  |
	| +   | Increment Address       |
	| 0-F | Numeric input           |
[te]
[i-2]
[=]
[h2]	Terminal functions
[=]
[i2]
[tb]
	| <0>-<F> | Hex characters                  |
	| <space> | Set current address             |
	| <.>     | Write value and advance address |
	| <enter> | Advance to next address         |
	| <G>     | Execute at current address      |
	| <Q>     | Dump to paper tape              |
	| <L>     | Load from paper tape            |
[te]
[i-2]
[=]
[h2]	Sample using Keypad
[=]
	Be sure that your Kim-I is set to use the keypad and not the tty 
	when trying this sample.
[=]
	We will enter a simple program that adds the two numbers together
	stored in address $0000 and $0001, placing the result into $0002.
[=]
	First, enter the program:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | XXXX XX    | [AD][0][2][0][0] |
	| 2    | 0200 XX    | [DA][1][8][+]    |
	| 3    | 0201 XX    | [A][5][+]        |
	| 4    | 0202 XX    | [0][0][+]        |
        | 5    | 0203 XX    | [6][5][+]        |
	| 6    | 0204 XX    | [0][1][+]        |
	| 7    | 0205 XX    | [8][5][+]        |
	| 8    | 0206 XX    | [0][2][+]        |
	| 9    | 0207 XX    | [4][C][+]        |
	| 10   | 0208 XX    | [4][F][+]        |
	| 11   | 0209 XX    | [1][C][+]        |
	| 12   | 020A XX    |                  |
[te]
[i-2]
[=]
	At this point the program is entered, lets go back over it to make
	sure there were no mistakes:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | 020A XX    | [AD][0][2][0][0] |
	| 2    | 0200 18    | [+]              |
	| 3    | 0201 A5    | [+]              |
	| 4    | 0202 00    | [+]              |
	| 5    | 0203 65    | [+]              |
	| 6    | 0204 01    | [+]              |
	| 7    | 0205 85    | [+]              |
	| 8    | 0206 02    | [+]              |
	| 9    | 0207 4C    | [+]              |
	| 10   | 0208 4F    | [+]              |
	| 11   | 0209 1C    |                  |
[te]
[i-2]
[=]
	If while looking through the program you find an entry that is wrong,
	just press the [DA] key and then correct value before hitting [+].
[=]
	Now we need to put some numbers into memory for the program to add:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | XXXX XX    | [AD][0][0][0][0] |
	| 2    | 0000 XX    | [DA][0][5][+]    |
	| 3    | 0001 XX    | [0][4][+]        |
[te]
[i-2]
[=]
	Now lets run the program:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | XXXX XX    | [AD][0][2][0][0] |
	| 2    | 0200 18    | [GO]             |
	| 3    | 0200 18    |                  |
[te]
[i-2]
[=]
	Now we can go and look at the results:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | XXXX XX    | [AD][0][0][0][2] |
	| 2    | 0002 09    |                  |
[te]
[i-2]
[=]
	If you did not get the result of 09, then go back over the sample and
	verify that you entered everything correctly.
[=]
[h2]	Sample using terminal
[=]
	Be sure that your Kim-I is set to use the teleprinter and not the keypad 
	when trying this sample.
[=]
	To distinguish between keypad buttons and terminal keys I will use []
	to indicate the key is a keypad key and <> to indicate when you type the
	key on your keyboard.
[=]
	We will enter a simple program that adds the two numbers together
	stored in address $0000 and $0001, placing the result into $0002.
[=]
	First, enter the program:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | XXXX XX    | <2><0><0><space> |
	| 2    | 0200 XX    | <1><8><.>        |
	| 3    | 0201 XX    | <A><5><.>        |
	| 4    | 0202 XX    | <0><0><.>        |
        | 5    | 0203 XX    | <6><5><.>        |
	| 6    | 0204 XX    | <0><1><.>        |
	| 7    | 0205 XX    | <8><5><.>        |
	| 8    | 0206 XX    | <0><2><.>        |
	| 9    | 0207 XX    | <4><C><.>        |
	| 10   | 0208 XX    | <4><F><.>        |
	| 11   | 0209 XX    | <1><C><.>        |
	| 12   | 020A XX    |                  |
[te]
[i-2]
[=]
	At this point the program is entered, lets go back over it to make
	sure there were no mistakes:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | 020A XX    | <2><0><0><space> |
	| 2    | 0200 18    | <enter>          |
	| 3    | 0201 A5    | <enter>          |
	| 4    | 0202 00    | <enter>          |
	| 5    | 0203 65    | <enter>          |
	| 6    | 0204 01    | <enter>          |
	| 7    | 0205 85    | <enter>          |
	| 8    | 0206 02    | <enter>          |
	| 9    | 0207 4C    | <enter>          |
	| 10   | 0208 4F    | <enter>          |
	| 11   | 0209 1C    |                  |
[te]
[i-2]
[=]
	If while looking through the program you find an entry that is wrong,
	just press the keys for the new value and press the <.> key.
[=]
	Now we need to put some numbers into memory for the program to add:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | XXXX XX    | <0><space>       |
	| 2    | 0000 XX    | <5><.>           |
	| 3    | 0001 XX    | <4><.>           |
[te]
[i-2]
[=]
	Now lets run the program:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | XXXX XX    | <2><0><0><space> |
	| 2    | 0200 18    | <G>              |
	| 3    | 0200 18    |                  |
[te]
[i-2]
[=]
	Now we can go and look at the results:
[=]
[i2]
[tb]
	| Step | On Display | Buttons to press |
	|------|------------|------------------|
	| 1    | XXXX XX    | <2><space>       |
	| 2    | 0002 09    |                  |
[te]
[i-2]
[=]
	If you did not get the result of 09, then go back over the sample and
	verify that you entered everything correctly.


[=]
[h2]	Instruction Set
[i2]
[tb]
	| Mnemonic | Mode      | Opcode | Operation                        |
	| -------- | --------- | ------ | -------------------------        |
	| ADC      | #nn       | 69     | Add with Carry                   |
	|          | $nn       | 65     |                                  |
	|          | $nn,X     | 75     |                                  |
	|          | $nnnn     | 6D     |                                  |
        |          | $nnnn,X   | 7D     |                                  |
        |          | $nnnn,Y   | 79     |                                  |
        |          | ($nn,X)   | 61     |                                  |
        |          | ($nn),Y   | 71     |                                  |
        |          | ($nn)     | 72     | 65c02 only                       | 
	|          |           |        |                                  | 
	| AND      | #nn       | 29     | Logical AND                      | 
	|          | $nn       | 25     |                                  |
        |          | $nn,X     | 35     |                                  |
        |          | $nnnn     | 2D     |                                  |
        |          | $nnnn,X   | 3D     |                                  |
        |          | $nnnn,Y   | 39     |                                  |
        |          | ($nn,X)   | 21     |                                  |
        |          | ($nn),Y   | 31     |                                  |
        |          | ($nn)     | 32     | 65c02 only                       |
	|          |           |        |                                  |
	| ASL      | A         | 0A     | Arithmetic Shift Left            |
        |          | $nn       | 06     |                                  |
        |          | $nn,X     | 16     |                                  |
        |          | $nnnn     | 0E     |                                  |
        |          | $nnnn,X   | 1E     |                                  |
	|          |           |        |                                  |
	| BCC      | $nn       | 90     | Branch on Carry Clear            |
	|          |           |        |                                  |
        | BCS      | $nn       | B0     | Branch on Carry Set              |
	|          |           |        |                                  |
        | BEQ      | $nn       | F0     | Branch on Equal Z=1              |
	|          |           |        |                                  |
	| BIT      | #nn       | 89     | Test Bits                        |
        |          | $nn       | 24     |                                  |
	|          | $nn,X     | 34     | 65c02 only                       |
	|          | $nnnn     | 2C     |                                  |
	|          | $nnnn,X   | 3C     | 65c02 only                       |
	|          |           |        |                                  |
	| BMI      | $nn       | 30     | Branch on minus N=1              |
	|          |           |        |                                  |
	| BNE      | $nn       | D0     | Branch on not Equal Z=0          |
	|          |           |        |                                  |
	| BPL      | $nn       | 10     | Branch on plus N=0               |
	|          |           |        |                                  |
	| BRA      | $nn       | 80     | Branch (65c02 only)              |
	|          |           |        |                                  |
	| BRK      |           | 00     | Break                            |
	|          |           |        |                                  |
	| BVC      | $nn       | 50     | Branch overflow Clear V=0        |
	|          |           |        |                                  |
	| BVS      | $nn       | 70     | Branch overflow set V=1          |
	|          |           |        |                                  |
	| CLC      |           | 18     | Clear Carry                      |
	|          |           |        |                                  |
	| CLD      |           | D8     | Clear Decimal                    |
	|          |           |        |                                  |
	| CLI      |           | 58     | Enable interrupts                |
	|          |           |        |                                  |
	| CLV      |           | B8     | Clear overflow                   |
	|          |           |        |                                  |
	| CMP      | #nn       | C9     | Compare A                        |
	|          | $nn       | C5     |                                  |
	|          | $nn,X     | D5     |                                  |
	|          | $nnnn     | CD     |                                  |
	|          | $nnnn,X   | DD     |                                  |
	|          | $nnnn,Y   | D9     |                                  |
	|          | ($nn,X)   | C1     |                                  |
	|          | ($nn),Y   | D1     |                                  |
	|          | ($nn)     | D2     | 65c02 only                       |
	|          |           |        |                                  |
	| CPX      | #nn       | E0     | Compare X                        |
	|          | $nn       | E4     |                                  |
	|          | $nnnn     | EC     |                                  |
	|          |           |        |                                  |
	| CPY      | #nn       | C0     | Compare Y                        |
	|          | $nn       | C4     |                                  |
	|          | $nnnn     | CC     |                                  |
	|          |           |        |                                  |
	| DEA      |           | 3A     | Decrement A (65c02 only)         |
	|          |           |        |                                  |
	| DEC      | $nn       | C6     | Decrement                        |
	|          | $nn,X     | D6     |                                  |
	|          | $nnnn     | CE     |                                  |
	|          | $nnnn,X   | DE     |                                  |
	|          |           |        |                                  |
	| DEX      |           | CA     | Decrement X                      |
	|          |           |        |                                  |
	| DEY      |           | 88     | Decrement Y                      |
	|          |           |        |                                  |
	| EOR      | #nn       | 49     | Exclusive Or                     |
	|          | $nn       | 45     |                                  |
	|          | $nn,X     | 55     |                                  |
	|          | $nnnn     | 4D     |                                  |
	|          | $nnnn,X   | 5D     |                                  |
	|          | $nnnn,Y   | 59     |                                  |
	|          | ($nn,X)   | 41     |                                  |
	|          | ($nn),Y   | 51     |                                  |
	|          | ($nn)     | 52     | 65c02 only                       |
	|          |           |        |                                  |
	| INA      |           | 1A     | Increment A (65c02 only)         |
	|          |           |        |                                  |
	| INC      | $nn       | E6     | Increment                        |
	|          | $nn,X     | F6     |                                  |
	|          | $nnnn     | EE     |                                  |
	|          | $nnnn,X   | FE     |                                  |
	|          |           |        |                                  |
	| INX      |           | E8     | Increment X                      |
	|          |           |        |                                  |
	| INY      |           | C8     | Increment Y                      |
	|          |           |        |                                  |
	| JMP      | $nnnn     | 4C     | Unconditional Jump               |
	|          | ($nnnn)   | 6C     |                                  |
	|          | ($nnnn,X) | 7C     | 65c02 only                       |
	|          |           |        |                                  |
	| JSR      | $nnnn     | 20     | Jump to subroutine               |
	|          |           |        |                                  |
	| LDA      | #nn       | A9     | Load A                           |
	|          | $nn       | A5     |                                  |
	|          | $nn,X     | B5     |                                  |
	|          | $nnnn     | AD     |                                  |
	|          | $nnnn,X   | BD     |                                  |
	|          | $nnnn,Y   | B9     |                                  |
	|          | ($nn,X)   | A1     |                                  |
	|          | ($nn),Y   | B1     |                                  |
	|          | ($nn)     | B2     | 65c02 only                       |
	|          |           |        |                                  |
	| LDX      | #nn       | A2     | Load X                           |
	|          | $nn       | A6     |                                  |
	|          | $nn,Y     | B6     |                                  |
	|          | $nnnn     | AE     |                                  |
	|          | $nnnn,Y   | BE     |                                  |
	|          |           |        |                                  |
	| LDY      | #nn       | A0     | Load Y                           |
	|          | $nn       | A4     |                                  |
	|          | $nn,X     | B4     |                                  |
	|          | $nnnn     | AC     |                                  |
	|          | $nnnn,X   | BC     |                                  |
	|          |           |        |                                  |
	| LSR      | A         | 4A     | Logical Shift Right              |
	|          | $nn       | 46     |                                  |
	|          | $nn,X     | 56     |                                  |
	|          | $nnnn     | 4E     |                                  |
	|          | $nnnn,X   | 5E     |                                  |
	|          |           |        |                                  |
	| NOP      |           | EA     | No operation                     |
	|          |           |        |                                  |
	| ORA      | #nn       | 09     | Logical OR                       |
	|          | $nn       | 05     |                                  |
	|          | $nn,X     | 15     |                                  |
	|          | $nnnn     | 0D     |                                  |
	|          | $nnnn,X   | 1D     |                                  |
	|          | $nnnn,Y   | 19     |                                  |
	|          | ($nn,X)   | 01     |                                  |
	|          | ($nn),Y   | 11     |                                  |
	|          | ($nn)     | 12     | 65c02 only                       |
	|          |           |        |                                  |
	| PHA      |           | 48     | Push A                           |
	|          |           |        |                                  |
	| PHX      |           | DA     | Push X (65c02 only)              |
	|          |           |        |                                  |
	| PHY      |           | 5A     | Push Y (65c02 only)              |
	|          |           |        |                                  |
	| PLA      |           | 68     | Pull A                           |
	|          |           |        |                                  |
	| PLX      |           | FA     | Pull X (65c02 only)              |
	|          |           |        |                                  |
	| PLY      |           | 7A     | Pull Y (65c02 only)              |
	|          |           |        |                                  |
	| ROL      | A         | 2A     | Rotate Left thru carry           |
	|          | $nn       | 26     |                                  |
	|          | $nn,X     | 36     |                                  |
	|          | $nnnn     | 2E     |                                  |
	|          | $nnnn,X   | 3E     |                                  |
	|          |           |        |                                  |
	| ROR      | A         | 6A     | Rotate Right thru carry          |
	|          | $nn       | 66     |                                  |
	|          | $nn,X     | 76     |                                  |
	|          | $nnnn     | 6E     |                                  |
	|          | $nnnn,X   | 7E     |                                  |
	|          |           |        |                                  |
	| RTI      |           | 40     | Return from Interrupt            |
	|          |           |        |                                  |
	| RTS      |           | 60     | Return from subroutine           |
	|          |           |        |                                  |
	| SBC      | #nn       | E9     | Subract with Carry               |
	|          | $nn       | E5     |                                  |
	|          | $nn,X     | F5     |                                  |
	|          | $nnnn     | ED     |                                  |
	|          | $nnnn,X   | FD     |                                  |
	|          | $nnnn,Y   | F9     |                                  |
	|          | ($nn,X)   | E1     |                                  |
	|          | ($nn),Y   | F1     |                                  |
	|          | ($nn)     | F2     | 65c02 only                       |
	|          |           |        |                                  |
	| SEC      |           | 38     | Set Carry                        |
	|          |           |        |                                  |
	| SED      |           | F8     | Set Decimal                      |
	|          |           |        |                                  |
	| SEI      |           | 78     | Disable interrupts               |
	|          |           |        |                                  |
	| STA      | $nn       | 85     | Store A                          |
	|          | $nn,X     | 95     |                                  |
	|          | $nnnn     | 8D     |                                  |
	|          | $nnnn,X   | 9D     |                                  |
	|          | $nnnn,Y   | 99     |                                  |
	|          | ($nn,X)   | 81     |                                  |
	|          | ($nn),Y   | 91     |                                  |
	|          | ($nn)     | 92     | 65c02 only                       |
	|          |           |        |                                  |
	| STX      | $nn       | 86     | Store X                          |
	|          | $nn,Y     | 96     |                                  |
	|          | $nnnn     | 8E     |                                  |
	|          |           |        |                                  |
	| STY      | $nn       | 84     | Store Y                          |
	|          | $nn,X     | 94     |                                  |
	|          | $nnnn     | 8C     |                                  |
	|          |           |        |                                  |
	| STZ      | $nn       | 64     | Store 0 (65c02 only)             |
	|          | $nn,X     | 74     | 65c02 only                       |
	|          | $nnnn     | 9C     | 65c02 only                       |
	|          | $nnnn,X   | 9E     | 65c02 only                       |
	|          |           |        |                                  |
	| TAX      |           | AA     | Transfer A to X                  |
	|          |           |        |                                  |
	| TAY      |           | A8     | Transfer A to Y                  |
	|          |           |        |                                  |
	| TRB      | $nn       | 14     | Test and Reset Bits (65c02 only) |
	|          | $nnnn     | 1C     | 65c02 only                       |
	|          |           |        |                                  |
	| TSB      | $nn       | 04     | Test and Set Bits (65c0s only)   |
	|          | $nnnn     | 0C     | 65c02 only                       |
	|          |           |        |                                  |
	| TSX      |           | BA     | Transfer S to X                  |
	|          |           |        |                                  |
	| TXA      |           | 8A     | Transfer X to A                  |
	|          |           |        |                                  |
	| TXS      |           | 9A     | Transfer X to S                  |
	|          |           |        |                                  |
	| TYA      |           | 98     | Transfer Y to A                  |
[te]
[i-2]
[=]
[h2]	Memory Map:
[i2]
[tb]
	| $0000 | - | $03ff | 1024 bytes of RAM                    |
	|       |   | $00ef | Program Counter - low (monitor)      |
	|       |   | $00f0 | Program Counter - high (monitor)     |
	|       |   | $00f1 | Status Register (monitor)            |
	|       |   | $00f2 | Stack Pointer (monitor)              |
	|       |   | $00f3 | Accumulator (monitor)                |
	|       |   | $00f4 | Y Index Register (monitor)           |
	|       |   | $00f5 | X Index Registr (monitor)            |
	|       |   | $00f9 | Data, data display                   |
	|       |   | $00fa | PointL, low monitor address display  |
	|       |   | $00fb | PointH, high monitor address display |
	|       |   |       |                                      |
	| $0400 | - | $13ff | Optional memory area                 |
	|       |   |       |                                      |
	| $1700 | - | $173f | 6530-003 I/O, Timer                  |
	|       |   | $1700 | 6530-003 A Data Register             |
	|       |   | $1701 | 6530-003 A Data Direction Register   |
	|       |   | $1702 | 6530-003 B Data Register             |
	|       |   | $1703 | 6530-003 B Data Direction Register   |
	|       |   | $1704 | 6530-003 timer, divider 1, no irq    |
	|       |   | $1705 | 6530-003 timer, divider 8, no irq    |
	|       |   | $1706 | 6530-003 timer, divider 64, no irq   |
	|       |   | $1707 | 6530-003 timer, divider 1024, no irq |
	|       |   | $170c | 6530-003 timer, divider 1, irq       |
	|       |   | $170d | 6530-003 timer, divider 8, irq       |
	|       |   | $170e | 6530-003 timer, divider 64, irq      |
	|       |   | $170f | 6530-003 timer, divider 1024, irq    |
	|       |   |       |                                      |
	| $1740 | - | $177f | 6530-002 I/O, Timer                  |
	|       |   | $1740 | 6530-002 A Data Register             |
	|       |   | $1741 | 6530-002 A Data Direction Register   |
	|       |   | $1742 | 6530-002 B Data Register             |
	|       |   | $1743 | 6530-002 B Data Direction Register   |
	|       |   | $1744 | 6530-002 timer, divider 1, no irq    |
	|       |   | $1745 | 6530-002 timer, divider 8, no irq    |
	|       |   | $1746 | 6530-002 timer, divider 64, no irq   |
	|       |   | $1747 | 6530-002 timer, divider 1024, no irq |
	|       |   | $174c | 6530-002 timer, divider 1, irq       |
	|       |   | $174d | 6530-002 timer, divider 8, irq       |
	|       |   | $174e | 6530-002 timer, divider 64, irq      |
	|       |   | $174f | 6530-002 timer, divider 1024, irq    |
	|       |   |       |                                      |
	| $1780 | - | $17bf | 64 bytes RAM from 6530-003           |
	|       |   |       |                                      |
	| $17c0 | - | $17ff | 64 bytes RAM from 6530-002           |
	|       |   | $17f5 | Tape start address low               |
	|       |   | $17f6 | Tape start address high              |
	|       |   | $17f7 | Tape end address low                 |
	|       |   | $17f8 | Tape end address high                |
	|       |   | $17f9 | Tape block ID                        |
	|       |   | $17fa | low byte of NMI vector in ram        |
	|       |   | $17fb | high byte of NMI vector in ram       |
	|       |   | $17fe | low byte of IRQ vector in ram        |
	|       |   | $17ff | high byte of IRQ vector in ram       |
	|       |   |       |                                      |
	| $1800 | - | $1bff | 1024 bytes ROM from 6530-003         |
	|       |   | $1800 | Save to tape routine                 |
	|       |   | $1873 | Load from tape routine               |
	|       |   |       |                                      |
	| $1c00 | - | $1fff | 1024 bytes ROM from 6530-002         |
	|       |   | $1C4F | Monitor loop (jmp)                   |
	|       |   | $1E5A | Get char from tty                    |
	|       |   | $1E9E | <space> to tty                       |
	|       |   | $1EA0 | Write char to tty                    |
	|       |   | $1EFE | Check for keypad key pressed         |
	|       |   | $1F19 | Display $f9, $fa, $fb                |
	|       |   | $1F6A | Get key from keypad                  |
[te]
[i-2]
[=]
[h2]	Assembler
[=]
	This emulator includes an integrated 65c02 assembler to help
	you in writing programs for this emulator.  
[=]
	In addition to the processor instructions listed above the
	assembler supports the folloing pseudo instructions:
[=]
[i2]
[tb]
	|       | ORG   | address | Specifies assembly address                         |
	| label | EQU   | expr    | Set label to value of expr                         |
	| label | =     | expr    | Set label to value of expr                         |
	|       | DFB   | expr    | Define a byte, may be a comma separated list       |
	|       | DFW   | expr    | Define a word, may be a comma separated list       |
	|       | DS    | expr    | Reserve epxr bytes                                 |
	|       | DSECT |         | Begin data section (prevents writing output bytes) |
	|       | DEND  |         | End of data section                                |
	|       | EXED  | expr    | Specify execution address                          |
[te]
[i-2]
[=]
	Constants may be defined as
[=]
[i2]
[tb]
	| $value | Value is specified in hex characters     |
	| %value | Value is specified as binary (0s and 1s) |
	| <value | High byte of value, must preceed $ or %  |
	| >value | Low byte of value, must preceed $ or %   |
	| *      | Current assembly address                 |
	| 'char' | Ascii value of char                      |
[te]
[i-2]
[=]
[h2]	Port A - RRIOT1 (6530-002)
[i2]
[tb]
	| Pin | Dir | Function                |
	|-----|-----|-------------------------|
	| PA0 | IO  | Keyboard/Display        |
	| PA1 | IO  | Keyboard/Display        |
	| PA2 | IO  | Keyboard/Display        |
	| PA3 | IO  | Keyboard/Display        |
	| PA4 | IO  | Keyboard/Display        |
	| PA5 | IO  | Keyboard/Display        |
	| PA6 | IO  | Keyboard/Display        |
	| PA7 | Out | TTY In                  |
[te]
[i-2]
[=]
[h2]	Port B - RRIOT1 (6530-002)
[i2]
[tb]
	| Pin | Dir | Function                |
	|-----|-----|-------------------------|
	| PB0 | Out | TTY out                 |
	| PB1 | Out | Keyboard/Display select |
	| PB2 | Out | Keyboard/Dispaly select |
	| PB3 | Out | Keyboard/Display select |
	| PB4 | Out | Keyboard/Display select |
	| PB5 | Out | IO Control              |
	| PB6 | --- | Unavailable             |
	| PB7 | IO  | Tape In/Out             |
[te]
[i-2]
[=]
[h2]	Keyboard/Display select
[=]
[i2]
[tb]
	|   | Code | Function                     |
	|   |------|------------------------------|
	| 0 |  $00 | Keyboard Row 0               |
	| 1 |  $02 | Keyboard Row 1               |
	| 2 |  $04 | Keyboard Row 2               |
	| 3 |  $06 | Keypad/teletype switch       |
	| 4 |  $08 | Address Display Highest      |
	| 5 |  $0A | Address Display next highest |
	| 6 |  $0C | Address Display next lowest  |
	| 7 |  $0E | Address Display Lowest       |
	| 8 |  $10 | Data Display highest         |
	| 9 |  $12 | Data Display lowest          |
[te]
[i-2]
[=]
[h2]	Keypad
[i2]
[tb]
	|       | PA0 | PA1 | PA2 | PA3 | PA4 | PA5 | PA6 |
	|-------|-----|-----|-----|-----|-----|-----|-----|
	| Row 0 |  6  |  5  |  4  |  3  |  2  |  1  |  0  |
	| Row 1 |  D  |  C  |  B  |  A  |  9  |  8  |  7  |
	| Row 2 | PC  | GO  |  +  | DA  | AD  |  F  |  E  |
[te]
[i-2]
[=]
[-]	RS is tied to 6502 reset pin
[-]	ST is tied to 6503 nmi pin
[=]
[h2]	Display:
[=]
[-]	.                       A
[-]	.                      ---
[-]	PA0 - segment A       |   |
[-]	PA1 - segment B      F|   |B
[-]	PA2 - segment C       | G |
[-]	PA3 - segment D        ---
[-]	PA4 - segment E       |   |
[-]     PA5 - segment F      E|   |C
[-]     PA6 - segment G       | D |
[-]     .                      ---
[=]
[i2]
[tb]
	|   |     | .GFE DCBA |
	|---|-----|-----------|
	| 0 | $BF | 1011 1111 |
	| 1 | $86 | 1000 0110 |
	| 2 | $DB | 1101 1011 |
	| 3 | $CF | 1100 1111 |
	| 4 | $E6 | 1110 0110 |
	| 5 | $ED | 1110 1101 |
	| 6 | $FD | 1111 1101 |
	| 7 | $87 | 1000 0111 |
	| 8 | $FF | 1111 1111 |
	| 9 | $EF | 1110 1111 |
	| A | $F7 | 1111 0111 |
	| B | $FC | 1111 1100 |
	| C | $B9 | 1011 1001 |
	| D | $DE | 1101 1110 |
	| E | $F9 | 1111 1001 |
	| F | $F1 | 1111 0001 |
[te]
[i-2]
[=]
[h2]Patch for using original KIM-I rom
[=]
	Due to the way the displays work on the KIM-I a patch is
	needed to make the original KIM-I rom work better with the
	emulator.  Set the following bytes in your rom file:
[=]
[i2]
[tb]
	| address | change to |
	| 1F50    | EA        |
	| 1F51    | EA        |
	| 1F52    | EA        |
[te]
[i-2]
[=]


