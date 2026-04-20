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

@ ================ GENERATED CODE
lit_code:                       @ generated code for LIT (compile mode)
    .word 0xe52d4004            @ opcode: push { r4 }
    .word 0xe59f4000            @ opcode: ldr r4, [pc, #0]
    .word 0xe28ff000            @ opcode: add pc, #0 @ value is stored after this instruction, it is stored by "compile" from r12

cal_code:                       @ generated code for word call (compile mode)
    .word 0xe28f5004            @ opcode: add r5, pc, #4
    .word 0xe9200020            @ opcode: stmdb r0!, {r5}
                                @ can be replaced safely on later ARM (not ARMv2) by :
@    .word 0xe520f004            @ opcode: str pc, [r0, #-4]! @ branch instruction is stored after this

@ =================== FIND A WORD ; start from dict. last word then up until the first one
@        last dict. word addr: r2
@                 word length: r5
@             word start addr: r9
@ ===================== ON RETURN
@        found word dict addr:r12
@    found word name end addr:r10
@ ===================== CLOBBERED
@       r6, r7, r8, r10, r11, r12
@ ===============================
find_word:
    mov r12, r2
    0:
        ldrb r7, [r12, #5]      @ get dict. word len.
        cmp r5, r7              @ word length match ?
        bne 2f                  @ skip word if not
        add r10, r12, #6        @ get dict. word addr.
        mov r11, r9             @ get input word addr.
        1:
            ldrb r6, [r11], #1  @ input word char.
            ldrb r8, [r10], #1  @ dict. word char.
            cmp r6, r8
            bne 2f              @ skip word on != char.
            subs r7, #1
            bne 1b
            b eval_word         @ found
        2:
        ldr r11, [r12]          @ get offset to previous dict. word
        cmp r11, #0
        subne r12, r12, r11     @ compute previous dict. word addr.
        bne 0b                  @ continue until no words to search for

@ ================== PARSE NUMBER
@                compile mode: r3
@                 word length: r5
@             word start addr: r9
@                          0: r11
@ ===================== ON RETURN
@ ===================== CLOBBERED
@       r4, r5, r7, r10, r11, r14
@ ===============================
parse_number:
    0:
        ldrb r7, [r9, #1]!      @ get char. (first char. is skipped, it is optional but safer; all numbers should be prefixed to avoid collisions with regular words)
        subs r10, r7, #87       @ get char. numeric value (a-f)
        sublts r10, r7, #'0'    @ get char. numeric value (0-9)
        addge r11,r10,r11,LSL #4@ n * 16 + v
        subs r5, r5, #1
        bgt 0b
        cmp r3, #FORTH_IMM_MODE @ immediate mode ?
        pusheq { r4 }           @ immediate mode: push old value to Forth stack
        moveq r4, r11           @ immediate mode: push new value to Forth stack
    1:                          @ else: compile mode
        adrne r10, lit_code
        ldmneia r10, {r8-r10}   @ load generated code
        stmneia r14!, {r8-r11}  @ store generated code at current definition code addr.
        b read_word

@ =============== EVALUATE A WORD
@                compile mode: r3
@    found word name end addr:r10 ; unaligned ok, will be aligned later
@        found word dict addr:r12
@ ===================== ON RETURN
@ ===================== CLOBBERED
@  r0, r5, r8, r10, r11, r12, r14
@ ===============================
eval_word:
    add r10, r10, #4            @ adjust word name end addr for alignment
    cmp r3, #FORTH_IMM_MODE     @ immediate mode ?
    ldrb r8, [r12, #4]          @ get word flag
    cmpne r8, #0                @ in compile mode : is an immediate word ?
        adreq r5, read_word     @ it is an immediate so get return address
        stmeqdb r0!, { r5 }     @ push return address
        biceq pc, r10, #3       @ align (point to code addr.) and jump to word code
    compile_word:
        bic r12, r10, #3        @ align (point to code addr.)
        sub r12, r14, r12       @ compute offset
        add r12, r12, #16       @ adjust
        mov r12, r12, lsr #2    @ word adjust
        rsb r12,r12,#0xeb000000 @ b instruction (negative offset)
        adr r10, cal_code
        ldmia r10, {r10-r11}    @ load template code
        stmia r14!, {r10-r12}   @ store generated code
@ with str pc... (replace adr r10... up to stmia r14!...)
@        ldr r11, [pc, #4]
@        stmia r14!, {r11-r12}   @ store generated code

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
@ ===================== ON RETURN
@                    end addr: r1
@                 word length: r5
@             word start addr: r9
@             cond. flags updated
@ ===================== CLOBBERED
@                      r5, r8, r9
@ ===============================
    read_word:
.if SKIP_SPACES
        0:
            ldrb r8, [r1]
            cmp r8, #0
            beq exit
            cmp r8, #' '
            addls r1, r1, #1
            bls 0b
.endif

        mov r9, r1
        0:
            ldrb r8, [r1], #1
            subs r8, #' '
            bgt 0b
        subs r5, r1, r9         @ get word length; update condition flags
        subgts r5, #1           @ adjust if not empty
        bne find_word           @ find word if len > 0
    exit:
    ldmia r0!, { pc }           @ "ret"
