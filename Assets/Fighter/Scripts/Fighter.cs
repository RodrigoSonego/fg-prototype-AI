using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Fighter : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float walkBackSpeed;
	[SerializeField] private Fighter opponent;
	[SerializeField] private Animator animator;

	private PlayerInput input;
	private Rigidbody2D rb;

	private float distanceToOpponent;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();

		input = new PlayerInput();
		input.Fighter.Walk.Enable();

		if(opponent ==  null) { return; }
		print(opponent.name);
	}

	private void FixedUpdate()
	{
		float inputDirection = input.Fighter.Walk.ReadValue<float>();

		if(opponent == null ) { return; }
		distanceToOpponent = opponent.transform.position.x - rb.position.x;

		if(WillWalkBack(inputDirection))
		{
			WalkBack(inputDirection);
		}
		else
		{
			Walk(inputDirection);
		}

		UpdateAnimation(inputDirection);
	}

    private void UpdateAnimation(float inputDirection)
    {
        if (animator == null) { return; }

		animator.SetBool("is_walking", inputDirection != 0);
		animator.SetFloat("walk_direction", inputDirection);
    }

    private bool WillWalkBack(float inputDirection)
	{
		return distanceToOpponent * inputDirection < 0;
	}

	private void Walk(float direction)
	{
		float newX = transform.position.x + (moveSpeed * direction * Time.deltaTime);
		rb.MovePosition(new (newX, rb.position.y));
	}

	private void WalkBack(float direction)
	{
		float newX = transform.position.x + (walkBackSpeed * direction * Time.deltaTime);
		rb.MovePosition(new(newX, rb.position.y));
	}
}
