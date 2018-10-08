using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabPanel : PanelBase {

	public int maxCampNum = 5;

	private int camp1Index = 0;
	private int camp2Index = 0;
	private Camera mapCamera;
	private Text text;
	private Transform list;
	private CameraMark cm;

	private Color camp1Alive = new Color(126, 255, 146);
	private Color camp1Dead = new Color(255, 127, 95);
	private Color camp2Alive = new Color(232, 210, 103);
	private Color camp2Dead = new Color(103, 160, 232);
	private Color player = new Color(246, 113, 255);

	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "TabPanel";
		layer = PanelLayer.Panel;
		mapCamera = GameObject.Find ("Map Camera").GetComponent<Camera>();
		cm = GameObject.Find ("Map Camera").GetComponent<CameraMark> ();
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		list = skinTrans.Find ("List");
		for (int i = 0; i < Battle.instance.battleTanks.Length; i++) {
			SetText (Battle.instance.battleTanks[i]);
		}
		// 把剩下的置为空
		for (int i = camp1Index + 1; i <= maxCampNum; i++) {
			SetText (null, 1, i);
		}
		for (int i = camp2Index + 1; i <= maxCampNum; i++) {
			SetText (null, 2, i);
		}
		// 显示小地图
		mapCamera.enabled = true;
		cm.enable = true;
	}

	private void SetText(BattleTank bt, int camp = 0, int index = 0)
	{
		string obName = "Camp";
		string name = "", killNum = "";
		Color color;
		if (bt != null) {
			obName += bt.camp;
			if (bt.camp == 1) {
				camp1Index++;
				obName += camp1Index;
				color = (bt.dead) ? camp1Dead : camp1Alive;
			} else {
				camp2Index++;
				obName += camp2Index;
				color = (bt.dead) ? camp2Dead : camp2Alive;
			}
			name = bt.name;
			killNum += bt.numKill;
			if (bt.isPlayer) {
				color = player;
			}
		} else {
			obName = obName + camp + index;
			color = new Color (0, 0, 0);
		}
		// 显示名字
		text = list.Find (obName).GetComponent<Text>();
		text.text = name;
		// 由于颜色改不了而做的妥协
		if (bt != null) {
			// 死了显示白色
			if (bt.dead) {
				text.color = color;
			}
			// 粗斜体表示玩家
			if (bt.isPlayer) {
				text.fontStyle = FontStyle.BoldAndItalic;
			}
		}
		// 显示击杀数
		text = list.Find (obName + "-Kill").GetComponent<Text>();
		text.text = killNum;
	}

	public override void OnClosing()
	{
		base.OnClosing ();
		mapCamera.enabled = false;
		cm.enable = false;
	}

}
