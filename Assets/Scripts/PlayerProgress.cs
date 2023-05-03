using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : MonoBehaviour {
	[Range(0f, 1f)]
	public float progress;
	
	public int lap;
	public List<double> lapTimes;
	
	void Start() {
		RaycastHit hit;
		
		// note that laps start at 1.
		// lap "0" is when you cross the line and start getting timed.
		// it will not be shown, and lap 0->1 will not be included in the total
		lap = 0;
		
		if (Physics.Raycast(
			transform.position + (Vector3.up * 0.25f),
			Vector3.down, out hit
		)) {
			RoadGenerator road = hit.transform.GetComponent<RoadGenerator>();
			if (road != null) {
				progress = road.GetProgressFromTriangleIndex(hit.triangleIndex);
				return;
			} else {
				Debug.Log("found non-road entity..");
			}
		}
		
		Debug.Log("couldn't find progress on road. defaults to 90% lap 0");
		progress = 0.9f;
	}
	
	void Update() {
		RoadGenerator road;
		RaycastHit hit;
		if (Physics.Raycast(
			transform.position + (Vector3.up * 0.25f),
			Vector3.down, out hit
		)) {
			road = hit.transform.GetComponent<RoadGenerator>();
		} else {
			Debug.Log("lost contact with road");
			return;
		}
		
		if (road == null) {
			Debug.Log("lost contact with road..");
			return;
		}
		
		float nextProgress = road.GetProgressFromTriangleIndex(hit.triangleIndex);
		
		if (nextProgress < 0.2f && progress > 0.8f) {
			// Record time if this is a lap that hasn't happened before.
			if (lapTimes.Count <= lap) lapTimes.Add(Time.unscaledTimeAsDouble);
			
			lap++;
		}
		
		// No going backwards!!!
		if (progress < 0.2f && nextProgress > 0.8f) lap--;
		
		progress = nextProgress;
	}
}
