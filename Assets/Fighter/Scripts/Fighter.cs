using System;
using System.Collections;
using UnityEngine;

public enum FighterState
{
	Idle,
	Walking,
	WalkingBack,
	Attacking,
	Hitstunned,
	Blockstunned,
	Blocking
}

[RequireComponent(typeof(Rigidbody2D))]
public class Fighter : MonoBehaviour
{
	[SerializeField] float moveSpeed;
	[SerializeField] float walkBackSpeed;
	[SerializeField] float proxityBlockRange = 5.0f;

	[SerializeField] float pushBackDistance = 0.3f;
	[SerializeField] float pushBlockDistance = 0.5f;

	[Space]
	[SerializeField] FighterAnimations animations;

	//"SERIALIZED FOR DEBUG ONLY!!!"
	[SerializeField] private FighterState state;
	public FighterState State { get { return state; } }

	[SerializeField] Fighter opponent;
	public Fighter Opponent { get { return opponent; } }

	/// <summary>
	/// Event that triggers when Fighter takes a hit, uses a bool as parameter 
	/// to determine if damage was done (if blocked, there is no damage)
	/// </summary>
	public event Action<bool> OnHitTaken;

	public float DistanceToOpponent { get { return distanceToOpponent; } }

	private Rigidbody2D rb;
	private float distanceToOpponent;

	private int blockstunFrames = 5;

	[SerializeField] private int blockOffsetFrames = 3;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();

		state = FighterState.Idle;
	}


	public void HandleMovementInput(float inputDirection)
	{
		if (opponent == null)
		{
			Debug.Log("No opponent found, will not take inputs");
			return;
		}
		distanceToOpponent = opponent.transform.position.x - rb.position.x;


		if ( CanAct() == false ) { return; }

		if (WillBlock(inputDirection))
		{
			SetBlockState(true);
			return;
		}

        SetBlockState(false);
        Move(inputDirection);

		animations.UpdateAnimation(state);
	}

	private void Move(float inputDirection)
	{
		if (WillWalkBack(inputDirection))
		{
			WalkBack(inputDirection);
			state = FighterState.WalkingBack;
		}
		else if (inputDirection == 0)
		{
			state = FighterState.Idle;
		}
		else
		{
			Walk(inputDirection);
			state = FighterState.Walking;
		}
	}

	private void SetBlockState(bool isBlocking)
	{
		state = isBlocking ? FighterState.Blocking : FighterState.Idle;
		animations.SetBlocking(isBlocking);

		if ( isBlocking )
		{
			StartCoroutine(BlockOffset());
		}
	}

	public void OnAttackPressed()
	{
		if (CanAct() == false) { return; }

		animations.PlayPunchAnimation();

		StartCoroutine(SetStateUnitlEndOfAnimation(FighterState.Attacking, 0.15f));
	}

	private bool WillWalkBack(float inputDirection)
	{
		return distanceToOpponent * inputDirection < 0;
	}

	private void Walk(float direction)
	{
		print(name + ": " + direction);

		float newX = transform.position.x + (moveSpeed * direction * Time.deltaTime);
		rb.MovePosition(new(newX, rb.position.y));
	}

	private void WalkBack(float direction)
	{
		float newX = transform.position.x + (walkBackSpeed * direction * Time.deltaTime);
		rb.MovePosition(new(newX, rb.position.y));
	}

	IEnumerator SetStateUnitlEndOfAnimation(FighterState state, float offset = 0.0f)
	{
		this.state = state;

		yield return new WaitForEndOfFrame();

		float animLength = animations.GetCurrentAnimationLength();

		yield return new WaitForSeconds(animLength + offset);
		yield return new WaitForEndOfFrame();

		this.state = FighterState.Idle;
	}

	public void HandleHitTaken()
	{
		if (state == FighterState.Blocking)
		{
			opponent.ApplyPushback(isBlocking: true);
			StartCoroutine(Block());

			return;
		}

		animations.PlayDamageAnimation();

		StartCoroutine(SetStateUnitlEndOfAnimation(FighterState.Hitstunned, 0.2f));

		ApplyPushback(isBlocking: false);
	}

	public bool CanAct()
	{
		return state == FighterState.Walking || state == FighterState.Idle || state == FighterState.WalkingBack;
	}

	private bool WillBlock(float inputDirection)
	{
		bool isInRightState = WillWalkBack(inputDirection) && opponent.State == FighterState.Attacking;
		bool isInDistance = Mathf.Abs(distanceToOpponent) <= proxityBlockRange;

		return isInDistance && isInRightState;
	}

	private void ApplyPushback(bool isBlocking)
	{
		int pushDirection = distanceToOpponent > 0 ? -1 : 1;

		float distance = isBlocking ? pushBlockDistance : pushBackDistance;
		Vector2 newPosition = new(rb.position.x + (distance * pushDirection), rb.position.y);
		rb.MovePosition(newPosition);
	}

	IEnumerator Block()
	{
		float currentFrame = 0;
		SetBlockState(true);

		while (currentFrame < blockstunFrames)
		{
			yield return new WaitForFixedUpdate();
			currentFrame++;
		}

		SetBlockState(false);
	}

	IEnumerator BlockOffset()
	{
		int framesElapsed = 0;

		while (framesElapsed < blockOffsetFrames)
		{
			yield return new WaitForFixedUpdate();
			framesElapsed++;
		}

		SetBlockState(false);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Hurtbox"))
		{
			opponent.HandleHitTaken();
		}
	}
}
