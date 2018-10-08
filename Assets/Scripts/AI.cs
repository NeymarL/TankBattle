using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

	public enum Status
	{
		Patrol,
		Attack
	}

	public TankBeheavior tank;

	private GameObject target;			// 锁定的坦克
	private float sightDistance = 100;	// 视野范围
	private float lastSearchTime = 0;	// 上次搜寻时间
	private float searchInterval = 2;	// 搜寻间隔

	private Status status = Status.Patrol;
	private Path path = new Path ();

	private float lastUpdateWaypointTime = float.MinValue;
	private float updateWaypointCD = 10;

	void Start()
	{
		ChangeStatus (status);
	}

	void OnDrawGizmos ()
	{
		// path.DrawWayPoints ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (tank.ctrlType != TankBeheavior.CtrlType.NPC) {
			return;
		}
		// 搜寻目标
		TargetUpdate ();

		switch (status) {
		case Status.Attack:
			AttackUpdate ();
			break;
		case Status.Patrol:
			PatrolUpdate ();
			break;
		}

		// 行走
		if (path.IsReach (transform)) {
			path.NextWaypoint ();
		}
	}

	public void ChangeStatus(Status status)
	{
		switch (status) {
		case Status.Attack:
			AttackStart ();
			break;
		case Status.Patrol:
			PatrolStart ();
			break;
		}
	}

	public void AttackStart()
	{
		Vector3 targetPos = target.transform.position;
		path.InitByNavMeshPath (transform.position, targetPos);
	}

	public void PatrolStart()
	{
		PatrolUpdate ();
	}

	public void AttackUpdate()
	{
		if (target == null) {
			ChangeStatus (Status.Patrol);
		}
		float interval = Time.time - lastUpdateWaypointTime;
		if (interval < updateWaypointCD) {
			return;
		}
		Vector3 targetPos = target.transform.position;
		path.InitByNavMeshPath (transform.position, targetPos);
	}

	public void PatrolUpdate()
	{
		if (target != null) {
			ChangeStatus (Status.Attack);
		}
		// 时间间隔
		float interval = Time.time - lastUpdateWaypointTime;
		if (interval < updateWaypointCD) {
			return;
		}
		// 处理巡逻点
		if (path.wayPoints == null || path.isFinish) {
			int camp = Battle.instance.GetCamp (tank.gameObject);
			GameObject obj = GameObject.Find ("Waypoints" + camp);
			int count = obj.transform.childCount;
			if (count == 0) {
				return;
			}
			int index = Random.Range (0, count);
			Vector3 targetPos = obj.transform.GetChild (index).position;
			path.InitByNavMeshPath (transform.position, targetPos);
		}
	}

	public void TargetUpdate()
	{
		float interval = Time.time - lastSearchTime;
		if (interval < searchInterval) {
			return;
		}
		lastSearchTime = Time.time;
		if (target != null) {
			HasTarget ();
		} else {
			NoTarget ();
		}
	}

	// 判断目标是否丢失
	public void HasTarget()
	{
		Vector3 pos = transform.position;
		Vector3 targetPos = target.transform.position;

		if (Vector3.Distance (pos, targetPos) > sightDistance) {
			target = null;
		}
	}

	// 搜索视野重的坦克
	public void NoTarget()
	{
		float minHp = float.MaxValue;
		GameObject[] targets = GameObject.FindGameObjectsWithTag ("Tank");
		for (int i = 0; i < targets.Length; i++) {
			TankBeheavior tank = targets [i].GetComponent<TankBeheavior> ();
			if (tank == null || targets[i] == gameObject) {
				continue;
			}
			// 判断是否同一阵营
			if (Battle.instance.IsSameCamp(gameObject, targets[i])) {
				continue;
			}
			// 判断距离
			Vector3 pos = transform.position;
			Vector3 targetPos = tank.transform.position;
			if (Vector3.Distance (pos, targetPos) > sightDistance) {
				continue;
			}
			// 判断生命值
			if (minHp > tank.GetHP ()) {
				minHp = tank.GetHP ();
				target = tank.gameObject;
			}
		}
	}

	// 被动搜寻
	public void OnAttacked(GameObject attackTank)
	{
		if (Battle.instance.IsSameCamp(gameObject, attackTank)) {
			// 队友
			return;
		}
		target = attackTank;
	}

	// 获取炮塔的目标角度
	public Vector3 GetTurretTarget()
	{
		// 没有目标，目视前方
		if (target == null) {
			float y = transform.eulerAngles.y;
			Vector3 rot = new Vector3 (0, y, 0);
			return rot;
		} else {
			Vector3 pos = transform.position;
			Vector3 targetPos = target.transform.position;
			Vector3 vec = targetPos - pos;
			return Quaternion.LookRotation (vec).eulerAngles;
		}
	}

	public bool IsShoot()
	{
		if (target == null) {
			return false;
		}
		// 目标角度差
		float turretRoll = tank.turret.eulerAngles.y;
		float angle = turretRoll - GetTurretTarget ().y;
		if (angle < 0) {
			angle += 360;
		}
		if (angle < 20 || angle > 340) {
			return true;
		}
		return false;
	}

	public float GetSteering()
	{
		Vector3 itp = transform.InverseTransformPoint (path.wayPoint);
		if (itp.x > path.deviation / 5) {
			// 右转
			return -tank.maxSteering;
		} else if (itp.x < -path.deviation / 5) {
			// 左转
			return +tank.maxSteering;
		} else {
			return 0;
		}
	}

	public float GetMotor()
	{
		if (tank == null) {
			return 0;
		}
		Vector3 itp = transform.InverseTransformPoint (path.wayPoint);
		float x = itp.x;
		float z = itp.z;
		float r = 6;

		if (z < 0 && Mathf.Abs (x) < -z && Mathf.Abs (x) < r) {
			return -tank.maxMotorTorque;
		} else {
			return tank.maxMotorTorque;
		}
	}

	public float GetBrakeTorque()
	{
		if (path.isFinish) {
			return tank.maxMotorTorque;
		} else {
			return 0;
		}
	}
		
}
