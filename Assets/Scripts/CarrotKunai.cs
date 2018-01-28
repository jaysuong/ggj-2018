using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrotKunai : MonoBehaviour {

	[SerializeField] ParticleSystem ps;
	private Rigidbody rb;
	public float InitialForce = 1000;
	public float damage = 2;
	// Use this for initialization
	void Start () {
		rb = gameObject.GetComponent<Rigidbody> ();
		rb.transform.Rotate (270f, 0f, 0f);
		rb.AddForce (transform.up * -InitialForce);
	}

	// Update is called once per frame
	void Update () {

	}

	void OnCollisionEnter (Collision c) {
		var health = c.collider.GetComponent<IHealth> ();
		if (health != null) {
			Debug.Log (health.CurrentHP);
			health.Damage (damage);
		}
		ps.Play ();
		Destroy (gameObject, 0.2f);
	}
}