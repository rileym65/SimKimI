; Timer sample 2

timera	=	$1707
pointl	=	$fa
ticks	=	$40
display	=	$1ff7

	org	$200
start	stz	$00		; Start count at 0
next	lda	#ticks		; get tick count
	sta	timera		; store into timer and start
loop	lda	$00		; Read current count from $00
	sta	pointl		; and put into display area
	jsr	display		; Refresh display
	lda	timera		; get status count
	bpl	loop		; loop back if high bit not set
	inc	$00		; increment counter
	jmp	next		; and restart timer




