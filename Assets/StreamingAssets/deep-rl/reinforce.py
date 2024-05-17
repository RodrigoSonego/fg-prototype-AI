import numpy as np
from keras.api.models import Model, model_from_json
from keras.api.layers import Input, Dense
from keras.api.saving import load_model

def load_model_json() -> Model:
	return load_model('rl.keras')

def build_model(n_input: int, n_output:int, load_model: bool) -> Model: 
	if (load_model):
		return load_model_json()

	input = Input(batch_shape=(1, n_input))
	x = Dense(20, activation='tanh')(input)
	outputs = Dense(n_output, activation='tanh')(x)
	model = Model(inputs=input, outputs=outputs)
	model.compile(optimizer='adam', loss='mse', metrics=['mae'])
	return model

def choose_action(input, epsilon: float, model:Model):  
	if np.random.random() < epsilon:
		return np.array(np.random.uniform(low=0.0, high=1.0, size=3))
	else:
		return np.ndarray.max(model(np.array(input), training=False).numpy(), axis=0).tolist()


def learn(reward:float, new_state:list, previous_state:list, discount_factor:float, model:Model):
	target = reward + (discount_factor * model(np.array(new_state), training=False))

	model.fit(np.array(previous_state), target, epochs=1, verbose=0, batch_size=1)
	return model


def save_model(model: Model):
	model.save('rl.keras', overwrite=True)

# model = build_model(3, 2, False)

# save_model(model)

# model1 = load_model_json()

# a = choose_action([[0.2,0.2,0.2]], 0, model)
# b = choose_action([[0.2,0.2,0.2]], 0, model1)
# print(a)
# print(b)
# input = [[2, 1, 2]]
# idk = choose_action(input, 0, model)

# print("\n----------------------------------------\n")
# print(idk)