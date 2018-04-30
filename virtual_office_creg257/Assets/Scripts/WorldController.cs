using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour {

	//The HUD is visualized by a cilinder of radius 1 around the user
	// Sliding of displays is dependant on 
	public GameObject HUD; //our top level Heads Up Display Object
	public List<GameObject> display; //Array of displays 
	public 
	int focus; //user's current display focus


	// Use this for initialization
	void Start () {
		HUD = GameObject.Find("HUD");
		display = new List<GameObject>(GameObject.FindGameObjectsWithTag("DISPLAY"));

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PointerEnter(){
		Debug.Log("Entered HUD");
	}
}
