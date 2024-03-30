try:
    import tensorflow as tf
    from PIL import Image
    import sys
    from tensorflow.keras.models import load_model
    from tensorflow.keras.layers import BatchNormalization
    from tensorflow.keras.optimizers import Adamax

    # Loading the model
    loaded_model = load_model('Assets/StreamingAssets/Trained.h5', compile=False, custom_objects={'DebuggableBatchNormalization': BatchNormalization})

    # Compiling the model
    loaded_model.compile(optimizer=Adamax(learning_rate=0.001), loss='categorical_crossentropy', metrics=['accuracy'])

    # Loading class labels
    def load_class_labels(label_file):
        with open(label_file, 'r') as f:
            class_labels = [line.strip() for line in f]
        return class_labels

    class_labels = load_class_labels('Assets/StreamingAssets/class_labels.txt')

    # Loading and preprocessing image
    image_path = sys.argv[1]
    image = Image.open(image_path)
    image = image.convert("RGB")
    img = image.resize((224, 224))
    img_array = tf.keras.preprocessing.image.img_to_array(img)
    img_array = tf.expand_dims(img_array, 0)

    # Making predictions
    predictions = loaded_model.predict(img_array)
    predicted_class_index = tf.argmax(predictions[0])
    predicted_class = class_labels[predicted_class_index]
    print(predicted_class, end='') 

except FileNotFoundError:
    print("Model file or label file not found.")
except Exception as e:
    print("An error occurred:", e)
