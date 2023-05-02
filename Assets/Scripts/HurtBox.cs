using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			PlayerControl po = other.GetComponent<PlayerControl>();
			if (po.aiPlayer) {
				po.enabled = false;
			} else {
				LevelManager.the.SendMessage("Die");
			}
		}
	}
}
