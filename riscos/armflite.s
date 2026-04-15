@ ARMv2 and related Forth with ARMv2 bundled assembler for RISC OS
@ Tailored for ~3mb (tweak constants for more)

@ FORTH MEMORY LAYOUT CONSTANTS
.equ FORTH_DATA_STACK_SIZE, 1024 * 500 @ always multiple of 4
.equ FORTH_RETN_STACK_SIZE, 1024 * 500 @ always multiple of 4
.equ FORTH_DICT_SIZE, 1024 * 1000      @ always multiple of 4

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
    stmdb r0!, { pc } @ note : on early ARM (ARMv2) STM with PC store curr. instruction addr. + 12 on stack
    mov r3, #FORTH_IMM_MODE
    @ save return addr. on return stack
    b forth

    @ = RISC OS calls to load / evaluate Forth source
    push { r0-r6 }
    @ OS_File -> load Forth source at base return stack addr.
    mov r0, #255
    adr r1, forth_filename
    adr r8, forth_retn_stack_addr
    ldr r2, [r8]
    mov r3, #0
    swi #0x8
    pop { r0-r6 }

    @ evaluate a program loaded at base return stack addr.
    stmdb r0!, { pc }
    ldr r1, [r8]
    b forth

    @ OS_Exit
    swi #0x11

    forth_data_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE)
    forth_retn_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE + FORTH_RETN_STACK_SIZE)
    forth_filename:
        .asciz "@.fsprog"
        .align 2
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
