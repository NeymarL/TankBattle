using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDestruct : MonoBehaviour {

	public float destroyTime = 1f;

	private float insTime;

	// Use this for initialization
	void Start () {
		insTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - insTime >= destroyTime) {
			Destroy (gameObject);
		}
		if (!Battle.instance.battleStart) {
			Destroy (gameObject);
		}
	}
}
