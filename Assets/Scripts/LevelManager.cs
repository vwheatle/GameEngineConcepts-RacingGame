using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
	private static LevelManager me;
	public static LevelManager the { get => me; }
	
	private GameObject player;
	
	public enum State { Intro, Playing, Dead }
	
	private State currentState = State.Playing;
	public State state { get => currentState; }
	
	void Awake() {
		me = this;
		player = GameObject.FindGameObjectWithTag("Player");
	}
	
	void Die() {
		currentState = State.Dead;
		StartCoroutine(DeathWait());
	}
	
	IEnumerator DeathWait() {
		yield return new WaitForSeconds(1.5f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
