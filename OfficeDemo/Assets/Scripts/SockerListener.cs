using System;
using System.Collections;
using UnityEngine;
using WebSocketSharp;

namespace GestureStream {
	public class SockerListener : MonoBehaviour {
		private WebSocket ws;
		public Message m_packet;

		public Vector3 getPoint(){
			return new Vector3(m_packet.x, m_packet.y, 0);
		}

		public Fingers getFingers() {
			return m_packet.fingers;
		}

		public bool getClick() {
			return m_packet.click;
		}

		void OnGUI() {
			GUILayout.Label("Started");
		}

		void Start () {
			m_packet = new Message(0f, 0f);
			m_packet.fingers = new Fingers(1, 1, 1, 1, 1);

			ws = new WebSocket("ws://18.220.146.229:3001");
			ws.OnOpen += (o, e) => {
				Debug.Log("Connected");
			};
			ws.OnMessage += (sender, e) => {
				m_packet = JsonUtility.FromJson<Message>(e.Data);
			};
			ws.Connect();
		}

		void Update () {}

		void OnApplicationQuit() {
			ws.Close();
			Debug.Log("[DEBUG] Closing web socket... Cleaning up...");
		}
	}
}