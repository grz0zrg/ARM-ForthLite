# ARM-ForthLite

Minimal, lightweight core [Forth](https://en.wikipedia.org/wiki/Forth_(programming_language)) implementation for ARM processors. (without [REPL](https://en.wikipedia.org/wiki/Read%E2%80%93eval%E2%80%93print_loop))

* example binary is **488 bytes** of which *76 bytes* is for setup, *412 bytes* is Forth core implementation
* use [Subroutine Threaded Code](https://www.bradrodriguez.com/papers/moving1.htm)
* parse number
* stack top is stored into a register (r4) and implementation use all available registers for additional speed
* [Thumb-2](https://en.wikipedia.org/wiki/ARM_architecture_family#Thumb-2) can be used with small adaptations (add IT and PC changes), result is *~410* bytes example binary
* not really written for bootstrapping support due to tricks but can still go with the [sectorforth](https://github.com/cesarblum/sectorforth) or [milliForth](https://github.com/fuzzballcat/milliForth) route
* target is a RPI Zero 1.3 (ARM1176JZF-S), probably works on any ARM, side goal was [ARMv2](https://en.wikichip.org/wiki/arm/armv2) support but didn't test it yet (compile related generated opcodes may require adaptation !)

Example is bare metal and independent so there is no REPL, idea is to wrap own REPL around it and own set of primitives as needed.

Example can be tested online on [https://cpulator.01xz.net/?sys=arm](CPUlator).

Primitives are defined into a separate file so a different set can be swapped easily, included ones are `+` `:` `;` and `immediate`.

Note that example text section start at 0x8000; see linker file.

Not standard compliant.

## Shortcuts

This implementation makes shortcuts to reduce code size that i consider ok because the REPL (or other methods) can handle it such as :

* it doesn't trim extra whitespaces; should always be exactly one whitespace between words
* no negative numbers parsing (can be built easily)
* must store return address at `forth_retn_addr` when `forth` is jumped to
* error code is returned in r5, right now it is just 0 for 'ok' and 1 for an unknown word; no errors for stack underflow

Code can be reduced further by inlining subroutines such as `read_word` `forth` etc. at the risk of being unreadable, "ret" could be put automatically also but require a "primitive" flag, may save some bytes with many primitives.

Can also be reduced greatly by abandoning number parsing since it can be implemented with primitives, this is what sectorforth or milliForth do, would result in a *~400b* example, perhaps enough to fit into a bootsector with a REPL and essential primitives in Thumb-2 mode.

## Speed

Target goal was mainly about code size / simplicity and modularity, speed may be okay. (didn't test much yet)

May be easy to inline code or compile further due to STC usage in case ones want speed.

## Registers

Usage of these registers is kept as-is for the whole Forth context :

| Reg | Forth context description |
| --- | --- |
| sp | data stack address |
| r0 | return stack address |
| r1 | curr. input buffer address |
| r2 | dict. last word address |
| r3 | current execution mode |
| r4 | stack top value |
| r14 | dictionary end address |

May be used in new words to implement some bootstrap primitives or be saved / loaded from somewhere to switch Forth context.

Immediate word can use r7 safely to save space, always 0 in this case. (this is what ';' actually do)

## Build

Assemble with [GNU Assembler](https://en.wikipedia.org/wiki/GNU_Assembler) and associated tools.

* `sudo apt-get install gcc-arm-linux-gnueabihf`

See `Makefile`, it use Raspberry PI toolchain by default :

* https://github.com/raspberrypi/tools

## License

BDS3