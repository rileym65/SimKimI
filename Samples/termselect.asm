porta	=	$1740
portad	=	$1741
portb	=	$1742
portbd	=	$1743
	org	$200
start	jsr	select
	bcs	iskey
	lda	#$bf
	jmp	disp
iskey	lda	#$86
disp	sta	$00
	lda	#$ff		; need A as output
	sta	portad		; write to port a direction register
	lda	$00		; get byte to display
	sta	porta		; write it to the port
	lda	#$12		; code to write lowest data display
	sta	portb		; write it to port b
	lda	#$00
	sta	portb
	jmp	start

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


