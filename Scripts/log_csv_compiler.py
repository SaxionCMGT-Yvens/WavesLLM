import sys
import file_utils

def main():
    folder = sys.argv[1]
    files = file_utils.get_valid_files_from_folder(folder)
    output_lines = []

    i = 0
    for lines in files:
        if lines[0] == "Matching files found: 0":
            continue
        if i == 0:
            # Header line
            output_lines.append(lines[-2])
            i += 1
        # CSV line
        output_lines.append(lines[-1])

    for output_line in output_lines:
        print(output_line)

if __name__ == "__main__":
    main()