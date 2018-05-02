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
	private GameObject m_mask; //This will be a quad which will follow the center of the camera
	private GameObject ironMan;
	private GameObject sphere1;
	private GameObject[] ir_mask;
	
	private Fingers prevHand;
	private Fingers hand;

	private int cubeIsSelected;
	private int cubeIsSelectedPrev;
	bool toggle;

	double DEBOUCE_DELAY_MS_SELECT = 1000;
	double DEBOUCE_DELAY_MS_FINGER = 50;

	struct DebounceTimes {
    	public double lastDebounceTimeSelect;
		public double lastDebounceTimeThumb;
		public double lastDebounceTimeIndex;
		public double lastDebounceTimeMiddle;
		public double lastDebounceTimeThird;
		public double lastDebounceTimePinky;
	}

	DebounceTimes debounceTimes;

	// Use this for initialization
	void Start () {
		// Start with all fingers raised
		debounceTimes = new DebounceTimes();

		prevHand = new Fingers(1, 1, 1, 1, 1);
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
				case "iron_man":
					ironMan = obj;
					break;
				case "m_sphere_1":
					sphere1 = obj;
					break;
				default: 
					Debug.Log("No match found for " + obj.name);
					break;
			}	
		}
		if (m_mask != null && m_stream != null && m_pointer != null && ironMan != null) {
			m_mask.transform.localPosition = new Vector3(0, 0, 0.9f);
			m_pointer.transform.localPosition = new Vector3(0, 0, m_mask.transform.localPosition.z);
			m_mask.transform.localScale = new Vector3((float) width, (float) height, m_mask.transform.localScale.z);
			m_pointer.transform.localScale = new Vector3(.05f, .1f, m_mask.transform.localScale.z);
			stream_t = m_stream.GetComponent<SockerListener>();
			ironMan.GetComponent<Renderer>().enabled = false;
		} else {
			Debug.Log("Error in IR_mask check for appropriete GameObjects in scene");
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 handLoc = stream_t.getPoint();
		float x = handLoc.x * 1.75f;
		float y = handLoc.y * 2.25f;

		checkIfCollision();

		if (toggle) {
			ironMan.gameObject.transform.rotation =
				Quaternion.Euler(y * 225, (x * 600) + 180, ironMan.gameObject.GetComponent<Renderer>().transform.rotation.z);
		} else {
			m_pointer.transform.localPosition = new Vector3(x, y, 0);
		}
	}

	void parseHandGesture() {
		Fingers newHand = stream_t.getFingers();
		prevHand = hand;

		if (newHand != null) {
			hand.thumb = Debounce(newHand.thumb, prevHand.thumb, ref debounceTimes.lastDebounceTimeThumb, DEBOUCE_DELAY_MS_FINGER);
			hand.index = Debounce(newHand.index, prevHand.index, ref debounceTimes.lastDebounceTimeIndex, DEBOUCE_DELAY_MS_FINGER);
			hand.second = Debounce(newHand.second, prevHand.second, ref debounceTimes.lastDebounceTimeMiddle, DEBOUCE_DELAY_MS_FINGER);
			hand.third = Debounce(newHand.third, prevHand.third, ref debounceTimes.lastDebounceTimeThird, DEBOUCE_DELAY_MS_FINGER);
			hand.pinky = Debounce(newHand.pinky, prevHand.pinky, ref debounceTimes.lastDebounceTimePinky, DEBOUCE_DELAY_MS_FINGER);
		}
	}

	void checkIfCollision() {
		int cubeIntersection = 0;
		int sphere1Collide = 0;
		int layerMask = 1 << 8;
		layerMask = ~layerMask;

		Vector3 pos = m_pointer.transform.position;
		pos.y = pos.y - 2f;

		// Debug.DrawRay(m_pointer.transform.position, Vector3.Project(m_pointer.transform.forward, pos) * 1000, Color.red, 10);

		RaycastHit hitObj;
		bool hit = Physics.Raycast(m_pointer.transform.position, Vector3.Project(m_pointer.transform.forward, pos), out hitObj, 10f, layerMask);
		cubeIntersection = (hit && hitObj.transform.gameObject.name == "m_cube") ? 1 : 0;
		sphere1Collide = (hit && hitObj.transform.gameObject.name == "m_sphere_1") ? 1 : 0;

		cubeIsSelectedPrev = cubeIsSelected;
		cubeIsSelected = Debounce(cubeIntersection, cubeIsSelectedPrev, ref debounceTimes.lastDebounceTimeSelect, DEBOUCE_DELAY_MS_SELECT);

		if (cubeIsSelected != cubeIsSelectedPrev && hit) {
			hitObj.transform.gameObject.GetComponent<Renderer>().material.color = (toggle ? Color.white : Color.red);
			toggle = !toggle;
			ironMan.gameObject.GetComponent<Renderer>().enabled = toggle;
		}

		if (sphere1Collide == 1) {
			Vector3 end = new Vector3(sphere1.transform.position.x - UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-3, 6), 0);
        	float smoothTime = 0.3F;
    		Vector3 velocity = Vector3.zero;
			sphere1.transform.position = Vector3.SmoothDamp(sphere1.transform.position, end, ref velocity, smoothTime);
		}
	}

	// Debounces a stream of integer values
	int Debounce(int newVal, int prevVal, ref double lastDebounceTime, double maxDelay) {
		double now = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		if (newVal == prevVal) {
			lastDebounceTime = now;
		}
		if ((now - lastDebounceTime) > maxDelay) {
			return newVal;
		}
		return prevVal;
	}
}
