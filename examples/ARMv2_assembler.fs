\ ---------------------------------------
\ ----------------------- ARMv2 ASSEMBLER
\ ---------------------------------------
\ --------------------------------- UTILS
: variable
    create $4 allot ;
: l,
    here l! $4 allot ;
: fand
    invert swap invert or invert ;
\ shorter but not Gforth compatible:
\    nand invert ;
: ROL32
    $1f fand swap $ffffffff fand swap
    over over lshift $ffffffff fand
    rot rot negate $1f fand rshift or $ffffffff fand ;
: ARM2_ENCODE_IMMEDIATE
    $10 $0 do
        dup i $2 * ROL32 \ 'rol' is shorter but not Gforth compatible
        dup $ffffff00 fand 0= if
            i $8 lshift or
            swap drop
            unloop exit
        then
        drop
    loop
    drop or ;
: ARM2_ENCODE_BO
    $8 negate + $2 rshift $ffffff fand ;
: ARM2_S $100000 or ;
: ARM2_IMMEDIATE $2000000 ;
: ARM2_ENCODE_RS $8 lshift ;
: ARM2_ENCODE_RD $c lshift ;
: ARM2_ENCODE_RN $10 lshift ;
: ARM2_ENCODE_RM_IMM
    ARM2_IMMEDIATE over fand if
        rot ARM2_ENCODE_IMMEDIATE
    else
        or rot $7 lshift
    then or ;
: ARM2_DPI_RN
    ARM2_ENCODE_RN swap
    ARM2_ENCODE_RM_IMM or ;
: ARM2_DPI_RD
    ARM2_ENCODE_RD swap
    ARM2_ENCODE_RM_IMM or ;
: ARM2_DPI_RD_RN
    ARM2_ENCODE_RD swap ARM2_ENCODE_RN or
    swap ARM2_ENCODE_RM_IMM or ;
\ ------------ SINGLE DATA TRANSFER UTILS
\ no supervisor support (T, bit 21)
: ARM2_UD $800000 ;
: ARM2_BW $400000 ;
: ARM2_WB $200000 ;
: ARM2_?UD
    dup 0< if
        negate
    else
        ARM2_UD or
    then ;
: ARM2_OFF?_RM?
    ARM2_IMMEDIATE over fand if
        drop $fdffffff fand swap ARM2_?UD
    else
        ARM2_?UD or or swap $7 lshift
    then or ;
: ARM2_SDT
    ARM2_ENCODE_RD swap ARM2_ENCODE_RN or
    $7000000 or swap
    ARM2_BW over fand if
        drop
        $feffffff fand
        swap
        ARM2_OFF?_RM?
    else
        ARM2_OFF?_RM?
        swap ARM2_WB over fand if
            drop ARM2_WB or
        else
            drop
        then
    then ;
\ ------------- BLOCK DATA TRANSFER UTILS
: ARM2_MDT ARM2_ENCODE_RN
    $10 $0 do
        swap
        dup $10 u< if
            $1 swap lshift or
        else
            ARM2_WB over fand if
                drop ARM2_WB or
            else
                drop
            then
            unloop exit
        then
    loop ;
\ ------------------------ CONDITION CODE
: ARM2_EQ $00000000 or ;
: ARM2_NE $10000000 or ;
: ARM2_CS $20000000 or ;
: ARM2_CC $30000000 or ;
: ARM2_MI $40000000 or ;
: ARM2_PL $50000000 or ;
: ARM2_VS $60000000 or ;
: ARM2_VC $70000000 or ;
: ARM2_HI $80000000 or ;
: ARM2_LS $90000000 or ;
: ARM2_GE $a0000000 or ;
: ARM2_LT $b0000000 or ;
: ARM2_GT $c0000000 or ;
: ARM2_LE $d0000000 or ;
: ARM2_AL $e0000000 or ;
: ARM2_NV $f0000000 or ;
\ ------------- ARM INSTRUCTIONS ENCODING
\ -------------------------------- BRANCH
: ARM2_B $a000000 or ;
: ARM2_BL $b000000 or ;
\ ----------------------- DATA PROCESSING
: ARM2_ADC $a00000 or ;
: ARM2_ADD $800000 or ;
: ARM2_AND $000000 or ;
: ARM2_BIC $1c00000 or ;
: ARM2_CMN $1600000 or ;
: ARM2_CMP $1400000 or ;
: ARM2_EOR $200000 or ;
: ARM2_MOV $1a00000 or ;
: ARM2_MVN $1e00000 or ;
: ARM2_ORR $1800000 or ;
: ARM2_RSB $600000 or ;
: ARM2_RSC $e00000 or ;
: ARM2_SBC $c00000 or ;
: ARM2_SUB $400000 or ;
: ARM2_TEQ $1200000 or ;
: ARM2_TST $1000000 or ;
\ ------------------ SINGLE DATA TRANSFER
: ARM2_STR $000000 or ;
: ARM2_LDR $100000 or ;
: ARM2_STRB $400000 or ;
: ARM2_LDRB $500000 or ;
\ ------------------- BLOCK DATA TRANSFER
: ARM2_LDM $8100000 or ;
: ARM2_STM $8000000 or ;
: ARM2_MIB $1800000 or ;
: ARM2_MIA $800000 or ;
: ARM2_MDB $1000000 or ;
: ARM2_MDA $0000000 or ;
\ -------------------- SOFTWARE INTERRUPT
: ARM2_SWI $f000000 or ;
\ ---------------------------- ARITHMETIC
: ARM2_MUL ARM2_ENCODE_RN or swap
    ARM2_ENCODE_RS or $000090 or ;
: ARM2_MLA ARM2_ENCODE_RN or rot
    ARM2_ENCODE_RD rot ARM2_ENCODE_RS or
    or $200090 or ;
\ ---------------------------- USER UTILS
: !LABEL here swap l! ; immediate 
: @LABEL l@ here - ; immediate
\ -------------------------- INSTRUCTIONS
\ --------------------------------- UTILS
: imm ARM2_IMMEDIATE ; immediate
: lsl $00 ; immediate
: lsr $20 ; immediate
: asr $40 ; immediate
: ror $60 ; immediate
: rrx $60 ; immediate
: lslr $01 ; immediate
: lsrr $30 ; immediate
: aslr $50 ; immediate
: rorr $70 ; immediate
: r0 $0 ; immediate : r1 $1 ; immediate
: r2 $2 ; immediate : r3 $3 ; immediate
: r4 $4 ; immediate : r5 $5 ; immediate
: r6 $6 ; immediate : r7 $7 ; immediate
: r8 $8 ; immediate : r9 $9 ; immediate
: r10 $a ; immediate : r11 $b ; immediate
: r12 $c ; immediate : r13 $d ; immediate
: r14 $e ; immediate : r15 $f ; immediate
: sp $d ; immediate
: lr $e ; immediate
: pc $f ; immediate
\ -------------------------------- BRANCH
: beq ARM2_ENCODE_BO ARM2_EQ ARM2_B l, ; immediate
: bne ARM2_ENCODE_BO ARM2_NE ARM2_B l, ; immediate
: bcs ARM2_ENCODE_BO ARM2_CS ARM2_B l, ; immediate
: bcc ARM2_ENCODE_BO ARM2_CC ARM2_B l, ; immediate
: bmi ARM2_ENCODE_BO ARM2_MI ARM2_B l, ; immediate
: bpl ARM2_ENCODE_BO ARM2_PL ARM2_B l, ; immediate
: bvs ARM2_ENCODE_BO ARM2_VS ARM2_B l, ; immediate
: bvc ARM2_ENCODE_BO ARM2_VC ARM2_B l, ; immediate
: bhi ARM2_ENCODE_BO ARM2_HI ARM2_B l, ; immediate
: bls ARM2_ENCODE_BO ARM2_LS ARM2_B l, ; immediate
: bge ARM2_ENCODE_BO ARM2_GE ARM2_B l, ; immediate
: blt ARM2_ENCODE_BO ARM2_LT ARM2_B l, ; immediate
: bgt ARM2_ENCODE_BO ARM2_GT ARM2_B l, ; immediate
: ble ARM2_ENCODE_BO ARM2_LE ARM2_B l, ; immediate
: b ARM2_ENCODE_BO ARM2_AL ARM2_B l, ; immediate
\ --------------------------- BRANCH LINK
: bleq ARM2_ENCODE_BO ARM2_EQ ARM2_BL l, ; immediate
: blne ARM2_ENCODE_BO ARM2_NE ARM2_BL l, ; immediate
: blcs ARM2_ENCODE_BO ARM2_CS ARM2_BL l, ; immediate
: blcc ARM2_ENCODE_BO ARM2_CC ARM2_BL l, ; immediate
: blmi ARM2_ENCODE_BO ARM2_MI ARM2_BL l, ; immediate
: blpl ARM2_ENCODE_BO ARM2_PL ARM2_BL l, ; immediate
: blvs ARM2_ENCODE_BO ARM2_VS ARM2_BL l, ; immediate
: blvc ARM2_ENCODE_BO ARM2_VC ARM2_BL l, ; immediate
: blhi ARM2_ENCODE_BO ARM2_HI ARM2_BL l, ; immediate
: blls ARM2_ENCODE_BO ARM2_LS ARM2_BL l, ; immediate
: blge ARM2_ENCODE_BO ARM2_GE ARM2_BL l, ; immediate
: bllt ARM2_ENCODE_BO ARM2_LT ARM2_BL l, ; immediate
: blgt ARM2_ENCODE_BO ARM2_GT ARM2_BL l, ; immediate
: blle ARM2_ENCODE_BO ARM2_LE ARM2_BL l, ; immediate
: bl ARM2_ENCODE_BO ARM2_AL ARM2_BL l, ; immediate
\ ------------------ SINGLE DATA TRANSFER
: [] ARM2_BW ; immediate
: [!] ARM2_BW ARM2_WB or ; immediate

