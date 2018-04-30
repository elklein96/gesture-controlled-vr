using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand_Controller : MonoBehaviour {
	private GameObject mask_pointer;
	private Camera main_camera;

	float smooth = 5.0f;
    float tiltAngle = 120.0f;
	// Use this for initialization
	void Start () {
		mask_pointer = GameObject.Find("m_pointer");
		main_camera  = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.position = mask_pointer.transform.position;

		float tiltAroundZ = Camera.main.transform.eulerAngles.x + 50f;
        float tiltAroundY = Camera.main.transform.eulerAngles.y - tiltAngle;
		float tiltAroundX = 10f;

		Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, tiltAroundZ);

		this.transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * smooth);
	}
}



   