porta	equ	$1740
portad	equ	$1741
portb	equ	$1742
portbd	equ	$1743
endl	equ	$17f7
endh	equ	$17f8
addr4	equ	$08
addr3	equ	$0a
addr2	equ	$0c
addr1	equ	$0e
monpcl	equ	$fa
monpch	equ	$fb
monp	equ	$f1
mons	equ	$f2
mona	equ	$f3
mony	equ	$f4
monx	equ	$f5
monmod	equ	$f6
tmp	equ	$f7
delay	equ	$f0
inl	equ	$f8
inh	equ	$f9
pointl	equ	$fa
pointh	equ	$fb
err	equ	$fc
chkl	equ	$17e7
chkh	equ	$17e8


work1	equ	$17ff

	org	$1c00
nmi	stx	$f5		; Save X
	sty	$f4		; Save Y
	sta	$f3		; Save A
	pla			; get processor flags
	sta	$f1		; and save
	pla			; get PC.0
	sta	$ef		; and save
	sta	$fa		; here as well
	pla			; get PC.1
	sta	$f0		; and sav
	sta	$fb		; here as well
	tsx			; get stack pointer
	sta	$f2		; and save
	jmp	cont		; now to monitor loop
run	ldx	$f2		; get stack
	txs			; place into S
	lda	$fb		; pc high
	pha			; place onto stack
	lda	$fa		; pc low
	pha			; place onto stack
	lda	$f1		; status register
	pha			; place on stack
	lda	$f3		; get A
	ldy	$f4		; get Y
	ldx	$f5		; get X
	rti			; execute as return from int
nmivec	jmp	($17fa)
irqvec	jmp	($17fe)
; *******************************************************
; ***** Determine if keyboard or terminal is active *****
; ***** c=set for keypad, c=clear for terminal      *****
; *******************************************************
select	lda	#$fe		; set PB1-PB7 to output
	sta	portbd		; write to port b direction register
	lda	#$00		; set PA0-PA7 to input
	sta	portad		; write to port a direction register
	lda	#$06		; Want to read row 6 (terminal select)
	sta	portb		; write to port
	lda	porta		; read it
	ror	a		; shift bit into carry
	rts			; return to caller

; **************************
; ***** Clear checksum *****
; **************************
clrchk	lda	#$00		; need to zero checksum
	sta	chkh
	sta	chkl
	rts			; return to caller


	org	$1c4f
cont	jsr	select		; check terminal/keypad
	bcs	loop		; jump if keypad
	jmp	prompt		; otherwise jump to prompt

start	lda	#$ff
	sta	portad
	sta	portbd
	lda	#$00
	sta	monmod
	jsr	select		; see if terminal or keypad is selected
	bcs	loop		; jump to main loop
	lda	#$7f		; be sure tty in is an input
	sta	portad		; write to port a direction register
	lda	#$01		; be sure PB0 is an output
	sta	portbd		; write to port b direction register
	jsr	setbaud		; configure baud rate
	jmp	prompt		; then jump to terminal loop


; *****************************
; ***** Main monitor loop *****
; *****************************
loop	lda	(monpcl)		; get byte from current address
	sta	$f9			; store for display routine

; ************************************
; ***** Check keypad for command *****
; ************************************
keypad	jsr	dspscn
	jsr	readkp
	bmi	cont
	jsr	execmd			; execute command
waitrl	jsr	dspscn
	jsr	ak			; See if button is still pressed
	bne	waitrl			; Loop until it is released
	bra	cont			; and back to main loop

; ********************************
; ***** Execute command in A *****
; ********************************
execmd	cmp	#$10			; check for numeric key
	bmi	number			; jump if so
	beq	keyad
	cmp	#$11			; check for DA key
	beq	keyda			; jump if so
	cmp	#$12			; Was + pressed
	beq	plus			; branch if so
	cmp	#$13			; check for GO key
	beq	exec			; jump to execute
	cmp	#$14			; Check for PC key
	beq	getpc			; recall pc
	rts				; Return to caller
