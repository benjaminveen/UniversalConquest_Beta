using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	
	public GUIStyle mainBox;
	public GUIStyle button;
	
	public float boxHeightPercent;
	public float boxWidthPercent;
	public float numButtons;
	public float buttonWidthPercent;

	private bool visible = true;

	private LoadMenu loadMenu;
	void Start()
	{
		loadMenu = (LoadMenu)gameObject.GetComponent ("LoadMenu");
	}

	void OnGUI () 
	{
		// Make a button. We pass in the GUIStyle defined above as the style to use
		float boxWidth = Screen.width * boxWidthPercent;
		float boxHeight = Screen.height * boxHeightPercent;

		float boxX = Screen.width / 2 - boxWidth / 2;
		float boxY = Screen.height / 2 - boxHeight / 2;

		float buttonHeight = boxHeight / numButtons;
		float buttonWidth = boxWidth * buttonWidthPercent;
		float buttonX = boxX + boxWidth / 2 - buttonWidth / 2;

		if (visible) 
		{
			GUI.Box (new Rect (boxX, boxY, boxWidth, boxHeight), "", mainBox);

			if(GUI.Button(new Rect(buttonX, boxY, buttonWidth, buttonHeight), "New Game")) 
			{
				Application.LoadLevel(1);
				audio.Play ();
			}
			if (GUI.Button (new Rect (buttonX, boxY + buttonHeight * 1, buttonWidth, buttonHeight), "Load Game")) 
			{
				audio.Play ();
				visible = false;
				loadMenu.setVisible (true);
			}
			
			if (GUI.Button (new Rect (buttonX, boxY + buttonHeight * 2, buttonWidth, buttonHeight), "Custom Scenario")) 
			{
				audio.Play ();
			}
			
			if (GUI.Button (new Rect (buttonX, boxY + buttonHeight * 3, buttonWidth, buttonHeight), "Quit")) 
			{
				Application.Quit();
				audio.Play ();
			}
		}
	}
	public void setVisible(bool v)
	{visible = v;}
}
