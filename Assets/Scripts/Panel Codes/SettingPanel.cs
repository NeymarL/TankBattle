using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : PanelBase {

	private Button closeBtn;
	private Button endBtn;

	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "SettingPanel";
		layer = PanelLayer.Panel;
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		// 关闭按钮
		closeBtn = skinTrans.Find("Close Button").GetComponent<Button>();
		endBtn = skinTrans.Find("End Button").GetComponent<Button>();

		closeBtn.onClick.AddListener (OnCloseClick);
		endBtn.onClick.AddListener (OnEndClick);
	}

	public void OnEndClick()
	{
		Battle.instance.battleStart = false;
		PanelMgr.instance.OpenPanel<ResultPanel> ("", false);
		Close ();
	}

	public void OnCloseClick()
	{
		Time.timeScale = 1;
		Close ();
	}
}
