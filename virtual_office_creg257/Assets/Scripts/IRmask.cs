using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using GestureStream;

public class IRmask : MonoBehaviour {
	public SockerListener stream_t; //stream listener needs to be attatched 
	public GameObject m_stream;
	public GameObject m_pointer; //Any attachable object which will follow as a pointer
	public bool show_pointer;
	private GameObject m_mask; //This will be a quad which will follow the center of the camera
	private GameObject[] ir_mask;
	private Vector3 tran; 
	private static Message msg;

	Vector3 parse_message(Message data){
		return new Vector3(data.x, data.y , 0);
	}
	// Use this for initialization
	void Start () {
		msg = new Message(0,0);
		var height = (Camera.main.orthographicSize * 2.0)/10;
     	var width = (height * Screen.width / Screen.height);

		ir_mask = GameObject.FindGameObjectsWithTag("ir_mask"); //array of object belonging to IRmask
		foreach(GameObject obj in ir_mask){
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
			//Debug.Log(stream_t.getPoint().ToString());
		}
		else{
			Debug.Log("Error in IR_mask check for appropriete GameObjects in scene");
		}
		
		//m_stream = GameObject.Find("SockerListener");
		//tran = new Vector3(0,0,0.9f);
		//m_pointer.transform.position = tran;
		

	}
	
	// Update is called once per frame
	void Update () {
		msg = stream_t.getMessage();
		Vector3 temp = parse_message(msg);
		float x_temp = temp.x;
		float y_temp = temp.y;

		if( (Mathf.Abs(x_temp) < 1.5 ) && (Mathf.Abs(y_temp)< 2) ){
			m_pointer.GetComponent<Renderer>().enabled = true;
			m_pointer.transform.localPosition = new Vector3(x_temp,y_temp,0);
		} else {
			m_pointer.GetComponent<Renderer>().enabled = false;
		}
		
		if(msg.click || Input.GetMouseButtonDown(0)){
			int layerMask = 1 << 8;
			layerMask = ~layerMask;
			RaycastHit hit;
			if(Physics.Raycast(m_pointer.transform.position, Camera.main.transform.forward, out hit, 5f, layerMask)){
				Debug.DrawRay(m_pointer.transform.position, Camera.main.transform.forward,Color.red, 10);
				Debug.Log("HIT!");
				hit.transform.gameObject.GetComponent<Renderer>().material.color = Color.red;
				//hit.transform.position = m_pointer.transform.position;
			}else{
				Debug.Log("NOT HIT!");
			}
		}
		
		//Physics.Raycast(transform.position, transform.forward, hit, length);

		//Debug.Log("X("+x_temp+")Y("+y_temp+")");
		//m_mask.transform.position = new Vector3(0,0,0.9f);
	}

}
