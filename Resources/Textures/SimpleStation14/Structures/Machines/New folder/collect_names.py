# Import the os module
import os

# Get the current working directory
directory = os.getcwd()

# Open a text file to write the file names
with open("file_names.txt", "w") as f:
  # Loop through all the files in the directory
  for file in os.listdir(directory):
    # Write the file name to the text file with a newline character
    f.write(file + "\n")