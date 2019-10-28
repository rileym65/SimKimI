sad	=	$1740		; 6530 A Data
padd	=	$1741		; 6350 A Data Direction
sbd	=	$1742		; 6350 B Data
pbdd	=	$1743		; 6350 B Data Direction
clk1t	=	$1744		; Div by 1 time
clk8t	=	$1745		; Div by 8 time
clk64t	=	$1746		; Div by 64 time
clkkt	=	$1747		; Div by 1024 time
clkrdi	=	$1747		; Read time out bit
clkrdt	=	$1746		; Read time
pcl	=	$ef		; Program cnt low
pch	=	$f0		; Program cnt hi
preg	=	$f1		; Current status reg
spuser	=	$f2		; Current stack pointer
acc	=	$f3		; Accumulator
yreg	=	$f4		; Y index
xreg	=	$f5		; X index
chkhi	=	$f6
chksum	=	$f7
inl	=	$f8		; input buffer
inh	=	$f9		; input buffer
pointl	=	$fa		; lsb of open cell
pointh	=	$fb		; msb of open cell
temp	=	$fc
tmpx	=	$fd
char	=	$fe
mode	=	$ff
chkl	=	$17e7
chkh	=	$17e8
savx	=	$17e9
veb	=	$17ec
cntl30	=	$17f2		; tty delay
cnth30	=	$17f3		; tty delay
timh	=	$17f4
sal	=	$17f5		; Low starting address
sah	=	$17f6		; hi starting address
eal	=	$17f7		; low ending address
eah	=	$17f8		; hi ending address
id	=	$17f9		; tape program id number
nmiv	=	$17fa		; Stop vector (stop=1c00)
rstv	=	$17fc		; Rst vector
irqv	=	$17fe		; irq vector (brk=1c00)

	org	$1c00
save	sta	acc		; Kim entry via stop (nmi)
	pla			; or brk (irq)
	sta	preg
save1	pla			; Kim entry via jsr (A lost)
	sta	pcl
	sta	pointl
	pla
	sta	pch
	sta	pointh
save2	sty	yreg
	stx	xreg
	tsx
	stx	spuser
	jsr	inits
	jmp	start

nmit	jmp	(nmiv)		; non-maskable interrupt trap
irqt	jmp	(irqv)		; interrupt trap

rst	ldx	#$ff		; Kim entry via rst
	txs
	stx	spuser
	jsr	inits

detcps	lda	#$ff		; Count start bit
	sta	cnth30		; zero cnth30
	lda	#$01		; mask high order bits
det1	bit	sad		; test
	bne	start		; keybd ssw test
	bmi	det1		; start bit test
	lda	#$fc
det3	clc			; this loop counts
	adc	#$01		; the start bit time
	bcc	det2
	inc	cnth30
det2	ldy	sad		; check for end of start bit
	bpl	det3
	sta	cntl30
	ldx	#$08
	jsr	get5		; Get rest of the char

start	jsr	init1
	lda	#$01
	bit	sad
	bne	ttykb
	jsr	crlf		; prt cr lf
	ldx	#$0a
	jsr	prtst
	jmp	show1

clear	lda	#$00
	sta	inl		; clear input buffer
	sta	inh
read	jsr	getch		; get char
	cmp	#$01
	beq	ttykb
	jsr	pack
	jmp	scan

ttykb	jsr	scand		; if a=0 no key
	bne	start
ttykb1	lda	#$01
	bit	sad
	beq	start
	jsr	scand
	beq	ttykb1
	jsr	scand
	beq	ttykb1

getk	jsr	getkey
	cmp	#$15
	bpl	start
	cmp	#$14
	beq	pccmd		; display pc
	cmp	#$10		; addr mode=1
	beq	addrm
	cmp	#$11		; data mode=1
	beq	datam
	cmp	#$12		; step
	beq	step
	cmp	#$13		; run
	beq gov
data	asl	a		; shift char into high
	asl	a		; order nybble
	asl	a
	asl	a
	sta	temp		; store in temp
	ldx	#$04
data1	ldy	mode		; test mode 1=addr
	bne	addr		; mode=0 data
	lda	(pointl),y	; get data
	asl	temp		; shift char
	rol	a		; shift data
	sta	(pointl),y	; store out data
	jmp	data2

addr	asl	a		; shift char
	rol	pointl		; shift addr
	rol	pointh		; shift addr hi
dat2	dex
	bne	data1		; do 4 times
	beq	datam2		; exit here

addrm	lda	#$01
	bne	datam1

datam	lda	#$00
datam1	sta	mode
datam2	jmp	start

step	jsr	incpt
	jmp	start

gov	jmp	goexec

pccmd	lda	pcl
	sta	pointl
	lda	pch
	sta	pointh
	jmp	start

load	jsr	getch		; look for first char
	cmp	#$3b		; semicolon
	bne	load
	lda	#$00
	sta	chksum
	sta	chkhi
	jsr	getbyt		; get byte cnt
	tax			; save in x index
	jsr	chk		; compute checksum
	jsr	getbyt		; get address hi
	sta	pointh
	jsr	chk
	jsr	getbyt		; get address lo
	sta	pointl
	jsr	chk
	txa			; if cnt=0 dont
	beq	load3		; get any data

load2	jsr	getbyt		; get data
	sta	(pointl),y	; store data
	jsr	chk
	jsr	incpt
	dex
	bne	load2
	inx

