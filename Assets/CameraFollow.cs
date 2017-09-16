using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public float roll = 30 * Mathf.PI / 180, rot = 0;
	public float maxDistance = 25, minDistance = 5, distance = 8;
	public float rotSpeed = 0.2f, rollSpeed = 0.2f, zoomSpeed = 0.2f;
	public float maxRoll = 70 * Mathf.PI / 180, minRoll = -10 * Mathf.PI / 180;
	private GameObject targetTank;
	// Use this for initialization
	void Start () {
		targetTank = GameObject.Find ("Tank");
		if (targetTank.transform.FindChild ("cameraPoint") != null)
			targetTank = targetTank.transform.FindChild ("cameraPoint").gameObject;
		
	}

	// Update is called once per frame
	void Update () {
//		float steerSpeed = 20f, moveSpeed = 5f;
//		float res = Input.GetAxis ("Horizontal");
//		targetTank.transform.Rotate (0, res * Time.deltaTime * steerSpeed, 0);
//
//		res = Input.GetAxis ("Vertical");
//		Vector3 s = targetTank.transform.forward * moveSpeed * Time.deltaTime * res;
//		targetTank.transform.position += s;
	}
	void LateUpdate() {
		if (targetTank == null)
			return;
		if (Camera.main == null)
			return;

		Vector3 targetPos = targetTank.transform.position;
		Vector3 cameraPos;
		float d = distance * Mathf.Cos (roll), height = distance * Mathf.Sin(roll);
		cameraPos.x = targetPos.x + d * Mathf.Sin (rot);
		cameraPos.y = targetPos.y + height;
		cameraPos.z = targetPos.z - d * Mathf.Cos (rot);
		Camera.main.transform.position = cameraPos;
		Camera.main.transform.LookAt (targetTank.transform);
		if(Input.GetMouseButton(1))
			m_Rotate ();
		m_Zoom ();
	}

	void m_Rotate() {
		float x = Input.GetAxis ("Mouse X") * rotSpeed;
		rot -= x;

		float y = Input.GetAxis ("Mouse Y") * rollSpeed * 0.5f;
		roll -= y;
		if (roll <= minRoll)
			roll = minRoll;
		if (roll >= maxRoll)
			roll = maxRoll;
	}

	void m_Zoom() {
		float res = Input.GetAxis ("Mouse ScrollWheel");
		if (res > 0) {
			if (distance > minDistance)
				distance -= zoomSpeed;
		} else if (res < 0) {
			if (distance < maxDistance)
				distance += zoomSpeed;
		}
	}
}
