using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using GestureStream;

public class IRmask : MonoBehaviour {
	public SockerListener stream_t; //stream listener needs to be attatched 
	public GameObject m_stream;
	public GameObject m_hand; //The Hand obj
	public GameObject m_pointer; //Point bound to the ir-interaction-plane
	private GameObject m_mask; //This will be a quad which will follow the center of the camera

	private GameObject ironMan;
	private GameObject sphere1;
	private GameObject sphere2;
	private GameObject sphere3;
	private GameObject toggleCube;
	
	private Fingers hand;
	private Fingers prevHand;

	private int cubeIsSelected;
	private int cubeIsSelectedPrev;
	bool toggle;

	double DEBOUCE_DELAY_MS_SELECT = 1000;
	double DEBOUCE_DELAY_MS_FINGER = 150;

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
		
		var height = (Camera.main.orthographicSize * 2.0)/10;
     	var width = (height * Screen.width / Screen.height);
		debounceTimes = new DebounceTimes();

		toggleCube = GameObject.Find("m_cube");
		sphere1 = GameObject.Find("m_sphere_1");
		sphere2 = GameObject.Find("m_sphere_2");
		sphere3 = GameObject.Find("m_sphere_3");
		ironMan = GameObject.Find("iron_man");
		ironMan.SetActive(false);
		
		prevHand = new Fingers(1, 1, 1, 1, 1);
		hand = new Fingers(1, 1, 1, 1, 1);

