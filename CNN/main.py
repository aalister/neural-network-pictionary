import glob
import os

from matplotlib import pyplot as plt
import numpy as np
from keras import layers, models, losses, metrics

# Input & output size definitions
INPUT_SIZE = (28, 28, 1)
OUTPUT_SIZE = 97


def make_one_hot_encoding(categories, index):
    return np.array([int(i == index) for i in range(categories)])


def load_data(start=0, number=500):
    input_data = np.empty([0, INPUT_SIZE[0], INPUT_SIZE[1], INPUT_SIZE[2]])
    labels = np.empty([0, OUTPUT_SIZE])

    all_files = glob.glob(os.path.join("D:/Google_Draw_Dataset/smaller/" "*.npy"))
    all_files.sort(key=lambda x: x.lower())
    print(all_files)
    for i, file in enumerate(all_files):
        data = np.load(file, mmap_mode="r")
        data = np.array([d.reshape(INPUT_SIZE) for d in data[start: number, :]])
        # print(type(data))
        one_hot_encoding = make_one_hot_encoding(OUTPUT_SIZE, i)
        label = [one_hot_encoding for _ in range(len(data))]

        # print(data.shape)
        input_data = np.concatenate((input_data, data), axis=0)
        labels = np.concatenate((labels, label), axis=0)

        print(f"{i} / {len(all_files) - 1} ({all_files[i]})")

    print(input_data.shape)
    print(labels.shape)

    # separate into training and testing
    permutation = np.random.permutation(len(input_data))
    input_data = input_data[permutation, :]
    labels = labels[permutation]

    # 0.2 = vfold ratio
    vfold_size = int(input_data.shape[0] / 100 * (0.2 * 100))

    test_data = input_data[0:vfold_size, :]
    test_labels = labels[0:vfold_size]

    train_data = input_data[vfold_size:input_data.shape[0], :]
    train_labels = labels[vfold_size:labels.shape[0]]

    return test_data, test_labels, train_data, train_labels


def train(path=None):
    if path:
        model = models.load_model(path)
    else:
        model = models.Sequential()

        # Input layer
        model.add(layers.Input(shape=INPUT_SIZE))

        # Convolutional layers
        model.add(layers.Conv2D(filters=32, kernel_size=(3, 3), activation="relu"))  # 64
        model.add(layers.MaxPool2D(pool_size=(2, 2)))
        model.add(layers.Conv2D(filters=64, kernel_size=(3, 3), activation="relu"))  # 128
        model.add(layers.MaxPool2D(pool_size=(2, 2)))
        model.add(layers.Conv2D(filters=256, kernel_size=(3, 3), activation="relu"))  # 360
        model.add(layers.MaxPool2D(pool_size=(2, 2)))

        # Fully connected layers
        model.add(layers.Flatten())
        model.add(layers.Dense(360, activation="relu"))
        model.add(layers.Dense(OUTPUT_SIZE, activation="softmax"))

        model.summary()
        model.compile(
            optimizer='adam',
            loss="categorical_crossentropy",  # losses.SparseCategoricalCrossentropy(from_logits=True),
            metrics=['accuracy', metrics.TopKCategoricalAccuracy(k=5)]
        )

    test_data, test_labels, train_data, train_labels = load_data()
    print(f"test_data: {test_data.shape}")
    print(f"test_labels: {test_labels.shape}")
    print(f"train_data: {train_data.shape}")
    print(f"train_labels: {train_labels.shape}")

    history = model.fit(
        train_data,
        train_labels,
        batch_size=256,
        epochs=5,
        validation_data=(test_data, test_labels)
    )

    plt.plot(history.history['accuracy'], label='accuracy')
    plt.plot(history.history['val_accuracy'], label='val_accuracy')
    plt.xlabel('Epoch')
    plt.ylabel('Accuracy')
    plt.legend(loc='lower right')
    plt.show()

    loss, acc, kacc = model.evaluate(test_data, test_labels)
    print(f"loss: {loss}")
    print(f"acc: {acc}")
    print(f"kacc: {kacc}")

    model.save("C:/Users/Joshua/Documents/Models/model3.h5")


def train_small(path=None):
    if path:
        model = models.load_model(path)
    else:
        model = models.Sequential()

        # Input layer
        model.add(layers.Input(shape=INPUT_SIZE))

        # Convolutional layers
        model.add(layers.Conv2D(filters=16, kernel_size=(3, 3), activation="relu"))  # 64
        model.add(layers.MaxPool2D(pool_size=(2, 2)))
        model.add(layers.Conv2D(filters=32, kernel_size=(3, 3), activation="relu"))  # 128
        model.add(layers.MaxPool2D(pool_size=(2, 2)))
        model.add(layers.Conv2D(filters=64, kernel_size=(3, 3), activation="relu"))  # 360
        model.add(layers.MaxPool2D(pool_size=(2, 2)))

        # Fully connected layers
        model.add(layers.Flatten())
        model.add(layers.Dense(128, activation="relu"))
        model.add(layers.Dense(OUTPUT_SIZE, activation="softmax"))

        model.summary()
        model.compile(
            optimizer='adam',
            loss="categorical_crossentropy",  # losses.SparseCategoricalCrossentropy(from_logits=True),
            metrics=['accuracy', metrics.TopKCategoricalAccuracy(k=5)]
        )

    test_data, test_labels, train_data, train_labels = load_data(start=0, number=5000)
    print(f"test_data: {test_data.shape}")
    print(f"test_labels: {test_labels.shape}")
    print(f"train_data: {train_data.shape}")
    print(f"train_labels: {train_labels.shape}")

    history = model.fit(
        train_data,
        train_labels,
        batch_size=256,
        epochs=20,
        validation_data=(test_data, test_labels)
    )

    plt.plot(history.history['accuracy'], label='accuracy')
    plt.plot(history.history['val_accuracy'], label='val_accuracy')
    plt.xlabel('Epoch')
    plt.ylabel('Accuracy')
    plt.legend(loc='lower right')
    plt.show()

    loss, acc, kacc = model.evaluate(test_data, test_labels)
    print(f"loss: {loss}")
    print(f"acc: {acc}")
    print(f"kacc: {kacc}")

    model.save("C:/Users/Joshua/Documents/Models/smallmodel.h5")


if __name__ == "__main__":
    train_small()
