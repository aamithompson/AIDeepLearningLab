//Environmental Sound
using UnityEngine;

[System.Serializable]
public class ESound {
    public AnimationCurve volumeCurve;
    public float bVolume; //Base Volume
    private float cVolume; //Current Volume
    public float duration;
    private float time = 0.0f;
    public bool IsExpired {  get; private set; }

    public ESound(ESound sound) {
        this.volumeCurve = sound.volumeCurve;
        this.bVolume = sound.bVolume;
        this.duration = sound.duration;

        this.cVolume = sound.cVolume;
        this.time = 0.0f;
        this.IsExpired = false;
    }

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