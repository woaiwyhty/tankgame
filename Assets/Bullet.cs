using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
	public float speed = 100f, maxLifetime = 2f, instantiateTime;
	public AudioClip explodeClip;
	public GameObject attackTank;
	public GameObject explodeEffect;
	// Use this for initialization
	void Start () {
		instantiateTime = Time.time;

	}
	
	// Update is called once per frame
	void Update () {
		transform.position += transform.forward * speed * Time.deltaTime;
		Debug.Log (transform.position);
		if (Time.time - instantiateTime > maxLifetime)
			Destroy (gameObject);
	}

	void OnCollisionEnter(Collision collisionInfo) {
		if (collisionInfo.gameObject == attackTank) {
			return;
		}
		GameObject obj = (GameObject) Instantiate (explodeEffect, transform.position, transform.rotation);
		AudioSource explodeSrc = obj.AddComponent<AudioSource> ();
		explodeSrc.spatialBlend = 1;
		explodeSrc.PlayOneShot (explodeClip);
		if (collisionInfo.gameObject != null) {
			Tank tk = collisionInfo.gameObject.GetComponent<Tank> ();
			if (tk != null)
				tk.BeAttcked (attackTank);
		}

		Destroy (gameObject);
	}
}
