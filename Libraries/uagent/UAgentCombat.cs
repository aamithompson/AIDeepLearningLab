//==============================================================================
// Filename: UAgentCombat.cs
// Author: Aaron Thompson
// Date Created: 2/21/2022
// Last Updated: 5/4/2022
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
[RequireComponent(typeof(UAgentCore))]
public class UAgentCombat : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
    //AGENT COMPONENT(s)
	public UAgentCore uAgentCore;

    //HEALTH
    public float health;
    public float maxHealth;

    //ACTION STATE
    public int currentAction = -1;
    public int step = 0;
    public float time;
    public Vector3 basePosition;
    public Vector3 displacement;

    //ACTION SETTINGS
    public List<CActionSettings> cActions;
    public List<List<GameObject>> dColliders;

// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
    void Start() {
        UpdateColliders();
    }

    void FixedUpdate() {

    }

// FUNCTIONS
//------------------------------------------------------------------------------
    public void CombatMove(int i=-1) {
        if(i != -1 && currentAction != i) {
            step = 0;
            time = 0;
            currentAction = i;
        }

        if(currentAction >= 0) {
            if (time > cActions[currentAction].moveSteps[step].time) {
                step++;
                //basePosition = transform.position;
                time = 0;
                displacement = Vector3.zero;
                if (step > cActions[currentAction].moveSteps.Count - 1) {
                    step = 0;
                    currentAction = -1;
                    return;
                }
            }


            if (step < cActions[currentAction].moveSteps.Count) {
                float t = (cActions[currentAction].moveSteps[step].time <= 0) ?
                    1.0f :
                    time / cActions[currentAction].moveSteps[step].time;
                Vector3 offset = Vector3.Lerp(Vector3.zero, cActions[currentAction].moveSteps[step].movement, t);
                float xMagnitude = Vector3.Scale(offset, Vector3.right).magnitude;
                float zMagnitude = Vector3.Scale(offset, Vector3.forward).magnitude;
                offset = (transform.right * xMagnitude) + (transform.up * offset.y) + (transform.forward * zMagnitude);

                transform.position += offset - displacement;
                displacement = offset;
            }

            time += Time.fixedDeltaTime;
        }/* else {
            basePosition = transform.position;
        }*/
    }

    public void CombatMove(Vector3 direction, int i=-1) {
        direction = direction.normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        CombatMove(i);
    }
    
    public void RecieveDamage(float damage) {
        health -= damage;
        health = Mathf.Min(health, maxHealth);
    }

    public void UpdateColliders() {
        dColliders = new List<List<GameObject>>();

        for (int i = 0; i < cActions.Count; i++) {
            dColliders.Add(new List<GameObject>());
            for (int j = 0; j < cActions[i].dRanges.Count; j++) {
                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Transform bt = box.transform;
                bt.position = cActions[i].dRanges[j].collider.offset;
                bt.localScale = Vector3.Scale(new Vector3(1 / bt.lossyScale.x, 1 / bt.lossyScale.y, 1 / bt.lossyScale.z), cActions[i].dRanges[j].collider.size);

                dColliders[i].Add(box);
            }
        }
    }

    public bool inIFrame(int index, float time) {
        for(int i = 0; i < cActions[index].iRanges.Count; i++) {
            if(time > cActions[index].iRanges[i].a && time < cActions[index].iRanges[i].b) {
                return true;
            }
        }
        return false;
    }

    public bool inDFrame(int index, float time, GameObject gameObject) {
        Collider collider = gameObject.GetComponent<Collider>();
        for(int i = 0; i < cActions[index].dRanges.Count; i++) {
            if(time > cActions[index].dRanges[i].interval.a && time < cActions[index].dRanges[i].interval.b) {
                for(int j = 0; j < dColliders[i].Count; j++) {
                    Collider dCollider = dColliders[i][j].GetComponent<Collider>();
                    if (dCollider.bounds.Intersects(collider.bounds)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

}

// HELPER CLASSES/STRUCTS
//------------------------------------------------------------------------------
//CAction = Combat Action
[System.Serializable]
public struct CActionSettings {
    public List<MoveStep> moveSteps;
    public List<DInterval> dRanges;
    public List<SInterval> iRanges;
}

[System.Serializable]
public struct DamageCollider {
    public Vector3 offset;
    public Vector3 size;
}

[System.Serializable]
public struct MoveStep {
    public Vector3 movement;
    public float time;

    public MoveStep(Vector3 movement, float time) {
        this.movement = movement;
        this.time = time;
    }
}

//S = single = float
[System.Serializable]
public struct SInterval {
    public float a;
    public float b;

    public SInterval(float a, float b) {
        this.a = a;
        this.b = b;
    }
}

//D = damage
[System.Serializable]
public struct DInterval {
    public SInterval interval;
    public DamageCollider collider;
}
//==============================================================================
//==============================================================================