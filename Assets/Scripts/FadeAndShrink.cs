using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeAndShrink : MonoBehaviour {
	private float startTime = 0f;
	public float deathTime = 0.25f;
	
	private Vector3 initialScale = Vector3.one;
	
	void Awake() {
		initialScale = transform.localScale;
		startTime = Time.unscaledTime;
	}

	void LateUpdate() {
		float diffTime = Time.unscaledTime - startTime;
		float percentDead = diffTime / deathTime;
		
		float scalarScale = Mathf.Max(0f, 1f - percentDead);
		transform.localScale = initialScale * scalarScale;
		
		if (percentDead >= 1f) Destroy(this.gameObject);
	}
}
