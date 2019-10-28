timera	=	$170f		; Timer /1024 with int
pointl	=	$fa		; low for address display
display	=	$1ff7		; os display routine
count	=	$10		; count timing

	org	$200
start	stz	pointl		; Store zero for initial count
	lda	#>irq		; get low byte of irq vector
	sta	$17fe		; and store into proper place
	lda	#<irq		; high byte too
	sta	$17ff
	lda	#count		; get count delay
	sta	timera		; store into timer and start
	cli			; enable interrupts
done	jsr	display		; call os display routine
	bra	done		; just stay in this loop

irq	inc	pointl		; increment count
	lda	#count		; get count delay
	sta	timera		; store into timer and start timer
	rti			; return from interrupt





