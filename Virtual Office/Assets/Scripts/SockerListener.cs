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
					coord.setX((float) Convert.ToDouble(match.Groups["X"].Value) * 4);
					coord.setY((float) Convert.ToDouble(match.Groups["Y"].Value) * 4);
				}
			};
			ws.Connect();
		}

		void Update () {
			cube.gameObject.GetComponent<Renderer>().transform.position = new Vector3(coord.getX(), coord.getY(), 0f);
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