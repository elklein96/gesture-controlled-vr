using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using WebSocketSharp;


namespace GestureStream {
	public class SockerListener : MonoBehaviour {
		private WebSocket ws;
		private static Message m_packet;
	//{"x":"0.076467","y":"0.068478","z":"0.525632","fingers":{"thumb":"1","index":"1","second":"1","third":"1","pinky":"1"},"click":"false"}
	
	public Message getMessage(){
		return m_packet;
	}

		//AWS ID: 18.220.146.229:3000
		void OnGUI() {
			GUILayout.Label("Started");
		}
		
		public Fingers getFingers() {
			return m_packet.fingers;
		}

		public Vector3 getPoint(){
			return new Vector3(m_packet.x, m_packet.y, 0);
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

		void Update () {
			
		}

		void OnApplicationQuit() {
			ws.Close();
			Debug.Log("[DEBUG] Closing web socket... Cleaning up...");
		}
	}
}