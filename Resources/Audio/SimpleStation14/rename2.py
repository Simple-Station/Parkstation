import os
import re

def remove_pattern(name, pattern):
    return re.sub(pattern, '', name)

def to_snake_case(name):
    s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', name)
    return re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1).lower()

def rename_files_in_directory(directory, pattern):
    for filename in os.listdir(directory):
        if os.path.isfile(os.path.join(directory, filename)):
            new_filename = to_snake_case(remove_pattern(filename, pattern))
            os.rename(os.path.join(directory, filename), os.path.join(directory, new_filename))

rename_files_in_directory('.', r'0. _dieter van der _westen _')
