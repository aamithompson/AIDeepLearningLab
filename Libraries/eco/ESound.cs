//Environmental Sound
using UnityEngine;

public class ESound {
    public AnimationCurve volumeCurve;
    public float bVolume; //Base Volume
    private float cVolume; //Current Volume
    public float duration;
    private float time;
    public bool IsExpired {  get; private set; }

    public float GetSound() {
        return cVolume;
    }

    public void AddTime(float deltaTime) {
        if(IsExpired) { 
            return;
        }

        time += deltaTime;
        if(time >= duration) {
            IsExpired = true;
            cVolume = 0f;
            return;
        }

        float t = Mathf.Clamp01(time/duration);
        cVolume = bVolume * volumeCurve.Evaluate(t);
    }
}