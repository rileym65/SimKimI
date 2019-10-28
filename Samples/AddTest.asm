pointl	equ	$fa
pointh	equ	$fb
start	equ	$1c4f
val1	dfb	2
val2	dfb	5
prog	clc
	lda	val1
	adc	val2
	sta	pointl
	lda	#0
	sta	pointh
	jmp	start

