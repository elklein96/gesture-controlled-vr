using System;

[System.Serializable]
public class Fingers {
    public Fingers(int thumb, int index, int second, int third, int pinky) {
        this.thumb = thumb;
        this.index = index;
        this.second = second;
        this.third = third;
        this.pinky = pinky;
    }

    public int thumb;
    public int index;
    public int second;
    public int third;
    public int pinky;

    public override bool Equals(Object eq) {
        Fingers obj = (Fingers) eq;
        return this.thumb == obj.thumb &&
            this.index == obj.index &&
            this.second == obj.second &&
            this.third == obj.third &&
            this.pinky == obj.pinky;
    }

    public string Gesture() {
        string gesture = "" + this.thumb + "" + this.index + "" + this.second + "" + this.third + "" + this.pinky;
        return gesture;
    }
}
