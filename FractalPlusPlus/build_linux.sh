#!/bin/bash
readonly OUTPUT=build_linux/
readonly IMAGE=/tmp/Mandelbrot.bmp
# Create directory and copy all necessary files in it
mkdir -p "$OUTPUT"
# "\cp" is used instead of "cp" because "cp" is sometimes aliased to "cp -i" which asks the user before overwritting
\cp "FractalPlusPlusGUI/FractalPlusPlusGUI.cpp" "$OUTPUT" # GUI files
\cp "FractalPlusPlusMPI/Complex.cpp" "FractalPlusPlusMPI/Complex.h" "FractalPlusPlusMPI/FractalPlusPlusMPI.cpp" "$OUTPUT" # MPI files

cd "$OUTPUT"

# Compile both projects
mpic++ "FractalPlusPlusMPI.cpp" "Complex.cpp" -lSDL -Wall -I/urs/local/include -o "FractalPlusPlusMPI"
g++ "FractalPlusPlusGUI.cpp" -lSDL -Wall -I/urs/local/include -o "FractalPlusPlusGUI"

# Check if the Mandelbrot image exists
if [[ -f "$IMAGE" ]]
then
	# Ask to delete the Mandelbrot image
	read -p "Delete $IMAGE [Y/y for yes] : " -n1 deleteImage
	echo # New line
	if [[ $deleteImage =~ ^[Yy]$ ]]
	then
		rm -f "$IMAGE"
	fi
fi

# Ask to run the program
read -p "Run the program [Y/y for yes] : " -n1 runGui
echo # New line
if [[ $runGui =~ ^[Yy]$ ]]
then
    ./FractalPlusPlusGUI
fi
