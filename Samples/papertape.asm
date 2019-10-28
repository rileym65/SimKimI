chkl	=	$17e7
chkh	=	$17e8
pointl	=	$fa
pointh	=	$fb
endl	=	$17f7
endh	=	$17f8
tx	=	$1ea0
rx	=	$1e5a
os	=	$1c4f
inl	=	$f8
inh	=	$f9

	org	$200
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
	jmp	os

	org	$300
clrchk	lda	#$00		; need to zero checksum
	sta	chkh
	sta	chkl
	rts			; return to caller

wrtchk	lda	chkh		; get high of checksum
	jsr	wrtbyt		; write it
	lda	chkl		; get low of checksum
	jsr	wrtbyt		; write it
	jsr	crlf		; move to next line
	rts			; return

wrtnxt	pha			; save a
	clc			; clear carry
	adc	chkl		; add to checksum
	sta	chkl		; store it back
	bcc	addchk1		; jump if no carry
	inc	chkh		; increment high byte
addchk1	pla			; recover a
	jsr	wrtbyt		; write the byte
	rts			; return to caller







	org	$400
; ****************************************
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

crlf	lda	#$0d		; carriage return
	jsr	tx		; send it
	lda	#$0a		; line feed
	jsr	tx		; send it
	rts			; return to caller
