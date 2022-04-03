import glob
import os

from matplotlib import pyplot as plt
import numpy as np
from keras import layers, models, losses, metrics

# Input & output size definitions
INPUT_SIZE = (28, 28, 1)
OUTPUT_SIZE = 345


def make_one_hot_encoding(categories, index):
    return np.array([int(i == index) for i in range(categories)])


def load_data():
    input_data = np.empty([0, INPUT_SIZE[0], INPUT_SIZE[1], INPUT_SIZE[2]])
    labels = np.empty([0, OUTPUT_SIZE])

    all_files = glob.glob(os.path.join("D:/Google_Draw_Dataset/" "*.npy"))
    all_files.sort(key=lambda x: x.lower())
    print(all_files)
    for i, file in enumerate(all_files):
        data = np.load(file, mmap_mode="r")
        # data = np.array([d.reshape(INPUT_SIZE) for d in data[5500: 6000, :]])
        data = np.array([d.reshape(INPUT_SIZE) for d in data[1500: 3000, :]])
        # print(type(data))
        one_hot_encoding = make_one_hot_encoding(OUTPUT_SIZE, i)
        label = [one_hot_encoding for _ in range(len(data))]

        # print(data.shape)
        input_data = np.concatenate((input_data, data), axis=0)
        labels = np.concatenate((labels, label), axis=0)

        print(f"{i} / {len(all_files)} ({all_files[i]})")

        # if all_files[i] == "D:/Google_Draw_Dataset\\axe.npy":
        #     for d in data:
        #         # print(data[0])
        #         # print(label)
        #         plt.imshow(d, interpolation='nearest')
        #         plt.show()

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


def main():
    model = models.load_model("model2.h5")
    model.summary()
    # model = models.Sequential()
    #
    # # Input layer
    # model.add(layers.Input(shape=INPUT_SIZE))
    #
    # # Convolutional layers
    # model.add(layers.Conv2D(filters=32, kernel_size=(3, 3), activation="relu"))  # 64
    # model.add(layers.MaxPool2D(pool_size=(2, 2)))
    # model.add(layers.Conv2D(filters=64, kernel_size=(3, 3), activation="relu"))  # 128
    # model.add(layers.MaxPool2D(pool_size=(2, 2)))
    # model.add(layers.Conv2D(filters=256, kernel_size=(3, 3), activation="relu"))  # 360
    # model.add(layers.MaxPool2D(pool_size=(2, 2)))
    #
    # # Fully connected layers
    # model.add(layers.Flatten())
    # model.add(layers.Dense(360, activation="relu"))
    # model.add(layers.Dense(OUTPUT_SIZE, activation="softmax"))
    #
    # model.summary()
    # # model.compile(
    # #     loss="categorical_crossentropy",
    # #     optimizer="adam",
    # #     metrics=["top_k_categorical_accuracy"]
    # # )
    # model.compile(
    #     optimizer='adam',
    #     loss="categorical_crossentropy",  # losses.SparseCategoricalCrossentropy(from_logits=True),
    #     metrics=['accuracy', metrics.TopKCategoricalAccuracy(k=5)]
    # )

    # model.save("model2.h5")

    data = np.load("D:/Google_Draw_Dataset\\apple.npy", mmap_mode="r")
    img = 20685
    print(data[img].reshape(28, 28))
    plt.imshow(data[img].reshape(28, 28), interpolation='nearest')
    plt.show()

    pred = model.predict(data[img].reshape(1, 28, 28, 1))[0]
    for i, v in enumerate(pred):
        print(f"{i}: {v}")
    # return

    for n in range(1, 344):
        pred_copy = [p for p in pred]
        pred_copy.sort(reverse=True)
        if len(pred_copy) > n:
            pred_copy = pred_copy[:n]
        if pred[8] in pred_copy:
            print(f"yay {n}")
            break
        else:
            print(f"nay {n}")

    return

    test_data, test_labels, train_data, train_labels = load_data()
    print(f"test_data: {test_data.shape}")
    print(f"test_labels: {test_labels.shape}")
    print(f"train_data: {train_data.shape}")
    print(f"train_labels: {train_labels.shape}")

    # model.fit(x=train_data, y=train_labels, validation_split=0.1, batch_size=256, verbose=2, epochs=5)
    # score = model.evaluate(train_data, train_labels, verbose=2)
    # print('Test accuracy: {:0.2f}%'.format(score[1] * 100))

    history = model.fit(
        train_data,
        train_labels,
        batch_size=256,
        epochs=2,
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

    model.save("model_6")


def test_model():
    model = models.load_model("anywhere")
    model.compile(
        optimizer='adam',
        loss="categorical_crossentropy",  # losses.SparseCategoricalCrossentropy(from_logits=True),
        # metrics=['accuracy', "top_k_categorical_accuracy"]
        metrics=["accuracy", metrics.TopKCategoricalAccuracy(k=10)]
    )
    test_data, test_labels, train_data, train_labels = load_data()
    loss, acc, kacc = model.evaluate(test_data, test_labels)
    print(f"loss: {loss}")
    print(f"acc: {acc}")
    print(f"kacc: {kacc}")


if __name__ == "__main__":
    main()
    # test_model()
