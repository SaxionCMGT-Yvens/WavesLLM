import sys
import os

def get_files_from_folder(folder_path):
    """Get all files from a folder and return them as a list."""
    try:
        all_items = os.listdir(folder_path)

        files_array = []
        for item in all_items:
            if item.startswith('.'):
                continue
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

def find_lines_containing_string(search_string, lines_array):
    return [single_line for single_line in lines_array if search_string in single_line]

def get_valid_files_from_folder(folder_path):
    files = get_files_from_folder(folder_path)
    valid_files = []
    for filename in files:
        file_path = os.path.join(folder_path, filename)
        with open(f"{file_path}", 'r', encoding='utf-8') as file:
            if filename.startswith('.') or filename.endswith(('.pyc', '.pyo', '.so', '.dll', '.exe', '.bin')):
                continue
            file_content = []
            # Iterate over the file and copy it
            for line in file:
                file_content.append(line.strip())
            valid_files.append(file_content)
    return valid_files