plus	lda	monpcl			; get address
	clc				; clear carry for add
	adc	#1			; add 1 to address
	sta	monpcl			; put it back
	lda	monpch			; get high byte
	adc	#0			; propagate carry
	sta	monpch			; put it back
	lda	(monpcl)
	sta	$f9
	rts
number	rol	a			; Shift to high nybble
	rol	a
	rol	a
	rol	a
	pha				; save for a moment
	ldx	#4			; need 4 shifts
	lda	monmod			; get mode
	beq	numadr			; jump if address mode
	lda	(monpcl)		; get value
	sta	$f9			; save where we can use
	pla				; recover a
numlp	asl	a
	rol	$f9
	dex
	bne	numlp
	lda	$f9			; put back where it belongs
	sta	(monpcl)
	rts				; Return
numadr  pla				; recover a
adrlp	rol	a
	rol	monpcl
	rol	monpch
	dex
	bne	adrlp
	lda	(monpcl)
	sta	$f9
	rts
keyda	lda	#$80
	sta	monmod
	rts
keyad	lda	#$00
	sta	monmod
	rts
exec	jsr	dspscn			; display screen
	jsr	ak			; see if go is still pressed
	beq	exec			; jump if so
	jmp	run			; otherwise, run code
getpc	lda	$ef			; transfer low
	sta	$fa
	lda	$f0			; transfer high
	sta	$fb
	rts				; wait for key release



addrds	dfb	$0e,$0c,$0a,$08,$12,$10


; *************************
; ***** Terminal loop *****
; *************************
prompt
	lda	#$00		; zero input buffer
	sta	inl
	sta	inh
	jsr	crlf		; output cr/lf
	lda	$fb		; high byte of address
	jsr	wrtbyt		; write it
	lda	$fa		; low byte of address
	jsr	wrtbyt		; write it
	jsr	txsp		; transmit a space
	lda	($fa)		; get byte from memory
	jsr	wrtbyt		; write it
	jsr	txsp		; transmit a space
waitin	jsr	rx		; receive a character
; ***** Check for CR, increment address
	cmp	#$0d		; was it cr
	bne	prompt1		; jump if not
incadr	inc	$fa		; increment low of address
	bne	prompt		; loop back if no need to increment high
	inc	$fb		; increment high of address
	bra	prompt		; back to prompt
; ***** Check for numbers
prompt1	cmp	#$30		; check for dgit
	bmi	prompt2
	cmp	#$3a		; check high range
	bpl	prompt2		; jump if not
	and	#$0f		; keep only low digits
	bra	innum		; combine with buffer
; ***** And hex digits
prompt2	cmp	#'A'		; check for hex digit
	bmi	prompt3
	cmp	#'G'		; check high range
	bpl	prompt3		; jump if not
	and	#$0f		; keep only low digits
	clc			; clear carry
	adc	#$09		; move to correct range
innum	ldx	#4		; 4 shift needed
innum1	rol	inl		; shift input buffer by 4 bits
	rol	inh
	dex			; decrement count
	bne	innum1		; loop until done
	ora	inl		; or input with buffer
	sta	inl		; write back to buffer
	bra	waitin		; wait for another key
; ***** check for space, set address
prompt3	cmp	#$20		; check for space
	bne	prompt4		; jumpt if not
	lda	inl		; transfer address
	sta	pointl		; to display address
	lda	inh
	sta	pointh
	bra	prompt		; back to main loop
; ***** check for ., write cell
prompt4	cmp	#'.'		; check for dot command
	bne	prompt5		; jump if not
	lda	inl		; get low of input buffer
	sta	(pointl)	; write to memory
	bra	incadr		; then increment address
prompt5	cmp	#'G'		; check for Go command
	bne	prompt6		; jump if not
	jmp	run		; otherwise run
prompt6	cmp	#'Q'		; check for Q, dump to tape command
	bne	prompt7		; jump if not
	jmp	dumpt		; otherwise, dump to tape
prompt7	cmp	#'L'		; Check for L, load from tape
	bne	prompt8		; jump if not
	jmp	loadt		; otherwise jump to load routine
prompt8	jmp	prompt

