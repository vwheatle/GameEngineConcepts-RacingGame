using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceOrganizer : MonoBehaviour {
	public PlayerProgress realPlayer;
	private List<PlayerProgress> participants;
	
	public TMP_Text placeText, lapText;
	
	void Start() {
		participants = new List<PlayerProgress>();
		foreach (Transform t in transform) {
			PlayerProgress p = t.GetComponent<PlayerProgress>();
			if (p != null) participants.Add(p);
		}
	}
	
	void Update() {
		(PlayerProgress, float)[] participantsSorted = new (PlayerProgress, float)[participants.Count];
		for (int i = 0; i < participants.Count; i++) {
			participantsSorted[i] = (
				participants[i],
				// race progress as a single number
				participants[i].lap + participants[i].progress
			);
		}
		
		// sort ascending by place in race
		System.Array.Sort(participantsSorted, (a, b) => b.Item2.CompareTo(a.Item2));
		
		// Inefficient :(
		for (int i = 0; i < participants.Count; i++) {
			if (participantsSorted[i].Item1 == realPlayer) {
				placeText.text = IndexToPlace(i);
				lapText.text = "Lap " + Mathf.Max(Mathf.FloorToInt(participantsSorted[i].Item2), 1).ToString();
			}
		}
	}
	
	string IndexToPlace(int i) =>
		i switch {
			0 => "1st",
			1 => "2nd",
			2 => "3rd",
			_ => (i + 1).ToString() // zzzz
		};
}
