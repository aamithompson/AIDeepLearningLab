using UnityEngine;

public class SimulationPlayButton : MonoBehaviour {
    public SimulationManager simulationManager;

    public void Resume() {
        simulationManager.Resume();
    }
}
