porta	equ	$1740
portad	equ	$1741
portb	equ	$1742
portbd	equ	$1743
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
delay	equ	$f0
inl	equ	$f8
inh	equ	$f9
pointl	equ	$fa
pointh	equ	$fb

	org	$200
	jsr	setbaud
	lda	#$00
	sta	inl
	sta	inh
	jmp	prompt


prompt
	lda	#$00		; zero input buffer
	sta	inl
	sta	inh
	lda	#$0d		; carriage return
	jsr	tx		; transmit it
	lda	#$0a		; line feed
	jsr	tx		; transmit it
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
prompt5
	jmp	prompt

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

	org	$0e5a
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

	org	$0e9e
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





