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
	public List<Unit> allUnits = new List<Unit> ();//all active units on the board.

	public float tileScale = 2.5f; //scale of board tiles.
	Vector3[,] tileCoordinates;
	public Color tileColor;

	//camera values
	public int ScrollWidth = 15;
	public int ScrollSpeed = 25;
	public int MaxCameraHeight = 100;
	public int MinCameraHeight = 10;

	public bool gameActive = true; //if game is still going on.

	public GUIText combatText;
	public GUIText victoryText;


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
		MoveCamera ();
		currentPlayer.TurnUpdate ();
		currentPlayer.Update ();
		//deselect unit
		if (Input.GetMouseButtonDown (1)) 
		{
			currentUnit = null;
			removeTileHighlights ();
			currentPlayer.resetUnits();
		}
		//check for winner
		foreach (Player p in players)
			if (p.hasLost ())
				gameActive = false;

		//end turn with spacebar
		if (Input.GetKeyDown ("space") && currentPlayer.GetType () != typeof(AIPlayer))
			nextTurn ();

	}
	void displayWinner()
	{
		string winner = "";
		foreach(Player p in players)
			if(!p.hasLost())
				winner = p.name;

		GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
		combatText.text = "";
		victoryText.text = winner + " has won!";
	}
	
	void OnGUI () {
		if (gameActive) 
			displayHud ();
		else
			displayWinner ();
	}
	void displayHud()
	{
		if (currentUnit != null && currentPlayer.units.Contains (currentUnit)) 
			currentUnit.TurnOnGUI();
		//end turn button
		float buttonHeight = 50;
		float buttonWidth = 150;
		Rect buttonRect = new Rect(0, Screen.height - buttonHeight * 1, buttonWidth, buttonHeight);	
		
		GUI.Label(new Rect(10f, 10f, 100f, 100f), "Player " + currentPlayerIndex + "'s turn");
		//GUI.TextArea
		if (GUI.Button(buttonRect, "End Turn")) 
		{
			removeTileHighlights ();
			nextTurn ();
		}
	}
	
	public void nextTurn() {
		currentPlayer.restoreActionPoints ();
		removeTileHighlights ();
		if (currentPlayerIndex + 1 < players.Count) 
			currentPlayerIndex++;
		else 
			currentPlayerIndex = 0;
		currentPlayer = players [currentPlayerIndex];
		Debug.Log ("Now it's player " + currentPlayer.name + "'s turn");
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
				if (!map[i][j].impassible) map[i][j].transform.renderer.material.color = tileColor;
			}
		}
	}
	public void moveCurrentPlayer(Tile destTile) {
		if (destTile.transform.renderer.material.color != tileColor && !destTile.impassible && currentUnit.positionQueue.Count == 0) {
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
		string combat = "";

		if (destTile.transform.renderer.material.color != tileColor && !destTile.impassible && destTile.gridPosition != currentUnit.gridPosition) {
			
			Unit target = null;
			foreach(Unit u in allUnits)
				if(u.gridPosition == destTile.gridPosition && !currentPlayer.units.Contains (u))
					target = u;
					

			if (target != null && target.HP > 0) {
								
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

						combat = currentUnit.unitName + " successfuly hit " + target.unitName + " for " + amountOfDamage + " damage!";
						if(target.HP <= 0)
							combat += "\n" + currentUnit.unitName + " has TERMINATED " + target.unitName;
					} else {
						combat = (currentUnit.unitName + " missed " + target.unitName + "!");
					}
					System.Threading.Thread.Sleep (1000);
				} else {
					combat = ("Target is not adjacent!");
				}
			}
		} else {
			combat = ("destination invalid");
		}
		combatText.text = combat;
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
		unit.unitName = "Bob";
		units.Add (unit);

		unit = ((GameObject)Instantiate(PlayerUnitPrefab, unitCoordinates (1,1), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
		unit.gridPosition = new Vector2 (1, 1);
		unit.unitName = "Bobbert";	
		units.Add (unit);

		player.addUnits (units);
		players.Add(player);
		player.name = "PLAYER 0";

		//team 1
		player = new AIPlayer ();
		units = new List<Unit> ();

		unit = ((GameObject)Instantiate(EnemyUnitPrefab, unitCoordinates (6,4), Quaternion.Euler(new Vector3()))).GetComponent<AIUnit>();
		unit.gridPosition = new Vector2(6,4);
		unit.unitName = "Carl";
		units.Add (unit);
		
		unit = ((GameObject)Instantiate(EnemyUnitPrefab, unitCoordinates (5,3), Quaternion.Euler(new Vector3()))).GetComponent<AIUnit>();
		unit.gridPosition = new Vector2(5,3);
		unit.unitName = "Carlton";
		units.Add (unit);

		player.addUnits (units);
		players.Add (player);
		player.name = "PLAYER 1";
	}
	
	private Vector3 unitCoordinates(int x, int z)
	{
		float tileSize = Mathf.Abs (tileCoordinates [0, 0].x - tileCoordinates [1, 0].x);
		return new Vector3 (tileCoordinates [x, z].x, 1.5f, tileCoordinates [x, z].z);
	}

	private void MoveCamera()
	{		
		
		Vector3 newPosition = Camera.mainCamera.transform.position;
	if (newPosition.x <= mapSize) {
		if (Input.mousePosition.x >= Screen.width - ScrollWidth) {            
			// Move the camera            
			Camera.mainCamera.transform.position += Camera.mainCamera.transform.right * Time.deltaTime * ScrollSpeed;            
		}
	}
	
	if(newPosition.x >= -mapSize){            
		if ( Input.mousePosition.x <= 0 + ScrollWidth )                
		{                
			// Move the camera                
			Camera.mainCamera.transform.position -= Camera.mainCamera.transform.right * Time.deltaTime * ScrollSpeed;                
		}        
	}
	if (newPosition.z <= mapSize) {
		if (Input.mousePosition.y >= Screen.height - ScrollWidth) {            
			// Move the camera            
			Camera.mainCamera.transform.position += Camera.mainCamera.transform.up * Time.deltaTime * ScrollSpeed;            
		}
	}
	
	if (newPosition.z >= -mapSize) {
		if (Input.mousePosition.y <= 0 + ScrollWidth) {            
			// Move the camera            
			Camera.mainCamera.transform.position -= Camera.mainCamera.transform.up * Time.deltaTime * ScrollSpeed;            
		}
	}
}


}
