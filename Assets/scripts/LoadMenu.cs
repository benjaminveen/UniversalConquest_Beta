using UnityEngine;
using System.Collections;

public class LoadMenu : MonoBehaviour {

	public float boxHeightPercent;
	public float boxWidthPercent;
	public float numButtons;
	public float buttonWidthPercent;

	private MainMenu mainMenu;
	private bool visible = false;

	void Start () 
	{
		mainMenu = (MainMenu)gameObject.GetComponent ("MainMenu");
	}
	
	// Update is called once per frame

	void OnGUI()
	{
		if (visible) 
		{
			float boxWidth = Screen.width * boxWidthPercent;
			float boxHeight = Screen.height * boxHeightPercent;
			
			float boxX = Screen.width / 2 - boxWidth / 2;
			float boxY = Screen.height / 2 - boxHeight / 2;
			
			float buttonHeight = boxHeight / numButtons;
			float buttonWidth = boxWidth * buttonWidthPercent;
			float buttonX = boxX + boxWidth / 2 - buttonWidth / 2;

			GUI.Box (new Rect (boxX, boxY, boxWidth, boxHeight), "", mainMenu.mainBox);

			if(GUI.Button(new Rect(buttonX, boxY, buttonWidth, buttonHeight), "Save 1")) 
			{
				audio.Play ();
			}
			if (GUI.Button (new Rect (buttonX, boxY + buttonHeight * 1, buttonWidth, buttonHeight), "Save 2")) 
			{
				audio.Play ();
			}
			
			if (GUI.Button (new Rect (buttonX, boxY + buttonHeight * 2, buttonWidth, buttonHeight), "Save 3")) 
			{
				audio.Play ();
			}
			
			if (GUI.Button (new Rect (buttonX, boxY + buttonHeight * 3, buttonWidth, buttonHeight), "Back to Main Menu")) 
			{
				visible = false;
				mainMenu.setVisible (true);
				audio.Play ();
			}
		}
	}
	public void setVisible(bool v)
	{visible = v;}
}
