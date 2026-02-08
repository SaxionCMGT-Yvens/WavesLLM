import sys
import json
from datetime import datetime

output_line = []

def find_lines_containing_string(search_string, lines_array):
    return [single_line for single_line in lines_array if search_string in single_line]

def get_model(faction, lines_array):
    faction_lines = find_lines_containing_string(faction, lines_array)
    first_faction_line = faction_lines[0]
    llm_faction_agent_line = first_faction_line.split(';')[2]
    llm_faction_model = llm_faction_agent_line.split('|')[2]
    return llm_faction_model

def parse_per_faction(faction, lines_array):
    faction_lines = find_lines_containing_string(faction, lines_array)

    # Parse model
    # 2026-02-06 20:54:42;[SYSTEM];[LLMAgent|Gemini|gemini-2.5-flash-lite|Green|1];INFO Start turn;5,5
    model = faction_lines[0].split(";")[2].split("|")[2]
    print(f"Faction = {faction}| Model = {model}")
    output_line.append(f"Faction = {faction}; Model = {model}")

    # Parse prompt lines
    # 2026-02-06 20:54:44;[SYSTEM];[LLMAgent|Gemini|gemini-2.5-flash-lite|Green|1];PRPT Prompt_2;3095
    prompts = find_lines_containing_string("PRPT", faction_lines)
    avg_prompt = 0
    for prompt in prompts:
        prompt_split = prompt.split(";")
        prompt_size = float(prompt_split[(len(prompt_split) - 1)])
        avg_prompt += prompt_size
    avg_prompt /= len(prompts)
    print(f"Average {model} prompt = {avg_prompt:.2f}")
    output_line.append(f"{avg_prompt:.2f}")

    # Parse request time
    # 2026-02-06 20:54:44;[SYSTEM];[LLMAgent|Gemini|gemini-2.5-flash-lite|Green|1];TIME {"request":439}
    requests = find_lines_containing_string("request", faction_lines)
    avg_req = 0
    max_req = float("-inf")
    min_req = float("+inf")
    for request in requests:
        last_part = request.split(';')[-1]
        json_part = last_part.split(' ', 1)[1]
        data = json.loads(json_part)
        request_time = data['request']
        avg_req += float(request_time)
        max_req = max(max_req, float(request_time))
        min_req = min(min_req, float(request_time))
    avg_req /= len(requests)
    print(f"Average {model} request = {avg_req:.2f} ({len(requests)} requests)")
    print(f"Max {model} request = {max_req:.2f}")
    print(f"Min {model} request = {min_req:.2f}")
    output_line.append(f"{avg_req:.2f}")
    output_line.append(f"{max_req:.2f}")
    output_line.append(f"{min_req:.2f}")

    # Parse attempts time
    # 2026-02-06 20:54:44;[SYSTEM];[LLMAgent|Gemini|gemini-2.5-flash-lite|Green|1];DATA {"attempts":1}
    attempts = find_lines_containing_string("attempts", faction_lines)
    avg_attempts = 0
    for attempt in attempts:
        last_part = attempt.split(';')[-1]
        json_part = last_part.split(' ', 1)[1]
        data = json.loads(json_part)
        number_attempts = data['attempts']
        avg_attempts += float(number_attempts)
    avg_attempts /= len(attempts)

    print(f"Average {model} attempts = {avg_attempts}")
    output_line.append(f"{avg_attempts:.2f}")

    # Parse internal data
    # 2026-02-06 21:02:55;[SYSTEM];[LLMAgent|Gemini|gemini-2.5-flash-lite|Green|3];DATA {"internalWrongMovementCount":3,"internalWrongAttackCount":10,"internalTotalRequestCount":31,"internalMovementAttemptCount":31,"internalAttackAttemptCount":11,"internalFaultyMessageCount":0,"averageRequestTime":654.7419,"averageRequestTimeCount":31,"maxRequestTime":1268,"minRequest":345,"averageAttempts":1,"kills":0}
    filtered_data_with_timestamps = find_lines_containing_string("internalWrongMovementCount", faction_lines)
    filtered_data = [single_line.split(';', 1)[1] for single_line in filtered_data_with_timestamps]
    filtered_unique_data = list(dict.fromkeys(filtered_data))
    total_internal_wrong_movements = 0
    total_internal_wrong_attack = 0
    total_internal_movement_attempts = 0
    total_internal_attack_attempts = 0
    total_faulty_message = 0
    for data in filtered_unique_data:
        # [SYSTEM];[LLMAgent|Gemini|gemini-2.5-flash-lite|Green|3];DATA
        # {"internalWrongMovementCount":3, CHECK
        # "internalWrongAttackCount":10, CHECK
        # "internalTotalRequestCount":31, IGNORE
        # "internalMovementAttemptCount":31, CHECK
        # "internalAttackAttemptCount":11, CHECK
        # "internalFaultyMessageCount":0, CHECK
        # "averageRequestTime":654.7419, IGNORE
        # "averageRequestTimeCount":31, IGNORE
        # "maxRequestTime":1268, IGNORE
        # "minRequest":345, IGNORE
        # "averageAttempts":1, IGNORE
        # "kills":0} IGNORE
        data_segment = data.split(';')[2]
        json_part = data_segment.split(' ', 1)[1]
        data = json.loads(json_part)
        total_internal_wrong_movements += float(data['internalWrongMovementCount'])
        total_internal_wrong_attack += float(data['internalWrongAttackCount'])
        total_internal_movement_attempts += float(data['internalMovementAttemptCount'])
        total_internal_attack_attempts += float(data['internalAttackAttemptCount'])
        total_faulty_message += float(data['internalFaultyMessageCount'])

    movements_attempts = len(find_lines_containing_string("MOVE", faction_lines))
    output_line.append(f"{movements_attempts:.2f}")
    # Parse failed movements
    # 2026-02-06 20:55:24;[SYSTEM];[LLMAgent|GPT|gpt-4.1-mini|Red|5];INFO Cannot reach target at GridTile(Clone) (6, 4).
    failed_movements = len(find_lines_containing_string("Cannot reach", faction_lines))
    output_line.append(f"{failed_movements:.2f}")
    # Success movements
    output_line.append(f"{movements_attempts - failed_movements:.2f}")

    percentage_failed_movements = 0
    if movements_attempts > 0:
        percentage_failed_movements = failed_movements / movements_attempts
    output_line.append(f"{percentage_failed_movements * 100:.2f}")
    print(f"Total of {model} failed movements = {failed_movements} / {movements_attempts} ({percentage_failed_movements * 100:.2f}%)")

    output_line.append(f"{total_internal_movement_attempts:.2f}")
    output_line.append(f"{total_internal_wrong_movements:.2f}")
    output_line.append(f"{total_internal_movement_attempts - total_internal_wrong_movements:.2f}")
    percentage_failed_movements_from_agents = total_internal_wrong_movements / total_internal_movement_attempts
    print(
        f"Total of {model} failed movements [from agents] = {total_internal_wrong_movements} / {total_internal_movement_attempts} ({percentage_failed_movements_from_agents * 100:.2f}%)")
    output_line.append(f"{percentage_failed_movements_from_agents * 100:.2f}")
    output_line.append(f"{(1 - percentage_failed_movements_from_agents) * 100:.2f}")

    # Parse attacks
    # 2026-02-06 20:56:56;[SYSTEM];[LLMAgent|GPT|gpt-4.1-mini|Red|2];ATTK {10, 8}
    attack_attempts = len(find_lines_containing_string("ATTK", faction_lines))
    print(f"Total of attack attempts from {model} = {attack_attempts}")
    output_line.append(f"{attack_attempts:.2f}")
    print(f"Total of attack attempts from {model} [from agents] = {total_internal_attack_attempts}")
    output_line.append(f"{total_internal_attack_attempts:.2f}")

    # Parse successful attacks
    # 2026-02-06 21:02:47;[SYSTEM];[LLMAgent|Gemini|gemini-2.5-flash-lite|Green|3];INFO Attacked succeeded at GridTile(Clone) (9, 13). Kill count = 0.
    success_attack_attempts = len(find_lines_containing_string("Attacked succeeded", faction_lines))
    output_line.append(f"{success_attack_attempts:.2f}")
    percentage_success_attacks = 0
    if attack_attempts > 0:
        percentage_success_attacks = success_attack_attempts / attack_attempts

    output_line.append(f"{percentage_success_attacks * 100:.2f}")
    output_line.append(f"{(1 - percentage_success_attacks) * 100:.2f}")

    print(
        f"Total of success attack attempts from {model}  = {success_attack_attempts} ({percentage_success_attacks * 100:.2f}%)")

    output_line.append(f"{total_internal_wrong_attack:.2f}")
    output_line.append(f"{total_internal_attack_attempts - total_internal_wrong_attack:.2f}")
    output_line.append(f"{total_internal_attack_attempts:.2f}")
    percentage_failed_attacks_from_agents = total_internal_wrong_attack / total_internal_attack_attempts
    output_line.append(f"{(1 - percentage_failed_attacks_from_agents) * 100:.2f}")
    output_line.append(f"{percentage_failed_attacks_from_agents * 100:.2f}")
    print(
        f"Total of success attack attempts from {model} [from agents] = {total_internal_attack_attempts - total_internal_wrong_attack} ({(1 - percentage_failed_attacks_from_agents) * 100:.2f}%)")
    print(f"Total of failed attack attempts from {model} [from agents] = {total_internal_wrong_attack} ({percentage_failed_attacks_from_agents * 100:.2f}%)")

    print(f"Total of faulty messages {model} = {total_faulty_message}")
    output_line.append(f"{total_faulty_message:.2f}")

