using System.Collections;
using UnityEngine;

public enum FighterState
{
	Idle,
	Walking,
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
	[SerializeField] Animator animator;
	[SerializeField] bool aiControlled = false;

	private PlayerInput input;
	private Rigidbody2D rb;

	private float distanceToOpponent;

	private FighterState state;
	public FighterState State { get { return state; } }

	// SERIALIZED FOR DEBUGGING THE BOT ONLY
	[SerializeField] private bool isWalkingBack = false;

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

	private void OnAttackPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
	{
		if (state != FighterState.Idle && state != FighterState.Walking) { return; }

		animator.SetTrigger("punch");

		StartCoroutine(SetStateUnitlEndOfAnimation(FighterState.Attacking));
	}

	private void FixedUpdate()
	{
		distanceToOpponent = opponent.transform.position.x - rb.position.x;

		bool isBlocking = isWalkingBack && opponent.State == FighterState.Attacking;
		if ( isBlocking )
		{
			state = FighterState.Blocking;
		}

		//  TODO: deixar assim por agora mas refatorar depois com uma 
		// estrutura melhor
		if (aiControlled) { return; }

		if (state != FighterState.Walking && state != FighterState.Idle) { return; }

		float inputDirection = input.Fighter.Walk.ReadValue<float>();

		if (opponent == null)
		{
			Debug.Log("No opponent found, will not take inputs");
			return;
		}

		if (WillWalkBack(inputDirection))
		{
			WalkBack(inputDirection);
			state = FighterState.Walking;
		}
		else if (inputDirection == 0)
		{
			state = FighterState.Idle;
			isWalkingBack = false;
		}
		else
		{
			Walk(inputDirection);
			state = FighterState.Walking;
		}

		UpdateAnimation(inputDirection);
	}

	private void UpdateAnimation(float inputDirection)
	{
		if (animator == null) { return; }

		animator.SetBool("blocking", state == FighterState.Blocking );
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
		rb.MovePosition(new(newX, rb.position.y));

		isWalkingBack = false;
	}

	private void WalkBack(float direction)
	{
		float newX = transform.position.x + (walkBackSpeed * direction * Time.deltaTime);
		rb.MovePosition(new(newX, rb.position.y));

		isWalkingBack = true;
	}

	IEnumerator SetStateUnitlEndOfAnimation(FighterState state)
	{
		this.state = state;

		yield return new WaitForEndOfFrame();

		// Only supposing we are using only layer 0
		float animLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

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

		animator.SetTrigger("damaged");

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
