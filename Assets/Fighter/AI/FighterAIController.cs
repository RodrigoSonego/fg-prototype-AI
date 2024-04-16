using System.Collections;
using System.Collections.Generic;
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

    bool isMoving = false;

	private void Awake()
	{
		if (fighter == null) { print("ASSIGN FIGHTER TO CONTROLLER " + name); return; }
	}

	private void FixedUpdate()
	{
		if (isMoving == false) 
		{ 
			StartCoroutine(Move(GetDirection()));
		}

		DecideIfWillBlock();
		DecideIfWillAttack();

		Debug.DrawLine(transform.position, transform.position + (Vector3.left * attackDistance), Color.red);
	}

	private int GetDirection()
	{
		if ( IsCloserThanMinDistance() )
		{
			return GetOppositeDirection();
		}

		return Random.Range(0.0f, 1.0f) < moveForwardProbability ? 
			GetForwardDirection() : GetForwardDirection();
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

		if (Random.Range(0.0f, 1.0f) < attackProbability)
		{
			fighter.OnAttackPressed();
		}
	}

    private void DecideIfWillBlock()
    {
        if (IsInAttackDistance() == false && fighter.Opponent.State != FighterState.Attacking) { return; }

        if (Random.Range(0.0f, 1.0f) < blockProbability)
        {
            fighter.HandleMovementInput(GetOppositeDirection());
        }
    }
}
