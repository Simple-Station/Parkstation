import os
import re

def title_to_snake(title):
    # Replace spaces with underscores
    snake = re.sub(r'\s+', '_', title)
    # Convert to lowercase
    snake = snake.lower()
    # Replace non-alphanumeric characters with underscores
    snake = re.sub(r'[^a-z0-9_]', '_', snake)
    # Remove consecutive underscores
    snake = re.sub(r'_{2,}', '_', snake)
    # Remove underscores at the beginning and end
    snake = snake.strip('_')
    return snake

# Set the directory containing the files
directory = os.path.dirname(os.path.abspath(__file__))

# Set the string to be removed from the file names
remove_string = "_mono"

# Loop through each file in the directory
for filename in os.listdir(directory):
    # Ignore the script file
    if filename == os.path.basename(__file__):
        continue
    # Ignore files that are not .ogg files
    if not filename.endswith('.ogg'):
        continue
    # Remove the string from the file name
    name = filename.replace(remove_string, '')
    # Convert the name to snake_case
    new_name = title_to_snake(name)
    # Rename the file with the new name and extension
    new_filename = remove_string + new_name + '.ogg'
    old_filepath = os.path.join(directory, filename)
    new_filepath = os.path.join(directory, new_filename)
    os.rename(old_filepath, new_filepath)