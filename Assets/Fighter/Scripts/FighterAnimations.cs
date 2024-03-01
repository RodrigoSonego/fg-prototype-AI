using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FighterAnimations : MonoBehaviour
{
	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void UpdateAnimation(FighterState state)
	{
		switch (state)
		{
			case FighterState.Idle:
				animator.SetBool("is_walking", false);
				break;
			case FighterState.Walking:
				animator.SetBool("is_walking", true);
				animator.SetFloat("walk_direction", 1);
				break;
			case FighterState.WalkingBack:
				animator.SetBool("is_walking", true);
				animator.SetFloat("walk_direction", -1);
				break;
			case FighterState.Hitstunned:
				animator.SetTrigger("damaged");
				break;
			default:
				break;
		}
	}

	public void PlayDamageAnimation()
	{
		animator.SetTrigger("damaged");
	}

	public void SetBlocking(bool isBlocking)
	{
		animator.SetBool("blocking", isBlocking);
	}

	public void PlayPunchAnimation()
	{
		animator.SetTrigger("punch");
	}

	public float GetCurrentAnimationLength()
	{
		// Only supposing we are using only layer 0
		return animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
	}
}
