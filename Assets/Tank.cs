using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ctrlType {
	none, computer, player
}

public class Tank : MonoBehaviour {
	public AudioSource motorSrc, shootSrc;
	public AudioClip motorClip, shootClip;
	public List<Axleinfo> m_Axleinfos;
	public ctrlType controlType = ctrlType.player;
	// Use this for initialization
	public Transform Turret, gun, tracks, wheels;
	public float targetAngel = 0, TurretSpeed = 0.5f, targetAngelX = 0;
	public GameObject bullet, fireEffect;
	public float shootTimeInterval = 0.5f;
	private float maxRoll = 10f, minRoll = -4f;
	public float maxHp = 100f, nowHp = 100f, attNum = 30f;
	private float motor = 0, brakeTorque = 0, steering = 0;
	public float maxMotor, maxBrakeTorque, maxSteerAngel;
	private float lastShoottime = 0;

	public Texture2D centerSight, tankSight, hpMaxBar, hpBar, killUI;
	private float killUIStartTime = float.MinValue;
	private AI ai;
	void Start () {
		Turret = transform.FindChild ("turret");
		gun = Turret.FindChild ("gun");
		tracks = transform.FindChild ("tracks");
		wheels = transform.FindChild ("wheels");
		motorSrc = gameObject.AddComponent<AudioSource> ();
		motorSrc.spatialBlend = 1;
		shootSrc = gameObject.AddComponent<AudioSource> ();
		shootSrc.spatialBlend = 1;

		if (controlType == ctrlType.computer) {
			ai = gameObject.AddComponent<AI> ();
			ai.tank = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(controlType == ctrlType.none)
			noneCtrl ();
		if(controlType == ctrlType.player)
			PlayerCtrl ();
		if (controlType == ctrlType.computer)
			ComputerCtrl ();
		foreach (Axleinfo ax in m_Axleinfos) {
			if (ax.steering) {
				ax.leftWheel.steerAngle = steering;
				ax.rightWheel.steerAngle = steering;
			}
			if (ax.motor) {
				ax.leftWheel.motorTorque = motor;
				ax.rightWheel.motorTorque = motor;
			}
			ax.leftWheel.brakeTorque = brakeTorque;
			ax.rightWheel.brakeTorque = brakeTorque;

		}
		MoveWheels (m_Axleinfos [1].leftWheel);
		MoveTrack ();
		RotateTurret ();
		RollGun ();
		LoadMusic ();
	}

	void OnGUI() {
		if (controlType == ctrlType.player) {
			DrawSight ();
			DrawHp ();
			DrawKillUI ();
		}
	}

	void RotateTurret() {
		float angel = Turret.eulerAngles.y - targetAngel;

		if(angel < 0)
			angel += 360;
		if (angel < 180 && angel > TurretSpeed) {
			Turret.Rotate (0, -TurretSpeed, 0);
		} else if (angel > 180 && 360 - angel > TurretSpeed)
			Turret.Rotate (0, TurretSpeed, 0);

	}

	void RollGun() {
		Vector3 worldAngel = gun.eulerAngles;
		Vector3 localAngel = gun.localEulerAngles;
		worldAngel.x = targetAngelX;
		gun.eulerAngles = worldAngel;
		Vector3 localAngel1 = gun.localEulerAngles;
		if (localAngel1.x > 180)
			localAngel1.x -= 360;
		if (localAngel1.x > maxRoll)
			localAngel1.x = maxRoll;
		if (localAngel1.x < minRoll)
			localAngel1.x = minRoll;
		gun.localEulerAngles = new Vector3(localAngel1.x, localAngel.y, localAngel.z);
	}

	void PlayerCtrl() {
		if (controlType != ctrlType.player)
			return;
		if (Input.GetMouseButton (0))
			Shoot ();


		motor = Input.GetAxis ("Vertical") * maxMotor;
		steering = Input.GetAxis ("Horizontal") * maxSteerAngel;
		brakeTorque = 0;
		foreach (Axleinfo ax in m_Axleinfos) {
			if (ax.leftWheel.rpm > 5 && motor < 0)
				brakeTorque = maxBrakeTorque;
			if (ax.leftWheel.rpm < -5 && motor > 0)
				brakeTorque = maxBrakeTorque;
		}

		TargetSignPos ();
	}

	void noneCtrl() {
		if (controlType != ctrlType.none)
			return;
		motor = 0;
		steering = 0;
		brakeTorque = maxBrakeTorque / 2;
	}

	void  ComputerCtrl() {
		if (controlType != ctrlType.computer)
			return;
		Vector3 vec = ai.GetTurretPos ();
		targetAngelX = vec.x;
		targetAngel = vec.y;
		if (ai.isShoot ()) {
			Shoot ();
		}

	}

	void MoveTrack() {

		if (tracks == null)
			return;

		float offset = 0;
		offset = wheels.GetChild (0).localEulerAngles.x / 90f;

		foreach (Transform track in tracks) {
			MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
			if (mr == null)
				return;
			Material mt = mr.material;
			mt.mainTextureOffset = new Vector2(0, offset);
		}

	}

	void MoveWheels(WheelCollider wc) {
		if (wheels == null)
			return;
		Vector3 position;
		Quaternion rotation;
		wc.GetWorldPose (out position, out rotation);
		foreach (Transform wheel in wheels) {
			wheel.rotation = rotation;
		}
	}

	void LoadMusic() {

		if (motor != 0 && motorSrc.isPlaying == false) {
			motorSrc.loop = true;
			motorSrc.clip = motorClip;
			motorSrc.Play ();
		} else if(motor == 0) {
			motorSrc.Pause ();
		}
	}

	void Shoot() {
		if (Time.time - lastShoottime < shootTimeInterval)
			return;
		if (bullet == null)
			return;
		Vector3 pos = gun.position + gun.forward * 5;
		GameObject obj = (GameObject) Instantiate (bullet, pos, gun.rotation);
		Bullet b = obj.GetComponent<Bullet> ();
		if (b != null)
			b.attackTank = this.gameObject;
		shootSrc.PlayOneShot (shootClip);
		lastShoottime = Time.time;
		//BeAttcked ();
	}

	public void BeAttcked(GameObject attackObj) {
		if (nowHp <= 0)
			return;
		nowHp -= attNum;
		if (nowHp <= 0) {
			GameObject destroyEffect = (GameObject)Instantiate (fireEffect, transform.position, transform.rotation);
			destroyEffect.transform.SetParent (transform, false);
			destroyEffect.transform.localPosition = Vector3.zero;
			controlType = ctrlType.none;
			if (attackObj != null) {
				Tank tk = attackObj.GetComponent<Tank> ();
				if (tk != null && tk.controlType == ctrlType.player)
					tk.StartShowKill ();
			}
		}

		if (controlType == ctrlType.computer) {
			ai.OnAttack (attackObj);
		}
	}

	public void StartShowKill() {
		killUIStartTime = Time.time;
	}

	void DrawKillUI() {
		if (Time.time - killUIStartTime < 1f) {
			Rect rect = new Rect (Screen.width / 2 - killUI.width / 2, 30, killUI.width, killUI.height);
			GUI.DrawTexture (rect, killUI);
		}
	}

	void TargetSignPos() {
		Vector3 hitPoint = Vector3.zero;
		Vector3 centerScreen = new Vector3 (Screen.width / 2, Screen.height / 2, 0);
		Ray ray = Camera.main.ScreenPointToRay (centerScreen);
		RaycastHit raycastHit;
		if (Physics.Raycast (ray, out raycastHit, 400.0f)) {
			hitPoint = raycastHit.point;
		} else {
			hitPoint = ray.GetPoint (400);
		}
		Vector3 dir = hitPoint - Turret.position;
		//dir = Turret.position - hitPoint;
		Quaternion q = Quaternion.LookRotation (dir);
		targetAngel = q.eulerAngles.y;
		targetAngelX = q.eulerAngles.x;
	}
	Vector3 CalcExplodePoint() {
		Vector3 hitPoint;
		Vector3 pos = gun.position + gun.forward * 5;
		Ray ray = new Ray(pos, gun.forward);
		RaycastHit raycastHit;
		if (Physics.Raycast (ray, out raycastHit, 400.0f)) {
			hitPoint = raycastHit.point;
		} else {
			hitPoint = ray.GetPoint (400);
		}
		return hitPoint;
	}

	void DrawSight() {
		Vector3 explodePoint = CalcExplodePoint ();
		Vector3 ScreenPoint = Camera.main.WorldToScreenPoint (explodePoint);
		Rect tankSightRect = new Rect (ScreenPoint.x - tankSight.width / 2, Screen.height - ScreenPoint.y - tankSight.height / 2,
			tankSight.width, tankSight.height);
		GUI.DrawTexture (tankSightRect, tankSight);

		Rect centerSightRect = new Rect (Screen.width / 2 - centerSight.width / 2, Screen.height / 2 - 
			centerSight.height / 2, centerSight.width, centerSight.height);
		GUI.DrawTexture (centerSightRect, centerSight);
	}

	void DrawHp() {
		Rect bgRect = new Rect (30, Screen.height - 30, hpMaxBar.width, hpMaxBar.height);
		GUI.DrawTexture (bgRect, hpMaxBar);

		float width = nowHp * 102 / maxHp;
		Rect hpRect = new Rect (bgRect.x + 29, bgRect.y + 9, width, hpBar.height);
		GUI.DrawTexture (hpRect, hpBar);

		string hpText = nowHp.ToString () + "/" + maxHp.ToString ();
		Rect hpTextRect = new Rect (bgRect.x + 80, bgRect.y - 10, 50, 50);
		GUI.Label (hpTextRect, hpText);
	}
}
