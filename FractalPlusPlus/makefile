EXT = cpp
CXX=mpic++
CFLAGS=-Wall -I/urs/local/include
CFLAGS=

LDFLAGS= -lSDL

SRC=$(wildcard *.$(EXT))
OBJ = $(SRC:.$(EXT)=.o)

DEBUBFLAG=-g

BIN=./
EXEC=FractalPlusPlusMPI

all: $(EXEC)

$(EXEC): $(OBJ)
        $(CXX)  -o $(BIN)$@ $^ $(LDFLAGS)

%.o: %.cpp
        $(CXX)  -o $@ -c $< $(CFLAGS)


clean:
        rm -rf *.o

mrproper: clean
        rm -rf $(BIN)$(EXEC)
