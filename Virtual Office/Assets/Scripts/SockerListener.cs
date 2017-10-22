using System.Collections;
using UnityEngine;
using WebSocketSharp;

namespace GestureStream {
	public class SockerListener : MonoBehaviour {
		WebSocket ws;

		void OnGUI() {
			GUILayout.Label("Started");
		}

		void Start () {
			ws = new WebSocket("ws://localhost:3001");
			ws.OnOpen += (o, e) => {
				Debug.Log("Open");
			};
			ws.OnMessage += (sender, e) => {
				Debug.Log(e.Data);
			};
			ws.Connect();
		}

		void Update () {
			
		}

		void OnApplicationQuit() {
			ws.Close();
			Debug.Log("Cleaning up...");
		}
	}
}