: streq ARM2_SDT ARM2_EQ ARM2_STR l, ; immediate
: strne ARM2_SDT ARM2_NE ARM2_STR l, ; immediate
: strcs ARM2_SDT ARM2_CS ARM2_STR l, ; immediate
: strcc ARM2_SDT ARM2_CC ARM2_STR l, ; immediate
: strmi ARM2_SDT ARM2_MI ARM2_STR l, ; immediate
: strpl ARM2_SDT ARM2_PL ARM2_STR l, ; immediate
: strvs ARM2_SDT ARM2_VS ARM2_STR l, ; immediate
: strvc ARM2_SDT ARM2_VC ARM2_STR l, ; immediate
: strhi ARM2_SDT ARM2_HI ARM2_STR l, ; immediate
: strls ARM2_SDT ARM2_LS ARM2_STR l, ; immediate
: strge ARM2_SDT ARM2_GE ARM2_STR l, ; immediate
: strlt ARM2_SDT ARM2_LT ARM2_STR l, ; immediate
: strgt ARM2_SDT ARM2_GT ARM2_STR l, ; immediate
: strle ARM2_SDT ARM2_LE ARM2_STR l, ; immediate
: str ARM2_SDT ARM2_AL ARM2_STR l, ; immediate

: streqb ARM2_SDT ARM2_EQ ARM2_STRB l, ; immediate
: strneb ARM2_SDT ARM2_NE ARM2_STRB l, ; immediate
: strcsb ARM2_SDT ARM2_CS ARM2_STRB l, ; immediate
: strccb ARM2_SDT ARM2_CC ARM2_STRB l, ; immediate
: strmib ARM2_SDT ARM2_MI ARM2_STRB l, ; immediate
: strplb ARM2_SDT ARM2_PL ARM2_STRB l, ; immediate
: strvsb ARM2_SDT ARM2_VS ARM2_STRB l, ; immediate
: strvcb ARM2_SDT ARM2_VC ARM2_STRB l, ; immediate
: strhib ARM2_SDT ARM2_HI ARM2_STRB l, ; immediate
: strlsb ARM2_SDT ARM2_LS ARM2_STRB l, ; immediate
: strgeb ARM2_SDT ARM2_GE ARM2_STRB l, ; immediate
: strltb ARM2_SDT ARM2_LT ARM2_STRB l, ; immediate
: strgtb ARM2_SDT ARM2_GT ARM2_STRB l, ; immediate
: strleb ARM2_SDT ARM2_LE ARM2_STRB l, ; immediate
: strb ARM2_SDT ARM2_AL ARM2_STRB l, ; immediate

: ldreq ARM2_SDT ARM2_EQ ARM2_LDR l, ; immediate
: ldrne ARM2_SDT ARM2_NE ARM2_LDR l, ; immediate
: ldrcs ARM2_SDT ARM2_CS ARM2_LDR l, ; immediate
: ldrcc ARM2_SDT ARM2_CC ARM2_LDR l, ; immediate
: ldrmi ARM2_SDT ARM2_MI ARM2_LDR l, ; immediate
: ldrpl ARM2_SDT ARM2_PL ARM2_LDR l, ; immediate
: ldrvs ARM2_SDT ARM2_VS ARM2_LDR l, ; immediate
: ldrvc ARM2_SDT ARM2_VC ARM2_LDR l, ; immediate
: ldrhi ARM2_SDT ARM2_HI ARM2_LDR l, ; immediate
: ldrls ARM2_SDT ARM2_LS ARM2_LDR l, ; immediate
: ldrge ARM2_SDT ARM2_GE ARM2_LDR l, ; immediate
: ldrlt ARM2_SDT ARM2_LT ARM2_LDR l, ; immediate
: ldrgt ARM2_SDT ARM2_GT ARM2_LDR l, ; immediate
: ldrle ARM2_SDT ARM2_LE ARM2_LDR l, ; immediate
: ldr ARM2_SDT ARM2_AL ARM2_LDR l, ; immediate

: ldreqb ARM2_SDT ARM2_EQ ARM2_LDRB l, ; immediate
: ldrneb ARM2_SDT ARM2_NE ARM2_LDRB l, ; immediate
: ldrcsb ARM2_SDT ARM2_CS ARM2_LDRB l, ; immediate
: ldrccb ARM2_SDT ARM2_CC ARM2_LDRB l, ; immediate
: ldrmib ARM2_SDT ARM2_MI ARM2_LDRB l, ; immediate
: ldrplb ARM2_SDT ARM2_PL ARM2_LDRB l, ; immediate
: ldrvsb ARM2_SDT ARM2_VS ARM2_LDRB l, ; immediate
: ldrvcb ARM2_SDT ARM2_VC ARM2_LDRB l, ; immediate
: ldrhib ARM2_SDT ARM2_HI ARM2_LDRB l, ; immediate
: ldrlsb ARM2_SDT ARM2_LS ARM2_LDRB l, ; immediate
: ldrgeb ARM2_SDT ARM2_GE ARM2_LDRB l, ; immediate
: ldrltb ARM2_SDT ARM2_LT ARM2_LDRB l, ; immediate
: ldrgtb ARM2_SDT ARM2_GT ARM2_LDRB l, ; immediate
: ldrleb ARM2_SDT ARM2_LE ARM2_LDRB l, ; immediate
: ldrb ARM2_SDT ARM2_AL ARM2_LDRB l, ; immediate
\ ------------------- BLOCK DATA TRANSFER
\ no PSD / force user mode (bit 22)
: {} $200001 negate ; immediate
: !{} ARM2_WB ; immediate
: stmeqda ARM2_MDT ARM2_MDA ARM2_STM ARM2_EQ l, ; immediate
: stmneda ARM2_MDT ARM2_MDA ARM2_STM ARM2_NE l, ; immediate
: stmcsda ARM2_MDT ARM2_MDA ARM2_STM ARM2_CS l, ; immediate
: stmccda ARM2_MDT ARM2_MDA ARM2_STM ARM2_CC l, ; immediate
: stmmida ARM2_MDT ARM2_MDA ARM2_STM ARM2_MI l, ; immediate
: stmplda ARM2_MDT ARM2_MDA ARM2_STM ARM2_PL l, ; immediate
: stmvsda ARM2_MDT ARM2_MDA ARM2_STM ARM2_VS l, ; immediate
: stmvcda ARM2_MDT ARM2_MDA ARM2_STM ARM2_VC l, ; immediate
: stmhida ARM2_MDT ARM2_MDA ARM2_STM ARM2_HI l, ; immediate
: stmlsda ARM2_MDT ARM2_MDA ARM2_STM ARM2_LS l, ; immediate
: stmgeda ARM2_MDT ARM2_MDA ARM2_STM ARM2_GE l, ; immediate
: stmltda ARM2_MDT ARM2_MDA ARM2_STM ARM2_LT l, ; immediate
: stmgtda ARM2_MDT ARM2_MDA ARM2_STM ARM2_GT l, ; immediate
: stmleda ARM2_MDT ARM2_MDA ARM2_STM ARM2_LE l, ; immediate
: stmda ARM2_MDT ARM2_MDA ARM2_STM ARM2_AL l, ; immediate

