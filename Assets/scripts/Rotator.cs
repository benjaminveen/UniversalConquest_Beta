using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {
	public float speed;

	// Use this for initialization
	void Start () {
		rigidbody.angularVelocity = new Vector3 (0, -1, 0) * speed;
	}

}
