using Microsoft.CSharp.RuntimeBinder;
using Python.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReinforcementManager : MonoBehaviour
{
	[Header("Model Config")]
	[SerializeField] int numberOfInputs;
	[SerializeField] int numberOfOutputs;
	[SerializeField] float epsilon;
	[SerializeField] float epsilonDecay;
	[SerializeField] float discountFactor;
	//[SerializeField] int framesBetweeenLearning = 15;

	[Space]
	[SerializeField] bool loadSavedModel;
	[SerializeField] bool willTrain;

	PyObject model;

	private int numberOfEpisodes = 0;
	dynamic reinforcement;
	private float totalReward;

	public static ReinforcementManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != this)
		{
			Destroy(Instance);
			Instance = this;
		}
	}

	private void Start()
	{ 
		BuildModel();

		reinforcement = Py.Import("reinforce");
	}

	private void BuildModel()
	{
		using (Py.GIL())
		{
			using dynamic reinforcement = Py.Import("reinforce");
			model = reinforcement.build_model(numberOfInputs, numberOfOutputs, loadSavedModel);
		}
	}

	public float[] ChooseAction(float[] inputs)
	{
		try
		{
			float[,] input = new float[1, 3] { { inputs[0], inputs[1], inputs[2] } };
			return (float[])reinforcement.choose_action(input, epsilon, model);
		}
		catch (RuntimeBinderException _)
		{
			return null;
		}
	}

	public void Learn(float reward, float[] input, float[] new_input)
	{
		float[,] mat_input = new float[1, 3] { { input[0], input[1], input[2] } };
		float[,] mat_new_input = new float[1, 3] { { new_input[0], new_input[1], new_input[2] } };

		reinforcement.learn(reward, mat_new_input, mat_input, discountFactor, model);
	}

	public void SaveModel()
	{
		reinforcement.save_model(model);
	}

	public void DecreaseEpsilon()
	{
		epsilon *= epsilonDecay;
	}
}