: stmeqdb ARM2_MDT ARM2_MDB ARM2_STM ARM2_EQ l, ; immediate
: stmnedb ARM2_MDT ARM2_MDB ARM2_STM ARM2_NE l, ; immediate
: stmcsdb ARM2_MDT ARM2_MDB ARM2_STM ARM2_CS l, ; immediate
: stmccdb ARM2_MDT ARM2_MDB ARM2_STM ARM2_CC l, ; immediate
: stmmidb ARM2_MDT ARM2_MDB ARM2_STM ARM2_MI l, ; immediate
: stmpldb ARM2_MDT ARM2_MDB ARM2_STM ARM2_PL l, ; immediate
: stmvsdb ARM2_MDT ARM2_MDB ARM2_STM ARM2_VS l, ; immediate
: stmvcdb ARM2_MDT ARM2_MDB ARM2_STM ARM2_VC l, ; immediate
: stmhidb ARM2_MDT ARM2_MDB ARM2_STM ARM2_HI l, ; immediate
: stmlsdb ARM2_MDT ARM2_MDB ARM2_STM ARM2_LS l, ; immediate
: stmgedb ARM2_MDT ARM2_MDB ARM2_STM ARM2_GE l, ; immediate
: stmltdb ARM2_MDT ARM2_MDB ARM2_STM ARM2_LT l, ; immediate
: stmgtdb ARM2_MDT ARM2_MDB ARM2_STM ARM2_GT l, ; immediate
: stmledb ARM2_MDT ARM2_MDB ARM2_STM ARM2_LE l, ; immediate
: stmdb ARM2_MDT ARM2_MDB ARM2_STM ARM2_AL l, ; immediate

: stmeqib ARM2_MDT ARM2_MIB ARM2_STM ARM2_EQ l, ; immediate
: stmneib ARM2_MDT ARM2_MIB ARM2_STM ARM2_NE l, ; immediate
: stmcsib ARM2_MDT ARM2_MIB ARM2_STM ARM2_CS l, ; immediate
: stmccib ARM2_MDT ARM2_MIB ARM2_STM ARM2_CC l, ; immediate
: stmmiib ARM2_MDT ARM2_MIB ARM2_STM ARM2_MI l, ; immediate
: stmplib ARM2_MDT ARM2_MIB ARM2_STM ARM2_PL l, ; immediate
: stmvsib ARM2_MDT ARM2_MIB ARM2_STM ARM2_VS l, ; immediate
: stmvcib ARM2_MDT ARM2_MIB ARM2_STM ARM2_VC l, ; immediate
: stmhiib ARM2_MDT ARM2_MIB ARM2_STM ARM2_HI l, ; immediate
: stmlsib ARM2_MDT ARM2_MIB ARM2_STM ARM2_LS l, ; immediate
: stmgeib ARM2_MDT ARM2_MIB ARM2_STM ARM2_GE l, ; immediate
: stmltib ARM2_MDT ARM2_MIB ARM2_STM ARM2_LT l, ; immediate
: stmgtib ARM2_MDT ARM2_MIB ARM2_STM ARM2_GT l, ; immediate
: stmleib ARM2_MDT ARM2_MIB ARM2_STM ARM2_LE l, ; immediate
: stmib ARM2_MDT ARM2_MIB ARM2_STM ARM2_AL l, ; immediate

: stmeqia ARM2_MDT ARM2_MIA ARM2_STM ARM2_EQ l, ; immediate
: stmneia ARM2_MDT ARM2_MIA ARM2_STM ARM2_NE l, ; immediate
: stmcsia ARM2_MDT ARM2_MIA ARM2_STM ARM2_CS l, ; immediate
: stmccia ARM2_MDT ARM2_MIA ARM2_STM ARM2_CC l, ; immediate
: stmmiia ARM2_MDT ARM2_MIA ARM2_STM ARM2_MI l, ; immediate
: stmplia ARM2_MDT ARM2_MIA ARM2_STM ARM2_PL l, ; immediate
: stmvsia ARM2_MDT ARM2_MIA ARM2_STM ARM2_VS l, ; immediate
: stmvcia ARM2_MDT ARM2_MIA ARM2_STM ARM2_VC l, ; immediate
: stmhiia ARM2_MDT ARM2_MIA ARM2_STM ARM2_HI l, ; immediate
: stmlsia ARM2_MDT ARM2_MIA ARM2_STM ARM2_LS l, ; immediate
: stmgeia ARM2_MDT ARM2_MIA ARM2_STM ARM2_GE l, ; immediate
: stmltia ARM2_MDT ARM2_MIA ARM2_STM ARM2_LT l, ; immediate
: stmgtia ARM2_MDT ARM2_MIA ARM2_STM ARM2_GT l, ; immediate
: stmleia ARM2_MDT ARM2_MIA ARM2_STM ARM2_LE l, ; immediate
: stmia ARM2_MDT ARM2_MIA ARM2_STM ARM2_AL l, ; immediate

: ldmeqda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_EQ l, ; immediate
: ldmneda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_NE l, ; immediate
: ldmcsda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_CS l, ; immediate
: ldmccda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_CC l, ; immediate
: ldmmida ARM2_MDT ARM2_MDA ARM2_LDM ARM2_MI l, ; immediate
: ldmplda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_PL l, ; immediate
: ldmvsda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_VS l, ; immediate
: ldmvcda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_VC l, ; immediate
: ldmhida ARM2_MDT ARM2_MDA ARM2_LDM ARM2_HI l, ; immediate
: ldmlsda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_LS l, ; immediate
: ldmgeda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_GE l, ; immediate
: ldmltda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_LT l, ; immediate
: ldmgtda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_GT l, ; immediate
: ldmleda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_LE l, ; immediate
: ldmda ARM2_MDT ARM2_MDA ARM2_LDM ARM2_AL l, ; immediate

: ldmeqdb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_EQ l, ; immediate
: ldmnedb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_NE l, ; immediate
: ldmcsdb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_CS l, ; immediate
: ldmccdb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_CC l, ; immediate
: ldmmidb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_MI l, ; immediate
: ldmpldb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_PL l, ; immediate
: ldmvsdb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_VS l, ; immediate
: ldmvcdb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_VC l, ; immediate
: ldmhidb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_HI l, ; immediate
: ldmlsdb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_LS l, ; immediate
: ldmgedb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_GE l, ; immediate
: ldmltdb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_LT l, ; immediate
: ldmgtdb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_GT l, ; immediate
: ldmledb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_LE l, ; immediate
: ldmdb ARM2_MDT ARM2_MDB ARM2_LDM ARM2_AL l, ; immediate

: ldmeqib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_EQ l, ; immediate
: ldmneib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_NE l, ; immediate
: ldmcsib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_CS l, ; immediate
: ldmccib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_CC l, ; immediate
: ldmmiib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_MI l, ; immediate
: ldmplib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_PL l, ; immediate
: ldmvsib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_VS l, ; immediate
: ldmvcib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_VC l, ; immediate
: ldmhiib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_HI l, ; immediate
: ldmlsib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_LS l, ; immediate
: ldmgeib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_GE l, ; immediate
: ldmltib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_LT l, ; immediate
: ldmgtib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_GT l, ; immediate
: ldmleib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_LE l, ; immediate
: ldmib ARM2_MDT ARM2_MIB ARM2_LDM ARM2_AL l, ; immediate

: ldmeqia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_EQ l, ; immediate
: ldmneia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_NE l, ; immediate
: ldmcsia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_CS l, ; immediate
: ldmccia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_CC l, ; immediate
: ldmmiia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_MI l, ; immediate
: ldmplia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_PL l, ; immediate
: ldmvsia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_VS l, ; immediate
: ldmvcia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_VC l, ; immediate
: ldmhiia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_HI l, ; immediate
: ldmlsia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_LS l, ; immediate
: ldmgeia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_GE l, ; immediate
: ldmltia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_LT l, ; immediate
: ldmgtia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_GT l, ; immediate
: ldmleia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_LE l, ; immediate
: ldmia ARM2_MDT ARM2_MIA ARM2_LDM ARM2_AL l, ; immediate
\ -------------------- SOFTWARE INTERRUPT
: swieq ARM2_EQ ARM2_SWI l, ; immediate
: swine ARM2_NE ARM2_SWI l, ; immediate
: swics ARM2_CS ARM2_SWI l, ; immediate
: swicc ARM2_CC ARM2_SWI l, ; immediate
: swimi ARM2_MI ARM2_SWI l, ; immediate
: swipl ARM2_PL ARM2_SWI l, ; immediate
: swivs ARM2_VS ARM2_SWI l, ; immediate
: swivc ARM2_VC ARM2_SWI l, ; immediate
: swihi ARM2_HI ARM2_SWI l, ; immediate
: swils ARM2_LS ARM2_SWI l, ; immediate
: swige ARM2_GE ARM2_SWI l, ; immediate
: swilt ARM2_LT ARM2_SWI l, ; immediate
: swigt ARM2_GT ARM2_SWI l, ; immediate
: swile ARM2_LE ARM2_SWI l, ; immediate
: swi ARM2_AL ARM2_SWI l, ; immediate
\ ---------------------------- ARITHMETIC
: muleq ARM2_MUL ARM2_EQ l, ; immediate
: mulne ARM2_MUL ARM2_NE l, ; immediate
: mulcs ARM2_MUL ARM2_CS l, ; immediate
: mulcc ARM2_MUL ARM2_CC l, ; immediate
: mulmi ARM2_MUL ARM2_MI l, ; immediate
: mulpl ARM2_MUL ARM2_PL l, ; immediate
: mulvs ARM2_MUL ARM2_VS l, ; immediate
: mulvc ARM2_MUL ARM2_VC l, ; immediate
: mulhi ARM2_MUL ARM2_HI l, ; immediate
: mulls ARM2_MUL ARM2_LS l, ; immediate
: mulge ARM2_MUL ARM2_GE l, ; immediate
: mullt ARM2_MUL ARM2_LT l, ; immediate
: mulgt ARM2_MUL ARM2_GT l, ; immediate
: mulle ARM2_MUL ARM2_LE l, ; immediate
: mul ARM2_MUL ARM2_AL l, ; immediate

