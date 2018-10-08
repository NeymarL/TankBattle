using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Path {

	public Vector3[] wayPoints;		// 路点的有序集合
	public int index = -1;			// 坦克的下一个目标点
	public Vector3 wayPoint;
	public bool isLoop = false;		// 是否循环
	public float deviation = 5;		// 到达误差
	public bool isFinish = false;	// 是否完成

	// 根据场景标识物生成路点
	public void InitByObj (GameObject obj, bool isLoop = false)
	{
		int length = obj.transform.childCount;
		if (length == 0) {
			wayPoints = null;
			index = -1;
			Debug.LogWarning ("Path.InitByObj No Children!!!");
			return;
		}
		wayPoints = new Vector3[length];
		for (int i = 0; i < length; i++) {
			Transform trans = obj.transform.GetChild (i);
			wayPoints [i] = trans.position;
		}
		index = 0;
		wayPoint = wayPoints [index];
		this.isLoop = isLoop;
		isFinish = false;
	}

	public void InitByNavMeshPath(Vector3 pos, Vector3 targetPos)
	{
		wayPoints = null;
		index = -1;
		// 计算路径
		NavMeshPath navPath = new NavMeshPath();
		bool hasFoundPath = NavMesh.CalculatePath (pos, targetPos, NavMesh.AllAreas, navPath);
		if (!hasFoundPath) {
			// Debug.LogWarning ("No Path Found!!! From " + pos + " To " + targetPos);
			return;
		}
		// 生成路径
		int length = navPath.corners.Length;
		wayPoints = new Vector3[length];
		for (int i = 0; i < length; i++) {
			wayPoints [i] = navPath.corners [i];
		}
		index = 0;
		wayPoint = wayPoints [index];
		isFinish = false;
	}


	public bool IsReach(Transform trans)
	{
		Vector3 pos = trans.position;
		float distance = Vector3.Distance (wayPoint, pos);
		return distance < deviation;
	}

	public void NextWaypoint()
	{
		if (index < 0) {
			return;
		}
		if (index < wayPoints.Length - 1) {
			index++;
		} else {
			if (isLoop) {
				index = 0;
			} else {
				isFinish = true;
			}
		}
		wayPoint = wayPoints [index];
	}

	public void DrawWayPoints()
	{
		if (wayPoints == null) {
			return;
		}
		int length = wayPoints.Length;
		for (int i = 0; i < length; i++) {
			if (i == index) {
				Gizmos.DrawSphere (wayPoints [i], 1);
			} else {
				Gizmos.DrawCube (wayPoints[i], Vector3.one);
			}
		}
	}
}
