input-path = examples
input-file = simple.fs

all: clean example.bin

program.fs.inc: $(input-path)/${input-file}
	sed -E 's/\\.*$$//; s/\([^)]*\)//g;' $< | tr '\n\t\r' '   ' | tr -s ' ' | sed -E 's/^ *//; s/ *$$//; s/^/.asciz "/; s/$$/"/' > $@

example.o: example.s
	arm-linux-gnueabihf-as example.s -o example.o

example.bin: program.fs.inc example.o
	arm-linux-gnueabihf-ld -T example.ld example.o -o example.elf
	arm-linux-gnueabihf-objcopy example.elf -O binary example.bin
	wc -c example.bin

clean:
	rm -f *.o *.elf example.bin program.fs.inc
