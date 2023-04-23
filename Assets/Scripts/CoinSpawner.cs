using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour {
	public GameObject whatToSpawn;
	
	public int quantity = 128;
	public float radius = 1f;
	public float spin = 8f;
	
	void Start() {
		for (int i = 0; i < quantity; i++) {
			float t = i / (float)(quantity - 1);
			
			GameObject coin = Instantiate(whatToSpawn, this.transform);
			coin.transform.localPosition = SphereFn(t) * radius;
			coin.transform.localRotation = Quaternion.identity;
		}
	}
	
	void OnDrawGizmosSelected() {
		Vector3? last = null;
		
		for (int i = 0; i < quantity; i++) {
			float t = i / (float)(quantity - 1);
			
			Vector3 next = transform.position + SphereFn(t) * radius;
			
			if (last.HasValue)
				Gizmos.DrawLine(last.Value, next);
			
			// Gizmos.DrawCube(next, Vector3.one);
			
			last = next;
		}
	}
	
	Vector3 SphereFn(float t) {
		float xzRadius = Mathf.Sin(Mathf.PI * t);
		return new Vector3(
			Mathf.Cos(2 * Mathf.PI * spin * t) * xzRadius,
			Mathf.Cos(Mathf.PI * t),
			Mathf.Sin(2 * Mathf.PI * spin * t) * xzRadius
		);
	}
}
