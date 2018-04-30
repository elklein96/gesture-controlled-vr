 using UnityEngine;
 using System.Collections;
 using System;


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


