#!/bin/bash

if [ $# -ne 2 ]; then
    echo "Usage: $0 INPUT_FOLDER OUTPUT_FOLDER"
    exit 1
fi

INPUT_FOLDER="$1"
OUTPUT_FOLDER="$2"

if [ ! -d "$INPUT_FOLDER" ]; then
    echo "Error: Input folder '$INPUT_FOLDER' does not exist"
    exit 1
fi

# Create output folder if it doesn't exist
mkdir -p "$OUTPUT_FOLDER"

# Process each log file in the input folder
for LOG_FILE in "$INPUT_FOLDER"/*.log; do
    # Skip if no .log files found
    if [ ! -e "$LOG_FILE" ]; then
        echo "No .log files found in $INPUT_FOLDER"
        break
    fi

    # Get the base filename without path
    FILENAME=$(basename "$LOG_FILE")
    BASE_NAME=$(basename "$LOG_FILE" .log)

    # Set output filename
    OUTPUT_FILE="$OUTPUT_FOLDER/${BASE_NAME}_py.txt"

    echo "Processing: $FILENAME -> $OUTPUT_FILE"

    # Run the python script
    python3 log_parser.py "$LOG_FILE" > "$OUTPUT_FILE"

    # Check if the command succeeded
    if [ $? -eq 0 ]; then
        echo "  Success"
    else
        echo "  Failed"
    fi
done

python3 log_csv_compiler.py "$OUTPUT_FOLDER" > "results.csv"

echo "Processing complete!"