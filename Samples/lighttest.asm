porta	equ	$1740
portad	equ	$1741
portb	equ	$1742
portbd	equ	$1743
addr4	equ	$08
addr3	equ	$0a
addr2	equ	$0c
addr1	equ	$0e
monpcl	equ	$ef
monpch	equ	$f0

	org	$1000
start	lda	#$ff
	sta	portad
	sta	portbd
	lda	#$10
	sta	monpch
	lda	#$00
	sta	monpcl

loop	lda	monpch
	ldy	#3
	jsr	dispbyt
	lda	monpcl
	ldy	#1
	jsr	dispbyt
	lda	(monpcl)
	ldy	#5
	jsr	dispbyt

	jmp	loop

; A = byte to display
; Y = Position of high nybble 0=lowest, 3=highest, 4=datalo, 5=datahi
dispbyt	pha
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

seg7	dfb	$bf,$86,$db,$cf,$e6,$ed,$fd,$87
	dfb	$ff,$ef,$f7,$fc,$b9,$de,$f9,$f1
addrds	dfb	$0e,$0c,$0a,$08,$10,$12

	org	$fffc
	dfw	start