: mlaeq ARM2_MLA ARM2_EQ l, ; immediate
: mlane ARM2_MLA ARM2_NE l, ; immediate
: mlacs ARM2_MLA ARM2_CS l, ; immediate
: mlacc ARM2_MLA ARM2_CC l, ; immediate
: mlami ARM2_MLA ARM2_MI l, ; immediate
: mlapl ARM2_MLA ARM2_PL l, ; immediate
: mlavs ARM2_MLA ARM2_VS l, ; immediate
: mlavc ARM2_MLA ARM2_VC l, ; immediate
: mlahi ARM2_MLA ARM2_HI l, ; immediate
: mlals ARM2_MLA ARM2_LS l, ; immediate
: mlage ARM2_MLA ARM2_GE l, ; immediate
: mlalt ARM2_MLA ARM2_LT l, ; immediate
: mlagt ARM2_MLA ARM2_GT l, ; immediate
: mlale ARM2_MLA ARM2_LE l, ; immediate
: mla ARM2_MLA ARM2_AL l, ; immediate
\ ------------------------ ARITHMETIC + S
: muleqs ARM2_MUL ARM2_S ARM2_EQ l, ; immediate
: mulnes ARM2_MUL ARM2_S ARM2_NE l, ; immediate
: mulcss ARM2_MUL ARM2_S ARM2_CS l, ; immediate
: mulccs ARM2_MUL ARM2_S ARM2_CC l, ; immediate
: mulmis ARM2_MUL ARM2_S ARM2_MI l, ; immediate
: mulpls ARM2_MUL ARM2_S ARM2_PL l, ; immediate
: mulvss ARM2_MUL ARM2_S ARM2_VS l, ; immediate
: mulvcs ARM2_MUL ARM2_S ARM2_VC l, ; immediate
: mulhis ARM2_MUL ARM2_S ARM2_HI l, ; immediate
: mullss ARM2_MUL ARM2_S ARM2_LS l, ; immediate
: mulges ARM2_MUL ARM2_S ARM2_GE l, ; immediate
: mullts ARM2_MUL ARM2_S ARM2_LT l, ; immediate
: mulgts ARM2_MUL ARM2_S ARM2_GT l, ; immediate
: mulles ARM2_MUL ARM2_S ARM2_LE l, ; immediate
: muls ARM2_MUL ARM2_S ARM2_AL l, ; immediate

: mlaeqs ARM2_MLA ARM2_S ARM2_EQ l, ; immediate
: mlanes ARM2_MLA ARM2_S ARM2_NE l, ; immediate
: mlacss ARM2_MLA ARM2_S ARM2_CS l, ; immediate
: mlaccs ARM2_MLA ARM2_S ARM2_CC l, ; immediate
: mlamis ARM2_MLA ARM2_S ARM2_MI l, ; immediate
: mlapls ARM2_MLA ARM2_S ARM2_PL l, ; immediate
: mlavss ARM2_MLA ARM2_S ARM2_VS l, ; immediate
: mlavcs ARM2_MLA ARM2_S ARM2_VC l, ; immediate
: mlahis ARM2_MLA ARM2_S ARM2_HI l, ; immediate
: mlalss ARM2_MLA ARM2_S ARM2_LS l, ; immediate
: mlages ARM2_MLA ARM2_S ARM2_GE l, ; immediate
: mlalts ARM2_MLA ARM2_S ARM2_LT l, ; immediate
: mlagts ARM2_MLA ARM2_S ARM2_GT l, ; immediate
: mlales ARM2_MLA ARM2_S ARM2_LE l, ; immediate
: mlas ARM2_MLA ARM2_S ARM2_AL l, ; immediate
\ ----------------------- DATA PROCESSING
: moveq ARM2_DPI_RD ARM2_EQ ARM2_MOV l, ; immediate
: movne ARM2_DPI_RD ARM2_NE ARM2_MOV l, ; immediate
: movcs ARM2_DPI_RD ARM2_CS ARM2_MOV l, ; immediate
: movcc ARM2_DPI_RD ARM2_CC ARM2_MOV l, ; immediate
: movmi ARM2_DPI_RD ARM2_MI ARM2_MOV l, ; immediate
: movpl ARM2_DPI_RD ARM2_PL ARM2_MOV l, ; immediate
: movvs ARM2_DPI_RD ARM2_VS ARM2_MOV l, ; immediate
: movvc ARM2_DPI_RD ARM2_VC ARM2_MOV l, ; immediate
: movhi ARM2_DPI_RD ARM2_HI ARM2_MOV l, ; immediate
: movls ARM2_DPI_RD ARM2_LS ARM2_MOV l, ; immediate
: movge ARM2_DPI_RD ARM2_GE ARM2_MOV l, ; immediate
: movlt ARM2_DPI_RD ARM2_LT ARM2_MOV l, ; immediate
: movgt ARM2_DPI_RD ARM2_GT ARM2_MOV l, ; immediate
: movle ARM2_DPI_RD ARM2_LE ARM2_MOV l, ; immediate
: mov ARM2_DPI_RD ARM2_AL ARM2_MOV l, ; immediate

: mvneq ARM2_DPI_RD ARM2_EQ ARM2_MVN l, ; immediate
: mvnne ARM2_DPI_RD ARM2_NE ARM2_MVN l, ; immediate
: mvncs ARM2_DPI_RD ARM2_CS ARM2_MVN l, ; immediate
: mvncc ARM2_DPI_RD ARM2_CC ARM2_MVN l, ; immediate
: mvnmi ARM2_DPI_RD ARM2_MI ARM2_MVN l, ; immediate
: mvnpl ARM2_DPI_RD ARM2_PL ARM2_MVN l, ; immediate
: mvnvs ARM2_DPI_RD ARM2_VS ARM2_MVN l, ; immediate
: mvnvc ARM2_DPI_RD ARM2_VC ARM2_MVN l, ; immediate
: mvnhi ARM2_DPI_RD ARM2_HI ARM2_MVN l, ; immediate
: mvnls ARM2_DPI_RD ARM2_LS ARM2_MVN l, ; immediate
: mvnge ARM2_DPI_RD ARM2_GE ARM2_MVN l, ; immediate
: mvnlt ARM2_DPI_RD ARM2_LT ARM2_MVN l, ; immediate
: mvngt ARM2_DPI_RD ARM2_GT ARM2_MVN l, ; immediate
: mvnle ARM2_DPI_RD ARM2_LE ARM2_MVN l, ; immediate
: mvn ARM2_DPI_RD ARM2_AL ARM2_MVN l, ; immediate

: eoreq ARM2_DPI_RD_RN ARM2_EQ ARM2_EOR l, ; immediate
: eorne ARM2_DPI_RD_RN ARM2_NE ARM2_EOR l, ; immediate
: eorcs ARM2_DPI_RD_RN ARM2_CS ARM2_EOR l, ; immediate
: eorcc ARM2_DPI_RD_RN ARM2_CC ARM2_EOR l, ; immediate
: eormi ARM2_DPI_RD_RN ARM2_MI ARM2_EOR l, ; immediate
: eorpl ARM2_DPI_RD_RN ARM2_PL ARM2_EOR l, ; immediate
: eorvs ARM2_DPI_RD_RN ARM2_VS ARM2_EOR l, ; immediate
: eorvc ARM2_DPI_RD_RN ARM2_VC ARM2_EOR l, ; immediate
: eorhi ARM2_DPI_RD_RN ARM2_HI ARM2_EOR l, ; immediate
: eorls ARM2_DPI_RD_RN ARM2_LS ARM2_EOR l, ; immediate
: eorge ARM2_DPI_RD_RN ARM2_GE ARM2_EOR l, ; immediate
: eorlt ARM2_DPI_RD_RN ARM2_LT ARM2_EOR l, ; immediate
: eorgt ARM2_DPI_RD_RN ARM2_GT ARM2_EOR l, ; immediate
: eorle ARM2_DPI_RD_RN ARM2_LE ARM2_EOR l, ; immediate
: eor ARM2_DPI_RD_RN ARM2_AL ARM2_EOR l, ; immediate

