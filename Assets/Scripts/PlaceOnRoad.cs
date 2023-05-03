using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOnRoad : MonoBehaviour {
	public RoadGenerator road;
	
	public bool straightUp = false;
	
	[Range(-0.5f, 1.5f)]
	public float percentageAlongRoad;
	
	void Start() {
		(Vector3 pos, Vector3 tan) = road.GetPositionTangentPairAt(percentageAlongRoad);
		transform.position = pos;
		if (straightUp) {
			transform.forward = Vector3.ProjectOnPlane(tan, Vector3.up).normalized;
		} else {
			transform.forward = tan.normalized;
		}
		// Destroy(this); // don't need it anymore! lol
	}
	
	void OnDrawGizmosSelected() {
		if (road == null) return;
		(Vector3 pos, Vector3 tan) = road.GetPositionTangentPairAt(percentageAlongRoad);
		// transform.position = pos;
		// transform.forward = tan.normalized;
		Quaternion rot = Quaternion.LookRotation(tan.normalized, Vector3.up);
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(pos, rot * Vector3.forward * 4f);
		Gizmos.color = Color.green;
		Gizmos.DrawRay(pos, rot * Vector3.up * 4f);
		Gizmos.color = Color.red;
		Gizmos.DrawRay(pos, rot * Vector3.right * 4f);
	}
}
