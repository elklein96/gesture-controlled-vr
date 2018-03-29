using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using WebSocketSharp;

namespace GestureStream {
	public class SockerListener : MonoBehaviour {
		private WebSocket ws;
		private GameObject cube;
		private Coordinate coord;

		void OnGUI() {
			GUILayout.Label("Started");
		}

		void Start () {
			cube = GameObject.Find("Cube1");
			coord = new Coordinate(0f, 0f);

			ws = new WebSocket("ws://18.220.146.229:3001");
			ws.OnOpen += (o, e) => {
				Debug.Log("Connected");
			};
			ws.OnMessage += (sender, e) => {
				foreach (Match match in Regex.Matches(e.Data, "(?<X>\\d+.\\d+),\\s+(?<Y>\\d+.\\d+)")) {
					// Scale the coordinates by a factor of 4 to see more pronounced movement by the Game Object
					coord.SetX((float) Convert.ToDouble(match.Groups["X"].Value) * 20);
					coord.SetY((float) Convert.ToDouble(match.Groups["Y"].Value) * 20);
				}
			};
			ws.Connect();
		}

		void Update () {
			cube.gameObject.GetComponent<Renderer>().transform.position = new Vector3(coord.GetX(), coord.GetY(), 0f);
		}

		void OnApplicationQuit() {
			ws.Close();
			Debug.Log("Cleaning up...");
		}
	}

	public class Coordinate {
		private float x;
		private float y;

		public Coordinate(float x, float y) {
			this.x = x;
			this.y = y;
		}

		public float GetX() {
			return this.x;
		}

		public float GetY() {
			return this.y;
		}

		public void SetX(float x) {
			this.x = x;
		}

		public void SetY(float y) {
			this.y = y;
		}
	}
}