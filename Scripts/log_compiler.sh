#!/bin/bash
LOG_FILE="$1"
MODEL="$2"
BASE_NAME=$(basename "$LOG_FILE" .log)
OUTPUT="$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "MOVE" > "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "ATTK" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "RESN" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "INFO" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "PRPT" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "attempt" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "request" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "Cannot" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "Attacked" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "Destroyed" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "internalWrongMovementCount" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "LevelController" >> "$OUTPUT"
./log_grepper.sh "$LOG_FILE" "$MODEL" "LevelGoal" >> "$OUTPUT"