using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player {


	public List<Unit> units = new List<Unit> ();
	public string name;
	
	void Awake () {
	}
	
	// Use this for initialization
	void Start () {
	}

	public void addUnits(List<Unit> addUnits)
	{
		units.AddRange (addUnits);
	}
	public bool hasUnit(Unit unit)
	{
		return units.Contains (unit);
	}
	public void restoreActionPoints()
	{
		foreach (Unit unit in units)
			unit.restoreActionPoints ();
	}
	public bool hasLost()
	{
		foreach (Unit unit in units)
			if (unit.HP > 0)
				return false;
		return true;
	}

	public void resetUnits()
	{
		foreach (Unit u in units) 
		{
			u.moving = false;
			u.attacking = false;
		}
	}
	// Update is called once per frame
	public virtual void Update () 
	{	
		//get next active unit by pressing C.
		if (Input.GetKeyDown ("c") && GameManager.instance.currentPlayer == this) 
		{
			Unit u = nextActiveUnit ();
			if(u != null)
			{
				GameManager.instance.currentUnit = u;
				GameManager.instance.removeTileHighlights();
				resetUnits ();
			}
		}

		//automatic turn end
		bool turnOver = true;
		foreach (Unit u in units)
			if (u.actionPoints > 0)
				turnOver = false;

		if (turnOver)
			GameManager.instance.nextTurn ();
	}
	protected Unit nextActiveUnit()
	{
		foreach(Unit u in units)
			if(u != GameManager.instance.currentUnit && u.actionPoints > 0 && u.HP > 0)
				return u;

		return null;
	}
	
	public virtual void TurnUpdate () {
		foreach (Unit unit in units) 
			if(unit.actionPoints > 0)
				unit.TurnUpdate ();
	}
	
	public virtual void TurnOnGUI () {
		
	}

	public void OnGUI() {
	}
}