: orreq ARM2_DPI_RD_RN ARM2_EQ ARM2_ORR l, ; immediate
: orrne ARM2_DPI_RD_RN ARM2_NE ARM2_ORR l, ; immediate
: orrcs ARM2_DPI_RD_RN ARM2_CS ARM2_ORR l, ; immediate
: orrcc ARM2_DPI_RD_RN ARM2_CC ARM2_ORR l, ; immediate
: orrmi ARM2_DPI_RD_RN ARM2_MI ARM2_ORR l, ; immediate
: orrpl ARM2_DPI_RD_RN ARM2_PL ARM2_ORR l, ; immediate
: orrvs ARM2_DPI_RD_RN ARM2_VS ARM2_ORR l, ; immediate
: orrvc ARM2_DPI_RD_RN ARM2_VC ARM2_ORR l, ; immediate
: orrhi ARM2_DPI_RD_RN ARM2_HI ARM2_ORR l, ; immediate
: orrls ARM2_DPI_RD_RN ARM2_LS ARM2_ORR l, ; immediate
: orrge ARM2_DPI_RD_RN ARM2_GE ARM2_ORR l, ; immediate
: orrlt ARM2_DPI_RD_RN ARM2_LT ARM2_ORR l, ; immediate
: orrgt ARM2_DPI_RD_RN ARM2_GT ARM2_ORR l, ; immediate
: orrle ARM2_DPI_RD_RN ARM2_LE ARM2_ORR l, ; immediate
: orr ARM2_DPI_RD_RN ARM2_AL ARM2_ORR l, ; immediate

: andeq ARM2_DPI_RD_RN ARM2_EQ ARM2_AND l, ; immediate
: andne ARM2_DPI_RD_RN ARM2_NE ARM2_AND l, ; immediate
: andcs ARM2_DPI_RD_RN ARM2_CS ARM2_AND l, ; immediate
: andcc ARM2_DPI_RD_RN ARM2_CC ARM2_AND l, ; immediate
: andmi ARM2_DPI_RD_RN ARM2_MI ARM2_AND l, ; immediate
: andpl ARM2_DPI_RD_RN ARM2_PL ARM2_AND l, ; immediate
: andvs ARM2_DPI_RD_RN ARM2_VS ARM2_AND l, ; immediate
: andvc ARM2_DPI_RD_RN ARM2_VC ARM2_AND l, ; immediate
: andhi ARM2_DPI_RD_RN ARM2_HI ARM2_AND l, ; immediate
: andls ARM2_DPI_RD_RN ARM2_LS ARM2_AND l, ; immediate
: andge ARM2_DPI_RD_RN ARM2_GE ARM2_AND l, ; immediate
: andlt ARM2_DPI_RD_RN ARM2_LT ARM2_AND l, ; immediate
: andgt ARM2_DPI_RD_RN ARM2_GT ARM2_AND l, ; immediate
: andle ARM2_DPI_RD_RN ARM2_LE ARM2_AND l, ; immediate
: and ARM2_DPI_RD_RN ARM2_AL ARM2_AND l, ; immediate

: biceq ARM2_DPI_RD_RN ARM2_EQ ARM2_BIC l, ; immediate
: bicne ARM2_DPI_RD_RN ARM2_NE ARM2_BIC l, ; immediate
: biccs ARM2_DPI_RD_RN ARM2_CS ARM2_BIC l, ; immediate
: biccc ARM2_DPI_RD_RN ARM2_CC ARM2_BIC l, ; immediate
: bicmi ARM2_DPI_RD_RN ARM2_MI ARM2_BIC l, ; immediate
: bicpl ARM2_DPI_RD_RN ARM2_PL ARM2_BIC l, ; immediate
: bicvs ARM2_DPI_RD_RN ARM2_VS ARM2_BIC l, ; immediate
: bicvc ARM2_DPI_RD_RN ARM2_VC ARM2_BIC l, ; immediate
: bichi ARM2_DPI_RD_RN ARM2_HI ARM2_BIC l, ; immediate
: bicls ARM2_DPI_RD_RN ARM2_LS ARM2_BIC l, ; immediate
: bicge ARM2_DPI_RD_RN ARM2_GE ARM2_BIC l, ; immediate
: biclt ARM2_DPI_RD_RN ARM2_LT ARM2_BIC l, ; immediate
: bicgt ARM2_DPI_RD_RN ARM2_GT ARM2_BIC l, ; immediate
: bicle ARM2_DPI_RD_RN ARM2_LE ARM2_BIC l, ; immediate
: bic ARM2_DPI_RD_RN ARM2_AL ARM2_BIC l, ; immediate

: addeq ARM2_DPI_RD_RN ARM2_EQ ARM2_ADD l, ; immediate
: addne ARM2_DPI_RD_RN ARM2_NE ARM2_ADD l, ; immediate
: addcs ARM2_DPI_RD_RN ARM2_CS ARM2_ADD l, ; immediate
: addcc ARM2_DPI_RD_RN ARM2_CC ARM2_ADD l, ; immediate
: addmi ARM2_DPI_RD_RN ARM2_MI ARM2_ADD l, ; immediate
: addpl ARM2_DPI_RD_RN ARM2_PL ARM2_ADD l, ; immediate
: addvs ARM2_DPI_RD_RN ARM2_VS ARM2_ADD l, ; immediate
: addvc ARM2_DPI_RD_RN ARM2_VC ARM2_ADD l, ; immediate
: addhi ARM2_DPI_RD_RN ARM2_HI ARM2_ADD l, ; immediate
: addls ARM2_DPI_RD_RN ARM2_LS ARM2_ADD l, ; immediate
: addge ARM2_DPI_RD_RN ARM2_GE ARM2_ADD l, ; immediate
: addlt ARM2_DPI_RD_RN ARM2_LT ARM2_ADD l, ; immediate
: addgt ARM2_DPI_RD_RN ARM2_GT ARM2_ADD l, ; immediate
: addle ARM2_DPI_RD_RN ARM2_LE ARM2_ADD l, ; immediate
: add ARM2_DPI_RD_RN ARM2_AL ARM2_ADD l, ; immediate

: adceq ARM2_DPI_RD_RN ARM2_EQ ARM2_ADC l, ; immediate
: adcne ARM2_DPI_RD_RN ARM2_NE ARM2_ADC l, ; immediate
: adccs ARM2_DPI_RD_RN ARM2_CS ARM2_ADC l, ; immediate
: adccc ARM2_DPI_RD_RN ARM2_CC ARM2_ADC l, ; immediate
: adcmi ARM2_DPI_RD_RN ARM2_MI ARM2_ADC l, ; immediate
: adcpl ARM2_DPI_RD_RN ARM2_PL ARM2_ADC l, ; immediate
: adcvs ARM2_DPI_RD_RN ARM2_VS ARM2_ADC l, ; immediate
: adcvc ARM2_DPI_RD_RN ARM2_VC ARM2_ADC l, ; immediate
: adchi ARM2_DPI_RD_RN ARM2_HI ARM2_ADC l, ; immediate
: adcls ARM2_DPI_RD_RN ARM2_LS ARM2_ADC l, ; immediate
: adcge ARM2_DPI_RD_RN ARM2_GE ARM2_ADC l, ; immediate
: adclt ARM2_DPI_RD_RN ARM2_LT ARM2_ADC l, ; immediate
: adcgt ARM2_DPI_RD_RN ARM2_GT ARM2_ADC l, ; immediate
: adcle ARM2_DPI_RD_RN ARM2_LE ARM2_ADC l, ; immediate
: adc ARM2_DPI_RD_RN ARM2_AL ARM2_ADC l, ; immediate

