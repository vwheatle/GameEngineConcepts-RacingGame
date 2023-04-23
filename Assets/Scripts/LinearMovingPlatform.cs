using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearMovingPlatform : MonoBehaviour {
	private Vector3 home;
	
	[Tooltip("Number of seconds it takes to complete a full cycle.")]
	public float oscillationDuration = 4f;
	
	[Tooltip("Maximum magnitude of movement away from the initial position.")]
	public Vector3 oscillationMagnitude = Vector3.one * 8f;
	
	void Start() {
		home = transform.localPosition;
	}
	
	void Update() {
		transform.localPosition = home + Vector3.Lerp(
			-oscillationMagnitude, oscillationMagnitude,
			Mathf.PingPong(Time.time * (2f / oscillationDuration), 1f)
		);
		// transform.eulerAngles += Vector3.up * Time.deltaTime * 15f;
	}
}
