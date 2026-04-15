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

armflite: clean
	$(MAKE) program.fs.inc input-file=ARMv2_assembler.fs
	cp riscos/platform.s platform.s
	arm-linux-gnueabihf-as --defsym SKIP_SPACES=1 -march=armv2 riscos/armflite.s -o armflite.o
	arm-linux-gnueabihf-ld -T example.ld armflite.o -o armflite.elf
	arm-linux-gnueabihf-objcopy armflite.elf -O binary armflite,ff8
	wc -c armflite,ff8

clean:
	rm -f *.o *.elf example.bin armflite,ff8 program.fs.inc platform.s

.PHONY: all clean armflite