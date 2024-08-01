
@ Macro to ease new words defs.
@
@ NOTE : IF dictionary data start at RAM address 0 a dummy word should be insterted as the first dictionary entry because
@        linked word address for the second word will be 0; first word will never be checked unless a dummy word is added

@ FORTH WORD CONSTANTS
.equ FORTH_WORD_FLAG_IMMEDIATE, 0
.equ FORTH_WORD_FLAG_DEFAULT, 1

.equ forth_word_counter, 0

@ generate word dictionary data
@ word code should be put after
@ the macro call and should end
@ with a "ret" macro call
.macro forth_word string, flag=FORTH_WORD_FLAG_DEFAULT
    .if forth_word_counter != 0  @ computed prev. word address
        .if forth_word_counter % 2 == 0
            888:
            .word 666b
        .else
            666:
            .word 888b
        .endif
    .else
        888:
        .word 0
    .endif
    .byte \flag                 @ word flags (1=default, 0=immediate)
    .byte (3f - 2f - 1)         @ word length
    2: .asciz "\string"         @ word
    3:
    .align 2
    .set forth_word_counter, forth_word_counter + 1
    @ ... code
.endm

@ to manipulate the return stack
.macro rpush r
    stmdb r0!, { \r }
.endm

.macro ret
    ldmia r0!, { pc }
.endm
