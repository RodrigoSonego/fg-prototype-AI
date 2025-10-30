# Fighting Game RL

This is a fighting game prototyped coupled with an AI agent that can train against the player. Based on the [Unity-PythonNet implementation done by shiena](https://github.com/shiena/Unity-PythonNet)

## 🔧Tools used

- Windows (does **not work on Linux**)
- [Python 3.11.9 Embedded Package](https://www.python.org/downloads/release/python-3119)
- [Python.NET](https://www.nuget.org/packages/pythonnet) 3.05
- [Keras](https://keras.io) 3.12
- [Tensorflow](https://www.tensorflow.org/) 2.16.1

## ⚙Setup

- Clone the project, open a terminal in the root and `cd` into the embedded python folder
    - `cd Assets/StreamingAssets/python-3.11.9-embed-amd64`
- Run the following command to install pip
    - `curl -L https://bootstrap.pypa.io/get-pip.py | ./python`
- After that, with pip installed, you can install the required packages described in `…/StreamingAssets/deep-rl/requirements.txt` running the following command (while still in the python root)
    - `./Scripts/pip.exe install -r ../deep-rl/requirements.txt`
- And that’s it! You should be able to run the project now.

## 💾Saving/Loading Models
- Saving and loading models is controlled by inspector properties of the `ReinforcementManager` script.
- To load a model on startup, certify that `Load Save Model` is ticked
- To use the data as training, tick `Will Train`
- To save the model simply click the `Save Model` button. 
By default the model is saved as `rl.keras`, located in `Assets/StreamingAssets/deep-rl/`