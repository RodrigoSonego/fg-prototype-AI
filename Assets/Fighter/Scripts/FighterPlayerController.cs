using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterPlayerController : MonoBehaviour
{
	[SerializeField] private Fighter fighter;

	PlayerInput input;

	private void Start()
	{
		if(fighter == null) { print("ASSIGN FIGHTER TO CONTROLLER"); return; }

		input = new PlayerInput();

		input.Fighter.Enable();
		input.Fighter.Attack.performed += OnAttackPressed;
	}

	private void FixedUpdate()
	{
		if (fighter == null) { print("ASSIGN FIGHTER TO CONTROLLER"); return; }

		bool blockPressed = input.Fighter.Block.ReadValue<float>() == 1;
		fighter.HandleBlockInput(blockPressed);

		if(blockPressed) { return; }

		float inputDirection = input.Fighter.Walk.ReadValue<float>();
		fighter.HandleMovementInput(inputDirection);
	}

	private void OnAttackPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
	{
		fighter.OnAttackPressed();
	}
}
