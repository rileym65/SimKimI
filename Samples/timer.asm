timera	=	$1707
tdata	=	$1706
pointl	=	$fa
display	=	$1ff7

	org	$200
start	lda	#$ff		; 255 counts
	sta	timera		; Store into timer and start
loop	lda	tdata		; Read current timer value
	sta	pointl		; Store for display
	jsr	display		; display 
	jmp	loop		; keep looping




