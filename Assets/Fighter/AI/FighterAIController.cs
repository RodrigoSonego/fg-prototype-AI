using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FighterAIController : MonoBehaviour
{
	[SerializeField] private Fighter fighter;

	[SerializeField] float moveTime = 1.0f;
	[SerializeField] float minDistanceToOpponent;
	[SerializeField] float moveForwardProbability = 0.6f;
	[Space]
	[SerializeField] float attackDistance = 2.5f;
	[SerializeField] float attackProbability = 0.5f;
    [SerializeField] float blockProbability = 0.5f;
	[Space]
	[SerializeField] int framesBetweenLearning = 15;

    bool isMoving = false;

	private ReinforcementManager reinforcement;

	private float[] lastInput;
	private float lastDistance;
	private int frameCount;

	private enum AgentAction
	{
		Attack,
		Back,
		None
	}

	private void Awake()
	{
		if (fighter == null) { print("ASSIGN FIGHTER TO CONTROLLER " + name); return; }
	}

	private void Start()
	{
		reinforcement = ReinforcementManager.Instance;

		fighter.OnZeroHP += OnAgentDied;
		fighter.OnHitTaken += OnHitTaken;
		fighter.OnHitBlocked += OnHitBlocked;

		fighter.Opponent.OnZeroHP += OnOpponentDefeated;
		fighter.Opponent.OnHitTaken += OnDamageDone;
		fighter.Opponent.OnHitBlocked += OnOpponentBlocked;
	}

	private void FixedUpdate()
	{
		if (isMoving == false)
		{
			StartCoroutine(Move(GetDirection()));
		}

		//Learn(CalculateReward());

		ChooseAndPerformAction();
	}

	private void Learn(float reward)
	{
		if (lastInput == null) { return; }

		if (frameCount == framesBetweenLearning - 1)
		{
			float[] input = GetNetworkInputs();

			reinforcement.Learn(reward, lastInput, input);

			frameCount = 0;
		}

		frameCount++;
	}

	private void ChooseAndPerformAction()
	{
		float[] input = GetNetworkInputs();
		lastDistance = fighter.DistanceToOpponent;

		float[] output = reinforcement.ChooseAction(input);

		if (output is null) { print("é null por algum motivo"); }

		AgentAction action = (AgentAction)GetIndexOfMaxValue(output);

		switch (action)
		{
			case AgentAction.Attack:
				fighter.OnAttackPressed();
				fighter.HandleBlockInput(isPressed: false);
				break;
			case AgentAction.Back:
				fighter.HandleBlockInput(isPressed: true);
				break;
			case AgentAction.None:
				fighter.HandleMovementInput(0.0f);
				fighter.HandleBlockInput(isPressed: false);
				break;
		}

		lastInput = input;
		//print($"lastDist no frame {frameCount}: {lastDistance}");
	}

	private int GetDirection()
	{
		if ( IsCloserThanMinDistance() )
		{
			return GetOppositeDirection();
		}

		return UnityEngine.Random.Range(0.0f, 1.0f) < moveForwardProbability ? 
			GetForwardDirection() : GetOppositeDirection();
    }

    private int GetForwardDirection()
    {
        return fighter.DistanceToOpponent > 0 ? 1 : -1;
    }

    private int GetOppositeDirection()
	{
        return fighter.DistanceToOpponent > 0 ? -1 : 1;
    }

	IEnumerator Move(int direction)
	{
		float timeElapsed = 0.0f;

		isMoving = true;
		while (timeElapsed < moveTime)
		{
			yield return new WaitForEndOfFrame();

			fighter.HandleMovementInput(direction);

			timeElapsed += Time.fixedDeltaTime;

            if (IsCloserThanMinDistance())
            {
				yield break;
            }

		}

		isMoving = false;
	}

	private bool IsCloserThanMinDistance()
	{
		return Mathf.Abs(fighter.DistanceToOpponent) < minDistanceToOpponent;
    }

	private float[] GetNetworkInputs()
	{
		float distanceToOpponent = Vector2.Distance(fighter.transform.position, fighter.Opponent.transform.position);
		int agentState = (int)fighter.State;
		int opponentState = (int)fighter.State;

		return new float[] { distanceToOpponent, agentState, opponentState };
	}

	private int GetIndexOfMaxValue(float[] array)
	{
		float maxValue = array.Max();

		return Array.IndexOf(array, maxValue);
	}

	private float CalculateReward()
	{
		if(lastInput is null) { return 0.0f; }

		float distDelta = Mathf.Abs(fighter.DistanceToOpponent) - Mathf.Abs(lastDistance);

		return distDelta;
	}

	private void LearnAndDecreaseEpsilon(float reward)
	{
		Learn(reward);

		reinforcement.DecreaseEpsilon();
	}

	private void OnAgentDied()
	{
		LearnAndDecreaseEpsilon(-2);
	}

	private void OnOpponentDefeated()
	{
		LearnAndDecreaseEpsilon(2);
	}

	private void OnHitTaken()
	{
		Learn(-1);
	}

	private void OnHitBlocked()
	{
		Learn(-0.5f);
	}

	private void OnDamageDone()
	{
		Learn(1);
	}

	private void OnOpponentBlocked()
	{
		Learn(0.5f);
	}
}