: rsbeq ARM2_DPI_RD_RN ARM2_EQ ARM2_RSB l, ; immediate
: rsbne ARM2_DPI_RD_RN ARM2_NE ARM2_RSB l, ; immediate
: rsbcs ARM2_DPI_RD_RN ARM2_CS ARM2_RSB l, ; immediate
: rsbcc ARM2_DPI_RD_RN ARM2_CC ARM2_RSB l, ; immediate
: rsbmi ARM2_DPI_RD_RN ARM2_MI ARM2_RSB l, ; immediate
: rsbpl ARM2_DPI_RD_RN ARM2_PL ARM2_RSB l, ; immediate
: rsbvs ARM2_DPI_RD_RN ARM2_VS ARM2_RSB l, ; immediate
: rsbvc ARM2_DPI_RD_RN ARM2_VC ARM2_RSB l, ; immediate
: rsbhi ARM2_DPI_RD_RN ARM2_HI ARM2_RSB l, ; immediate
: rsbls ARM2_DPI_RD_RN ARM2_LS ARM2_RSB l, ; immediate
: rsbge ARM2_DPI_RD_RN ARM2_GE ARM2_RSB l, ; immediate
: rsblt ARM2_DPI_RD_RN ARM2_LT ARM2_RSB l, ; immediate
: rsbgt ARM2_DPI_RD_RN ARM2_GT ARM2_RSB l, ; immediate
: rsble ARM2_DPI_RD_RN ARM2_LE ARM2_RSB l, ; immediate
: rsb ARM2_DPI_RD_RN ARM2_AL ARM2_RSB l, ; immediate

: rsceq ARM2_DPI_RD_RN ARM2_EQ ARM2_RSC l, ; immediate
: rscne ARM2_DPI_RD_RN ARM2_NE ARM2_RSC l, ; immediate
: rsccs ARM2_DPI_RD_RN ARM2_CS ARM2_RSC l, ; immediate
: rsccc ARM2_DPI_RD_RN ARM2_CC ARM2_RSC l, ; immediate
: rscmi ARM2_DPI_RD_RN ARM2_MI ARM2_RSC l, ; immediate
: rscpl ARM2_DPI_RD_RN ARM2_PL ARM2_RSC l, ; immediate
: rscvs ARM2_DPI_RD_RN ARM2_VS ARM2_RSC l, ; immediate
: rscvc ARM2_DPI_RD_RN ARM2_VC ARM2_RSC l, ; immediate
: rschi ARM2_DPI_RD_RN ARM2_HI ARM2_RSC l, ; immediate
: rscls ARM2_DPI_RD_RN ARM2_LS ARM2_RSC l, ; immediate
: rscge ARM2_DPI_RD_RN ARM2_GE ARM2_RSC l, ; immediate
: rsclt ARM2_DPI_RD_RN ARM2_LT ARM2_RSC l, ; immediate
: rscgt ARM2_DPI_RD_RN ARM2_GT ARM2_RSC l, ; immediate
: rscle ARM2_DPI_RD_RN ARM2_LE ARM2_RSC l, ; immediate
: rsc ARM2_DPI_RD_RN ARM2_AL ARM2_RSC l, ; immediate

: sbceq ARM2_DPI_RD_RN ARM2_EQ ARM2_SBC l, ; immediate
: sbcne ARM2_DPI_RD_RN ARM2_NE ARM2_SBC l, ; immediate
: sbccs ARM2_DPI_RD_RN ARM2_CS ARM2_SBC l, ; immediate
: sbccc ARM2_DPI_RD_RN ARM2_CC ARM2_SBC l, ; immediate
: sbcmi ARM2_DPI_RD_RN ARM2_MI ARM2_SBC l, ; immediate
: sbcpl ARM2_DPI_RD_RN ARM2_PL ARM2_SBC l, ; immediate
: sbcvs ARM2_DPI_RD_RN ARM2_VS ARM2_SBC l, ; immediate
: sbcvc ARM2_DPI_RD_RN ARM2_VC ARM2_SBC l, ; immediate
: sbchi ARM2_DPI_RD_RN ARM2_HI ARM2_SBC l, ; immediate
: sbcls ARM2_DPI_RD_RN ARM2_LS ARM2_SBC l, ; immediate
: sbcge ARM2_DPI_RD_RN ARM2_GE ARM2_SBC l, ; immediate
: sbclt ARM2_DPI_RD_RN ARM2_LT ARM2_SBC l, ; immediate
: sbcgt ARM2_DPI_RD_RN ARM2_GT ARM2_SBC l, ; immediate
: sbcle ARM2_DPI_RD_RN ARM2_LE ARM2_SBC l, ; immediate
: sbc ARM2_DPI_RD_RN ARM2_AL ARM2_SBC l, ; immediate

: subeq ARM2_DPI_RD_RN ARM2_EQ ARM2_SUB l, ; immediate
: subne ARM2_DPI_RD_RN ARM2_NE ARM2_SUB l, ; immediate
: subcs ARM2_DPI_RD_RN ARM2_CS ARM2_SUB l, ; immediate
: subcc ARM2_DPI_RD_RN ARM2_CC ARM2_SUB l, ; immediate
: submi ARM2_DPI_RD_RN ARM2_MI ARM2_SUB l, ; immediate
: subpl ARM2_DPI_RD_RN ARM2_PL ARM2_SUB l, ; immediate
: subvs ARM2_DPI_RD_RN ARM2_VS ARM2_SUB l, ; immediate
: subvc ARM2_DPI_RD_RN ARM2_VC ARM2_SUB l, ; immediate
: subhi ARM2_DPI_RD_RN ARM2_HI ARM2_SUB l, ; immediate
: subls ARM2_DPI_RD_RN ARM2_LS ARM2_SUB l, ; immediate
: subge ARM2_DPI_RD_RN ARM2_GE ARM2_SUB l, ; immediate
: sublt ARM2_DPI_RD_RN ARM2_LT ARM2_SUB l, ; immediate
: subgt ARM2_DPI_RD_RN ARM2_GT ARM2_SUB l, ; immediate
: suble ARM2_DPI_RD_RN ARM2_LE ARM2_SUB l, ; immediate
: sub ARM2_DPI_RD_RN ARM2_AL ARM2_SUB l, ; immediate
\ ------------------- DATA PROCESSING + S
: moveqs ARM2_DPI_RD ARM2_S ARM2_EQ ARM2_MOV l, ; immediate
: movnes ARM2_DPI_RD ARM2_S ARM2_NE ARM2_MOV l, ; immediate
: movcss ARM2_DPI_RD ARM2_S ARM2_CS ARM2_MOV l, ; immediate
: movccs ARM2_DPI_RD ARM2_S ARM2_CC ARM2_MOV l, ; immediate
: movmis ARM2_DPI_RD ARM2_S ARM2_MI ARM2_MOV l, ; immediate
: movpls ARM2_DPI_RD ARM2_S ARM2_PL ARM2_MOV l, ; immediate
: movvss ARM2_DPI_RD ARM2_S ARM2_VS ARM2_MOV l, ; immediate
: movvcs ARM2_DPI_RD ARM2_S ARM2_VC ARM2_MOV l, ; immediate
: movhis ARM2_DPI_RD ARM2_S ARM2_HI ARM2_MOV l, ; immediate
: movlss ARM2_DPI_RD ARM2_S ARM2_LS ARM2_MOV l, ; immediate
: movges ARM2_DPI_RD ARM2_S ARM2_GE ARM2_MOV l, ; immediate
: movlts ARM2_DPI_RD ARM2_S ARM2_LT ARM2_MOV l, ; immediate
: movgts ARM2_DPI_RD ARM2_S ARM2_GT ARM2_MOV l, ; immediate
: movles ARM2_DPI_RD ARM2_S ARM2_LE ARM2_MOV l, ; immediate
: movs ARM2_DPI_RD ARM2_S ARM2_AL ARM2_MOV l, ; immediate

: teqeq ARM2_DPI_RN ARM2_S ARM2_EQ ARM2_TEQ l, ; immediate
: teqne ARM2_DPI_RN ARM2_S ARM2_NE ARM2_TEQ l, ; immediate
: teqcs ARM2_DPI_RN ARM2_S ARM2_CS ARM2_TEQ l, ; immediate
: teqcc ARM2_DPI_RN ARM2_S ARM2_CC ARM2_TEQ l, ; immediate
: teqmi ARM2_DPI_RN ARM2_S ARM2_MI ARM2_TEQ l, ; immediate
: teqpl ARM2_DPI_RN ARM2_S ARM2_PL ARM2_TEQ l, ; immediate
: teqvs ARM2_DPI_RN ARM2_S ARM2_VS ARM2_TEQ l, ; immediate
: teqvc ARM2_DPI_RN ARM2_S ARM2_VC ARM2_TEQ l, ; immediate
: teqhi ARM2_DPI_RN ARM2_S ARM2_HI ARM2_TEQ l, ; immediate
: teqls ARM2_DPI_RN ARM2_S ARM2_LS ARM2_TEQ l, ; immediate
: teqge ARM2_DPI_RN ARM2_S ARM2_GE ARM2_TEQ l, ; immediate
: teqlt ARM2_DPI_RN ARM2_S ARM2_LT ARM2_TEQ l, ; immediate
: teqgt ARM2_DPI_RN ARM2_S ARM2_GT ARM2_TEQ l, ; immediate
: teqle ARM2_DPI_RN ARM2_S ARM2_LE ARM2_TEQ l, ; immediate
: teq ARM2_DPI_RN ARM2_S ARM2_AL ARM2_TEQ l, ; immediate

