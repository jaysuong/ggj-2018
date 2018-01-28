using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrotKunai : MonoBehaviour {

	[SerializeField] ParticleSystem ps;
	private Rigidbody rb;
	public float InitialForce = 1000;
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
		ps.Play ();
		Destroy (gameObject, 0.2f);
	}
}