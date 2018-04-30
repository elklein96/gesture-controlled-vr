using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using WebSocketSharp;


namespace GestureStream {
	public class SockerListener : MonoBehaviour {
		private WebSocket ws;
		private static Message msg;
	//{"x":"0.076467","y":"0.068478","z":"0.525632","fingers":{"thumb":"1","index":"1","second":"1","third":"1","pinky":"1"},"click":"false"}
	
	public Message getMessage(){
		return msg;
	}

		//AWS ID: 18.220.146.229:3000
		void OnGUI() {
			GUILayout.Label("Started");
		}

		void Start () {
			msg = new Message(0f, 0f);
			ws = new WebSocket("ws://18.220.146.229:3001");//"18.220.146.229:3001" "ws://localhost:3001" :3000 is REST API
			ws.OnOpen += (o, e) => {
				Debug.Log("Connected");
			};
			ws.OnMessage += (sender, e) => {
				msg = JsonUtility.FromJson<Message>(e.Data);

				//msg.x = msg.x *25;
				//msg.y = msg.y * 25;

				double dateReturn = Math.Round((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) - 14400000;

				Debug.Log(msg.x + ", "  + msg.y + ", embedded: " + msg.time_embedded + ", unity: " + dateReturn);
			};
			ws.Connect();
		}

		void Update () {
			//this.transform.position = new Vector3(coord.getX()*8, coord.getY()*8, 0f);
			//cube.gameObject.GetComponent<Renderer>().enabled = showCube;
		}

		void OnApplicationQuit() {
			ws.Close();
			Debug.Log("[DEBUG] Closing web socket... Cleaning up...");
		}
	}
}