# ARM-ForthLite

Minimal, lightweight core [Forth](https://en.wikipedia.org/wiki/Forth_(programming_language)) implementation for ARM processors. (without [REPL](https://en.wikipedia.org/wiki/Read%E2%80%93eval%E2%80%93print_loop))

* example binary is **440 bytes** of which *72 bytes* is for setup, *368 bytes* is Forth core implementation
* use [Subroutine Threaded Code](https://www.bradrodriguez.com/papers/moving1.htm)
* parse hex number
* stack top is stored into a register (r4) and implementation use all available registers for additional speed
* [Thumb-2](https://en.wikipedia.org/wiki/ARM_architecture_family#Thumb-2) can be used with small adaptations (add IT and PC changes), result is *~400* bytes example binary
* not really written for bootstrapping support due to tricks but can still go with the [sectorforth](https://github.com/cesarblum/sectorforth) or [milliForth](https://github.com/fuzzballcat/milliForth) route (see experiment / misc)
* target is a RPI Zero 1.3 (ARM1176JZF-S), probably works on any ARM that support conditional instructions, side goal was [ARMv2](https://en.wikichip.org/wiki/arm/armv2) support but didn't test it yet (compile related generated opcodes may require adaptation !)

Example is bare metal and independent so there is no REPL, idea is to wrap own REPL around it and own set of primitives as needed. (or do the REPL in Forth !)

Example can be tested online on [CPUlator](https://cpulator.01xz.net/?sys=arm)

Primitives are defined into a separate file so a different set can be swapped easily, included ones are `+` `:` `;` and `immediate`.

Note that example text section start at 0x8000; see linker file.

Not standard compliant.

## Shortcuts

This implementation makes shortcuts to reduce code size that i consider ok because the REPL (or other methods) can handle it such as :

* it doesn't trim extra whitespaces; should always be exactly one whitespace between words
* no negative numbers parsing (can be built easily)
* no errors handling such as stack underflow: check first commit for a version with unknown word error and stricter base 10 number parsing
* no unknown words, they are parsed as number (base 16) since it is more convenient and to save some instructions
* must store return address at `forth_retn_addr` when `forth` is jumped to

Code can be reduced further by inlining subroutines at the risk of being unreadable, "ret" could be put automatically also but require a "primitive" flag, may save some bytes with many primitives.

Can also be reduced greatly by abandoning number parsing since it can be implemented with primitives, this is what sectorforth or milliForth do, would result in a *< 400b* example.

Another option is to abandon the mode (compile / immediate) and just generate code somewhere in RAM then jump to it at the end, it is like being always in compile mode, this has the advantage of freeing many registers so they can be used to hold temporary values likes in Machine Forth which avoid stack noise, it is also much faster and tinier, the code here is easily adaptable to do this.

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

r7 in word definition can be used safely to save space, always 0 in this case. (this is what ';' do)

## sectorforth experiment

[sectorforth](https://github.com/cesarblum/sectorforth/tree/master) is a cool 16-bit x86 Forth that fits in a 512-byte boot sector

Although this project differ slightly in goal i tried to implement the sectorforth dictionary as a testbed experiment (see `misc` directory) with a result of about ~648b of code in normal ARM mode without parsing numbers nor REPL.

The main "limitation" compared to sectorforth is that the vars are all in registers here instead of in memory which ease some stuff but this require more words to modify them and sectorforth examples needs to be adapted in consequence, i converted about 50% of the sectorforth example until i reached the vars issue, may still be doable to reach 512b with a REPL with slightly different structure (or just perhaps pushing all vars on stack) or in Thumb-2 mode with size optimizations outlined above if 512b is a goal.

## Build

Assemble with [GNU Assembler](https://en.wikipedia.org/wiki/GNU_Assembler) and associated tools.

* `sudo apt-get install gcc-arm-linux-gnueabihf`

See `Makefile`, it use Raspberry PI toolchain by default :

* https://github.com/raspberrypi/tools

## License

BDS3