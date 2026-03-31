using UnityEngine;

public class SimulationPauseButton : MonoBehaviour {
    public SimulationManager simulationManager;

    public void Pause() {
        simulationManager.Pause();
    }
}
