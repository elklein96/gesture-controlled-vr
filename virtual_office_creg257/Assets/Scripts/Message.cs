using System;

[System.Serializable]
public class Message {
    public Message(float x, float y) {
        this.x = x;
        this.y = y;
    }

    public float x;
    public float y;
    public Fingers fingers;
    public bool click;
}