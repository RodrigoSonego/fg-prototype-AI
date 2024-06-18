using System;
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
		input.Fighter.Attack.performed += OnLightAttackPressed;
		input.Fighter.HeavyPunch.performed += OnHeavyAttackPressed;
	}

	private void FixedUpdate()
	{
		if (fighter == null) { print("ASSIGN FIGHTER TO CONTROLLER"); return; }

		bool blockPressed = input.Fighter.Block.ReadValue<float>() == 1;
		fighter.HandleBlockInput(blockPressed);

		if (blockPressed) { return; }

		float inputDirection = input.Fighter.Walk.ReadValue<float>();
		fighter.HandleMovementInput(inputDirection);
	}

	private void OnLightAttackPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
	{
		fighter.OnAttackPressed(isHeavy: false);
	}

	private void OnHeavyAttackPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
	{
		fighter.OnAttackPressed(isHeavy: true);
	}
}
