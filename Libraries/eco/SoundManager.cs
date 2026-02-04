using NUnit.Framework;
using PlasticGui;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    private List<SoundEvent> sounds;
    public float updateDeltaTime = 0.25f;
    private float lastUpdateTime = 0;

    public void Awake() {
        sounds = new List<SoundEvent>();
        lastUpdateTime = Time.time;
        StartCoroutine(IESounds());
    }

    IEnumerator IESounds() {
        while(true) {
            lastUpdateTime = Time.time;
            for(int i = 0; i < sounds.Count; i++) {
                sounds[i].sound.AddTime(updateDeltaTime);
                if(sounds[i].sound.IsExpired) {
                    sounds.RemoveAt(i);
                    i--;
                }
            }

            yield return new WaitForSeconds(updateDeltaTime);
        }
    }

    public void AddSound(ESound sound, Vector3 position) {
        sounds.Add(new SoundEvent(sound, position));
    }

    //Possible optimizations:
    //1) utilizing pooling, i.e., instead of instantiating and destroying objects
    //   have a queue of already created objects to recycle.
    //2) Create "regions" of sound, check distance with those first, then check sounds
    //   in the region.
    public List<SoundEvent> GetSounds(Vector3 position, float maxDistance) {
        List<SoundEvent> result = new List<SoundEvent>();

        for(int i = 0; i < sounds.Count; i++) {
            Vector3 sPosition = sounds[i].position;
            float distance = (position - sPosition).sqrMagnitude;
            if(distance <= maxDistance * maxDistance) {
                result.Add(sounds[i]);
            }
        }

        return result;
    }
}

public class SoundEvent {
    public ESound sound;
    public Vector3 position;
    public SoundEvent(ESound sound, Vector3 position) {
        this.sound = sound;
        this.position = position;
    }
}
