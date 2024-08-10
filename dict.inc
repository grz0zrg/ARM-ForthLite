.include "dict_utils.inc"

forth_word "immediate"
    mov r5, #FORTH_WORD_FLAG_IMMEDIATE
    strb r5, [r2, #4]
    ret_opcode:
    ret

forth_word ":" FORTH_WORD_FLAG_IMMEDIATE
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
    strb r3, [r2, #4]           @ store FORTH_WORD_FLAG_DEFAULT flag (note: use r3 since COM_MODE is same constant)
    add r14, #3
    bic r14, #3                 @ align to get to end of dict. addr.
    ret

forth_word ";" FORTH_WORD_FLAG_IMMEDIATE
    mov r3, #FORTH_IMM_MODE     @ switch to immediate mode
    adr r7, ret_opcode
    ldr r7, [r7]                @ get opcode for RET instruction   
    str r7, [r14], #4           @ store RET opcode at the end of the newly defined word
    ret

forth_last_word_addr:
forth_word "+"
    pop { r5 }
    add r4, r5
    ret
forth_dict_end_addr:
