import os
import mutagen
from mutagen.oggvorbis import OggVorbis
import yaml
from re import sub

# Function to convert a string to Snake Case
def snake_case(string):
    return sub(r'\W|_', ' ', string).title().replace(" ", "")

# Get the current directory where the script is located
directory = os.path.dirname(os.path.realpath(__file__))

# Output list to store metadata
metadata_list = []

# Iterate through files in the directory
for filename in os.listdir(directory):
    if filename.endswith(".ogg"):  # Filter for .ogg files
        file_path = os.path.join(directory, filename)

        # Get the audio duration using Mutagen
        audio = mutagen.File(file_path)
        duration = str(int(audio.info.length // 60)) + ":" + str(int(audio.info.length % 60)).zfill(2)

        # Extract the file name without extension
        name = os.path.splitext(filename)[0]

        # Create the ID and formatted name
        id = "Apelsinsaft" + snake_case(name)
        formatted_name = name.replace("_", " ")

        # Create the paths for audio and art
        audio_path = f"/Audio/SimpleStation14/JukeboxTracks/Apelsinsaft/{filename}"
        art_path = f"/Textures/SimpleStation14/JukeboxTracks/Apelsinsaft/{name}.png"

        # Create a metadata dictionary for this file
        metadata = {
            "type": "jukeboxTrack",
            "id": id,
            "name": formatted_name,
            "path": audio_path,
            "duration": duration,
            "artPath": art_path
        }

        # Append the metadata to the list
        metadata_list.append(metadata)

# Define the output file path
output_file = "metadata.yaml"

# Write the formatted metadata list as a single YAML document
with open(output_file, "w") as file:
    yaml.dump(metadata_list, file, default_flow_style=False)

print(f"Metadata has been written to {output_file}")
