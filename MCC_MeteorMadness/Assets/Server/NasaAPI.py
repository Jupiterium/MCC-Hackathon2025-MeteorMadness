import requests
import json
from flask import Flask, jsonify

# Initialize the Flask application to serve as a local API server
app = Flask(__name__)

# Define API URL and parameters for NASA Neo feed
API_URL = "https://api.nasa.gov/neo/rest/v1/feed"
API_PARAMS = {
    "api_key": "5P4uLC1RY5tlh0rigvI5Cfd08vzuf0zWHvE4Cbwe",  # Replace with your actual API key
    "start_date": "2025-09-07",
    "end_date": "2025-09-08"
}

# Fetch NASA data from API
def fetch_nasa_data():
    response = requests.get(API_URL, params=API_PARAMS)
    
    if response.status_code == 200:
        return response.json()  # Return the parsed JSON data
    else:
        print(f"Request failed with status code: {response.status_code}")
        return None

# Define an API route that Unity can call to get the JSON data
@app.route('/get_neo_data', methods=['GET'])
def get_neo_data():
    data = fetch_nasa_data()  # Fetch the data from NASA API
    if data:
        return jsonify(data)  # Return the data as JSON
    else:
        return jsonify({"error": "Failed to fetch data"}), 500

# Start the local server
if __name__ == '__main__':
    app.run(debug=True, port=5000)