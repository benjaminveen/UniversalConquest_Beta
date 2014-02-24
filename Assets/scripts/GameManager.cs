using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	
	public GameObject TilePrefab;
	public GameObject PlayerUnitPrefab;
	public GameObject EnemyUnitPrefab;
	
	public int mapSize = 22;
	
	public List <List<Tile>> map = new List<List<Tile>>();
	public List <Player> players = new List<Player>();
	public int currentPlayerIndex = 0;


	public Unit currentUnit;//currently selected/highlighted unit
	public Player currentPlayer;//current player taking actions.
	List<Unit> allUnits = new List<Unit> ();//all active units on the board.

	public float tileScale = 2.5f; //scale of board tiles.
	Vector3[,] tileCoordinates;

	void Awake() {
		instance = this;
	}
	
	// Use this for initialization
	void Start () {		
		tileCoordinates = new Vector3[mapSize, mapSize];
		generateMap();
		generatePlayers();
		currentPlayer = players [currentPlayerIndex];
		foreach (Player p in players)
			allUnits.AddRange (p.units);
	}
	
	// Update is called once per frame
	void Update () {
		currentPlayer.TurnUpdate ();
	}
	
	void OnGUI () {
		if (currentUnit != null && currentPlayer.units.Contains (currentUnit)) 
			currentUnit.TurnOnGUI();
		//end turn button
		float buttonHeight = 50;
		float buttonWidth = 150;
		Rect buttonRect = new Rect(0, Screen.height - buttonHeight * 1, buttonWidth, buttonHeight);		
		if (GUI.Button(buttonRect, "End Turn")) 
		{
			removeTileHighlights ();
			nextTurn ();
		}
	}
	
	public void nextTurn() {
		currentPlayer.restoreActionPoints ();
		if (currentPlayerIndex + 1 < players.Count) 
			currentPlayerIndex++;
		else 
			currentPlayerIndex = 0;
		currentPlayer = players [currentPlayerIndex];
		Debug.Log ("Now it's player " + currentPlayerIndex + " turn");
	}

	public void highlightTilesAt(Vector2 originLocation, Color highlightColor, int distance) {
		highlightTilesAt(originLocation, highlightColor, distance, true);
	}

	public void highlightTilesAt(Vector2 originLocation, Color highlightColor, int distance, bool ignorePlayers) {

		List <Tile> highlightedTiles = new List<Tile>();

		if (ignorePlayers) 
			highlightedTiles = TileHighlight.FindHighlight(map[(int)originLocation.x][(int)originLocation.y], distance);
		else 
			highlightedTiles = TileHighlight.FindHighlight(map[(int)originLocation.x][(int)originLocation.y], distance, allUnits.Where(x => x.gridPosition != originLocation).Select(x => x.gridPosition).ToArray());
		
		foreach (Tile t in highlightedTiles) {
			t.transform.renderer.material.color = highlightColor;
		}
	}
	
	public void removeTileHighlights() {
		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				if (!map[i][j].impassible) map[i][j].transform.renderer.material.color = Color.white;
			}
		}
	}
 	
	public void moveCurrentPlayer(Tile destTile) {
		if (destTile.transform.renderer.material.color != Color.white && !destTile.impassible && currentUnit.positionQueue.Count == 0) {
			removeTileHighlights();
			currentUnit.moving = false;
			foreach(Tile t in TilePathFinder.FindPath
			        (map[(int)currentUnit.gridPosition.x][(int)currentUnit.gridPosition.y],destTile, 
			 		allUnits.Where(x => x.gridPosition != destTile.gridPosition && x.gridPosition != currentUnit.gridPosition).Select(x => x.gridPosition).ToArray())) {
				currentUnit.positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position + 1.5f * Vector3.up);
				Debug.Log("(" + currentUnit.positionQueue[currentUnit.positionQueue.Count - 1].x + "," + currentUnit.positionQueue[currentUnit.positionQueue.Count - 1].y + ")");
			}			
			currentUnit.gridPosition = destTile.gridPosition;

		} else {
			Debug.Log ("destination invalid");
		}
	}
	
	public void attackWithCurrentPlayer(Tile destTile) {
		if (destTile.transform.renderer.material.color != Color.white && !destTile.impassible && destTile.gridPosition != currentUnit.gridPosition) {
			
			Unit target = null;
			foreach(Unit u in allUnits)
				if(u.gridPosition == destTile.gridPosition && !currentPlayer.units.Contains (u))
						target = u;
					

			if (target != null) {
								
				//Debug.Log ("p.x: " + players[currentPlayerIndex].gridPosition.x + ", p.y: " + players[currentPlayerIndex].gridPosition.y + " t.x: " + target.gridPosition.x + ", t.y: " + target.gridPosition.y);
				if (currentUnit.gridPosition.x >= target.gridPosition.x - 1 && currentUnit.gridPosition.x <= target.gridPosition.x + 1 &&
					currentUnit.gridPosition.y >= target.gridPosition.y - 1 && currentUnit.gridPosition.y <= target.gridPosition.y + 1) {
					currentUnit.actionPoints--;
					
					removeTileHighlights();
					currentUnit.attacking = false;			
					
					//attack logic
					//roll to hit
					bool hit = Random.Range(0.0f, 1.0f) <= currentUnit.attackChance;
					
					if (hit) {
						//damage logic
						int amountOfDamage = (int)Mathf.Floor(currentUnit.damageBase + Random.Range(0, currentUnit.damageRollSides));
						
						target.HP -= amountOfDamage;
						
						Debug.Log(currentUnit.unitName + " successfuly hit " + target.unitName + " for " + amountOfDamage + " damage!");
					} else {
						Debug.Log(currentUnit.unitName + " missed " + target.unitName + "!");
					}
				} else {
					Debug.Log ("Target is not adjacent!");
				}
			}
		} else {
			Debug.Log ("destination invalid");
		}
	}
	
	void generateMap() {
		map = new List<List<Tile>>();
		for (int i = 0; i < mapSize; i++) {
			List <Tile> row = new List<Tile>();
			for (int j = 0; j < mapSize; j++) {
				float x = tileScale * ( i - Mathf.Floor(mapSize/2 ));
				float z = tileScale * (-j + Mathf.Floor(mapSize/2));
				tileCoordinates[i,j] = new Vector3(x,0,z);
				Tile tile = ((GameObject)Instantiate(TilePrefab, tileCoordinates[i,j], Quaternion.identity)).GetComponent<Tile>();
				tile.gridPosition = new Vector2(i, j);
				row.Add (tile);
			}
			map.Add(row);
		}
	}
	void generatePlayers() {

		//add units
		//team 0
		Player player = new Player ();
		List<Unit> units = new List<Unit> ();
		Unit unit;

		unit = ((GameObject)Instantiate(PlayerUnitPrefab, unitCoordinates (0,0), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
		unit.gridPosition = new Vector2(0,0);
		unit.unitName = "Bob_Team1";
		units.Add (unit);

		unit = ((GameObject)Instantiate(PlayerUnitPrefab, unitCoordinates (1,1), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
		unit.gridPosition = new Vector2 (1, 1);
		unit.unitName = "Brian_Team1";	
		units.Add (unit);

		player.addUnits (units);
		players.Add(player);

		//team 1
		player = new Player ();
		units = new List<Unit> ();

		unit = ((GameObject)Instantiate(EnemyUnitPrefab, unitCoordinates (6,4), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
		unit.gridPosition = new Vector2(6,4);
		unit.unitName = "Carl_Team2";
		units.Add (unit);
		
		unit = ((GameObject)Instantiate(EnemyUnitPrefab, unitCoordinates (5,3), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
		unit.gridPosition = new Vector2(5,3);
		unit.unitName = "Cameron_Team2";
		units.Add (unit);

		player.addUnits (units);
		players.Add (player);
	}
	private Vector3 unitCoordinates(int x, int z)
	{
		//.25f is just some arbitrary offset used to try and center the unit. it's not working so far.
		return new Vector3 (tileCoordinates [x, z].x + .25f, 1.5f, tileCoordinates [x, z].z - .25f);
	}
}
