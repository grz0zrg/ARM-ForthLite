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
    ldr r1, =forth_code
    @ dict last word
    ldr r2, =forth_last_word_addr
    @ dict end
    ldr lr, =forth_dict_end_addr
    @ compile flag
    mov r3, #FORTH_IMM_MODE
    @ save return addr. on return stack
    add r5, pc, #4
    stmdb r0!, { r5 }
    b forth

    0:
        b 0b

    forth_data_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE)
    forth_retn_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE + FORTH_RETN_STACK_SIZE)
    .pool

    @ code to evaluate :
    forth_code:
        .include "program.fs.inc"
        .align 2

    @ = FORTH
    .include "forth.s"

    @ = FORTH DICTIONARY
    .include "dict.inc"
_end:
