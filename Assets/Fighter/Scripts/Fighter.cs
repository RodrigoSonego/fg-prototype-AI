using System.Collections;
using UnityEngine;

public enum FighterState
{
	Idle,
	Walking,
	WalkingBack,
	Attacking,
	Hitstunned,
	Blocking
}

[RequireComponent(typeof(Rigidbody2D))]
public class Fighter : MonoBehaviour
{
	[SerializeField] float moveSpeed;
	[SerializeField] float walkBackSpeed;
	[SerializeField] Fighter opponent;
	[SerializeField] float proxityBlockRange = 5.0f;

	[SerializeField] float pushBackDistance = 0.3f;
	[SerializeField] float pushBlockDistance = 0.5f;

	[Space]
	[SerializeField] FighterAnimations animations;

	private Rigidbody2D rb;

	private float distanceToOpponent;

	[Tooltip("SERIALIZED FOR DEBUG ONLY!!!")]
	[SerializeField] private FighterState state;
	public FighterState State { get { return state; } }

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

		if (CanAct() == false) { return; }

		if ( WillBlock(inputDirection) )
		{
			SetBlockState();
			return;
		}

		animations.SetBlocking(false);
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

	private void SetBlockState()
	{
		state = FighterState.Blocking;

		animations.SetBlocking(true);
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
			opponent.ApplyPushback(isEnemyBlocking: true);
			return;
		}

		animations.PlayDamageAnimation();

		StartCoroutine(SetStateUnitlEndOfAnimation(FighterState.Hitstunned));

		opponent.ApplyPushback(isEnemyBlocking: false);
	}

	private bool CanAct()
	{
		return state == FighterState.Walking || state == FighterState.Idle || state == FighterState.WalkingBack;
	}

	private bool WillBlock(float inputDirection)
	{
		bool isInRightState = inputDirection == -1 && opponent.State == FighterState.Attacking;
		bool isInDistance = Mathf.Abs(distanceToOpponent) <= proxityBlockRange;

		return isInDistance && isInRightState;
	}

	private void ApplyPushback(bool isEnemyBlocking)
	{
		int pushDirection = distanceToOpponent > 0 ? -1 : 1;

		float distance = isEnemyBlocking ? pushBlockDistance : pushBackDistance;

		Vector2 newPosition = new(rb.position.x + (distance * pushDirection), rb.position.y);
		rb.MovePosition(newPosition);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Hurtbox"))
		{
			print("colidiu com " + collision.gameObject.name);
			opponent.HandleHitTaken();
		}
	}
}
