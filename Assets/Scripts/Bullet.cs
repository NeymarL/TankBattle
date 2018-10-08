using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	public float speed = 200f;
	public GameObject explode;
	public GameObject dustExplode;
	public float maxLiftTime = 2f;
	public float maxDamageAmount = 100;

	private GameObject attackTank;

	private float instantiateTime = 0f;

	// Use this for initialization
	void Start () {
		instantiateTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		// 前进
		transform.position += transform.forward * speed * Time.deltaTime;
		// 摧毁
		if (Time.time - instantiateTime > maxLiftTime) {
			Instantiate (explode, transform.position, transform.rotation);
			Destroy(gameObject);
		}
	}

	void OnCollisionEnter(Collision collisionInfo) {
		if (collisionInfo.gameObject == attackTank) {
			// 打到自身
			return;
		}
		if (collisionInfo.gameObject.name == "Terrain") {
			Instantiate (dustExplode, transform.position, transform.rotation);
		} else {
			Instantiate (explode, transform.position, transform.rotation);
		}
		Destroy (gameObject);
		// 击中坦克
		TankBeheavior tank = collisionInfo.gameObject.GetComponent<TankBeheavior>();
		if (tank != null) {
			tank.BeAttacked (GetDemage (), attackTank);
		}
	}

	private float GetDemage()
	{
		float damage = maxDamageAmount - (Time.time - instantiateTime) * 40;
		if (damage < 1) {
			damage = 1;
		}
		return damage;
	}

	public void setAttackTank(GameObject att)
	{
		attackTank = att;
	}

	public GameObject getAttackTank()
	{
		return attackTank;
	}
}
