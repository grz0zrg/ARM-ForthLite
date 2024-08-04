@ FORTH MEMORY LAYOUT CONSTANTS
.equ FORTH_DATA_STACK_SIZE, 1024 * 1000 @ always multiple of 4
.equ FORTH_RETN_STACK_SIZE, 1024 * 1000 @ always multiple of 4
.equ FORTH_DICT_SIZE, 1024 * 1000       @ always multiple of 4

.global _start

_start:
    @ = FORTH SETUP
    @ data stack
    adr r8, forth_data_stack_addr
    ldr sp, [r8]
    @ return stack
    adr r8, forth_retn_stack_addr
    ldr r0, [r8]
    @ input code
    adr r1, forth_code
    @ dict last word
    adr r2, forth_last_word_addr
    @ compile flag
    mov r3, #FORTH_IMM_MODE
    @ dict end
    adr lr, forth_dict_end_addr
    @ save return addr.
    adr r9, forth_retn_addr
    str pc, [r9]
    b forth

    @ r4 has value 1e at this point
    0:
        b 0b

    .include "forth.s"

    forth_data_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE)
    forth_retn_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE + FORTH_RETN_STACK_SIZE)
    @ code to evaluate :
    forth_code:
        .asciz ": 1e f f + ; 1e"
        .align 2

    @ = FORTH DICTIONARY
    .include "dict.inc"
_end:
