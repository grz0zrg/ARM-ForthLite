all: clean example.bin

example.o: example.s
	arm-linux-gnueabihf-as example.s -o example.o

example.bin: example.o
	arm-linux-gnueabihf-ld -T example.ld example.o -o example.elf
	arm-linux-gnueabihf-objcopy example.elf -O binary example.bin
	wc -c example.bin

clean:
	rm -f *.o *.elf example.bin