; *************************************************
; ***** Routine to perform dump to paper tape *****
; *************************************************
dumpt	jsr	crlf		; start on empty line
	lda	#$00		; zero count
	sta	inl
	sta	inh
dmplin	jsr	clrchk		; clear the checksum
	lda	#$3b		; send semicolon
	jsr	tx
	lda	#$18		; record size
	jsr	wrtnxt
	lda	pointh		; high of address
	jsr	wrtnxt
	lda	pointl		; low of address
	jsr	wrtnxt
	ldx	#$18		; 18 bytes to write
dt1	phx			; save x
	lda	(pointl)	; get next byte
	jsr	wrtnxt
	inc	pointl		; increment address
	bne	dt2		; jump if do not need to propagate
	inc	pointh		; increment high
dt2	plx			; recover x
	dex			; decrement x
	bne	dt1		; loop back for next byte
	jsr	wrtchk		; write out checksum and cr/lf
	inc	inl		; increment record count
	bne	dt3		; jump if no need to propogate
	inc	inh		; increment high
dt3	sec			; set carry for subtraction
	lda	pointl
	sbc	endl
	lda	pointh
	sbc	endh
	bcc	dmplin

	jsr	clrchk		; clear the checksum
	lda	#$3b		; write a semicolon
	jsr	tx		; transmit it
	lda	#$00		; length byte
	jsr	wrtnxt		; write it
	lda	inh		; high of record count
	jsr	wrtnxt		; write it
	lda	inl		; low of record count
	jsr	wrtnxt		; write it
	jsr	wrtchk		; write the checksum
	jsr	rx		; receive 1 key before returning to os
	jmp	cont		; back to loop

; ********************************
; ***** Load from paper tape *****
; ********************************
loadt	lda	#$00		; zero the line count
	sta	inl
	sta	inh
	sta	err		; indicate no errors
readln	jsr	clrchk		; clear checksum
readln1	jsr	rx		; receive a byte
	cmp	#$3b		; look for semicolon
	bne	readln1		; keep reading until I get one
	jsr	read2b		; read next two characters
	pha			; save count for now
	jsr	read2b		; read high of address
	sta	pointh		; save to address space
	jsr	read2b		; read low of address
	sta	pointl		; write to address
	plx			; recover count into x
	beq	readend		; jump if final record
readlp1	phx			; save x
	jsr	read2b		; read 2 characters
	sta	(pointl)	; write to memory
	jsr	addchk		; add to checksum
	inc	pointl		; increment address
	bne	readlp2		; jump if high does not need incrementing
	inc	pointh		; increment high address
readlp2	plx			; recover x
	dex			; decrement count
	bne	readlp1		; loop back for next character
	jsr	read2b		; read checksum
	jsr	read2b
	inc	inl		; increment line count
	bne	readln		; loop if no need to propagate
	inc	inh		; increment high count
	bra	readln		; jump to read next line
readerr	lda	#$01		; indicate error
	sta	err		; store it
	bra	readln		; then read next line
readend	jsr	read2b		; reach checksum
	jsr	read2b
	jmp	cont		; return to os

	org	$1e5a
; **************************************
; ***** Receive A from serial port *****
; **************************************
rx	lda	delay		; get bit width
	lsr	a		; cut in half
	clc			; clear carry for add
	adc	delay		; a= 1 1/2 bit delay
	tay			; move to y
	lda	#$00		; Start with blank character
	ldx	#$8		; 8 bits to receive
rx1	bit	porta		; read port
	bmi	rx1		; wait for start bit
rx2	nop			; 2 extra timing cycles needed   ~2 = ~2
	nop			; 2 more                         ~2 = ~4
	dey			; decrement y                    ~2 = ~6
	bne	rx2		; wait for delay                 ~2 = ~8
rx3	rol	porta		; transfer bit to c              ~6 = ~6
	ror	a		; shift into answer              ~2 = ~8
	ldy	delay		; get 1 bit delay                ~4 = ~11
	dex			; decrement bit count            ~2 = ~13
	bne	rx2		; loop back if not done          ~2 = ~15
	and	#$7f		; Stip parity bit off
	ldy	delay		; Setup delay for reading stop bit
