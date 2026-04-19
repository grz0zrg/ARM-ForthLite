@ ARMv2 dict. generator
@ Tailored for ~3mb (tweak constants for more)

@ FORTH MEMORY LAYOUT CONSTANTS
.equ FORTH_DATA_STACK_SIZE, 1024 * 500 @ always multiple of 4
.equ FORTH_RETN_STACK_SIZE, 1024 * 500 @ always multiple of 4
.equ FORTH_DICT_SIZE, 1024 * 1000      @ always multiple of 4

.global _start

_start:
    @ = RISC OS call to load Forth source
    @ OS_File -> load Forth source at base return stack addr.
    mov r0, #255
    adr r1, forth_filename
    adr r8, forth_retn_stack_addr
    ldr r2, [r8]
    mov r3, #0
    swi #0x8

    @ = FORTH SETUP
    @ data stack
    adr r8, forth_data_stack_addr
    ldr sp, [r8]
    @ return stack
    adr r8, forth_retn_stack_addr
    ldr r0, [r8]
    @ input code
    ldr r1, [r8]
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

    @ save last word offset so we can retrieve it later
    @ this reuse the fact that the first word prev word distance is always 0
    @ note : should be set to 0 again after dict. file is loaded (see armflite.s)
    adr r4, dict_start
    sub r5, r2, r4
    str r5, [r4]
    @ = RISC OS call to save block of memory as a file
    @ OS_File -> save dict. as a raw file
    mov r0, #10
    adr r1, dict_filename
    adr r2, dict_filetype
    ldr r2, [r2]
    mov r5, r14
    swi #0x8

    @ OS_Exit
    swi #0x11

    forth_data_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE)
    forth_retn_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE + FORTH_RETN_STACK_SIZE)
    forth_filename:
        .asciz "@.armv2as"
        .align 2
    dict_filename:
        .asciz "@.dict"
        .align 2
    dict_filetype:
        .word 0xffd
    .pool

    @ = FORTH
    .include "forth.s"

    @ = FORTH DICTIONARY
    dict_start:
        .include "dict.inc"
_end:
