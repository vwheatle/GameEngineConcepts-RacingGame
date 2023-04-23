using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour {
	private int coins = 0;
	
	public DisplayCounter coinCounter;
	
	void Start() {
		coins = 0;
	}
	
	void Collect(GameObject _subject) {
		coins++;
		coinCounter.value = coins;
	}
}