rx4	nop			; need to waste 4 cycles
	nop			; here are the other two
	dey			; decrement delay count
	bne	rx4		; loop until stop bit is complete
	rts			; return to caller	

; ***********************************************
; ***** Write A as hex value to serial port *****
; ***********************************************
wrtbyt	pha			; Save copy of A
	lsr	a		; Move high nybble to low
	lsr	a
	lsr	a
	lsr	a
	jsr	outbyt		; write high nybble
	pla			; recover a
outbyt	and	#$0f		; mask out high bits
	clc			; clear carry for add
	adc	#$30		; convert to ascii
	cmp	#$3a		; check for high of digits
	bmi	outbyt1		; jump if good
	clc
	adc	#$07		; add 7 more
outbyt1	jsr	tx		; transmit the character
	rts			; return to caller


	org	$1e9e
; *********************************
; ***** Send A to serial port *****
; *********************************
txsp	lda	#$20		; space character
tx	ldy	#$01		; only PB0 will be output
	sty	portbd
	ldy	delay		; setup delay
	ldx	#0		; 8 bits to send
	stx	portb		; start bit
	ldx	#$09
tx1	nop			; 2 extra timing cycles needed   ~2 = ~2
	nop			; 2 more                         ~2 = ~4
	dey			; decrement delay count          ~2 = ~6
	bne	tx1		; loop back if not done          ~2 = ~8
	sta	portb		; set next bit                   ~4 = ~4
	lsr	a		; move bit down                  ~2 = ~6
	ldy	delay		; reload bit delay               ~3 = ~9
	nop			; waste 2 cycles                 ~2 = ~11
	dex			; decrement bit count            ~2 = ~13
	bne	tx1		; loop back until all bits set   ~2 = ~15
	lda	#1		; need a stop bit
	sta	portb		; write to port
	ldy	delay		; setup delay for stop bit
tx2	nop			; Need 4 cycles
	nop			; here are the other 2
	dey			; decrement delay count
	bne	tx2		; loop until delay complete
	rts			; return to caller
; *********************************************************
; ***** Measure start bit of <DEL> to get pulse width *****
; *********************************************************
setbaud	lda	#$00		; want all inputs on A to be read
	sta	portad		; write to port a direction register
	ldx	#$00		; set count to zero
wait1	bit	porta		; read port
	bmi	wait1		; loop until A7 goes low, beginning of start bit
wait2	inx			; increment count               ~2 = ~2
	bit	porta		; check end of stop bit         ~4 = ~6
	bpl	wait2		; wait until end of start bit   ~2 = ~8
	dex			; adjust for interbit instructions
	dex
	stx	delay		; and write to delay storage
	ldx	#$0a		; Setup to read rest of sync bits
wait3	ldy	delay		; delay period
wait4	nop			; 4 cycles needed
	nop			; here are the other 2
	dey			; decrement delay count
	bne	wait4		; loop back until bit is done
	dex			; decrement bit count
	bne	wait3		; loop back until all bits read
	rts			; return to caller

seg7	dfb	$bf,$86,$db,$cf,$e6,$ed,$fd,$87
	dfb	$ff,$ef,$f7,$fc,$b9,$de,$f9,$f1

	org	$1efe
; **************************************************************
; ***** Check if a key is pressed                          *****
; ***** Returns: A=0 when no key pressed, otherwise A<>0   *****
; **************************************************************
ak	lda	#0		; Set port A to read all bits
	sta	portad		; write to RRIOT1
	ldx	#4		; Start at row 2
	lda	#$ff		; Setup a for test
aklp	stx	portb		; select row
	and	porta		; combine with total
	dex			; decrement x
	dex
	bpl	aklp		; loop back if not done
	ora	#$80
	eor	#$ff		; Flip the bits
	rts			; return to caller

	org	$1f19
; ******************************************************
; ***** Display bytes in $f9, $fa, $fb on displays *****
; ******************************************************
dspscn	lda	monpch
	ldy	#3
	jsr	dispbyt
	lda	monpcl
	ldy	#1
	jsr	dispbyt
	lda	$f9
	ldy	#5
	jsr	dispbyt
	rts

