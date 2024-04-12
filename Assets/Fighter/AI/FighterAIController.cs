using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterAIController : MonoBehaviour
{
	[SerializeField] private Fighter fighter;

	[SerializeField] float moveTime = 1.0f;
	[SerializeField] float minDistanceToOpponent;

	bool isMoving = false;

	private void Awake()
	{
		if (fighter == null) { print("ASSIGN FIGHTER TO CONTROLLER " + name); return; }
	}

	private void FixedUpdate()
	{
		if (isMoving) { return; }

		float direction = Random.Range(0.0f, 1.0f) < 0.5 ? 1.0f : -1.0f;

		StartCoroutine(Move(direction));
	}

	IEnumerator Move(float direction)
	{
		float timeElapsed = 0.0f;

		isMoving = true;
		yield return new WaitWhile(() =>
		{
			fighter.HandleMovementInput(direction);

			timeElapsed += Time.deltaTime;

			return timeElapsed < moveTime;
		});

		isMoving = false;
	}
}
