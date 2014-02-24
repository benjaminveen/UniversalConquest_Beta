using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	
	public GameObject TilePrefab;
	public GameObject PlayerUnitPrefab;
	public GameObject EnemyUnitPrefab;

	//should be same as prefab's scaling
	//needs to be applied to the position vector of BOTH tiles and units during their generation
	//consider just shrinking the size of the models
	public int Xscaling = 1;
	public int Zscaling = 1;

	public int mapSize = 22;
	
	public List <List<Tile>> map = new List<List<Tile>>();
	public List <Player> players = new List<Player>();
	public int currentPlayerIndex = 0;


	public Unit currentUnit;//currently selected/highlighted unit
	public Player currentPlayer;//current player taking actions.
	List<Unit> allUnits = new List<Unit> ();//all active units on the board.

	//camera values
	public int ScrollWidth = 15;
	public int ScrollSpeed = 25;
	public int MaxCameraHeight = 100;
	public int MinCameraHeight = 10;

	void Awake() {
		instance = this;
	}
	
	// Use this for initialization
	void Start () {		
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
				Tile tile = ((GameObject)Instantiate(TilePrefab, new Vector3((i - Mathf.Floor(mapSize/2))*Xscaling,0, (-j + Mathf.Floor(mapSize/2))*Zscaling), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
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

		unit = ((GameObject)Instantiate(PlayerUnitPrefab, new Vector3(0 - Mathf.Floor(mapSize/2),1.5f, -0 + Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
		unit.gridPosition = new Vector2(0,0);
		unit.unitName = "Bob_Team1";
		units.Add (unit);

		unit = ((GameObject)Instantiate(PlayerUnitPrefab, new Vector3((mapSize-1) - Mathf.Floor(mapSize/2),1.5f, -(mapSize-1) + Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
		unit.gridPosition = new Vector2(mapSize-1,mapSize-1);
		unit.unitName = "Brian_Team1";	
		units.Add (unit);

		player.addUnits (units);
		players.Add(player);

		//team 1
		player = new Player ();
		units = new List<Unit> ();

		unit = ((GameObject)Instantiate(EnemyUnitPrefab, new Vector3(6 - Mathf.Floor(mapSize/2),1.5f, -4 + Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
		unit.gridPosition = new Vector2(6,4);
		unit.unitName = "Carl_Team2";
		units.Add (unit);
		
		unit = ((GameObject)Instantiate(EnemyUnitPrefab, new Vector3(5 - Mathf.Floor(mapSize/2),1.5f, -3 + Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<PlayerUnit>();
		unit.gridPosition = new Vector2(5,3);
		unit.unitName = "Cameron_Team2";
		units.Add (unit);

		player.addUnits (units);
		players.Add (player);
	}

	private void MoveCamera() {
		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		float zpos = Input.mousePosition.z;
		Vector3 movement = new Vector3(0,0,0);
		bool mouseScroll = false;
		
		//horizontal camera movement
		if(xpos >= 0 && xpos < ScrollWidth) {
			movement.x -= ScrollSpeed;
			mouseScroll = true;
		} else if(xpos <= Screen.width && xpos > Screen.width - ScrollWidth) {
			movement.x += ScrollSpeed;
			mouseScroll = true;
		}
		
		//vertical camera movement
		if(ypos >= 0 && ypos < ScrollWidth) {
			movement.y -= ScrollSpeed;
			mouseScroll = true;
		} else if(ypos <= Screen.height && ypos > Screen.height - ScrollWidth) {
			movement.y += ScrollSpeed;
			mouseScroll = true;
		}
		
		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.mainCamera.transform.TransformDirection(movement);
		movement.y = 0;
		
		//away from ground movement
		movement.y -= ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");
		
		//calculate desired camera position based on received input
		Vector3 origin = Camera.mainCamera.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;
		
		//limit away from ground movement to be between a minimum and maximum distance
		if(destination.y > MaxCameraHeight) {
			destination.y = MaxCameraHeight;
		} else if(destination.y < MinCameraHeight) {
			destination.y = MinCameraHeight;
		}
		
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.mainCamera.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ScrollSpeed);
		}

	}
}
