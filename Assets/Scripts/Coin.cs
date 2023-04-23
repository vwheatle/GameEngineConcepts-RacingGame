using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {
	const float REVOLUTIONS_PER_SECOND = 2f;
	const float ROTATE_SPEED = 360f / REVOLUTIONS_PER_SECOND;
	
	const float SHRINK_DURATION = 0.25f;
	
	private Vector3 initialScale;
	
	private float? collectTime = null;
	
	void Awake() {
		initialScale = transform.localScale;
		ResetLevel();
	}
	
	void ResetLevel() {
		collectTime = null;
		transform.localScale = initialScale;
		gameObject.SetActive(true);
	}
	
	void Update() {
		// yes, global rotation set. when it spins around it spins pointing up.
		transform.eulerAngles = Vector3.up * ROTATE_SPEED * Time.time;
		
		if (collectTime.HasValue) {
			float startTime = collectTime.Value;
			
			float diffTime = Time.time - startTime;
			float percentDead = diffTime / SHRINK_DURATION;
			
			// pop out a little while animating out. feels nice
			float scalarScale = Mathf.Max(0f, 1f - percentDead) * 1.25f;
			
			transform.localScale = initialScale * scalarScale;
			
			if (percentDead >= 1f) gameObject.SetActive(false);
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (collectTime.HasValue) return;
		if (other.CompareTag("Player")) {
			SendMessageUpwards("Collect", this.gameObject, SendMessageOptions.RequireReceiver);
			collectTime = Time.time;
		}
	}
}
