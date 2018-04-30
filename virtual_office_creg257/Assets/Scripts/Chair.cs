using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour {
	float smooth = 5.0f;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float tiltAroundY = Camera.main.transform.eulerAngles.y + 90;
		Quaternion target = Quaternion.Euler(-90, 0, tiltAroundY);

		this.transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * smooth);
	}

}
