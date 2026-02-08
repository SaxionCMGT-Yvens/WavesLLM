import sys
import os

def get_files_from_folder(folder_path):
    """Get all files from a folder and return them as a list."""
    try:
        all_items = os.listdir(folder_path)

        files_array = []
        for item in all_items:
            full_path = os.path.join(folder_path, item)
            if os.path.isfile(full_path):
                files_array.append(item)

        return files_array
    except FileNotFoundError:
        print(f"Error: Folder '{folder_path}' not found.")
        sys.exit(1)
    except PermissionError:
        print(f"Error: Permission denied for folder '{folder_path}'.")
        sys.exit(1)
    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)


folder = sys.argv[1]
files = get_files_from_folder(folder)
output_lines = []

i = 0
for filename in files:
    file_path = os.path.join(folder, filename)
    with open(f"{file_path}", 'r', encoding='utf-8') as file:
        if filename.startswith('.') or filename.endswith(('.pyc', '.pyo', '.so', '.dll', '.exe', '.bin')):
            continue

        lines = file.read().splitlines()
        if i == 0:
            # Header line
            output_lines.append(lines[-2])
            i += 1
        # CSV line
        output_lines.append(lines[-1])

for output_line in output_lines:
    print(output_line)