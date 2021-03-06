﻿using UnityEngine;
using System.Collections;

public class PlayerUnit : Unit {
	
	// Use this for initialization
	void Start () {
		
	}
	// Update is called once per frame
	public override void Update () {
		//green for currently selected unit
		//white by default
		if (GameManager.instance.currentUnit == this) 
			transform.renderer.material.color = Color.green;
		else
			transform.renderer.material.color = Color.white;

		if (GameManager.instance.currentUnit == this && GameManager.instance.currentPlayer.hasUnit (this))
		{
			if(actionPoints > 0 && HP > 0)
			{
				if (Input.GetKeyDown ("z"))
					moveCommand ();
				if (Input.GetKeyDown ("x"))
					attackCommand ();
			}
		}
		base.Update();
	}

	public override void TurnUpdate ()
	{
		//highlight
		if (positionQueue.Count > 0) {
			transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
			
			if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f) {
				transform.position = positionQueue[0];
				positionQueue.RemoveAt(0);
				if (positionQueue.Count == 0) {
					actionPoints--;
				}
			}
		}
	}
	private void displayButtons()
	{
		float buttonHeight = 50;
		float buttonWidth = 150;
		
		Rect buttonRect = new Rect(0, Screen.height - buttonHeight * 3, buttonWidth, buttonHeight);

		//move button
		if (GUI.Button (buttonRect, "Move"))
			moveCommand ();
		
		//attack button
		buttonRect = new Rect(0, Screen.height - buttonHeight * 2, buttonWidth, buttonHeight);
		
		if (GUI.Button (buttonRect, "Attack"))
			attackCommand ();
	}
	private void moveCommand()
	{
		if (!moving) 
		{
			GameManager.instance.removeTileHighlights();
			moving = true;
			attacking = false;
			GameManager.instance.highlightTilesAt(gridPosition, Color.blue, movementPerActionPoint, false);
		} 
		else 
		{
			moving = false;
			attacking = false;
			GameManager.instance.removeTileHighlights();
		}
	}
	private void attackCommand()
	{
		if (!attacking) 
		{
			GameManager.instance.removeTileHighlights();
			moving = false;
			attacking = true;
			GameManager.instance.highlightTilesAt(gridPosition, Color.red, attackRange);
		} 
		else 
		{
			moving = false;
			attacking = false;
			GameManager.instance.removeTileHighlights();
		}
	}
	public override void TurnOnGUI () 
	{
		if(actionPoints > 0 && HP > 0)
			displayButtons ();
	}
}
