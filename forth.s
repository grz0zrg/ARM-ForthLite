@  _____          _   _     
@ |  ___|__  _ __| |_| |__  
@ | |_ / _ \| '__| __| '_ \ 
@ |  _| (_) | |  | |_| | | |
@ |_|  \___/|_|   \__|_| |_|
@ -------------------------->
@ ARM based Forth core by grz

@ =============== EXECUTION MODES
.equ FORTH_IMM_MODE, 0
.equ FORTH_COM_MODE, 1

@ =================== FIND A WORD
@        last dict. word addr: r2
@                 word length: r5
@             word start addr: r9
@ ===================== ON RETURN
@        found word dict addr: r6
@    found word name end addr:r10
@ ===================== CLOBBERED
@       r6, r7, r8, r10, r11, r12
@ ===============================
find_word:
    mov r6, r2
    0:
        ldrb r7, [r6, #5]       @ get dict. word len.
        cmp r5, r7              @ word length match ?
        bne 2f                  @ skip word if not
        add r10, r6, #6         @ get dict word addr.
        mov r11, r9             @ get word addr.
        1:
            ldrb r12, [r11], #1 @ word char.
            ldrb r8, [r10], #1  @ dict. word char.
            cmp r12, r8
            bne 2f              @ skip word on != char.
            subs r7, #1
            bne 1b
            b eval_word         @ found
        2:
        ldr r6, [r6]            @ get previous dict. word addr.
        cmp r6, #0
        bne 0b                  @ continue until no words to search for

@ ================== PARSE NUMBER
@                compile mode: r3
@                 word length: r5
@             word start addr: r9
@                          0: r10
@ ===================== ON RETURN
@ ===================== CLOBBERED
@             r4, r5, r6, r7, r10
@ ===============================
parse_number:
    0:
        ldrb r7, [r9], #1       @ get char.
        subs r10, r7, #87       @ get char. numeric value (a-f)
        sublts r10, r7, #'0'    @ get char. numeric value (0-9)
        addge r6,r10,r6,LSL #4  @ n * 16 + v
        subs r5, #1
        bgt 0b
        cmp r3, #FORTH_IMM_MODE @ immediate mode ?
        pusheq { r4 }           @ immediate mode: push old value to Forth stack
        moveq r4, r6            @ immediate mode: push new value to Forth stack
        beq read_word
    1:                          @ else: compile mode
        adr r10, 2f
        b compile
    2:                          @ generated code (compile mode)
        .word 0xe52d4004        @ opcode: push { r4 }
        .word 0xe59f4000        @ opcode: ldr r4, [pc, #0]
        .word 0xe28ff000        @ opcode: add pc, #0
        .word 0                 @ value

@ =============== EVALUATE A WORD
@                compile mode: r3
@        found word dict addr: r6
@    found word name end addr:r10 ; unaligned ok, will be aligned later
@ ===================== ON RETURN
@ ===================== CLOBBERED
@         r0, r5, r6, r8, r9, r10
@ ===============================
eval_word:
    add r10, #3                 @ adjust word name end addr for alignment
    cmp r3, #FORTH_IMM_MODE     @ immediate mode ?
    ldrb r8, [r6, #4]           @ get word flag
    andnes r9, r8, #0xff        @ in compile mode : is an immediate word ?
    bne compile_word
        adr r5, read_word
        stmdb r0!, { r5 }       @ push return address
        bic pc, r10, #3         @ align (point to code addr.) and jump to word code
    compile_word:
        bic r6, r10, #3         @ align (point to code addr.)
        adr r10, 1f
        b compile
    1:                          @ generated code (compile mode)
    .word 0xe28f5008            @ opcode: add r5, pc, #8
    .word 0xe9200020            @ opcode: stmdb r0!, {r5}
    .word 0xe51ff004            @ opcode: ldr pc, [pc, #-4]
    .word 0                     @ word code addr. to jump to

@ ======================= COMPILE
@              value to store: r6
@        generated code addr.:r10
@    current def. code addr. :r14
@ ===================== ON RETURN
@ ===================== CLOBBERED
@          r9, r10, r11, r12, r14
@ ===============================
compile:
    str r6, [r10, #12]          @ store value
    ldmia r10, {r9-r12}         @ load generated code
    stmia r14!, {r9-r12}        @ store generated code at current definition code addr.

@ ============= FORTH INTERPRETER
@             data stack addr: sp
@           return stack addr: r0
@           input buffer addr: r1
@         dict last word addr: r2
@                compile mode: r3
@             stack top value: r4
@            dict. end addr. :r14
@ ===================== ON RETURN
@ ===============================
forth:
@ =================== READ A WORD ; any printable characters delimited by ' '
@ ===================== ON RETURN ; does not skip / trim extra spaces !
@                    end addr: r1
@                 word length: r5
@             word start addr: r9
@             cond. flags updated
@ ===================== CLOBBERED
@                      r5, r8, r9
@ ===============================
    read_word:
        mov r9, r1
        0:
            ldrb r8, [r1], #1
            subs r8, #' '
            bgt 0b
        subs r5, r1, r9         @ get word length; update condition flags
        subgts r5, #1           @ adjust if not empty
        bne find_word           @ find word if len > 0
    ldr pc, [pc, #-4]           @ jump to return addr.

forth_retn_addr:
    .word 0
