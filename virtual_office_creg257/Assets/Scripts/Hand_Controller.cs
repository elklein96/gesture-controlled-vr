using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand_Controller : MonoBehaviour {
	private GameObject mask_pointer;
	public GameObject hands;
	public List<GameObject> hands_Obj = new List<GameObject>();
	public Mesh default_mesh;

	float smooth = 5.0f;
    float tiltAngle = 80.0f;
	// Use this for initialization
	void Start () {

		mask_pointer = GameObject.Find("m_pointer");
		this.transform.position = mask_pointer.transform.position;
		default_mesh = this.GetComponentInChildren<MeshFilter>().sharedMesh;
		GameObject obj = null;
     	int counter = 0;
     	bool done = false;
		hands = Resources.Load("hand_pos") as GameObject;

		while(!done)
		{
			// We just keep loading until obj becomes null
			obj = this.transform.Find("hand_"+counter).gameObject;
			if(obj == null){
				done = true; // Let's stop this now.
			}   
			else{
				obj.GetComponent<Renderer>().enabled = false;
				hands_Obj.Add(obj);
				Debug.Log("Found "+ obj.name);
			}
				
			++counter;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		float tiltAroundZ = Camera.main.transform.eulerAngles.x + tiltAngle;
        float tiltAroundY = Camera.main.transform.eulerAngles.y - tiltAngle - 30f;
		float tiltAroundX = 10f;
		Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, tiltAroundZ);
		this.transform.position = mask_pointer.transform.position;
		this.transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * smooth);

	}

	public void printStatus(){
		Debug.Log("Message from Hand");
	}

	public void update_hand(Fingers fin){
		//Mesh m = hands_Obj[5].GetComponent<MeshFilter>().sharedMesh;
		string gesture = fin.Gesture();
		Debug.Log("Gesture is: "+ gesture);
		switch(gesture){
			case "00000"://empty fist
				  	hands_Obj[0].GetComponent<Renderer>().enabled = true;
					hands_Obj[1].GetComponent<Renderer>().enabled = false;
					hands_Obj[2].GetComponent<Renderer>().enabled = false;
					hands_Obj[3].GetComponent<Renderer>().enabled = false;
					hands_Obj[4].GetComponent<Renderer>().enabled = false;
					hands_Obj[5].GetComponent<Renderer>().enabled = false;
					hands_Obj[6].GetComponent<Renderer>().enabled = false;

			break;
			case "10000"://thumb
					hands_Obj[0].GetComponent<Renderer>().enabled = false;
					hands_Obj[1].GetComponent<Renderer>().enabled = false;
					hands_Obj[2].GetComponent<Renderer>().enabled = false;
					hands_Obj[3].GetComponent<Renderer>().enabled = false;
					hands_Obj[4].GetComponent<Renderer>().enabled = false;
					hands_Obj[5].GetComponent<Renderer>().enabled = false;
					hands_Obj[6].GetComponent<Renderer>().enabled = true;
					
			break;
			case "01000"://index
					hands_Obj[0].GetComponent<Renderer>().enabled = false;
					hands_Obj[1].GetComponent<Renderer>().enabled = true;
					hands_Obj[2].GetComponent<Renderer>().enabled = false;
					hands_Obj[3].GetComponent<Renderer>().enabled = false;
					hands_Obj[4].GetComponent<Renderer>().enabled = false;
					hands_Obj[5].GetComponent<Renderer>().enabled = false;
					hands_Obj[6].GetComponent<Renderer>().enabled = false;
					
			break;
			case "01100"://two fingers no thumb
					hands_Obj[0].GetComponent<Renderer>().enabled = false;
					hands_Obj[1].GetComponent<Renderer>().enabled = false;
					hands_Obj[2].GetComponent<Renderer>().enabled = true;
					hands_Obj[3].GetComponent<Renderer>().enabled = false;
					hands_Obj[4].GetComponent<Renderer>().enabled = false;
					hands_Obj[5].GetComponent<Renderer>().enabled = false;
					hands_Obj[6].GetComponent<Renderer>().enabled = false;
					
			break;
			case "01110"://three fingers no thumb
					hands_Obj[0].GetComponent<Renderer>().enabled = false;
					hands_Obj[1].GetComponent<Renderer>().enabled = false;
					hands_Obj[2].GetComponent<Renderer>().enabled = false;
					hands_Obj[3].GetComponent<Renderer>().enabled = true;
					hands_Obj[4].GetComponent<Renderer>().enabled = false;
					hands_Obj[5].GetComponent<Renderer>().enabled = false;
					hands_Obj[6].GetComponent<Renderer>().enabled = false;
			break;
			case "01111"://four fingers no thumb
					hands_Obj[0].GetComponent<Renderer>().enabled = false;
					hands_Obj[1].GetComponent<Renderer>().enabled = false;
					hands_Obj[2].GetComponent<Renderer>().enabled = false;
					hands_Obj[3].GetComponent<Renderer>().enabled = false;
					hands_Obj[4].GetComponent<Renderer>().enabled = true;
					hands_Obj[5].GetComponent<Renderer>().enabled = false;
					hands_Obj[6].GetComponent<Renderer>().enabled = false;
			break;
			case "11111"://all fingers
					hands_Obj[0].GetComponent<Renderer>().enabled = false;
					hands_Obj[1].GetComponent<Renderer>().enabled = false;
					hands_Obj[2].GetComponent<Renderer>().enabled = false;
					hands_Obj[3].GetComponent<Renderer>().enabled = false;
					hands_Obj[4].GetComponent<Renderer>().enabled = false;
					hands_Obj[5].GetComponent<Renderer>().enabled = true;
					hands_Obj[6].GetComponent<Renderer>().enabled = false;
			break;
			default:
					hands_Obj[0].GetComponent<Renderer>().enabled = false;
					hands_Obj[1].GetComponent<Renderer>().enabled = false;
					hands_Obj[2].GetComponent<Renderer>().enabled = false;
					hands_Obj[3].GetComponent<Renderer>().enabled = false;
					hands_Obj[4].GetComponent<Renderer>().enabled = false;
					hands_Obj[5].GetComponent<Renderer>().enabled = true;
					hands_Obj[6].GetComponent<Renderer>().enabled = false;
			break;
		}
		
		//Debug.Log("Fingers Detected: THUMB("+hand.thumb+") INDEX("+hand.index+") SECOND("+hand.second+") THIRD("+hand.third+") PINKY ("+hand.pinky+")");	
	}
}

//Mesh swap
// m = hands_Obj[3].GetComponent<MeshFilter>().sharedMesh;
					// m.RecalculateNormals();
     				// m.RecalculateBounds();
				  	// this.GetComponentInChildren<MeshFilter>().mesh = m ;

   