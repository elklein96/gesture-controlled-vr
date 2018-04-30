using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log("Init. Display");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PointerEnter(){
		Debug.Log("Pointing to Display");
	}

	public void TeleportRandomly() {
    Vector3 direction = Random.onUnitSphere;
    direction.y = Mathf.Clamp(direction.y, 0.5f, 1f);
    float distance = 2 * Random.value + 1.5f;
    this.transform.localPosition = direction * distance;
  }
}
