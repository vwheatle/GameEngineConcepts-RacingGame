using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour {
	[HideInInspector]
	public bool listenForCountdown = true;
	
	public Mesh[] numbers;
	public Mesh goText;
	
	IEnumerator Start() {
		while (true) {
			yield return new WaitUntil(() => LevelManager.the.state == LevelManager.State.Intro);
			if (listenForCountdown) {
				yield return DoCountdown(() => LevelManager.the.SendMessage("StartRace"));
			}
		}
	}
	
	public IEnumerator DoCountdown(System.Action action = null) {
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		MeshFilter filter = GetComponent<MeshFilter>();
		
		transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 4f), Camera.MonoOrStereoscopicEye.Mono);
		transform.forward = -Camera.main.transform.forward;
		renderer.enabled = true;
		
		foreach (Mesh mesh in numbers) {
			filter.mesh = mesh;
			yield return new WaitForSeconds(1f);
		}
		
		action?.Invoke();
		
		filter.mesh = goText;
		yield return new WaitForSeconds(1f);
		
		renderer.enabled = false;
		
	}
}
