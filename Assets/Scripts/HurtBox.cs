using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player"))
			LevelManager.the.SendMessage("Die");
	}
}
