using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Battle : MonoBehaviour {

	public static Battle instance;		// 单例
	public BattleTank[] battleTanks;	// 战场中所有的坦克
	public GameObject[] tankPrefabs;

	public int[] n = new int[2];		// 每个阵营的坦克数

	public string[] names;

	public GameObject tips;

	public bool battleStart = false;
	public AudioClip[] bgMusics;
	public AudioSource audio;

	public float countdown = 120;
	public GameObject countdownOb;
	private Image countdownImg;
	private Text countdownText;
	private float currentTime;

	// Use this for initialization
	void Start () {
		instance = this;
		audio = gameObject.GetComponent<AudioSource> ();
		countdownImg = countdownOb.transform.Find ("Image").GetComponent<Image> ();
		countdownText = countdownOb.transform.Find ("Text").GetComponent<Text> ();
		currentTime = countdown;
	}

	void Update() {
		if (battleStart) {
			if (Input.GetButtonDown ("Cancel")) {
				Time.timeScale = 0;
				PanelMgr.instance.OpenPanel<SettingPanel> ("");
			}
			if (Input.GetKeyDown (KeyCode.Tab)) {
				PanelMgr.instance.OpenPanel<TabPanel> ("");
			}
			if (Input.GetKeyUp (KeyCode.Tab)) {
				PanelMgr.instance.ClosePanel ("TabPanel");
			}
			currentTime -= Time.deltaTime;
			countdownText.text = currentTime.ToString ("0");
			if (currentTime <= 0) {
				// game over
				int[] campAlive = new int[2];
				campAlive [0] = campAlive [1] = 0;
				for (int i = 0; i < battleTanks.Length; i++) {
					if (!battleTanks [i].dead) {
						campAlive [battleTanks [i].camp - 1]++;
					}
				}
				if (campAlive [0] > campAlive [1]) {
					GameOver (1, "时间到！剩余坦克数量较多的一方获胜！");
				} else if (campAlive [0] < campAlive [1]){
					GameOver (2, "时间到！剩余坦克数量较多的一方获胜！");
				}
			}
		}
		countdownImg.enabled = battleStart;
		countdownText.enabled = battleStart;
	}

	public int GetCamp(GameObject tankObj)
	{
		for (int i = 0; i < battleTanks.Length; i++) {
			BattleTank bt = battleTanks [i];
			if (bt == null) {
				return 0;
			}
			if (bt.tank != null && bt.tank.gameObject == tankObj) {
				return bt.camp;
			}
		}
		return 0;
	}

	public bool IsSameCamp(GameObject tank1, GameObject tank2)
	{
		return GetCamp (tank1) == GetCamp (tank2);
	}

	public bool IsWin(int camp)
	{
		for (int i = 0; i < battleTanks.Length; i++) {
			if (battleTanks [i].camp != camp) {
				if (!battleTanks[i].dead) {
					return false;
				}
			}
		}
		GameOver (camp);
		return true;
	}

	public void GameOver(int camp, string msg = null)
	{
		Debug.Log ("阵营" + camp + "获胜！");
		for (int i = 0; i < battleTanks.Length; i++) {
			if (battleTanks [i].isPlayer) {
				// 停止游戏
				battleStart = false;
				Time.timeScale = 0;
				// 弹出鼠标
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				// 弹出结束面板
				PanelMgr.instance.OpenPanel<ResultPanel> ("", (battleTanks[i].camp == camp), msg);
				break;
			}
		}
	}

	public bool IsWin(GameObject tank)
	{
		int camp = GetCamp (tank);
		return IsWin (camp);
	}

	public void Clear()
	{
		Debug.Log ("Clear Battle!!");
		GameObject[] tanks = GameObject.FindGameObjectsWithTag ("Tank");
		for (int i = 0; i < tanks.Length; i++) {
			Destroy (tanks[i]);
		}
		WatchBattle wb = gameObject.GetComponent<WatchBattle> ();
		wb.SetWatchMode (false);
		battleStart = false;
	}

	public void StartTwoCampBattle(int n1, int n2)
	{
		n [0] = n1;
		n [1] = n2;
		// 获取出生点
		Transform sp = GameObject.Find("SwopPoints").transform;
		Transform spCamp1 = sp.GetChild (0);
		Transform spCamp2 = sp.GetChild (1);
		// 清理战场
		Clear();
		battleTanks = new BattleTank[n1 + n2];
		for (int i = 0; i < n1; i++) {
			GenerateTank (1, i, spCamp1, i);
		}
		for (int i = 0; i < n2; i++) {
			GenerateTank (2, i, spCamp2, n1+i);
		}
		int index = Random.Range (0, n1 + n2);
		TankBeheavior tank = battleTanks [index].tank;
		battleTanks [index].isPlayer = true;
		tank.ctrlType = TankBeheavior.CtrlType.Player;
		tank.maxSteering = 50;
		CameraFollow cf = Camera.main.gameObject.GetComponent<CameraFollow> ();
		GameObject cameraTarget = tank.transform.GetChild (1).GetChild (1).gameObject;
		cf.SetTarget (cameraTarget);
		// 播放背景音乐
		AudioClip bgm = bgMusics[Random.Range(0, bgMusics.Length)];
		audio.clip = bgm;
		audio.loop = true;
		audio.Play ();
		// 锁定鼠标
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		// 开始游戏
		Time.timeScale = 1;
		battleStart = true;
		currentTime = countdown;
	}

	/**
	 * camp  : 坦克所在阵营
	 * num   : 阵营中的编号
	 * sp    : 出生点容器
	 * index : 战场中的编号
	**/
	public void GenerateTank(int camp, int num, Transform sp, int index)
	{
		Transform tr = sp.GetChild (num);
		GameObject tankObj = (GameObject)Instantiate (tankPrefabs[camp - 1], 
			tr.position, tr.rotation);
		TankBeheavior tank = tankObj.GetComponent<TankBeheavior> ();
		tank.ctrlType = TankBeheavior.CtrlType.NPC;
		battleTanks [index] = new BattleTank ();
		battleTanks [index].tank = tank;
		battleTanks [index].camp = camp;
		if (index < names.Length) {
			tankObj.name = names [index] + "-" + camp;
			battleTanks [index].name = names [index];
			TextMesh tm = tank.transform.GetChild (5).GetChild (0).GetComponent<TextMesh> ();
			tm.text = names [index];
		}
	}

	public void SetDead(TankBeheavior tank, bool isPlayer = false)
	{
		for (int i = 0; i < battleTanks.Length; i++) {
			if (tank == battleTanks [i].tank) {
				battleTanks [i].dead = true;
				if (isPlayer) {
					WatchBattle wb = gameObject.GetComponent<WatchBattle> ();
					wb.SetWatchMode (isPlayer, battleTanks [i].camp);
				}
				Debug.Log (tank.name + " Dead!");
			}
		}
	}

	public void IncraeseKillNum(TankBeheavior tank)
	{
		for (int i = 0; i < battleTanks.Length; i++) {
			if (tank == battleTanks [i].tank) {
				battleTanks [i].numKill += 1;
			}
		}
	}

	public void DrawKillTip(string name1, int camp1, string name2)
	{
		GameObject killTip = Instantiate<GameObject> (tips);
		Transform killTrans = killTip.transform;
		Text a = killTrans.Find ("A").GetComponent<Text> ();
		Text b = killTrans.Find ("B").GetComponent<Text> ();
		a.text = name1;
		b.text = name2;
		if (camp1 == 1) {
			a.color = new Color (0x28, 0xFF, 0x00);
			b.color = new Color (0xFF, 0x00, 0x64);
		} else {
			b.color = new Color (0x28, 0xFF, 0x00);
			a.color = new Color (0xFF, 0x00, 0x64);
		}
		GameObject canvas = GameObject.Find ("Canvas");
		killTrans.SetParent (canvas.transform, false);
	}
		
}
