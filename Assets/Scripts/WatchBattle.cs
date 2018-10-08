using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchBattle : MonoBehaviour {

	private bool watchMode = false;
	private int observedCamp = 0;
	private int index = 0;

	
	// Update is called once per frame
	void Update () {
		if (!watchMode || observedCamp == 0) {
			return;
		}

		if (Input.GetMouseButton (0)) {
			Debug.Log ("Change Target: " + index);
			ChangeObserveTarget ();
		}
	}

	public void SetWatchMode(bool wm, int camp = 0)
	{
		watchMode = wm;
		observedCamp = camp;
		if (wm) {
			Debug.Log ("Enter Observersion Mode");
			index = 0;
			ChangeObserveTarget ();
		}
	}

	public void ChangeObserveTarget()
	{
		for (int i = 0; i < Battle.instance.battleTanks.Length; i++) {
			BattleTank bt = Battle.instance.battleTanks [i];
			if (bt.camp == observedCamp && !bt.dead && i >= index) {
				bool updated = false;
				for (int j = i + 1; j < Battle.instance.battleTanks.Length; j++) {
					BattleTank bt2 = Battle.instance.battleTanks [j];
					if (bt2.camp == observedCamp && !bt2.dead) {
						index = j;
						updated = true;
						break;
					}
				}
				if (!updated) {
					index = 0;
				}
				Debug.Log ("Update index: " + index);
				CameraFollow cf = Camera.main.gameObject.GetComponent<CameraFollow> ();
				GameObject cameraTarget = bt.tank.transform.GetChild (1).GetChild (1).gameObject;
				cf.SetTarget (cameraTarget);
				return;
			}
		}
	}
}
