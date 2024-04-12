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

	private Rigidbody2D rb;
	private float distanceToOpponent;


	private float blockstunFrames = 5;

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

		SetBlockState(WillBlock(inputDirection));

		// TODO: achar uma forma de descagar e conseguir sair do block
		if ( CanAct() == false ) { return; }

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
		if (isBlocking == false && state != FighterState.Blocking)
		{
			return;
		}

		state = isBlocking ? FighterState.Blocking : FighterState.Idle;
		animations.SetBlocking(isBlocking);
	}

	public void OnAttackPressed()
	{
		if (CanAct() == false) { return; }

		animations.PlayPunchAnimation();

		StartCoroutine(SetStateUnitlEndOfAnimation(FighterState.Attacking));
	}

	private bool WillWalkBack(float inputDirection)
	{
		return distanceToOpponent * inputDirection < 0;
	}

	private void Walk(float direction)
	{
		float newX = transform.position.x + (moveSpeed * direction * Time.deltaTime);
		rb.MovePosition(new(newX, rb.position.y));
	}

	private void WalkBack(float direction)
	{
		float newX = transform.position.x + (walkBackSpeed * direction * Time.deltaTime);
		rb.MovePosition(new(newX, rb.position.y));
	}

	IEnumerator SetStateUnitlEndOfAnimation(FighterState state)
	{
		this.state = state;

		yield return new WaitForEndOfFrame();

		float animLength = animations.GetCurrentAnimationLength();

		yield return new WaitForSeconds(animLength);
		yield return new WaitForEndOfFrame();

		this.state = FighterState.Idle;
	}

	public void HandleHitTaken()
	{
		if (state == FighterState.Blocking)
		{
			opponent.ApplyPushblock();
			StartCoroutine(Block());
			return;
		}

		animations.PlayDamageAnimation();

		StartCoroutine(SetStateUnitlEndOfAnimation(FighterState.Hitstunned));

		ApplyPushback();
	}

	public bool CanAct()
	{
		return state == FighterState.Walking || state == FighterState.Idle || state == FighterState.WalkingBack;
	}

	private bool WillBlock(float inputDirection)
	{
		float oppositeToOpponent = distanceToOpponent > 0 ? -1 : 1;

		bool isInRightState = inputDirection == oppositeToOpponent && opponent.State == FighterState.Attacking;
		bool isInDistance = Mathf.Abs(distanceToOpponent) <= proxityBlockRange;

		return isInDistance && isInRightState;
	}

	private void ApplyPushback()
	{
		int pushDirection = distanceToOpponent > 0 ? -1 : 1;

		Vector2 newPosition = new(rb.position.x + (pushBackDistance * pushDirection), rb.position.y);
		rb.MovePosition(newPosition);
	}
	
	private void ApplyPushblock()
	{
		int pushDirection = distanceToOpponent < 0 ? -1 : 1;

		Vector2 newPosition = new(rb.position.x + (pushBackDistance * pushDirection), rb.position.y);
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

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Hurtbox"))
		{
			opponent.HandleHitTaken();
		}
	}
}
