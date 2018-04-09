using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using WebSocketSharp;

namespace GestureStream {
	public class SockerListener : MonoBehaviour {
		private WebSocket ws;
		private GameObject cube;
		private Message msg;

		void OnGUI() {
			GUILayout.Label("Started");
		}

		void Start () {
			cube = GameObject.Find("Cube1");
			msg = new Message(0f, 0f);

			ws = new WebSocket("ws://18.220.146.229:3001");
			ws.OnOpen += (o, e) => {
				Debug.Log("Connected");
			};
			ws.OnMessage += (sender, e) => {
				msg = JsonUtility.FromJson<Message>(e.Data);

				msg.x = msg.x * 25;
				msg.y = msg.y * 25;

				double dateReturn = Math.Round((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) - 14400000;

				Debug.Log(msg.x + ", "  + msg.y + ", embedded: " + msg.time_embedded + ", unity: " + dateReturn);
			};
			ws.Connect();
		}

		void Update () {
			cube.gameObject.GetComponent<Renderer>().transform.position = new Vector3(msg.x, msg.y, 0f);
		}

		void OnApplicationQuit() {
			ws.Close();
			Debug.Log("Cleaning up...");
		}
	}

	public class Message {
		public Message(float x, float y) {
			this.x = x;
			this.y = y;
		}

		public float x;
		public float y;
		public Fingers fingers;
		public bool click;
		public String time_embedded;
	}

	public class Fingers {
		public bool thumb;
		public bool index;
		public bool second;
		public bool third;
		public bool pinky;
	}
}