		GameObject[] ir_mask = GameObject.FindGameObjectsWithTag("ir_mask"); //array of object belonging to IRmask
		foreach(GameObject obj in ir_mask){
			switch(obj.name){
				case "m_mask":
					m_mask = obj;
					break;
				case "m_hand":
					m_hand = obj;//Grab the hand obj
					break;
				case "m_pointer":
					m_pointer = obj;//Grab the hand obj
					break;
				case "m_stream":
					m_stream = obj;
					break;
				default: 
					Debug.Log("No match found");
					break;
			}
				
		}
		if(m_mask != null && m_stream != null && m_hand!= null){
			m_mask.transform.localPosition = new Vector3(0,0,0.9f);
			m_mask.transform.localScale = new Vector3((float)width, (float)height, m_mask.transform.localScale.z);
			m_pointer.transform.localPosition = new Vector3(0, 0, 0);
			//m_hand.SendMessage("printStatus");
			stream_t = m_stream.GetComponent<SockerListener>();
			Debug.Log("Everything set");
		}
		else{
			Debug.Log("Error in IRmask check for appropriete GameObjects in scene");
		}
		
	}
	void Update () {
		Vector3 handLoc = stream_t.getPoint();
		float x = handLoc.x * 1.75f;
		float y = handLoc.y * 2.25f;
		
		if (x != 0 && y != 0) {
			parseHandGesture();
			collisionEvents();
			gestureEvents();

			if (toggle) {
				ironMan.gameObject.transform.rotation =
					Quaternion.Euler(y * 225, (x * 600) + 180, ironMan.gameObject.GetComponent<Renderer>().transform.rotation.z);
			} else {
				m_pointer.transform.localPosition = new Vector3(x, y, 0);
				m_hand.SendMessage("update_hand",hand); 
			}
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
	


	void collisionEvents() {
		RaycastHit hitObj;
		int layerMask = 1 << 8;
		layerMask = ~layerMask;

		Vector3 pos = m_pointer.transform.position;
		pos.y = pos.y - 2f;

		bool hit = Physics.Raycast(m_pointer.transform.position, Vector3.Project(m_pointer.transform.forward, pos), out hitObj, 10f, layerMask);
		Debug.DrawRay(m_pointer.transform.position, Vector3.Project(m_pointer.transform.forward, pos), Color.red, 1000);
		int cubeIntersection = (hit && hitObj.transform.gameObject.name == "m_cube") ? 1 : 0;
		int sphere1Collide = (hit && hitObj.transform.gameObject.name == "m_sphere_1") ? 1 : 0;
		int sphere2Collide = (hit && hitObj.transform.gameObject.name == "m_sphere_2") ? 1 : 0;
		int sphere3Collide = (hit && hitObj.transform.gameObject.name == "m_sphere_3") ? 1 : 0;

		cubeIsSelectedPrev = cubeIsSelected;
		cubeIsSelected = Debounce(cubeIntersection, cubeIsSelectedPrev, ref debounceTimes.lastDebounceTimeSelect, DEBOUCE_DELAY_MS_SELECT);

		if (cubeIsSelected != cubeIsSelectedPrev && !hand.Gesture().Equals("00000")) {
			updateIronManDemo();
		}

		if (sphere1Collide == 1) {
			Vector3 force = new Vector3(0, 0, sphere1.transform.position.z - UnityEngine.Random.Range(4, 8));
			sphere1.GetComponent<Rigidbody>().AddForce(force);
		}

		if (sphere2Collide == 1) {
			Vector3 force = new Vector3(0, 0, sphere2.transform.position.z - UnityEngine.Random.Range(-2, 2));
			sphere2.GetComponent<Rigidbody>().AddForce(force);
		}

		if (sphere3Collide == 1) {
			Vector3 force = new Vector3(0, 0, sphere3.transform.position.z - UnityEngine.Random.Range(-6, -2));
			sphere3.GetComponent<Rigidbody>().AddForce(force);
		}
	}

	void gestureEvents() {
		if (hand.Gesture().Equals("00000")) {
			if (toggle) {
				updateIronManDemo();
			}
			resetCollisionDemo();
		}

		if (hand.Gesture().Equals("01110")) {
			placeScreen();
		}
	}

	void updateIronManDemo() {
		toggleCube.GetComponent<Renderer>().material.color = (toggle ? Color.white : Color.red);
		toggle = !toggle;
		ironMan.SetActive(toggle);
	}

	void resetCollisionDemo() {
		sphere1.transform.position = new Vector3(5f, 2f, -1.5f);
		sphere2.transform.position = new Vector3(5f, 2f, 0f);
		sphere3.transform.position = new Vector3(5f, 2f, 1.5f);
	}

	void placeScreen() {
		Debug.Log("PLACING SCREEN");
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




// Update is called once per frame
	// void Update () {
	// 	//Initialize variables
	// 	Fingers prev_hand = hand;
	// 	Message msg = new Message(0,0); 
	// 	Vector3 temp = new Vector3(0f,0f,0f);

	// 	if(stream_t != null){
	// 		msg = stream_t.getMessage();
	// 		Fingers new_hand = msg.fingers;
	// 		temp = stream_t.getPoint();
	// 		hand = Debounce_Hand(new_hand, prev_hand);
	// 	}
	// 	m_pointer.transform.localPosition = new Vector3(msg.x,msg.y,0f);
	// 	Debug.Log("Moving Pointer to "+ msg.x + " " + msg.y );
	// 	m_hand.SendMessage("update_hand",hand); //update hand
	// 	//Debug.Log("Fingers Detected: THUMB("+fingers_msg.thumb+") INDEX("+fingers_msg.index+") SECOND("+fingers_msg.second+") THIRD("+fingers_msg.third+") PINKY ("+fingers_msg.pinky+")");
		
	// 	if( (msg.click || Input.GetMouseButtonDown(0)) && stream_t != null){
	// 		int layerMask = 1 << 8;
	// 		layerMask = ~layerMask;
	// 		RaycastHit hit;
	// 		if(Physics.Raycast(m_pointer.transform.position, Camera.main.transform.forward, out hit, 10f, layerMask)){
	// 			Debug.DrawRay(m_pointer.transform.position, Camera.main.transform.forward,Color.red, 10);
	// 			Debug.Log("HIT!");
	// 			hit.transform.gameObject.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
				
	// 			//hit.transform.position = m_pointer.transform.position;
	// 		}else{
	// 			Debug.Log("NOT HIT!");
	// 		}
	// 	}
		
		
	// }