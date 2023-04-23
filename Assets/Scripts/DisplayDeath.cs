using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDeath : MonoBehaviour {
	private Image image;
	private AudioSource sound;
	private RectTransform child;
	
	private float? startTime = null;
	
	public float wipeSeconds = 1f;
	
	void Start() {
		image = GetComponent<Image>();
		child = transform.GetChild(0).GetComponent<RectTransform>();
		sound = GetComponent<AudioSource>();
	}
	
	void Update() {
		// TODO: it would be nice to  not commit evil sins
		// and to instead pass messages like a normal person
		if (LevelManager.the.state == LevelManager.State.Dead) Die();
		
		if (startTime.HasValue) {
			float progress = Mathf.Clamp01((Time.time - startTime.Value) / wipeSeconds);
			
			image.fillAmount = Mathf.Pow(Mathf.SmoothStep(0f, 1f, progress), 3);
			child.localPosition = Vector3.down * Mathf.Pow(Mathf.SmoothStep(1f, 0f, progress), 2f);
		}
	}
	
	void Die() {
		if (!startTime.HasValue) {
			startTime = Time.time;
			sound.Play();
		}
	}
}
