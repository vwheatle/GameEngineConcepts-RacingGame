using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Fancy counter for any integer value you want!
// Just set the value in another script
// and this'll add it to an accumulator
// and eventually (in a second or so) commit it to a larger label!
// And it looks nice doing it! It has nice jolts for some extra juice.

public class DisplayCounter : MonoBehaviour {
	public TMP_Text displayLabel;
	private (string, string) displaySurroundings;
	
	private AudioSource updateSound, finalizeSound;
	
	// The label that displays "+5" or whatever.
	// It accumulates things.
	public TMP_Text accumulatorLabel;
	private (string, string) accumulatorSurroundings;
	
	// Splits a format string into two strings.
	// (Format string literally just has a '%' placeholder somewhere in there)
	// Returns two strings: one from before the placeholder, one from after.
	static (string, string) GetSurroundings(string formatString) {
		string[] slices = formatString.Split('%');
		Debug.Assert(slices.Length <= 2);
		return (slices[0] ?? "", slices[1] ?? "");
	}
	
	// How long it should wait before contributing to the displayed value.
	public float accumulatorWaitTime = 1.5f;
	private float? accumulatorLastChangeTime = null;
	
	private float accumulatorJolt = 0f;
	private float accumulatorJoltVelocity;
	
	private float displayJolt = 0f;
	private float displayJoltVelocity;
	
	private int displayedValue, nextValue;
	
	public int value {
		get => nextValue;
		set {
			if (value != displayedValue) {
				nextValue = value;
				
				accumulatorJolt = 1f;
				accumulatorJoltVelocity = 0.5f;
				accumulatorLastChangeTime = Time.time;
				
				UpdateAccumulatorLabel();
				updateSound.pitch = Mathf.Max(0.9f, 1f + ((nextValue - displayedValue) % 10) / 16f);
				updateSound.Play();
			}
		}
	}
	
	void Awake() {
		displayedValue = nextValue = 0;
		
		// Extract surroundings strings. yknow blah blah
		displaySurroundings = GetSurroundings(displayLabel.text);
		accumulatorSurroundings = GetSurroundings(accumulatorLabel.text);
		
		updateSound = accumulatorLabel.GetComponent<AudioSource>();
		finalizeSound = displayLabel.GetComponent<AudioSource>();
		
		UpdateDisplayLabel();
		UpdateAccumulatorLabel();
	}
	
	void Update() {
		accumulatorJolt = Mathf.SmoothDamp(accumulatorJolt, 0f, ref accumulatorJoltVelocity, 1/8f);
		displayJolt = Mathf.SmoothDamp(displayJolt, 0f, ref displayJoltVelocity, 1/4f);
		
		displayLabel.rectTransform.localScale = Vector3.one * (1f + displayJolt / 8f);
		accumulatorLabel.rectTransform.localScale = Vector3.one * (1f + accumulatorJolt / 8f);
		
		accumulatorLabel.enabled = accumulatorLastChangeTime.HasValue;
		if (accumulatorLastChangeTime.HasValue) {
			float startTime = accumulatorLastChangeTime.Value;
			
			float diffTime = Time.time - startTime;
			float finalization = diffTime / accumulatorWaitTime;
			
			if (finalization >= 1f) {
				accumulatorLastChangeTime = null;
				
				displayJolt = 1;
				displayJoltVelocity = 0.5f;
				displayedValue = nextValue;
				
				UpdateDisplayLabel();
				finalizeSound.Play();
			}
		}
	}
	
	void UpdateAccumulatorLabel() {
		var (before, after) = accumulatorSurroundings;
		accumulatorLabel.text = string.Format(
			"{0}{1}{2}{3}",
			before,
			(nextValue > displayedValue) ? "+" : "-",
			nextValue - displayedValue,
			after
		);
	}
	
	void UpdateDisplayLabel() {
		var (before, after) = displaySurroundings;
		displayLabel.text = before + nextValue.ToString() + after;
	}
	
}
