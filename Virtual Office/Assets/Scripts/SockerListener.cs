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

			ws = new WebSocket("ws://localhost:3001");
			ws.OnOpen += (o, e) => {
				Debug.Log("Connected");
			};
			ws.OnMessage += (sender, e) => {
				Debug.Log(e.Data);
				var results = Regex.Matches(e.Data, "\\((?<X>\\d+),\\s+(?<Y>\\d+)");
				for (int i = 0; i < results.Count; i++) {
					coord.setX((float) Convert.ToDouble(results[i].Groups["X"]));
					coord.setY((float) Convert.ToDouble(results[i].Groups["Y"]));
				}
			};
			ws.Connect();
		}

		void Update () {
			cube.transform.position += new Vector3(coord.getX(), coord.getY(), 0f);
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

		public float getX() {
			return this.x;
		}

		public float getY() {
			return this.y;
		}

		public void setX(float x) {
			this.x = x;
		}

		public void setY(float y) {
			this.y = y;
		}
	}
}