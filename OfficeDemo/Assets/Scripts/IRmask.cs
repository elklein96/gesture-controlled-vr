using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using GestureStream;

public class IRmask : MonoBehaviour {
	public SockerListener stream_t; //stream listener needs to be attatched 
	public GameObject m_stream;
	public GameObject m_pointer; //Any attachable object which will follow the curpos
	public bool show_pointer;
	private GameObject m_mask; //This will be a quad which will follow the center of the camera
	private GameObject[] ir_mask;
	private Vector3 tran;
	
	private Fingers prev_hand;
	private Fingers hand;

	private double DEBOUCE_DELAY_MS = 50;
	private double lastDebounceTime;

	// Use this for initialization
	void Start () {
		// Start with all fingers raised
		prev_hand = new Fingers(1, 1, 1, 1, 1);
		hand = new Fingers(1, 1, 1, 1, 1);
	
		var height = (Camera.main.orthographicSize * 2.0)/10;
     	var width = (height * Screen.width / Screen.height);

		ir_mask = GameObject.FindGameObjectsWithTag("ir_mask");
		foreach (GameObject obj in ir_mask) {
			switch(obj.name){
				case "m_mask":
					m_mask = obj;
					break;
				case "m_pointer":
					m_pointer = obj;
					break;
				case "m_stream":
					m_stream = obj;
					break;
				default: 
					Debug.Log("No match found");
					break;
			}	
		}
		if(m_mask != null && m_stream != null && m_pointer!= null){
			m_mask.transform.localPosition = new Vector3(0,0,0.9f);
			m_pointer.transform.localPosition = new Vector3(0,0,m_mask.transform.localPosition.z);
			m_mask.transform.localScale = new Vector3((float)width, (float)height, m_mask.transform.localScale.z);
			m_pointer.transform.localScale = new Vector3(.05f, .1f, m_mask.transform.localScale.z);
			m_pointer.GetComponent<Renderer>().enabled = show_pointer;
			stream_t = m_stream.GetComponent<SockerListener>();
		}
		else{
			Debug.Log("Error in IR_mask check for appropriete GameObjects in scene");
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 handLoc = stream_t.getPoint();
		Fingers new_hand = stream_t.getFingers();
		prev_hand = hand;

		if (new_hand != null) {
			hand.thumb = Debounce(new_hand.thumb, prev_hand.thumb);
			hand.index = Debounce(new_hand.index, prev_hand.index);
			hand.second = Debounce(new_hand.second, prev_hand.second);
			hand.third = Debounce(new_hand.third, prev_hand.third);
			hand.pinky = Debounce(new_hand.pinky, prev_hand.pinky);
		}

		float x = handLoc.x * 1.75f;
		float y = handLoc.y * -2.25f;
		m_pointer.transform.localPosition = new Vector3(x, y, 0);
	}

	// Debounces a stream of integer values
	public int Debounce(int new_val, int prev_val) {
		double now = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		if (new_val == prev_val) {
			lastDebounceTime = now;
		}
		if ((now - lastDebounceTime) > DEBOUCE_DELAY_MS) {
			return new_val;
		}
		return prev_val;
	}
}
