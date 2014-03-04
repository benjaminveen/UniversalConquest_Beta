using UnityEngine;
using System.Collections;

public class AIPlayer : Player 
{
	public int aiTurnDelay = 2;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public override void Update () 
	{
		//automatic turn end
		bool turnOver = true;
		foreach (Unit u in units)
			if (u.actionPoints > 0)
				turnOver = false;
		
		if (turnOver)
			GameManager.instance.nextTurn ();
	}

	public override void TurnUpdate()
	{
		foreach (Unit unit in units) 
			if (unit.actionPoints > 0) 
			{
				unit.TurnUpdate ();
				turnDelay ();
			}
	}
	IEnumerator turnDelay()
	{
		// Waits an amount of time
		yield return new WaitForSeconds (aiTurnDelay);
	}
}
