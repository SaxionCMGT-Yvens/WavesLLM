#!/bin/bash
LOG_FILE="$1"
MODEL="$2"
BASE_NAME=$(basename "$LOG_FILE" .log)
./log_grepper.sh "$LOG_FILE" "$MODEL" "MOVE" > "$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "ATTK" >> "$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "RESN" >> "$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "INFO" >> "$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "attempt" >> "$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "request" >> "$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "Cannot" >> "$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "Attacked" >> "$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "Destroyed" >> "$BASE_NAME"_"$MODEL"_c.log
./log_grepper.sh "$LOG_FILE" "$MODEL" "internalWrongMovementCount" >> "$BASE_NAME"_"$MODEL"_c.log