using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
	[SerializeField] Fighter fighter1;
	[SerializeField] Fighter fighter2;

	[SerializeField] Slider healthBar1;
	[SerializeField] Slider healthBar2;

	private void Start()
	{
		healthBar1.maxValue = fighter1.MaxHealth;
		healthBar1.value = fighter1.MaxHealth;

		healthBar2.maxValue = fighter2.MaxHealth;
		healthBar2.value = fighter2.MaxHealth;

		fighter1.OnHitTaken += () => { healthBar1.value = fighter1.Health; };
		fighter2.OnHitTaken += () => { healthBar2.value = fighter2.Health; };
	}

	public void ResetHealthToMax()
	{
		healthBar1.value = healthBar1.maxValue;
		healthBar2.value = healthBar2.maxValue;
	}
}