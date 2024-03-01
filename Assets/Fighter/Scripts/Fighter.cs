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

	[Space]
	[SerializeField] FighterAnimations animations;

	private PlayerInput input;
	private Rigidbody2D rb;

	private float distanceToOpponent;

	private FighterState state;
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

		bool isBlocking = state == FighterState.WalkingBack && opponent.State == FighterState.Attacking;
		if (isBlocking)
		{
			state = FighterState.Blocking;
		}

		//  TODO: deixar assim por agora mas refatorar depois com uma 
		// estrutura melhor
		if (aiControlled) { return; }

		if (state != FighterState.Walking && state != FighterState.Idle && state != FighterState.WalkingBack) { return; }

		float inputDirection = input.Fighter.Walk.ReadValue<float>();

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
		if (state != FighterState.Walking && state != FighterState.Idle && state != FighterState.WalkingBack) { return; }

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
			//TODO: pushblock stuff
			return;
		}

		animations.PlayDamageAnimation();

		StartCoroutine(SetStateUnitlEndOfAnimation(FighterState.Hitstunned));
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
