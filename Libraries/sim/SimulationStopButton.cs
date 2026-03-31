using UnityEngine;

public class SimulationStopButton : MonoBehaviour {
    public SimulationManager simulationManager;

    public void Stop() {
        simulationManager.Stop();
    }
}
