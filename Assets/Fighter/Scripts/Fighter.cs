using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Fighter : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float walkBackSpeed;
	[SerializeField] private Fighter opponent;
	[SerializeField] private Animator animator;
	[SerializeField] private bool aiControlled = false;

	private PlayerInput input;
	private Rigidbody2D rb;

	private float distanceToOpponent;

	private bool canAct = true;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();

		//  TODO: deixar assim por agora mas refatorar depois com uma 
		// estrutura melhor
		if (aiControlled) { return; }

		input = new PlayerInput();
		input.Fighter.Enable();
		input.Fighter.Attack.performed += OnAttackPressed;

		if(opponent ==  null) { return; }
		print(opponent.name);
	}

    private void OnAttackPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
		if (canAct == false) { return; }

		animator.SetTrigger("punch");

		StartCoroutine(DisableMovementUnitlEndOfAnimation());
    }

    private void FixedUpdate()
	{
        //  TODO: deixar assim por agora mas refatorar depois com uma 
        // estrutura melhor
        if (aiControlled) { return; }

		if (canAct == false) { return; }

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

	IEnumerator DisableMovementUnitlEndOfAnimation()
	{
		canAct = false;

		yield return new WaitForEndOfFrame();

		// Only supposing we are using only layer 0
        float animLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
		print("punch len: " + animLength);

		yield return new WaitForSeconds(animLength);
		yield return new WaitForEndOfFrame();
		canAct = true;
    }
}
