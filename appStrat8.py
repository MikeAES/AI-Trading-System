#OneThatWorks - Pasted for New AI Strategy 8
from flask import Flask, request, jsonify
import json
import numpy as np
from sklearn.preprocessing import MinMaxScaler
from keras.models import load_model
import requests
import json

app = Flask(__name__)

model = load_model('2DModelBuy3Percent.h5')

@app.route('/predict', methods=['POST'])
def predict_single_value():
    data = request.json  # Get JSON data from the request
    print("Raw Data:", data)
    
    # Parse the JSON data to a list of dictionaries
    if isinstance(data, str):
        data = json.loads(data)

    # Extract values from the first dictionary in the list
    features_list0 = list(data.values())
    #features_list1 = [list(data[1].values())]
    #features_list2 = [list(data[2].values())]
    #features_list3 = [list(data[3].values())]
    #features_list4 = [list(data[4].values())]
    #features_list5 = [list(data[5].values())]
    print(features_list0)
    arrayjoin = features_list0
    print(arrayjoin)
    features_array = np.reshape(arrayjoin, (6,9))
    print("Array: ", features_array)
    print("ArrayShape: ", features_array.shape)

    # Scale the data using MinMaxScaler
    scaler = MinMaxScaler()
    scaled_features = scaler.fit_transform(features_array)

    # Reshape to 3D array for model
    X_predict = np.reshape(scaled_features,(1,6,9))
    print(X_predict)

    # Make predictions using the scaled input data
    y_pred = model.predict(X_predict)

    # Set predictions above the threshold to 1 and below to -1
    threshold = 0.5
    response = np.where(y_pred > threshold, 1, -1)

    # Return the prediction as JSON
    return jsonify({'response': response.tolist()})

if __name__ == '__main__':
   app.run(debug=True)
