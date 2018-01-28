using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeSpillTester : MonoBehaviour {
	[SerializeField] private GameObject spill;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("r"))
			Instantiate (spill, gameObject.transform.position, Quaternion.identity);
	}
}