@ ARMv2 and related Forth with ARMv2 bundled assembler for RISC OS
@ Tailored for ~3mb (tweak constants for more)
@ "dict,ffd" must be generated with dictgen.s as it bundle an ARMv2 assembler

@ FORTH MEMORY LAYOUT CONSTANTS
.equ FORTH_DATA_STACK_SIZE, 1024 * 500 @ always multiple of 4
.equ FORTH_RETN_STACK_SIZE, 1024 * 500 @ always multiple of 4
.equ FORTH_DICT_SIZE, 1024 * 1000      @ always multiple of 4

.global _start

_start:
    adr r8, forth_retn_stack_addr

    @ = RISC OS call to load / evaluate Forth source
    @ OS_File -> load Forth source at base return stack addr.
    mov r0, #255
    adr r1, forth_filename
    ldr r2, [r8]
    mov r3, #0
    swi #0x8

    @ = FORTH SETUP
    @ return stack
    ldr r0, [r8]
    @ dict end
    adr lr, forth_dict_end_addr
    ldr lr, [lr]
    @ load dict last word addr. (see dictgen.s)
    adr r3, dict_start
    ldr r2, [r3]
    add r2, r2, r3
    @ reset first word prev word distance to 0
    mov r1, #0
    str r1, [r3]
    @ compile flag
    mov r3, #FORTH_IMM_MODE
    @ Forth source address
    ldr r1, [r8]
    @ data stack
    adr r8, forth_data_stack_addr
    ldr sp, [r8]
    @ save return addr. on return stack
    add r5, pc, #4
    stmdb r0!, { r5 }
    @ evaluate the program loaded at base return stack addr.
    b forth

    @ = RISC OS calls to get stacks depth as strings
    adr r5, forth_retn_stack_addr
    ldr r5, [r5]
    sub r0, r5, r0
    mov r0, r0, asr #2
    adr r1, retrn_depth_addr
    mov r2, #12
    @ OS_BinaryToDecimal
    swi #0x28
    adr r0, forth_data_stack_addr
    ldr r0, [r0]
    sub r0, r0, sp
    mov r0, r0, asr #2
    adr r1, stack_depth_addr
    mov r2, #12
    @ OS_BinaryToDecimal
    swi #0x28

    @ = RISC OS call to output log file
    mov r0, #10
    adr r1, log_filename
    adr r2, log_filetype
    ldr r2, [r2]
    adr r4, log_content_start
    adr r5, log_content_end
    @ OS_File
    swi #0x8

    @ OS_Exit
    swi #0x11

    @ = LOG FILE RELATED CONTENT
    log_content_start:
    .ascii "stack depth: "
    stack_depth_addr:
    .space 11, 0x20
    .ascii "\nretrn depth: "
    retrn_depth_addr:
    .space 10, 0x20
    .ascii "\n"
    log_content_end:
    .align 2
    log_filetype:
        .word 0xfff
    log_filename:
        .asciz "@.armflog"
        .align 2

    @ = FORTH CTX. CONFIG.
    forth_data_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE)
    forth_retn_stack_addr:
        .word (_end + FORTH_DICT_SIZE + FORTH_DATA_STACK_SIZE + FORTH_RETN_STACK_SIZE)
    forth_dict_end_addr:
        .word _end
    forth_filename:
        .asciz "@.fsprog"
        .align 2
    .pool

    @ = FORTH
    .include "forth.s"

    @ = FORTH DICTIONARY
    dict_start:
        .incbin "dict,ffd"
_end:
