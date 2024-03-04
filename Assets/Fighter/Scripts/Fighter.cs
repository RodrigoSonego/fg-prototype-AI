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
	[SerializeField] bool aiControlled = false;
	[SerializeField] float proxityBlockRange = 5.0f;

	[SerializeField] float pushBackDistance = 0.3f;
	[SerializeField] float pushBlockDistance = 0.5f;

	[Space]
	[SerializeField] FighterAnimations animations;

	private PlayerInput input;
	private Rigidbody2D rb;

	private float distanceToOpponent;

	[Tooltip("SERIALIZED FOR DEBUG ONLY!!!")]
	[SerializeField] private FighterState state;
	public FighterState State { get { return state; } }

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();

		state = FighterState.Idle;

		//  TODO: deixar assim por agora mas refatorar depois com uma 
		// estrutura melhor
		if (aiControlled) { return; }

		input = new PlayerInput();
		input.Fighter.Enable();
		input.Fighter.Attack.performed += OnAttackPressed;

		if (opponent == null) { return; }
		print(opponent.name);
	}

	private void FixedUpdate()
	{
		distanceToOpponent = opponent.transform.position.x - rb.position.x;

		//  TODO: deixar assim por agora mas refatorar depois com uma 
		// estrutura melhor
		if (aiControlled) { return; }

		float inputDirection = input.Fighter.Walk.ReadValue<float>();

		bool willBlock = WillBlock(inputDirection);

		state = willBlock ? FighterState.Blocking : FighterState.Idle;

		animations.SetBlocking(willBlock);

		if (CanAct()) { return; }

		if (opponent == null)
		{
			Debug.Log("No opponent found, will not take inputs");
			return;
		}

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

		animations.UpdateAnimation(state);
	}

	private void OnAttackPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
	{
		if (CanAct()) { return; }

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
			opponent.ApplyPushBlock();
			return;
		}

		animations.PlayDamageAnimation();

		StartCoroutine(SetStateUnitlEndOfAnimation(FighterState.Hitstunned));

		opponent.ApplyPushback();
	}

	private bool CanAct()
	{
		return state != FighterState.Walking && state != FighterState.Idle && state != FighterState.WalkingBack;
	}

	private bool WillBlock(float inputDirection)
	{
		bool isInRightState = inputDirection == -1 && opponent.State == FighterState.Attacking;
		bool isInDistance = Mathf.Abs(distanceToOpponent) <= proxityBlockRange;

		return isInDistance && isInRightState;
	}

	private void ApplyPushback()
	{
		int pushbackDirection = distanceToOpponent > 0 ? -1 : 1;

		Vector2 newPosition = new(rb.position.x + (pushBackDistance * pushbackDirection), rb.position.y);
		rb.MovePosition(newPosition);
	}

	private void ApplyPushBlock()
	{
		int pushblockDirection = distanceToOpponent > 0 ? -1 : 1;

		Vector2 newPosition = new(rb.position.x + (pushBlockDistance * pushblockDirection), rb.position.y);
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
