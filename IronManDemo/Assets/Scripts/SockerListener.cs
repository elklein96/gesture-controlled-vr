using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using WebSocketSharp;

namespace GestureStream {
	public class SockerListener : MonoBehaviour {
		private WebSocket ws;
		private GameObject cube;
		private Message tmp;
		private Message msg;
		private readonly String GAME_OBJ = "IronManDOT";

		void OnGUI() {
			GUILayout.Label("Started");
		}

		void Start () {
			cube = GameObject.Find(GAME_OBJ);
			msg = new Message(1f, 1f);

			ws = new WebSocket("ws://18.220.146.229:3001");
			ws.OnOpen += (o, e) => {
				Debug.Log("Connected");
			};
			ws.OnMessage += (sender, e) => {
				tmp = JsonUtility.FromJson<Message>(e.Data);
				if (tmp.x != 0 && tmp.y != 0) {
					msg.x = tmp.x * 1000;
					msg.y = tmp.y * -500;
				}
			};
			ws.Connect();
		}

		void Update () {
			cube.gameObject.transform.rotation = Quaternion.Euler(msg.y, msg.x, cube.gameObject.GetComponent<Renderer>().transform.rotation.z);
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