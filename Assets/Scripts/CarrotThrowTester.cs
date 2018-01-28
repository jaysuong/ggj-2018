using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrotThrowTester : MonoBehaviour {
	[SerializeField] private GameObject carrot;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp ("e"))
			Instantiate (carrot, gameObject.transform.position, Quaternion.identity);
	}
}