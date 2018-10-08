using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public float distance = 15;		// 距离
	public float rot = 0;			// 横向角度
	public GameObject target;		// 目标物体
	public float rotSpeed = 0.2f;	// 相机旋转速度
	public float rollSpeed = 0.2f;  // 相机纵向旋转速度
	public float zoomSpeed = 0.2f;	// 缩放速度

	private float roll = 30f * Mathf.PI * 2 / 360; 	// 纵向角度
	private float maxRoll = 70f * Mathf.PI * 2 / 360;
	private float minRoll = 5f * Mathf.PI * 2 / 360;
	private float maxDistance = 22f;
	private float minDistance = 5f;

	
	// Update is called once per frame
	void LateUpdate () {
		if (target == null) {
			return;
		}
		if (Camera.main == null) {
			return;
		}
		if (Time.timeScale == 0) {
			return;
		}
		Rotate ();
		Roll ();
		Zoom ();
		Vector3 targetPos = target.transform.position;
		Vector3 cameraPos;
		float d = distance * Mathf.Cos (roll);
		float height = distance * Mathf.Sin (roll);
		cameraPos.x = targetPos.x + d * Mathf.Cos (rot);
		cameraPos.z = targetPos.z + d * Mathf.Sin (rot);
		cameraPos.y = targetPos.y + height;
		Camera.main.transform.position = cameraPos;
		Camera.main.transform.LookAt (target.transform);
	}

	void Rotate()
	{
		float w = Input.GetAxis ("Mouse X") * rotSpeed;
		rot -= w;
	}

	void Roll()
	{
		float w = Input.GetAxis ("Mouse Y") * rollSpeed * 0.5f;
		roll -= w;
		if (roll > maxRoll) {
			roll = maxRoll;
		}
		if (roll < minRoll) {
			roll = minRoll;
		}
	}

	void Zoom()
	{
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
			if (distance > minDistance) {
				distance -= zoomSpeed;
			}
		} else if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			if (distance < maxDistance) {
				distance += zoomSpeed;
			}
		}
	}

	public void SetTarget(GameObject target)
	{
		this.target = target;
	}
}
