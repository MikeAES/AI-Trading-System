import pandas as pd
import numpy as np
from keras.models import Sequential
from keras.layers import LSTM, Dense
from keras.optimizers import Adam
from sklearn.preprocessing import MinMaxScaler
from sklearn.metrics import accuracy_score
import matplotlib.pyplot as plt
from keras.callbacks import EarlyStopping

# Replace 'your_file_path' with the actual path to your CSV file
file_path_train = 'BinaryDataBuy.csv'

# Read the training CSV file into a Pandas DataFrame
df_train = pd.read_csv(file_path_train, header=None)

# Ensure that all values in the DataFrame are numeric
df_train = df_train.apply(pd.to_numeric, errors='coerce')

# Extract the number of features and days for training
n_features_train = df_train.shape[1]
n_days_train = 6  # Number of days in each sample

# Create sequences of samples and targets for training
sequences_train = []
targets_train = []

for i in range(n_days_train, df_train.shape[0]):
    sequences_train.append(df_train.iloc[i - n_days_train:i, :n_features_train-1].values)
    targets_train.append(df_train.iloc[i, n_features_train-1])

# Convert lists to NumPy arrays
X_train = np.array(sequences_train)
y_train = np.array(targets_train)

# Normalize the input features for training
scaler_train = MinMaxScaler()
X_scaled_train = scaler_train.fit_transform(X_train.reshape((X_train.shape[0], -1)))

# Reshape X back to the original shape
X_scaled_train = X_scaled_train.reshape((X_scaled_train.shape[0], n_days_train, n_features_train-1))

# Build a more complex LSTM model
model = Sequential()
model.add(LSTM(100, input_shape=(n_days_train, n_features_train-1), return_sequences=True))
model.add(LSTM(50, return_sequences=True))
model.add(LSTM(25, return_sequences=True))  # Add another LSTM layer
model.add(LSTM(10))  # Add a fourth LSTM layer with fewer units
model.add(Dense(1))
model.compile(optimizer=Adam(lr=0.001), loss='mse')

# Define early stopping callback
early_stopping = EarlyStopping(monitor='val_loss', patience=15, restore_best_weights=True)


# Train the model on the training set and keep track of the loss history
epochs = 250  # Increase the number of epochs
history = model.fit(X_scaled_train, y_train, epochs=epochs, batch_size=32, verbose=1)

# Plot training loss over epochs
plt.plot(history.history['loss'], label='Training Loss')
#plt.plot(history.history['val_loss'], label='Validation Loss')
plt.title('Model Loss')
plt.xlabel('Epoch')
plt.ylabel('Loss')
plt.legend()
plt.show()

# Print normalized input data
#print("Normalized Input Data for Prediction:")
#print(X_scaled_train[-1, :, :])

# Predict the next day
X_pred = X_scaled_train[-1, :, :]  # Use the last day as input for prediction
y_pred_continuous = model.predict(X_pred.reshape((1, n_days_train, n_features_train-1)))

# Set predictions above the threshold to 1 and below to -1
threshold = 0.5
y_pred = np.where(y_pred_continuous > threshold, 1, -1)
print("Predicted outcome:", y_pred)

# Estimate accuracy on the training set
y_pred_train_continuous = model.predict(X_scaled_train)
accuracy_train = accuracy_score(y_train, np.where(y_pred_train_continuous > threshold, 1, -1))
print(f'Accuracy on the training set: {accuracy_train * 100:.2f}%')
print("Number of Epochs Run:", len(history.history['loss']))

model.save('2DModelBuy3Percent.h5')
