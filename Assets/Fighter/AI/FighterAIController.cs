using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterAIController : MonoBehaviour
{
	[SerializeField] private Fighter fighter;

	[SerializeField] float moveTime = 1.0f;
	[SerializeField] float minDistanceToOpponent;
	[SerializeField] float attackDistance = 2.5f;

	bool isMoving = false;

	private void Awake()
	{
		if (fighter == null) { print("ASSIGN FIGHTER TO CONTROLLER " + name); return; }
	}

	private void FixedUpdate()
	{
		if (isMoving) { return; }

		int direction = GetDirection();

		StartCoroutine(Move(direction));
	}

	private int GetDirection()
	{
		if ( IsCloserThanMinDistance() )
		{
			return fighter.DistanceToOpponent > 0 ? -1 : 1;
		}

		return Random.Range(0.0f, 1.0f) < 0.5f ? 1 : -1;
    }

	IEnumerator Move(int direction)
	{
		float timeElapsed = 0.0f;

		isMoving = true;
		yield return new WaitWhile(() =>
		{
			fighter.HandleMovementInput(direction);

			timeElapsed += Time.deltaTime;

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
}
