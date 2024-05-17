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
	}

	private void FixedUpdate()
	{
		if (isMoving == false) 
		{ 
			StartCoroutine(Move(GetDirection()));
		}

		if (fighter.CanAct() == false) { return; }

		Learn();

		ChooseAndPerformAction();
	}

	private void Learn()
	{
		if (lastInput == null) { return; }

		if (frameCount == framesBetweenLearning - 1)
		{
			float[] input = GetNetworkInputs();
			float reward = CalculateReward();

			reinforcement.Learn(reward, lastInput, input);

			frameCount = 0;
		}

		frameCount++;
	}

	private void ChooseAndPerformAction()
	{
		float[] input = GetNetworkInputs();

		float[] output = reinforcement.ChooseAction(input);

		if (output is null) { print("é null por algum motivo"); }

		AgentAction action = (AgentAction)GetIndexOfMaxValue(output);

		switch (action)
		{
			case AgentAction.Attack:
				fighter.OnAttackPressed();
				break;
			case AgentAction.Back:
				fighter.HandleMovementInput(GetOppositeDirection());
				break;
			case AgentAction.None:
				fighter.HandleMovementInput(0.0f);
				break;
		}

		lastInput = input;
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
		yield return new WaitWhile(() =>
		{
			fighter.HandleMovementInput(direction);

			timeElapsed += Time.fixedDeltaTime;

            if (IsCloserThanMinDistance())
            {
				return false;
            }

            return timeElapsed < moveTime;
		});

		isMoving = false;
	}

	private bool IsCloserThanMinDistance()
	{
		return Mathf.Abs(fighter.DistanceToOpponent) < minDistanceToOpponent;
    }

	private bool IsInAttackDistance()
	{
        return Mathf.Abs(fighter.DistanceToOpponent) < attackDistance;
    }

	private void DecideIfWillAttack()
	{
		if (IsInAttackDistance() == false) { return; }

		if (UnityEngine.Random.Range(0.0f, 1.0f) < attackProbability)
		{
			fighter.OnAttackPressed();
		}
	}

    private void DecideIfWillBlock()
    {
        if (IsInAttackDistance() == false && fighter.Opponent.State != FighterState.Attacking) { return; }

        if (UnityEngine.Random.Range(0.0f, 1.0f) < blockProbability)
        {
            fighter.HandleMovementInput(GetOppositeDirection());
        }
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
		float opponentHpLost = fighter.Opponent.MaxHealth - fighter.Opponent.Health;
		float agentHpLost = fighter.MaxHealth - fighter.Health;

		return (opponentHpLost - agentHpLost) / fighter.MaxHealth;
	}
}
