using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

	// Use this for initialization
	public Tank tank;
	public float intervalSearchTime = 0.5f;
	private float LastSearchTime = float.MinValue;
	public float SearchMaxDistance = 20f;
	private GameObject target;

	public enum Status {
		Patrol, Attack,
	}
	private Status st = Status.Patrol;
	void Start () {
		
	} 
	
	// Update is called once per frame
	void Update () {
		if (tank.controlType != ctrlType.computer)
			return;
		if (st == Status.Attack) {
			AttackUpdate ();
		} else if (st == Status.Patrol)
			PatrolUpdate ();
		TargetSearch ();
	}

	public void ChangeStatus (Status status) {
		if (status == Status.Patrol) {
			PatrolStart ();
		} else if (status == Status.Attack)
			AttackStart ();
		
	}

	void PatrolStart() {
		
	}

	void AttackStart() {

	}

	void PatrolUpdate() {

	}

	void AttackUpdate() {

	}

	void TargetSearch() {
		if (Time.time - LastSearchTime < intervalSearchTime)
			return;
		
		LastSearchTime = Time.time;
		if (target != null)
			HasTarget ();
		else
			NoTarget ();
	}

	void HasTarget() {
		Vector3 pos = target.transform.position;
		Vector3 tankPos = gameObject.transform.position;
		Tank targetTank = target.GetComponent<Tank> ();
		if (targetTank.controlType == ctrlType.none) {
			target = null;
			Debug.Log ("die");
			return;
		}
		if (Vector3.Distance (pos, tankPos) > SearchMaxDistance) {
			target = null;
			Debug.Log ("too far");
			return;
		}


	}

	void NoTarget() {
		float minHp = float.MaxValue;
		GameObject[] objs = GameObject.FindGameObjectsWithTag ("Tank");
		for(int i = 0 ; i < objs.Length ; ++i) {
			Tank t = objs[i].GetComponent<Tank> ();
			if (t == null)
				continue;
			Vector3 pos = t.transform.position;
			if (Vector3.Distance (pos, gameObject.transform.position) > SearchMaxDistance)
				continue;
			if (t.controlType == ctrlType.none)
				continue;
			if (objs[i] == gameObject)
				continue;
			if (t.nowHp < minHp) {
				target = objs[i];
				minHp = t.nowHp;
			}
		}
		if(target != null)
			Debug.Log ("New target:" + target.name);
	}

	public void OnAttack(GameObject obj) {
		target = obj;
	}

	public Vector3 GetTurretPos() {
		if (target == null) {
			float y = transform.eulerAngles.y;
			Vector3 vec = new Vector3 (0, y, 0);
			return vec;
		}

		Vector3 pos = target.transform.position;
		Vector3 pos1 = transform.position;
		return Quaternion.LookRotation (pos - pos1).eulerAngles;
	}

	public bool isShoot() {
		if (target == null)
			return false;
		float deltaAngle = tank.Turret.eulerAngles.y - GetTurretPos ().y;
		if (deltaAngle < 0)
			deltaAngle += 360;
		if (deltaAngle < 30 || deltaAngle > 330)
			return true;
		return false;
	}
}
