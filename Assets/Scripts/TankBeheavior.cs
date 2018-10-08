using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBeheavior : MonoBehaviour {

	public float turretRotSpeed = 0.5f; // 炮塔旋转速度
	public Transform turret; 			// 炮塔
	public Transform gun;				// 炮管

	public List<AxleInfo> axleInfos;	// 轮轴
	private float motor = 0;			// 马力
	public float maxMotorTorque;		// 最大马力
	private float brakeTorque = 0;		// 制动
	public float maxBrakeTorque; 		// 最大制动
	private float steering = 0;			// 转向角
	public float maxSteering;			// 最大转向角

	public AudioSource motorAudioSource;// 马达音源
	public AudioClip motorClip;			// 马达音效
	public AudioSource shootAudioSource;// 发射炮弹音源
	public AudioClip shootClip;			// 发射音效

	public GameObject bullet;			// 炮弹
	public float shootInterval = 1f;	// 开跑间隔
	private float lastShootTime;		// 上次开炮时间

	public enum CtrlType				// 操控类型
	{
		None,
		Player,
		NPC
	}
	public CtrlType ctrlType = CtrlType.Player;

	public float maxHP = 100f;			// 最大生命值
	private float hp;					// 当前生命值

	public GameObject smokeEffect;		// 烟雾特效
	public GameObject explodeEffect; 	// 爆炸特效

	public Texture2D centerSight;		// 中心准心
	public Texture2D tankSight;			// 坦克准心
	public Texture2D hpBarBg;			// 生命条指示素材
	public Texture2D hpBar;
	public Texture2D killUI;			// 击杀提示图标
	private float killUIStartTime = float.MinValue;

	private float turretRotTarget = 0; 	// 炮塔目标角度
	private float gunRollTarget = 0;
	private float maxRoll = 10f;
	private float minRoll = -4f;
	private Transform wheels;
	private Transform tracks;
	private Vector3 lastHitPoint;

	private AI ai;

	// Use this for initialization
	void Start () {
		if (turret == null) {
			Debug.Log ("No Turret Found!!!");
		}
		// 获取轮子
		wheels = transform.Find("Wheels");
		if (wheels == null) {
			Debug.Log ("No Wheels Found!!!");
		}
		// 获取履带
		tracks = transform.Find("Tracks");
		if (tracks == null) {
			Debug.Log ("No Tracks Found!!!");
		}
		motorAudioSource = gameObject.AddComponent<AudioSource> ();
		motorAudioSource.spatialBlend = 1f;
		shootAudioSource = gameObject.AddComponent<AudioSource> ();
		shootAudioSource.spatialBlend = 0.9f;
		hp = maxHP;
		lastHitPoint = Vector3.zero;
		if (ctrlType == CtrlType.NPC) {
			ai = gameObject.AddComponent<AI> ();
			ai.tank = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeScale == 0) {
			return;
		}
		PlayerCtrl ();
		NPCControl ();
		foreach (AxleInfo axleInfo in axleInfos) {
			// 转向
			if (axleInfo.steering) {
				axleInfo.leftWheel.steerAngle = steering;
				axleInfo.rightWheel.steerAngle = steering;
			}
			// 马力
			if (axleInfo.motor) {
				axleInfo.leftWheel.motorTorque = motor;
				axleInfo.rightWheel.motorTorque = motor;
			}
			// 制动
			axleInfo.leftWheel.brakeTorque = brakeTorque;
			axleInfo.rightWheel.brakeTorque = brakeTorque;
			// 转到轮子
			if (axleInfos [1] != null) {
				WheelsRotation (axleInfos [1].leftWheel);
				TrackMove ();
			}
		}
		// 炮塔炮管旋转
		TurretRotation ();
		GunRoll ();
		// 马达声音
		MotorSound();
	}

	// 绘图
	void OnGUI()
	{
		if (ctrlType == CtrlType.Player && Battle.instance.battleStart) {
			DrawSight ();
			DrawHp ();
			DrawKillUI ();
		}
	}

	public void TurretRotation()
	{
		if (Camera.main == null) {
			return;
		}
		if (turret == null) {
			return;
		}
		float angle = turret.eulerAngles.y - turretRotTarget;
		if (angle < 0) {
			angle += 360;
		}
		if (angle > turretRotSpeed && angle < 180) {
			turret.Rotate (0f, -turretRotSpeed, 0f);
		} else if (angle > 180 && angle < 360 - turretRotSpeed) {
			turret.Rotate (0f, turretRotSpeed, 0f);
		}
	}

	public void GunRoll()
	{
		//获取角度
		Vector3 worldEuler = gun.eulerAngles;
		Vector3 localEuler = gun.localEulerAngles;
		//世界坐标系角度计算
		worldEuler.x = gunRollTarget;
		if (ctrlType == CtrlType.Player) {
			worldEuler.x -= 10;
		}
		gun.eulerAngles = worldEuler;
		//本地坐标系角度限制
		Vector3 euler = gun.localEulerAngles;
		if (ctrlType == CtrlType.NPC) {
			// Debug.Log ("Target = " + gunRollTarget + " World = " + worldEuler + " Local = " + euler);
		}
		if (euler.x > maxRoll && euler.x < 180)
			euler.x = maxRoll;
		if (euler.x < minRoll + 360 && euler.x > 180)
			euler.x = minRoll;
		gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);
	}

	public void PlayerCtrl()
	{
		// 只有玩家控制的坦克才会生效
		if (ctrlType != CtrlType.Player) {
			return;
		}
		// 马力和转向角
		motor = maxMotorTorque * Input.GetAxis ("Vertical");
		steering = maxSteering * Input.GetAxis ("Horizontal");
		// 制动
		brakeTorque = 0;
		foreach (AxleInfo axleInfo in axleInfos) {
			if (axleInfo.leftWheel.rpm > 5 && motor < 0) {
				brakeTorque = maxBrakeTorque;
			}
			if (axleInfo.leftWheel.rpm < -5 && motor > 0) {
				brakeTorque = maxBrakeTorque;
			}
		}
		// 炮塔角度
		TargetSignPos ();
		// 发射炮弹
		if (Input.GetMouseButton (0)) {
			Shoot ();
		}
		// 翻身
		Vector3 euler = transform.eulerAngles;
		if (euler.z >= 60 && euler.z <= 300 && Input.GetKeyDown(KeyCode.R)) {
			TurnOver ();
		}
	}

	public void NPCControl()
	{
		if (ctrlType != CtrlType.NPC) {
			return;
		}
		// 炮塔目标角度
		Vector3 rot = ai.GetTurretTarget();
		turretRotTarget = rot.y;
		gunRollTarget = rot.x;
		// 发射炮弹
		if (ai.IsShoot ()) {
			Shoot ();
		}
		motor = ai.GetMotor ();
		steering = ai.GetSteering ();
		brakeTorque = ai.GetBrakeTorque ();
		// 翻身
		Vector3 euler = transform.eulerAngles;
		if (euler.z >= 90 && euler.z <= 270) {
			TurnOver ();
		}
		// Debug.Log ("Motor = " + motor + " Steering = " + steering + " Brake = " + brakeTorque);
	}

	public void WheelsRotation(WheelCollider collider)
	{
		if (wheels == null) {
			return;
		}
		// 获取旋转信息
		Vector3 position;
		Quaternion rotation;
		collider.GetWorldPose (out position, out rotation);
		// 旋转每个轮子
		foreach (Transform wheel in wheels) {
			wheel.rotation = rotation;
		}
	}

	public void TrackMove()
	{
		if (tracks == null) {
			return;
		}
		float offset = 0;
		// 根据轮子的角度确定偏移量
		if (wheels.GetChild (0) != null) {
			offset = wheels.GetChild (0).localEulerAngles.x / 180f;
		}
		foreach (Transform track in tracks) {
			// 获取材质
			MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer> ();
			if (mr == null) {
				continue;
			}
			// 设置主贴图偏移
			Material mtl = mr.material;
			mtl.mainTextureOffset = new Vector2 (0, offset);
		}
	}

	public void MotorSound()
	{
		if (motor != 0 && !motorAudioSource.isPlaying) {
			motorAudioSource.loop = true;
			motorAudioSource.clip = motorClip;
			motorAudioSource.Play ();
		} else if (motor == 0) {
			motorAudioSource.Pause ();
		}
	}

	public void Shoot()
	{
		// 发射间隔
		if (Time.time - lastShootTime < shootInterval) {
			return;
		}
		// 子弹
		if (bullet == null) {
			return;
		}
		// 发射
		Vector3 pos = gun.position + gun.forward * 4;
		Quaternion rot = gun.rotation;
		GameObject bul = Instantiate (bullet, pos, rot);
		// 设置谁发射了炮弹
		Bullet bulletCmp = bul.GetComponent<Bullet> ();
		if (bulletCmp != null) {
			bulletCmp.setAttackTank (this.gameObject);
		}
		shootAudioSource.PlayOneShot (shootClip);
		lastShootTime = Time.time;
	}

	public void BeAttacked(float damageAmount, GameObject attackTank)
	{
		if (Battle.instance.IsSameCamp (gameObject, attackTank)) {
			return;
		}
		if (hp > 0) {
			hp -= damageAmount;
		}
		if (hp <= 0) {
			Instantiate (explodeEffect, transform.position, transform.rotation);
			Battle.instance.SetDead (this.gameObject.GetComponent<TankBeheavior>(), ctrlType == CtrlType.Player);
			Destroy (gameObject);
			if (attackTank != null) {
				// 显示击杀提示
				TankBeheavior tankCmp = attackTank.GetComponent<TankBeheavior> ();
				if (tankCmp != null && tankCmp.ctrlType == CtrlType.Player) {
					tankCmp.StartDrawKill ();
				}
				Battle.instance.DrawKillTip (attackTank.name, Battle.instance.GetCamp(attackTank), gameObject.name);
				Battle.instance.IncraeseKillNum (tankCmp);
				// 战场结算
				Battle.instance.IsWin(attackTank);
			}
		} else if (hp <= maxHP / 2) {
			GameObject smoke = Instantiate (smokeEffect, transform.position, transform.rotation);
			smoke.transform.parent = gameObject.transform;
		}
		if (ai != null) {
			ai.OnAttacked (attackTank);
		}
	}

	// 计算目标角度
	public void TargetSignPos()
	{
		// 碰撞信息和碰撞点
		Vector3 hitPoint = Vector3.zero;
		RaycastHit raycastHit;
		// 屏幕中心位置
		Vector3 centerVec = new Vector3(Screen.width / 2, Screen.height / 2, 0);
		Ray ray = Camera.main.ScreenPointToRay (centerVec);
		// 获取 hitPoint
		if (Physics.Raycast (ray, out raycastHit, 400.0f)) {
			hitPoint = raycastHit.point;
		} else {
			hitPoint = ray.GetPoint (400f);
		}
		// 计算目标角度
		Vector3 dir = hitPoint - turret.position;
		Quaternion angle = Quaternion.LookRotation (dir);
		turretRotTarget = angle.eulerAngles.y;
		if (Mathf.Abs (gunRollTarget - angle.eulerAngles.x) >= 1) {
			gunRollTarget = angle.eulerAngles.x;
		}
	}

	// 计算实际射击位置
	public Vector3 CalExplodePoint()
	{
		// 碰撞信息和碰撞点
		Vector3 hitPoint = Vector3.zero;
		RaycastHit hit;
		// 沿着炮管方向的射线
		Vector3 pos = gun.position + gun.forward * 5;
		Ray ray = new Ray (pos, gun.forward);
		// 射线检测
		if (Physics.Raycast (ray, out hit, 400.0f)) {
			// Debug.Log ("Hit " + hit.collider.gameObject.name);
			if (hit.collider.gameObject.tag == "Bullet") {
				hitPoint = lastHitPoint;
			} else {
				hitPoint = hit.point;
			}
			hitPoint = hit.point;
		} else {
			// Debug.Log ("No Hit");
			hitPoint = ray.GetPoint (400f);
		}
		lastHitPoint = hitPoint;
		return hitPoint;
	}

	// 绘制准心
	public void DrawSight()
	{
		Vector3 explodePoint = CalExplodePoint ();
		// 获取坦克准心坐标
		Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);
		// 绘制坦克准心
		Rect tankRect = new Rect(screenPoint.x - tankSight.width / 2,
			Screen.height - screenPoint.y - tankSight.height / 2,
			tankSight.width, tankSight.height);
		GUI.DrawTexture (tankRect, tankSight);
		// 绘制中心准心
		Rect centerRect = new Rect(screenPoint.x - centerSight.width / 2,
			Screen.height - screenPoint.y - centerSight.height / 2,
			centerSight.width, centerSight.height);
		GUI.DrawTexture (centerRect, centerSight);
	}

	// 绘制生命条
	public void DrawHp()
	{
		// 底框
		Rect bgRect = new Rect(30, Screen.height - hpBarBg.height - 15,
			hpBarBg.width, hpBarBg.height);
		GUI.DrawTexture (bgRect, hpBarBg);
		// 指示条
		float width = hp * 102 / maxHP;
		Rect hpRect = new Rect (bgRect.x + 29, bgRect.y + 9, width, hpBar.height);
		GUI.DrawTexture (hpRect, hpBar);
		// 文字
		string text = Mathf.Ceil(hp).ToString() + "/" + Mathf.Ceil(maxHP).ToString();
		Rect textRect = new Rect (bgRect.x + 80, bgRect.y - 10, 50, 50);
		GUI.Label (textRect, text);
	}

	// 显示击杀图标
	public void StartDrawKill()
	{
		killUIStartTime = Time.time;
	}

	// 绘制击杀图标
	private void DrawKillUI()
	{
		if (Time.time - killUIStartTime < 1f) {
			Rect rect = new Rect (Screen.width / 2 - killUI.width / 2, 30,
				killUI.width, killUI.height);
			GUI.DrawTexture (rect, killUI);
		}
	}

	public float GetHP()
	{
		return hp;
	}

	public void TurnOver()
	{
		Vector3 euler = transform.eulerAngles;
		if (euler.z < 0) {
			euler.z += 360;
		}
		Vector3 rot = new Vector3 (0, 0, 180);
		transform.Rotate (rot);
		Vector3 pos = transform.position;
		pos.y += 1;
		transform.position = pos;
	}
		
}
