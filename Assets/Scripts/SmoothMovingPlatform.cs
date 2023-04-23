using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothMovingPlatform : MonoBehaviour {
	private Vector3 home;
	
	[Tooltip("Number of seconds it takes to complete a full cycle.")]
	public float oscillationDuration = 4f;
	
	[Tooltip("Maximum magnitude of movement away from the initial position.")]
	public Vector3 oscillationMagnitude = Vector3.one * 8f;
	
	void Start() {
		home = transform.localPosition;
	}

	void Update() {
		float oscillationSpeed = 1f / oscillationDuration;
		const float TAU = Mathf.PI * 2f;
		transform.localPosition = home + new Vector3(
			oscillationMagnitude.x * Mathf.Sin(oscillationSpeed * TAU * Time.time),
			oscillationMagnitude.y * Mathf.Cos(oscillationSpeed * TAU * Time.time),
			oscillationMagnitude.z * Mathf.Sin(oscillationSpeed * TAU * Time.time)
		);
		// my vector3 library is 10x better than this built-in garbage
		// (i have operator*(Vector3, Vector3)->Vector3 implemented)
		// ( part of the reason i think it's not implemented here is to 
	}
}