# Get the filename from command line arguments
filename = sys.argv[1]

output_line.append(f"{filename}")

# Read the file
with open(filename, 'r') as file:
    lines = file.readlines()

# General notes
print("General Information")
print("="*40)

first_line = lines[0]
last_line = lines[-1]

initial_time = first_line.split(';', 1)[0].lstrip('\ufeff')
last_time = last_line.split(';', 1)[0].lstrip('\ufeff')
print(f"Initial time = [{initial_time}]")
output_line.append(f"{initial_time}")
print(f"Last time = [{last_time}]")
start = datetime.strptime(initial_time, "%Y-%m-%d %H:%M:%S")
end = datetime.strptime(last_time, "%Y-%m-%d %H:%M:%S")
diff = end - start
total_seconds = diff.total_seconds()
minutes = int(total_seconds // 60)
seconds = int(total_seconds % 60)
print(f"Total time: {minutes}:{seconds}")
output_line.append(f"{total_seconds}")

green_faction_name = "|Green"
red_faction_name = "|Red"

# Parse models
llm_green_model = get_model(green_faction_name, lines)
llm_red_model = get_model(red_faction_name, lines)
print(f"RED {llm_red_model} x GREEN {llm_green_model}")
output_line.append(f"RED {llm_red_model}")
output_line.append(f"GREEN {llm_green_model}")

winner = find_lines_containing_string("won", lines)
if len(winner) == 0:
    print("No winner. Draw!")
    output_line.append(f"DRAW")
else:
    winning_lines = []
    for line in winner:
        winning_model = line.split(';')[3].strip()
        print(winning_model)
        winning_lines.append(winning_model)
    output_line.append(f"{'-'.join(winning_lines)}")

print("=" * 40)

move_lines = find_lines_containing_string("MOVE", lines)
green_moves = find_lines_containing_string(green_faction_name, move_lines)
red_moves = find_lines_containing_string(red_faction_name, move_lines)
print(f"Number of movements = {len(move_lines)}")
output_line.append(f"{len(move_lines)}")
print(f"Number of green movements = {len(green_moves)}")
print(f"Number of red movements = {len(red_moves)}")
move_ratio = max(len(green_moves), len(red_moves)) / len(move_lines)
print(f"Movement ratio = {move_ratio:.2f}/{1 - move_ratio:.2f}")
output_line.append(f"{move_ratio:.2f}/{1 - move_ratio:.2f}")
print("="*40)
print("General Information per Faction")
print("=" * 40)
parse_per_faction(green_faction_name, lines)
print("=" * 40)
parse_per_faction(red_faction_name, lines)

print("CSV line")
print("="*40)
print("filename; initial time; seconds; RED model; GREEN model; win/draw; total moves; move ratio;"
      "faction; model; avg_prompt; avg_req; max_req; min_req; avg_attempts; move_attempts; failed_moves; success_moves;"
      "% failed moves; total_internal_moves; total_internal_wrong_moves; total_internal_success_moves; %internal_failed_moves; %internal_success_moves;"
      "attack_attempts; attack_internal_attempts; success_attack_attempts; %success_attempts; %failed_attempts;"
      "total_internal_wrong_attacks; total_internal_success_attack; total_attacks; %success_attacks; %failed_attacks; %faulty_messages;"
      "faction; model; avg_prompt; avg_req; max_req; min_req; avg_attempts; move_attempts; failed_moves; success_moves;"
      "% failed moves; total_internal_moves; total_internal_wrong_moves; total_internal_success_moves; %internal_failed_moves; %internal_success_moves;"
      "attack_attempts; attack_internal_attempts; success_attack_attempts; %success_attempts; %failed_attempts;"
      "total_internal_wrong_attacks; total_internal_success_attack; total_attacks; %success_attacks; %failed_attacks; %faulty_messages;")
print(';'.join(output_line))