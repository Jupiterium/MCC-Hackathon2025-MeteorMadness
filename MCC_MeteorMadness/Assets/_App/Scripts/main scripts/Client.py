import socket
import csv
import time
import numpy as np
import pandas as pd

# Read data from CSV using pandas
simulated_data = pd.read_csv("simulated_MP01.csv")

# Define data stream names (modify these based on your actual stream names)
data_stream_names = ["CHPTemp1", "LoopTemp1", "Stoom"]  #Edit this with the actual names, add as many as you want as long as you update the c# script as well

# HOST and PORT for connection (same as before)
HOST = '127.0.0.1'  # localhost
PORT = 25002

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.connect((HOST, PORT))

    # Loop through each row of data (excluding the header)
    for index, row in simulated_data.iterrows():
        if index == 0:  # Skip the header row
            continue

        # Create a single string with all four data points
        data_string = ""
        for stream_name in data_stream_names:
            # Access value directly from row (using its index)
            if stream_name in row.index:  # Check if name matches index (column name)
                data_point = row[stream_name]  # Access value by column name (index)
            else:
                # Handle missing column (optional)
                data_point = "NA"  # Or a default value

            data_string += f"{stream_name} {data_point}\n"  # Use f-string for formatted string

        # Send the combined data string
        s.sendall(data_string.encode('utf-8'))
        print(f"Sent: {data_string}")  # Print sent data for debugging

        # Wait a bit before sending the next data row
        time.sleep(0.1)
