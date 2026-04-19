SKIP_SPACES ?= 0

input-path = examples
input-file = simple.fs

all: clean example.bin

program.fs.inc: $(input-path)/${input-file}
	sed -E 's/\\.*$$//; s/\([^)]*\)//g;' $< | tr '\n\t\r' '   ' | tr -s ' ' | sed -E 's/^ *//; s/ *$$//; s/^/.asciz "/; s/$$/"/' > $@ ; echo >> $@

platform.s:
	touch $@

example.o: example.s platform.s
	arm-linux-gnueabihf-as --defsym SKIP_SPACES=$(SKIP_SPACES) -march=armv2 example.s -o example.o

example.bin: program.fs.inc example.o
	arm-linux-gnueabihf-ld -T example.ld example.o -o example.elf
	arm-linux-gnueabihf-objcopy example.elf -O binary example.bin
	wc -c example.bin

dictgen: clean
	cp riscos/platform.s platform.s
	arm-linux-gnueabihf-as --defsym SKIP_SPACES=1 -march=armv2 riscos/dictgen/dictgen.s -o dictgen.o
	arm-linux-gnueabihf-ld -T example.ld dictgen.o -o dictgen.elf
	arm-linux-gnueabihf-objcopy dictgen.elf -O binary dictgen,ff8
	wc -c dictgen,ff8

armflite: clean
	arm-linux-gnueabihf-as --defsym SKIP_SPACES=1 -march=armv2 riscos/armflite.s -o armflite.o
	arm-linux-gnueabihf-ld -T example.ld armflite.o -o armflite.elf
	arm-linux-gnueabihf-objcopy armflite.elf -O binary armflite,ff8
	wc -c armflite,ff8

clean:
	rm -f *.o *.elf example.bin armflite,ff8 program.fs.inc platform.s dictgen,ff8

.PHONY: all clean armflite dictgen