.include "dict_utils.inc"

forth_word "immediate"
    mov r5, #FORTH_WORD_FLAG_IMMEDIATE
    strb r5, [r2, #4]
ret_opcode:
    ret

forth_word "create"
    adr r5, store_create_content
    stmdb r0!, { r5 }           @ next call will continue after word creation
    b define_word               @ create dict. word
store_create_content:
    adr r10, lit_code           @ prepare to compile default word behavior (LIT)
    add r12, r14, #20           @ will store addr. of word content (note : will be after RET)
    ldmia r10, {r9-r11}         @ load generated code
    stmia r14!, {r9-r12}        @ store generated code at current definition code addr.
    b end_define_word

forth_word ":" FORTH_WORD_FLAG_IMMEDIATE
define_word:
    str r2, [r14]               @ store previous word addr.
    mov r2, r14                 @ update last dict. word addr.
    add r14, #6                 @ point to name field
    0:
        ldrb r8, [r1], #1
        cmp r8, #' '
        strgtb r8, [r14], #1    @ copy name
        addgt r7, #1            @ update length (note: r7 implied to be 0 before loop which is the case after "find_word")
        bgt 0b
    strb r3, [r14]              @ append \0 (note: use r3 since mode is 0 at this point; nested defs don't make sens)
    strb r7, [r2, #5]           @ store word length
    mov r3, #FORTH_COM_MODE     @ switch to compile mode
    mov r5, #FORTH_WORD_FLAG_DEFAULT
    strb r5, [r2, #4]           @ store FORTH_WORD_FLAG_DEFAULT flag
    add r14, #4
    bic r14, #3                 @ align to get to end of dict. addr.
    ret

forth_word ";" FORTH_WORD_FLAG_IMMEDIATE
end_define_word:
    mov r3, #FORTH_IMM_MODE     @ switch to immediate mode
    adr r7, ret_opcode
    ldr r7, [r7]                @ get opcode for RET instruction
    str r7, [r14], #4           @ store RET opcode at the end of the newly defined word
    ret

@ ============= PLATFORM SPECIFIC
.include "platform.s"

@ ========================== MISC
forth_word "exit"
    ldmia r0!, { r5, r15 }      @ trash return address from return stack and return from higher-level word

forth_word "\\" FORTH_WORD_FLAG_IMMEDIATE
0:  ldrb r5, [r1], #1
    cmp r5, #0x20
    bhs 0b
    ret

@ ========================= STACK
forth_word "dup"
    push { r4 }
    ret

forth_word "drop"
    pop { r4 }
    ret

forth_word "swap"
    mov r5, r4
    pop { r4 }
    push { r5 }
    ret

forth_word "over"
    push { r4 }
    ldr r4, [sp, #4]
    ret

forth_word "rot"
    pop { r5, r6 }
    push { r5 }
    push { r4 }
    mov r4, r6
    ret

@ ================== FLOW CONTROL
forth_word "if" FORTH_WORD_FLAG_IMMEDIATE
    adr r10, if_branch_code
    ldmia r10, {r8-r11}         @ load generated code
    stmia r14!, {r8-r12}        @ store generated code at current definition code addr.
    push { r4 }
cond_end:
    sub r4, r14, #4             @ save jump addr. to TOS for later patching
    ret
    if_branch_code:             @ generated branch code
    .word 0xe3540000            @ opcode: cmp r4, #0
    .word 0xe49d4004            @ opcode: pop { r4 }
    .word 0x128ff004            @ opcode: addne pc, pc, #4 @ go into "if" body if true
    .word 0x051ff004            @ opcode: ldreq pc, [pc, #-4] @ go into "else" or skip if false, addr. to jump to is stored after this from r12, patched later

forth_word "then" FORTH_WORD_FLAG_IMMEDIATE
    str r14, [r4]               @ patch jump addr. saved on TOS
    pop { r4 }
    ret

forth_word "else" FORTH_WORD_FLAG_IMMEDIATE
    adr r10, el_branch_code
    ldr r11, [r10]              @ load generated code
    stmia r14!, {r11-r12}       @ store generated code at current definition code addr.
    str r14, [r4]               @ patch jump addr. saved on TOS
    b cond_end
    el_branch_code:             @ generated branch code
    .word 0xe51ff004            @ opcode: ldr pc, [pc, #-4] @ word code addr. to jump to, stored after this from r12, patched later

forth_word "do" FORTH_WORD_FLAG_IMMEDIATE
    push { r4 }
    adr r10, do_code
    ldmia r10, {r9-r11}         @ load generated code
    stmia r14!, {r9-r11}        @ store generated code at current definition code addr.
    mov r4, r14                 @ keep track of where the loop body start
    ret
    do_code:
    .word 0xe49d5004            @ opcode: pop { r5 }
    .word 0xe9200030            @ opcode: stmdb r0!, { r4, r5 } @ push loop params on return stack
    .word 0xe49d4004            @ opcode: pop { r4 }

forth_word "loop" FORTH_WORD_FLAG_IMMEDIATE
    adr r10, loop_code
    mov r12, r4
    ldmia r10, {r6-r11}         @ load generated code
    stmia r14!, {r6-r12}        @ store generated code at current definition code addr.
    pop { r4 }
    ret
    loop_code:
    .word 0xe8b00060            @ opcode: ldmia r0!, { r5, r6 } @ get loop params
    .word 0xe2855001            @ opcode: add r5, r5, #1 @ increment loop index
    .word 0xe1550006            @ opcode: cmp r6, r5 @ compare with loop limit
    .word 0x19200060            @ opcode: stmnedb r0!, { r5, r6 } @ put loop params back
    .word 0x159ff000            @ opcode: ldrne pc, [pc, #0] @ continue loop, stored after addeq from r12
    .word 0x028ff000            @ opcode: addeq pc, pc, #0 @ exit loop

forth_word "unloop"
    ldmia r0!, { r5, r6, r7 }   @ trash loop params from return stack
    mov pc, r5

forth_word "i"
    push { r4 }
    ldr r4, [r0, #4]            @ get loop param from return stack
    ret

@ ======================== MEMORY
forth_word "allot"
    add r14, r14, r4
    pop { r4 }
    ret

forth_word "here"
    push { r4 }
    mov r4, r14
    ret

forth_word "l@"
    ldr r4, [r4]
    ret

forth_word "l!"
    pop { r5 }
    str r5, [r4]
    pop { r4 }
    ret

@ ========================= LOGIC
forth_word "invert"
    mvn r4, r4
    ret

forth_word "nand"
    pop { r5 }
    and r4, r5, r4
    mvn r4, r4
    ret

forth_word "0="
    cmp r4, #0
    moveq r4, #-1
    movne r4, #0
    ret

forth_word "0>"
    cmp r4, #0
    movgt r4, #-1
    movle r4, #0
    ret

forth_word "u<"
    pop { r5 }
    cmp r4, r5
    movlo r4, #-1
    movhs r4, #0
    ret

forth_word "or"
    pop { r5 }
    orr r4, r5, r4
    ret

forth_word "lshift"
    pop { r5 }
    lsl r4, r5, r4
    ret

forth_word "rshift"
    pop { r5 }
    asr r4, r5, r4
    ret

forth_word "rol"
    pop { r5 }
    rsb r6, r4, #32
    ror r4, r5, r6
    ret

@ ==================== ARITHMETIC
forth_word "negate"
    rsb r4, r4, #0
    ret

forth_word "*"
    pop { r5 }
    mul r4, r5, r4
    ret

forth_word "-"
    pop { r5 }
    sub r4, r5, r4
    ret

forth_last_word_addr:
forth_word "+"
    pop { r5 }
    add r4, r4, r5
    ret
forth_dict_end_addr:
