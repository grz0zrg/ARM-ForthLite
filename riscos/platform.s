forth_word "dump"
    mov r2, r4
    pop { r5 }
    pop { r4 }
    push { r0-r3 }
    mov r0, #10
    adr r1, dump_filename
    swi #0x8
    pop { r0-r3 }
    pop { r4 }
    ret
dump_filename:
    .asciz "@.out"
    .align 2
