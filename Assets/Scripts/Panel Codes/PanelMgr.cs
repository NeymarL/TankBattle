using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PanelLayer
{
	Panel,		// 面板
	Tips,		// 提示
}

public class PanelMgr : MonoBehaviour {

	public static PanelMgr instance; 			// 单例
	private GameObject canvas;					// 画板
	public Dictionary<string, PanelBase> dict;	// 已打开面板
	// 层级
	private Dictionary<PanelLayer, Transform> layerDict;

	public void Awake()
	{
		instance = this;
		InitLayer ();
		dict = new Dictionary<string, PanelBase>();
	}

	private void InitLayer()
	{
		canvas = GameObject.Find ("Canvas");	// 指向场景中的画布
		if (canvas == null) {
			Debug.LogError ("PanelMgr.InitLayer: Canvas is NULL!!!");
		}
		layerDict = new Dictionary<PanelLayer, Transform> ();
		// 遍历层级，找到层级的父物体
		foreach (PanelLayer pl in Enum.GetValues(typeof(PanelLayer))) {
			string name = pl.ToString ();
			Transform transform = canvas.transform.Find (name);
			layerDict.Add (pl, transform);
		}

	}

	public void OpenPanel<T>(string skinPath, params object[] args) where T : PanelBase
	{
		// already open
		string name = typeof(T).ToString();
		if (dict.ContainsKey (name)) {
			return;
		}
		// 画板脚本
		PanelBase panel = canvas.AddComponent<T>();
		panel.Init (args);
		dict.Add (name, panel);
		// 加载皮肤
		skinPath = (skinPath != "" ? skinPath : panel.skinPath);
		GameObject skin = Resources.Load<GameObject> (skinPath);
		if (skin == null) {
			Debug.LogError ("Panel.OpenPanel: Skin is NULL!!!");
		}
		panel.skin = (GameObject)Instantiate (skin);
		// 坐标
		Transform skinTrans = panel.skin.transform;
		PanelLayer pl = panel.layer;
		Transform parent = layerDict [pl];
		skinTrans.SetParent (parent, false);
		// panel 的生命周期
		panel.OnShowing();
		panel.OnShowed ();
	}

	public void ClosePanel(string name)
	{
		PanelBase panel = (PanelBase)dict [name];
		if (panel == null) {
			return;
		}
		panel.OnClosing ();
		dict.Remove (name);
		panel.OnClosed ();
		GameObject.Destroy (panel.skin);
		Component.Destroy (panel);
	}
}
