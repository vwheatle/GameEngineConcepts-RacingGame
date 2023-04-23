using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
	public Transform target;
	
	public float dampSpeed = 0.125f;
	
	private Vector3 positionOffset;
	
	private Vector3 velocity;
	
	void Start() {
		positionOffset = transform.position - target.position;
	}
	
	void Update() {
		Vector3 nextPosition = target.position + positionOffset;
		transform.position = Vector3.SmoothDamp(transform.position, nextPosition, ref velocity, dampSpeed);
	}
}
