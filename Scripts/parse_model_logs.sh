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

models=("deepseek-chat" "deepseek-reasoner"
"claude-haiku-4-5-20251001" "claude-sonnet-4-5-20250929"
"gemini-2.5-flash-lite" "gemini-2.5-flash"
"gpt-4.1-mini" "gpt-4.1"
"magistral-small-2509" "ministral-3b-2512" )

for model in "${models[@]}"; do
    echo -e "Processing: $model"
    OUTPUT_FILE="$OUTPUT_FOLDER/${model}_compile.txt"
    python3 log_model_parse.py "$INPUT_FOLDER" "$model" > "$OUTPUT_FILE"
done

python3 log_csv_compiler.py "$OUTPUT_FOLDER" > "results_per_model.csv"

echo "Processing complete!"