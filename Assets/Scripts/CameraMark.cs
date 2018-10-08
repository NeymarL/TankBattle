using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMark : MonoBehaviour {

	public Texture2D minePosTexture;
	public Texture2D othersPosTexture;

	public bool enable = false;

	private Camera mapCamera;
	private int camp = 0;				// 玩家所在阵营

	// Use this for initialization
	void Start () {
		mapCamera = GameObject.Find ("Map Camera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
		if (camp == 0 && enable) {
			for (int i = 0; i < Battle.instance.battleTanks.Length; i++) {
				if (Battle.instance.battleTanks[i].isPlayer) {
					camp = Battle.instance.battleTanks [i].camp;
					break;
				}
			}
		}
	}

	void OnGUI ()
	{
		if (enable) {
			for (int i = 0; i < Battle.instance.battleTanks.Length; i++) {
				BattleTank bt = Battle.instance.battleTanks [i];
				if (bt.camp == camp && !bt.dead) {
					if (bt.isPlayer) {
						DrawPosition (bt.tank.transform.position, minePosTexture);
					} else {
						DrawPosition (bt.tank.transform.position, othersPosTexture);
					}
				}
			}
		}
	}

	public void DrawPosition(Vector3 realWorldPos, Texture2D texture)
	{
		Vector3 screenPoint = mapCamera.WorldToScreenPoint(realWorldPos);
		// 绘制
		Rect rect = new Rect(screenPoint.x - texture.width / 2,
			Screen.height - screenPoint.y - texture.height / 2,
			texture.width, texture.height); 
		GUI.DrawTexture (rect, texture);
	}
}
