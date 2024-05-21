using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField] Fighter fighter1;
	[SerializeField] Fighter fighter2;

	public static GameController instance;

	Vector3 p1InitialPos;
	Vector3 p2InitialPos;

	private void Awake()
	{
		if(instance != this)
		{
			Destroy(instance);
			instance = this;
		}

		p1InitialPos = fighter1.transform.position;
		p2InitialPos = fighter2.transform.position;

		fighter1.OnZeroHP += ResetGame;
		fighter2.OnZeroHP += ResetGame;
	}

	private void ResetGame()
	{
		fighter1.transform.position = p1InitialPos;
		fighter2.transform.position = p2InitialPos;

		fighter1.ResetHealth();
		fighter2.ResetHealth();
	}
}
