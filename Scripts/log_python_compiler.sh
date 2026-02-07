#!/bin/bash
LOG_FILE="$1"
BASE_NAME=$(basename "$LOG_FILE" .log)
OUTPUT="$BASE_NAME"_"$MODEL"_py.log
python3 log_parser.py "$LOG_FILE" > "$OUTPUT"