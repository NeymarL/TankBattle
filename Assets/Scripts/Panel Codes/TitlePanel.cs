using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitlePanel : PanelBase {

	private Button startBtn;
	private Button infoBtn;

	public override void Init(params object[] args)
	{
		base.Init (args);
		skinPath = "TitlePanel";
		layer = PanelLayer.Panel;
	}

	public override void OnShowing()
	{
		base.OnShowing ();
		Transform skinTrans = skin.transform;
		startBtn = skinTrans.Find ("Start Button").GetComponent<Button> ();
		infoBtn = skinTrans.Find ("Info Button").GetComponent<Button> ();

		startBtn.onClick.AddListener (OnStartClick);
		infoBtn.onClick.AddListener (OnInfoClick);
	}

	public void OnStartClick()
	{
		PanelMgr.instance.OpenPanel<OptionPanel> ("");
	}

	public void OnInfoClick()
	{
		PanelMgr.instance.OpenPanel<InfoPanel> ("");
	}
}
