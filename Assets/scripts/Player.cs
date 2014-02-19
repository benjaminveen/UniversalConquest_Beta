using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player {


	public List<Unit> units = new List<Unit> ();

	
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
	
	// Update is called once per frame
	public virtual void Update () {	
	}
	
	public virtual void TurnUpdate () {
		//each player has to manually end the turn for now.
		foreach (Unit unit in units) 
			unit.TurnUpdate ();
	}
	
	public virtual void TurnOnGUI () {
		
	}

	public void OnGUI() {
	}
}
