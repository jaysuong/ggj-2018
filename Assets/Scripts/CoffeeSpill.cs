using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent (typeof (SphereCollider))]
public class CoffeeSpill : MonoBehaviour {
	[SerializeField] ParticleSystem ps;
	void Start () {

		// Spawn a sphere near the ground (height??)
		RaycastHit hitInfo;
		if (Physics.Raycast (transform.position, -transform.up, out hitInfo)) {
			SphereCollider sc = gameObject.GetComponent<SphereCollider> ();
			sc.center = hitInfo.point - gameObject.transform.position;
			Debug.Log (sc.center);
			// sc.transform.
		}
		ps.Play ();

		Destroy (gameObject, ps.main.startLifetimeMultiplier);
	}

	// Update is called once per frame
	void Update () {

	}
}