: tsteq ARM2_DPI_RN ARM2_S ARM2_EQ ARM2_TST l, ; immediate
: tstne ARM2_DPI_RN ARM2_S ARM2_NE ARM2_TST l, ; immediate
: tstcs ARM2_DPI_RN ARM2_S ARM2_CS ARM2_TST l, ; immediate
: tstcc ARM2_DPI_RN ARM2_S ARM2_CC ARM2_TST l, ; immediate
: tstmi ARM2_DPI_RN ARM2_S ARM2_MI ARM2_TST l, ; immediate
: tstpl ARM2_DPI_RN ARM2_S ARM2_PL ARM2_TST l, ; immediate
: tstvs ARM2_DPI_RN ARM2_S ARM2_VS ARM2_TST l, ; immediate
: tstvc ARM2_DPI_RN ARM2_S ARM2_VC ARM2_TST l, ; immediate
: tsthi ARM2_DPI_RN ARM2_S ARM2_HI ARM2_TST l, ; immediate
: tstls ARM2_DPI_RN ARM2_S ARM2_LS ARM2_TST l, ; immediate
: tstge ARM2_DPI_RN ARM2_S ARM2_GE ARM2_TST l, ; immediate
: tstlt ARM2_DPI_RN ARM2_S ARM2_LT ARM2_TST l, ; immediate
: tstgt ARM2_DPI_RN ARM2_S ARM2_GT ARM2_TST l, ; immediate
: tstle ARM2_DPI_RN ARM2_S ARM2_LE ARM2_TST l, ; immediate
: tst ARM2_DPI_RN ARM2_S ARM2_AL ARM2_TST l, ; immediate

: cmneq ARM2_DPI_RN ARM2_S ARM2_EQ ARM2_CMN l, ; immediate
: cmnne ARM2_DPI_RN ARM2_S ARM2_NE ARM2_CMN l, ; immediate
: cmncs ARM2_DPI_RN ARM2_S ARM2_CS ARM2_CMN l, ; immediate
: cmncc ARM2_DPI_RN ARM2_S ARM2_CC ARM2_CMN l, ; immediate
: cmnmi ARM2_DPI_RN ARM2_S ARM2_MI ARM2_CMN l, ; immediate
: cmnpl ARM2_DPI_RN ARM2_S ARM2_PL ARM2_CMN l, ; immediate
: cmnvs ARM2_DPI_RN ARM2_S ARM2_VS ARM2_CMN l, ; immediate
: cmnvc ARM2_DPI_RN ARM2_S ARM2_VC ARM2_CMN l, ; immediate
: cmnhi ARM2_DPI_RN ARM2_S ARM2_HI ARM2_CMN l, ; immediate
: cmnls ARM2_DPI_RN ARM2_S ARM2_LS ARM2_CMN l, ; immediate
: cmnge ARM2_DPI_RN ARM2_S ARM2_GE ARM2_CMN l, ; immediate
: cmnlt ARM2_DPI_RN ARM2_S ARM2_LT ARM2_CMN l, ; immediate
: cmngt ARM2_DPI_RN ARM2_S ARM2_GT ARM2_CMN l, ; immediate
: cmnle ARM2_DPI_RN ARM2_S ARM2_LE ARM2_CMN l, ; immediate
: cmn ARM2_DPI_RN ARM2_S ARM2_AL ARM2_CMN l, ; immediate

: cmpeq ARM2_DPI_RN ARM2_S ARM2_EQ ARM2_CMP l, ; immediate
: cmpne ARM2_DPI_RN ARM2_S ARM2_NE ARM2_CMP l, ; immediate
: cmpcs ARM2_DPI_RN ARM2_S ARM2_CS ARM2_CMP l, ; immediate
: cmpcc ARM2_DPI_RN ARM2_S ARM2_CC ARM2_CMP l, ; immediate
: cmpmi ARM2_DPI_RN ARM2_S ARM2_MI ARM2_CMP l, ; immediate
: cmppl ARM2_DPI_RN ARM2_S ARM2_PL ARM2_CMP l, ; immediate
: cmpvs ARM2_DPI_RN ARM2_S ARM2_VS ARM2_CMP l, ; immediate
: cmpvc ARM2_DPI_RN ARM2_S ARM2_VC ARM2_CMP l, ; immediate
: cmphi ARM2_DPI_RN ARM2_S ARM2_HI ARM2_CMP l, ; immediate
: cmpls ARM2_DPI_RN ARM2_S ARM2_LS ARM2_CMP l, ; immediate
: cmpge ARM2_DPI_RN ARM2_S ARM2_GE ARM2_CMP l, ; immediate
: cmplt ARM2_DPI_RN ARM2_S ARM2_LT ARM2_CMP l, ; immediate
: cmpgt ARM2_DPI_RN ARM2_S ARM2_GT ARM2_CMP l, ; immediate
: cmple ARM2_DPI_RN ARM2_S ARM2_LE ARM2_CMP l, ; immediate
: cmp ARM2_DPI_RN ARM2_S ARM2_AL ARM2_CMP l, ; immediate

: mvneqs ARM2_DPI_RD ARM2_S ARM2_EQ ARM2_MVN l, ; immediate
: mvnnes ARM2_DPI_RD ARM2_S ARM2_NE ARM2_MVN l, ; immediate
: mvncss ARM2_DPI_RD ARM2_S ARM2_CS ARM2_MVN l, ; immediate
: mvnccs ARM2_DPI_RD ARM2_S ARM2_CC ARM2_MVN l, ; immediate
: mvnmis ARM2_DPI_RD ARM2_S ARM2_MI ARM2_MVN l, ; immediate
: mvnpls ARM2_DPI_RD ARM2_S ARM2_PL ARM2_MVN l, ; immediate
: mvnvss ARM2_DPI_RD ARM2_S ARM2_VS ARM2_MVN l, ; immediate
: mvnvcs ARM2_DPI_RD ARM2_S ARM2_VC ARM2_MVN l, ; immediate
: mvnhis ARM2_DPI_RD ARM2_S ARM2_HI ARM2_MVN l, ; immediate
: mvnlss ARM2_DPI_RD ARM2_S ARM2_LS ARM2_MVN l, ; immediate
: mvnges ARM2_DPI_RD ARM2_S ARM2_GE ARM2_MVN l, ; immediate
: mvnlts ARM2_DPI_RD ARM2_S ARM2_LT ARM2_MVN l, ; immediate
: mvngts ARM2_DPI_RD ARM2_S ARM2_GT ARM2_MVN l, ; immediate
: mvnles ARM2_DPI_RD ARM2_S ARM2_LE ARM2_MVN l, ; immediate
: mvns ARM2_DPI_RD ARM2_S ARM2_AL ARM2_MVN l, ; immediate

: eoreqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_EOR l, ; immediate
: eornes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_EOR l, ; immediate
: eorcss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_EOR l, ; immediate
: eorccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_EOR l, ; immediate
: eormis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_EOR l, ; immediate
: eorpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_EOR l, ; immediate
: eorvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_EOR l, ; immediate
: eorvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_EOR l, ; immediate
: eorhis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_EOR l, ; immediate
: eorlss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_EOR l, ; immediate
: eorges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_EOR l, ; immediate
: eorlts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_EOR l, ; immediate
: eorgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_EOR l, ; immediate
: eorles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_EOR l, ; immediate
: eors ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_EOR l, ; immediate

: orreqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_ORR l, ; immediate
: orrnes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_ORR l, ; immediate
: orrcss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_ORR l, ; immediate
: orrccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_ORR l, ; immediate
: orrmis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_ORR l, ; immediate
: orrpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_ORR l, ; immediate
: orrvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_ORR l, ; immediate
: orrvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_ORR l, ; immediate
: orrhis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_ORR l, ; immediate
: orrlss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_ORR l, ; immediate
: orrges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_ORR l, ; immediate
: orrlts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_ORR l, ; immediate
: orrgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_ORR l, ; immediate
: orrles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_ORR l, ; immediate
: orrs ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_ORR l, ; immediate

: andeqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_AND l, ; immediate
: andnes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_AND l, ; immediate
: andcss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_AND l, ; immediate
: andccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_AND l, ; immediate
: andmis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_AND l, ; immediate
: andpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_AND l, ; immediate
: andvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_AND l, ; immediate
: andvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_AND l, ; immediate
: andhis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_AND l, ; immediate
: andlss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_AND l, ; immediate
: andges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_AND l, ; immediate
: andlts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_AND l, ; immediate
: andgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_AND l, ; immediate
: andles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_AND l, ; immediate
: ands ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_AND l, ; immediate

: biceqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_BIC l, ; immediate
: bicnes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_BIC l, ; immediate
: biccss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_BIC l, ; immediate
: bicccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_BIC l, ; immediate
: bicmis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_BIC l, ; immediate
: bicpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_BIC l, ; immediate
: bicvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_BIC l, ; immediate
: bicvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_BIC l, ; immediate
: bichis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_BIC l, ; immediate
: biclss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_BIC l, ; immediate
: bicges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_BIC l, ; immediate
: biclts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_BIC l, ; immediate
: bicgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_BIC l, ; immediate
: bicles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_BIC l, ; immediate
: bics ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_BIC l, ; immediate

: addeqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_ADD l, ; immediate
: addnes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_ADD l, ; immediate
: addcss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_ADD l, ; immediate
: addccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_ADD l, ; immediate
: addmis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_ADD l, ; immediate
: addpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_ADD l, ; immediate
: addvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_ADD l, ; immediate
: addvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_ADD l, ; immediate
: addhis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_ADD l, ; immediate
: addlss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_ADD l, ; immediate
: addges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_ADD l, ; immediate
: addlts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_ADD l, ; immediate
: addgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_ADD l, ; immediate
: addles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_ADD l, ; immediate
: adds ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_ADD l, ; immediate

: adceqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_ADC l, ; immediate
: adcnes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_ADC l, ; immediate
: adccss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_ADC l, ; immediate
: adcccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_ADC l, ; immediate
: adcmis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_ADC l, ; immediate
: adcpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_ADC l, ; immediate
: adcvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_ADC l, ; immediate
: adcvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_ADC l, ; immediate
: adchis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_ADC l, ; immediate
: adclss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_ADC l, ; immediate
: adcges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_ADC l, ; immediate
: adclts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_ADC l, ; immediate
: adcgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_ADC l, ; immediate
: adcles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_ADC l, ; immediate
: adcs ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_ADC l, ; immediate

: rsbeqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_RSB l, ; immediate
: rsbnes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_RSB l, ; immediate
: rsbcss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_RSB l, ; immediate
: rsbccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_RSB l, ; immediate
: rsbmis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_RSB l, ; immediate
: rsbpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_RSB l, ; immediate
: rsbvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_RSB l, ; immediate
: rsbvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_RSB l, ; immediate
: rsbhis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_RSB l, ; immediate
: rsblss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_RSB l, ; immediate
: rsbges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_RSB l, ; immediate
: rsblts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_RSB l, ; immediate
: rsbgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_RSB l, ; immediate
: rsbles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_RSB l, ; immediate
: rsbs ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_RSB l, ; immediate

: rsceqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_RSC l, ; immediate
: rscnes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_RSC l, ; immediate
: rsccss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_RSC l, ; immediate
: rscccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_RSC l, ; immediate
: rscmis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_RSC l, ; immediate
: rscpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_RSC l, ; immediate
: rscvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_RSC l, ; immediate
: rscvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_RSC l, ; immediate
: rschis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_RSC l, ; immediate
: rsclss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_RSC l, ; immediate
: rscges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_RSC l, ; immediate
: rsclts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_RSC l, ; immediate
: rscgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_RSC l, ; immediate
: rscles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_RSC l, ; immediate
: rscs ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_RSC l, ; immediate

: sbceqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_SBC l, ; immediate
: sbcnes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_SBC l, ; immediate
: sbccss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_SBC l, ; immediate
: sbcccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_SBC l, ; immediate
: sbcmis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_SBC l, ; immediate
: sbcpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_SBC l, ; immediate
: sbcvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_SBC l, ; immediate
: sbcvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_SBC l, ; immediate
: sbchis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_SBC l, ; immediate
: sbclss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_SBC l, ; immediate
: sbcges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_SBC l, ; immediate
: sbclts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_SBC l, ; immediate
: sbcgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_SBC l, ; immediate
: sbcles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_SBC l, ; immediate
: sbcs ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_SBC l, ; immediate

: subeqs ARM2_DPI_RD_RN ARM2_S ARM2_EQ ARM2_SUB l, ; immediate
: subnes ARM2_DPI_RD_RN ARM2_S ARM2_NE ARM2_SUB l, ; immediate
: subcss ARM2_DPI_RD_RN ARM2_S ARM2_CS ARM2_SUB l, ; immediate
: subccs ARM2_DPI_RD_RN ARM2_S ARM2_CC ARM2_SUB l, ; immediate
: submis ARM2_DPI_RD_RN ARM2_S ARM2_MI ARM2_SUB l, ; immediate
: subpls ARM2_DPI_RD_RN ARM2_S ARM2_PL ARM2_SUB l, ; immediate
: subvss ARM2_DPI_RD_RN ARM2_S ARM2_VS ARM2_SUB l, ; immediate
: subvcs ARM2_DPI_RD_RN ARM2_S ARM2_VC ARM2_SUB l, ; immediate
: subhis ARM2_DPI_RD_RN ARM2_S ARM2_HI ARM2_SUB l, ; immediate
: sublss ARM2_DPI_RD_RN ARM2_S ARM2_LS ARM2_SUB l, ; immediate
: subges ARM2_DPI_RD_RN ARM2_S ARM2_GE ARM2_SUB l, ; immediate
: sublts ARM2_DPI_RD_RN ARM2_S ARM2_LT ARM2_SUB l, ; immediate
: subgts ARM2_DPI_RD_RN ARM2_S ARM2_GT ARM2_SUB l, ; immediate
: subles ARM2_DPI_RD_RN ARM2_S ARM2_LE ARM2_SUB l, ; immediate
: subs ARM2_DPI_RD_RN ARM2_S ARM2_AL ARM2_SUB l, ; immediate

: nop $e320f000 l, ; immediate

\ ---------------------------------------
\ --------------------------- SAMPLE CODE
\ ---------------------------------------
\ This place an ARMv2 program in the word
\ ARCHISMALL_ARMv2_CODE, the program is a
\ 64 bytes RISC OS/Acorn Archimedes intro
\ see:
\ https://github.com/grz0zrg/codegolfing
\ BBC BASIC ARM syntax is shown commented
\ remove \ # to test it out
\ # : OS_WriteI $100 ; immediate
\ # : OS_ReadMonotonicTime $42 ; immediate
\ # : OS_ReadEscapeState $2c ; immediate
\ # : OS_Exit $11 ; immediate

\ # variable archismall_loop

\ # create ARCHISMALL_ARMv2_CODE
\ #     OS_WriteI $16 + swi            \ swi OS_WriteI+22
\ #     OS_WriteI $d + swi             \ swi OS_WriteI+13
\ #     [] $2c imm r15 r9 ldr          \ ldr r9,[r15,#44]
\ #     $140 imm r4 mov                \ mov r4,#320
\ #     archismall_loop !LABEL         \ .archismall_loop
\ #         OS_ReadMonotonicTime swi   \ swi OS_ReadMonotonicTime
\ #         $1 asr r3 r2 r2 add        \ add r2,r2,r3,asr #1
\ #         $1 asr r2 r3 r3 sub        \ sub r3,r3,r2,asr #1
\ #         $13 lsl r0 r2 r2 sub       \ sub r2,r2,r0,lsl #19

\ #         $18 lsr r3 r6 mov          \ mov r6,r3,lsr #24
\ #         r9 r4 r6 r7 mla            \ mla r7,r6,r4,r9

\ #         $4 lsr r0 r6 mov           \ mov r6,r0,lsr #4
\ #         [] $18 lsr r2 r7 r6 strb   \ strb r6,[r7,r2,lsr #24]

\ #         OS_ReadEscapeState swi     \ swi OS_ReadEscapeState
\ #         archismall_loop @LABEL bcc \ bcc archismall_loop
\ #     OS_Exit swi                    \ swi OS_Exit
\ .screenAddr
\ #     $1fec020 l,                    \ dcd &1fec020

\ To dump ARCHISMALL_ARMv2_CODE content in Gforth :
\ here constant ARCHISMALL_ARMv2_CODE_END

\ a pretty hex. dump :
\ ARCHISMALL_ARMv2_CODE ARCHISMALL_ARMv2_CODE_END ARCHISMALL_ARMv2_CODE - dump

\ a 32-bit hex. words dump :
( addr bytes -- )
\ : ARM32_OPCODE
\     hex 0 ?do dup i + l@ 8 u.r cr 4 +loop drop decimal ;
\ ARCHISMALL_ARMv2_CODE ARCHISMALL_ARMv2_CODE_END ARCHISMALL_ARMv2_CODE - cr ARM32_OPCODE