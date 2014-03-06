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

	//only one unit acts at a time.
	int currentUnit = 0;
	public override void TurnUpdate ()
	{

		if (currentUnit >= units.Count)
			currentUnit = 0;

		if(units[currentUnit].actionPoints > 0)
			units[currentUnit].TurnUpdate();
		else
		{
			if(units.Count > 1)
				currentUnit++;
		}
	}
}
