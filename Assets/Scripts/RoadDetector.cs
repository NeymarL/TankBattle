using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadDetector : MonoBehaviour {

	public bool isOnRamp = false;

	// Use this for initialization
	void Start () {
		isOnRamp = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision collisionInfo) {
		Debug.LogWarning (collisionInfo.gameObject.name);
		if (collisionInfo.gameObject.tag == "Ramp") {
			isOnRamp = true;
		} else {
			isOnRamp = false;
		}
	}


}
