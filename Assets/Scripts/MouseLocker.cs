using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLocker : MonoBehaviour {

	// Use this for initialization
	void Start () {
		LockCursor (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeScale != 0) {
			// if ESCAPE key is pressed, then unlock the cursor
			if (Input.GetButtonDown ("Cancel")) {
				LockCursor (false);
			}

			// if the player fires, then relock the cursor
			if (Input.GetButtonDown ("Fire1")) {
				LockCursor (true);
			}
		} else {
			LockCursor (false);
		}
	}

	private void LockCursor(bool isLocked)
	{
		if (isLocked) 
		{
			// make the mouse pointer invisible
			Cursor.visible = false;

			// lock the mouse pointer within the game area
			Cursor.lockState = CursorLockMode.Locked;
		} else {
			// make the mouse pointer visible
			Cursor.visible = true;

			// unlock the mouse pointer so player can click on other windows
			Cursor.lockState = CursorLockMode.None;
		}
	}
}
