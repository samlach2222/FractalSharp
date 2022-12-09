#!/bin/bash
readonly OUTPUT=build_linux
# Create directory and copy all necessary files in it
mkdir -p "$OUTPUT"
# "\cp" is used instead of "cp" because "cp" is sometimes aliased to "cp -i" which asks the user before overwritting
\cp */*.cpp "$OUTPUT"
\cp */*.h "$OUTPUT"
\cp FractalPlusPlusGUI/loading.gif "$OUTPUT"

cd "$OUTPUT"

# Compile both projects
mpic++ "FractalPlusPlusMPI.cpp" "Complex.cpp" -lSDL -pthread -Wall -I/urs/local/include -o "FractalPlusPlusMPI"
g++ "FractalPlusPlusGUI.cpp" -lSDL -pthread -Wall -I/urs/local/include -o "FractalPlusPlusGUI"

# Ask to run the program
read -p "Run the program [Y/y for yes] : " -n1 run
echo # New line
if [[ $run =~ ^[Yy]$ ]]
then
    ./FractalPlusPlusGUI
fi
