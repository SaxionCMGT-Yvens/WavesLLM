import sys
import file_utils
import log_parser

def main():
    folder = sys.argv[1]
    model = sys.argv[2]

    files = file_utils.get_valid_files_from_folder(folder)
    matching_files_count = 0
    output_lines = [model]
    avg_battle_duration = 0
    avg_prompt = 0
    avg_req = 0
    max_req = 0
    min_req = 0
    num_req = 0
    attempts = 0
    num_attempts = 0
    total_faulty_message = 0
    total_internal_attack_attempts = 0
    total_internal_wrong_attack = 0
    total_internal_wrong_movements = 0
    total_internal_movement_attempts = 0
    num_failed_movements = 0
    avg_movements = 0

    for file in files:
        lines_model = file_utils.find_lines_containing_string(model, file)
        if len(lines_model) <= 0:
            continue

        matching_files_count += 1
        avg_battle_duration += log_parser.parse_duration_seconds(lines_model, [])
        avg_prompt += log_parser.extract_prompt_data(lines_model, model, [])
        l_avg_req, l_max_req, l_min_req, l_num_req = log_parser.extract_request_data(lines_model, model, [])
        avg_req += l_avg_req
        max_req += l_max_req
        min_req += l_min_req
        num_req += l_num_req
        l_attempt, l_num_attempt = log_parser.extract_attempts_data(lines_model, model, [])
        attempts += l_attempt
        num_attempts += l_num_attempt
        l_total_faulty_message, l_total_internal_attack_attempts, l_total_internal_wrong_attack, l_total_internal_wrong_movements, l_total_internal_movement_attempts = log_parser.extract_internal_data(lines_model, model, [])
        total_faulty_message += l_total_faulty_message
        total_internal_attack_attempts += l_total_internal_attack_attempts
        total_internal_wrong_attack += l_total_internal_wrong_attack
        total_internal_wrong_movements += l_total_internal_wrong_movements
        total_internal_movement_attempts += l_total_internal_movement_attempts
        l_failed_movement, l_movement = log_parser.extract_movement(lines_model, model, [])
        num_failed_movements += l_failed_movement
        avg_movements += l_movement
        print("=" * 40)

    print(f"Matching files found: {matching_files_count}")
    avg_battle_duration /= matching_files_count
    avg_prompt /= matching_files_count
    avg_req /= matching_files_count
    max_req /= matching_files_count
    min_req /= matching_files_count
    num_req /= matching_files_count
    attempts /= matching_files_count
    num_attempts /= matching_files_count
    total_faulty_message /= matching_files_count
    total_internal_attack_attempts /= matching_files_count
    total_internal_wrong_attack /= matching_files_count
    total_internal_wrong_movements /= matching_files_count
    total_internal_movement_attempts /= matching_files_count
    num_failed_movements /= matching_files_count
    avg_movements /= matching_files_count

    output_lines.append(f"{avg_battle_duration:.2f}")
    output_lines.append(f"{avg_prompt:.2f}")
    output_lines.append(f"{avg_req:.2f}")
    output_lines.append(f"{max_req:.2f}")
    output_lines.append(f"{min_req:.2f}")
    output_lines.append(f"{num_req:.2f}")
    output_lines.append(f"{attempts:.2f}")
    output_lines.append(f"{num_attempts:.2f}")
    output_lines.append(f"{total_faulty_message:.2f}")
    output_lines.append(f"{total_internal_attack_attempts:.2f}")
    output_lines.append(f"{total_internal_wrong_attack:.2f}")
    output_lines.append(f"{total_internal_wrong_movements:.2f}")
    output_lines.append(f"{total_internal_movement_attempts:.2f}")
    output_lines.append(f"{num_failed_movements:.2f}")
    output_lines.append(f"{avg_movements:.2f}")
    print("=" * 40)
    print("CSV line")
    print("model;avg_battle_duration;avg_prompt;avg_req;max_req;min_req;num_req;attempts;num_attempts;"
          "total_faulty_message;total_internal_attack_attempts;"
          "total_internal_wrong_attack;total_internal_wrong_movements;"
          "total_internal_movement_attempts;num_failed_movements;avg_movements")
    print(';'.join(output_lines))

if __name__ == "__main__":
    main()