using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour {
	void Update() {
		// you thought i was gonna lie to you?
		transform.localEulerAngles = new Vector3(
			Time.time * 9f, 0f, Time.time * 6f
		);
	}
}
