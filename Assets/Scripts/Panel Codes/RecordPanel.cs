using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordPanel : PanelBase {

	public int maxCampNum = 5;

	private int camp1Index = 0;
	private int camp2Index = 0;
	private int[] scores;
	private int maxKill = 0;

	private bool isWin;
	private Transform list;
	private Text winText, failText, text;
	private Button closeBtn;
	private Transform mvpImage;

	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "RecordPanel";
		layer = PanelLayer.Panel;
		scores = new int[2];
		scores [0] = scores [1] = 0;
		// 参数 args[0] 代表己方阵营的输赢
		if (args.Length >= 1) {
			isWin = (bool)args [0];
		}
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		list = skinTrans.Find ("List");
		// 显示MVP小标标
		mvpImage = list.Find("MVPImage");
		// 显示战绩
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
		// 显示胜利/失败
		winText = skinTrans.Find ("Win").GetComponent<Text> ();
		failText = skinTrans.Find ("Fail").GetComponent<Text> ();
		if (isWin) {
			winText.enabled = true;
			failText.enabled = false;
		} else {
			winText.enabled = false;
			failText.enabled = true;
		}
		// 显示总比分
		for (int i = 0; i < Battle.instance.battleTanks.Length; i++) {
			if (Battle.instance.battleTanks[i].dead) {
				scores [Battle.instance.battleTanks [i].camp - 1]++;
			}
		}
		text = skinTrans.Find ("Num1").GetComponent<Text> ();
		text.text = scores [1].ToString ();
		text = skinTrans.Find ("Num2").GetComponent<Text> ();
		text.text = scores [0].ToString ();
		// 关闭按钮
		closeBtn = skinTrans.Find("Close Button").GetComponent<Button>();
		closeBtn.onClick.AddListener (OnCloseClick);
	}

	private void SetText(BattleTank bt, int camp = 0, int index = 0)
	{
		bool mvp = false;
		string obName = "Camp";
		string name = "", killNum = "";
		if (bt != null) {
			obName += bt.camp;
			if (bt.camp == 1) {
				camp1Index++;
				obName += camp1Index;
			} else {
				camp2Index++;
				obName += camp2Index;
			}
			name = bt.name;
			killNum += bt.numKill;
			if (bt.numKill > maxKill) {
				maxKill = bt.numKill;
				mvp = true;
			}
		} else {
			obName = obName + camp + index;
		}
		// 显示名字
		text = list.Find (obName).GetComponent<Text>();
		text.text = name;
		if (bt != null) {
			// 粗斜体表示玩家
			if (bt.isPlayer) {
				text.fontStyle = FontStyle.BoldAndItalic;
			}
		}
		// 显示击杀数
		text = list.Find (obName + "-Kill").GetComponent<Text>();
		text.text = killNum;
		// 显示MVP标识
		if (mvp) {
			Vector3 mvpPos;

			if (bt.camp == 1) {
				Vector3 targetPos = list.Find (obName).position;
				mvpPos = new Vector3 (targetPos.x - 100, targetPos.y + 15, targetPos.z);
			} else {
				Vector3 targetPos = list.Find (obName + "-Kill").position;
				mvpPos = new Vector3 (targetPos.x + 60, targetPos.y + 15, targetPos.z);
			}
			mvpImage.position = mvpPos;
		}
	}

	public void OnCloseClick()
	{
		Time.timeScale = 1;
		Battle.instance.Clear ();
		PanelMgr.instance.OpenPanel<TitlePanel> ("");
		Close ();
	}
}
