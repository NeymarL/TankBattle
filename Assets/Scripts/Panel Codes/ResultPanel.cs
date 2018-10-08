using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : PanelBase {

	private Image winImage;
	private Image failImage;
	private Text text;
	private Button closeBtn;
	private bool isWin;
	private string msg;

	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "ResultPanel";
		layer = PanelLayer.Panel;
		// 参数 args[0] 代表己方阵营的输赢
		if (args.Length >= 1) {
			isWin = (bool)args [0];
			if (args.Length == 2) {
				msg = (string)args [1];
			}
		}
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Battle.instance.audio.Stop ();
		Transform skinTrans = skin.transform;
		// 关闭按钮
		closeBtn = skinTrans.Find("Close Button").GetComponent<Button>();
		closeBtn.onClick.AddListener (OnCloseClick);
		// 图片和文字
		winImage = skinTrans.Find("Win Image").GetComponent<Image>();
		failImage = skinTrans.Find ("Fail Image").GetComponent<Image> ();
		text = skinTrans.Find ("ResText").GetComponent<Text> ();

		if (isWin) {
			failImage.enabled = false;
			failImage.GetComponent<AudioSource> ().enabled = false;
			winImage.enabled = true;
			winImage.GetComponent<AudioSource> ().enabled = true;
			if (msg == null) {
				text.text = "党和人民感谢你！";
			} else {
				text.text = msg;
			}
		} else {
			failImage.enabled = true;
			failImage.GetComponent<AudioSource> ().enabled = true;
			winImage.enabled = false;
			winImage.GetComponent<AudioSource> ().enabled = false;
			if (msg == null) {
				text.text = "祖国和人民对你很失望！";
			} else {
				text.text = msg;
			}
		}
	}

	public void OnCloseClick()
	{
		PanelMgr.instance.OpenPanel<RecordPanel> ("", isWin);
		Close ();
	}
}
