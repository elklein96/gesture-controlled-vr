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
}
