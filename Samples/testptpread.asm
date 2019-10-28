chkl	=	$17e7
chkh	=	$17e8
pointl	=	$fa
pointh	=	$fb
endl	=	$17f7
endh	=	$17f8
tx	=	$1ea0
rx	=	$1e5a
os	=	$1c4f
tmp	=	$f7
inl	=	$f8
inh	=	$f9
buffer	=	$400

	org	$200
	lda	#$00		; zero the line count
	sta	inl
	sta	inh
	jsr	clrbuf
readln	jsr	rx		; receive a byte
	cmp	#$3b		; look for semicolon
	bne	readln		; keep reading until I get one
	jsr	read2b		; read next two characters
	pha			; save count for now
	jsr	read2b		; read high of address
	sta	pointh		; save to address space
	jsr	read2b		; read low of address
	sta	pointl		; write to address
	plx			; recover count into x
	beq	readend		; jump if final record
readlp1	phx			; save x
	txa
	sta	($10)
	inc	$10
	jsr	read2b		; read 2 characters
	sta	(pointl)	; write to memory
	inc	pointl		; increment address
	bne	readlp2		; jump if high does not need incrementing
	inc	pointh		; increment high address
readlp2	plx			; recover x
	txa
	sta	($10)
	inc	$10
	dex			; decrement count
	bne	readlp1		; loop back for next character
stop	jmp	stop
	jsr	read2b		; read checksum
	jsr	read2b
	bra	readln		; jump to read next line
readend	jsr	read2b		; reach checksum
	jsr	read2b
	jmp	os		; return to os

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


	org	$300
clrchk	lda	#$00		; need to zero checksum
	sta	chkh
	sta	chkl
	rts			; return to caller

frhex	sec			; set carry for subtract
	sbc	#'0'		; convert to binary
	cmp	#$0a		; check against numbers
	bmi	frhexd		; jump if good
	sbc	#$07		; subtract another 7
frhexd	rts			; return to caller

setbuf	lda	#>buffer
	sta	$10
	lda	#<buffer
	sta	$11
	rts

clrbuf	jsr	setbuf
	ldx	#$ff
	lda	#$00
clrbuf1	sta	($10)
	inc	$10
	dex
	bne	clrbuf1
	jsr	setbuf
	rts

