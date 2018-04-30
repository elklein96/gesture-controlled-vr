using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log("Init. Display");
	}
	
	// Update is called once per frame
	void Update () {}

	public void PointerEnter(){
		Debug.Log("Pointing to Display");
	}
}