; A = byte to display
; Y = Position of high nybble 0=lowest, 3=highest, 4=datalo, 5=datahi
dispbyt	ldx	#$7f		; Need to set A to all outputs except A7
	stx	portad
	pha
	lsr	a
	lsr	a
	lsr	a
	lsr	a
	tax
	jsr	wrt7seg
	pla
	and	#$f
	tax
	dey
	jsr	wrt7seg
	rts

; X = Digit to display
; Y = Position 0=lowest, 3=highest, 4=datalo, 5=datahi
wrt7seg	lda	#0
	sta	portb
	lda	seg7,x
	sta	porta
	lda	addrds,y
	sta	portb
	rts

; *************************************************************
; ***** Write byte as ascii to terminal, adds to checksum *****
; *************************************************************
wrtnxt	pha			; save a
	clc			; clear carry
	adc	chkl		; add to checksum
	sta	chkl		; store it back
	bcc	addchk1		; jump if no carry
	inc	chkh		; increment high byte
addchk1	pla			; recover a
	jsr	wrtbyt		; write the byte
	rts			; return to caller

	org	$1f6a
; ******************************************************
; ***** read key from keypad                       *****
; ***** returns a=keycode, A=$ff if no key pressed *****
; ******************************************************
readkp	lda	#0		; Set port a to read
	sta	portad 
	jsr	bitpos 
	beq	rkrow1		; jump if no keys pressed
rkgood	dea
rkret	pha
	lda	#$ff
	sta	portad
	pla
	rts			; and return
rkrow1	lda	#2		; Want to read row 1
	jsr	bitpos		; read row
	beq	rkrow2		; jump if no key found
	adc	#7		; add row offset
	bra	rkgood		; and return good key
rkrow2	lda	#4		; Want to read row 2
	jsr	bitpos		; read row
	beq	rknone		; jump if no key found
	adc	#14		; add row offset
	bra	rkgood		; and return good key
rknone	clc
	lda	#$ff		; signal no key read
	bra	rkret
	rts
bitpos	sta	portb		; write line to port
	lda	porta		; read keys from line
	ldx	#7		; seven bits to read
bplp	lsr	a               ; shift keys
	bcc	bpfnd           ; jump if key found
	dex                     ; decrement count
	bne	bplp            ; loop if more to read
bpfnd	txa                     ; transfer to a
	rts                     ; and return

; **************************************
; ***** Write checksum to terminal *****
; **************************************
wrtchk	lda	chkh		; get high of checksum
	jsr	wrtbyt		; write it
	lda	chkl		; get low of checksum
	jsr	wrtbyt		; write it
	jsr	crlf		; move to next line
	rts			; return

; ************************************
; ***** Output cr/lf to terminal *****
; ************************************
crlf	lda	#$0d		; carriage return
	jsr	tx		; send it
	lda	#$0a		; line feed
	jsr	tx		; send it
	rts			; return to caller

; **************************************
; ***** Add value in A to checksum *****
; **************************************
addchk	clc			; clear carry for add
	adc	chkl		; add in low byte
	sta	chkl		; store result
	bcc	addchkn		; jump if no carry
	inc	chkh		; otherwise propagate carry to high byte
addchkn	rts			; return to caller

; ********************************************
; ***** Convert ascii hex in A to binary *****
; ********************************************
frhex	sec			; set carry for subtract
	sbc	#'0'		; convert to binary
	cmp	#$0a		; check against numbers
	bmi	frhexd		; jump if good
	sbc	#$07		; subtract another 7
frhexd	rts			; return to caller

; ***************************************************************
; ***** Read 2 ascii hex from terminal and return as binary *****
; ***************************************************************
read2b	jsr	rx		; read a byte
	jsr	frhex		; convert from hex
	asl	a		; move to high nybble
	asl	a
	asl	a
	asl	a
	sta	tmp		; save it
	jsr	rx		; read another byte
	jsr	frhex		; convert it
	ora	tmp		; combine with high byte
	rts			; return to caller

	org	$1ffa
	dfw	nmivec
	dfw	start
	dfw	irqvec







