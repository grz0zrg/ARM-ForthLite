@ sectorforth dictionary : https://github.com/cesarblum/sectorforth
@ the few primitives that sectorforth use to bootstrap itself
@ NOTE : difference with sectorforth
@        is that VARS stuff cannot be modified so this limit the use case a bit here...
@        these are all stored in registers on this forth implementation so some
@        more words may be required to modify them, this would result in slightly different
@        implementation of the sectorforth examples.

.include "../dict_utils.inc"

@ ======================= COMPILE
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
    ret_opcode:
    ret

forth_word ";" FORTH_WORD_FLAG_IMMEDIATE
    mov r3, #FORTH_IMM_MODE     @ switch to immediate mode
    adr r7, ret_opcode
    ldr r7, [r7]                @ get opcode for RET instruction   
    str r7, [r14], #4           @ store RET opcode at the end of the newly defined word
    ret

@ ======================== MEMORY
forth_word "@"
    ldr r4, [r4]
    ret

forth_word "!"
    pop { r5, r6 }
    str r5, [r4]
    mov r4, r6
    ret

@ ========================= STACK
forth_word "sp@"
    push { r4 }
    mov r4, sp
    ret

forth_word "rp@"
	push { r4 }
    mov r4, r0
    ret
	
@ ========================== VARS
forth_word "state"
	push { r4 }
    mov r4, r3
    ret

forth_word ">in"
	push { r4 }
    mov r4, r1
    ret

forth_word "here"
	push { r4 }
    mov r4, r14
    ret

forth_word "latest"
	push { r4 }
    mov r4, r2
    ret

@ ================== FLOW CONTROL
forth_word "0#"
    cmp r4, #0
    movne r4, #-1
    ret

@ =========================== I/O
forth_word "key"
    @ ... getc ...
    ret

forth_word "emit"
    @ ... putc ...
    ret

@ ========================== MATH
forth_word "nand"
    pop { r5 }
    and r4, r5, r4
    mvn r4, r4
    ret

forth_last_word_addr:
forth_word "+"
    pop { r5 }
    add r4, r5
    ret
forth_dict_end_addr:
