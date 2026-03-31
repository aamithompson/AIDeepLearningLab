using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.UIElements;
using System.Data;
using adl;

public class AgentUICore : MonoBehaviour {
    public Canvas canvas;
    public string highlightLayer;
    private int hLNum; //highlight layer number
    public string defaultLayer;
    private int dLNum;
    public GameObject gameObjectMO; //mouse-over gameobject
    public GameObject Agent;
    private UAgentSensor AgentSensor;
    public GameObject AgentSightCamera;
    public RawImage AgentSightCameraRI;
    private UAgentMetabolism agentMetabolism;
    private UAgentCombat agentCombat;
    private UAgentCognition agentCognition;

    public TMP_Text healthText;
    public TMP_Text staminaText;
    public TMP_Text satiationText;
    public TMP_Text hydrationText;
    public TMP_Text restText;
    public float originImageSize;
    public List<GameObject> agentImages;
    public List<RawImage> agentImagesRI;
    public RadialWireGraphic visualGraphic;
    public RadialWireGraphic audioGraphic;
    public List<List<RawImage>> visualUIAgents; //List of agents in the visual radial grid.
    public float visualUIAgentSize = 4.0f;

    public LinearGraphGraphic networkGraphic;
    private FFNeuralNetwork network;

    void Start() {
        canvas = GetComponent<Canvas>();

        AgentSensor = Agent.GetComponent<UAgentSensor>();
        AgentSightCameraRI = AgentSightCamera.GetComponent<RawImage>();
        AgentSightCameraRI.texture = AgentSensor.tCam2DTex;
        hLNum = LayerMask.NameToLayer(highlightLayer);
        dLNum = LayerMask.NameToLayer(defaultLayer);
        agentMetabolism = Agent.GetComponent<UAgentMetabolism>();
        agentCombat = Agent.GetComponent<UAgentCombat>();
        agentCognition = Agent.GetComponent<UAgentCognition>();

        visualGraphic.columns = AgentSensor.columns;
        visualGraphic.rows = AgentSensor.rows;
        visualGraphic.leftAngle = AgentSensor.leftSightAngle;
        visualGraphic.rightAngle = AgentSensor.rightSightAngle;
        visualUIAgents = new List<List<RawImage>>();
        float maxRadius = Mathf.Min(visualGraphic.rectTransform.rect.width, visualGraphic.rectTransform.rect.height) * 0.5f;
        float minRadius = maxRadius * visualGraphic.startRadius;
        float startAngle = -AgentSensor.leftSightAngle + 90.0f;
        float endAngle = AgentSensor.rightSightAngle + 90.0f;
        for (int i = 0; i < AgentSensor.columns; i++) {
            visualUIAgents.Add(new List<RawImage>());
            for(int j = 0; j < AgentSensor.rows; j++) {
                GameObject gb = new GameObject("Visual Agent Representation");
                gb.transform.SetParent(visualGraphic.transform, false);

                RawImage ri = gb.AddComponent<RawImage>();
                RectTransform rt = ri.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                float tRadius = (float) ((j * 2) + 1)/(visualGraphic.rows * 2);
                float radius = Mathf.Lerp(minRadius, maxRadius, tRadius);
                float tAngle = (float) ((i * 2) + 1)/(visualGraphic.columns * 2);
                float angle = Mathf.Lerp(startAngle, endAngle, tAngle) * Mathf.Deg2Rad;
                float x = radius * Mathf.Cos(angle);
                float y = radius * Mathf.Sin(angle);
                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = new Vector2(visualUIAgentSize, visualUIAgentSize);

                gb.SetActive(false);
                visualUIAgents[i].Add(ri);
            }
        }

        audioGraphic.columns = AgentSensor.hearingSectors;
        audioGraphic.rows = 1;
        audioGraphic.sectort = new List<float>(new float[AgentSensor.hearingSectors]);
        for(int i = 0; i < agentImages.Count; i++) {
            agentImagesRI.Add(agentImages[i].GetComponent<RawImage>());
            originImageSize = agentImagesRI[i].rectTransform.sizeDelta.x;
            Renderer renderer = Agent.GetComponent<Renderer>();
            agentImagesRI[i].color = renderer.material.color;
            RectTransform rt = agentImagesRI[i].rectTransform;
            float rmax = MathF.Max(Agent.transform.localScale.x, Agent.transform.localScale.z);
            float rwidth = Agent.transform.localScale.x / rmax;
            float rheight = Agent.transform.localScale.z / rmax;
            Vector2 rv = new Vector2(rwidth, rheight);
            agentImagesRI[i].rectTransform.sizeDelta = rv * originImageSize;
        }

        network = agentCognition.GetNetwork();
        networkGraphic.SetLayerDepth(network.depth);
        Debug.Log($"Depth Set to {network.depth}");
        for(int i = 0; i < network.depth; i++) {
            networkGraphic.SetVertexCount(i, network.GetLayerWidth(i));
            Debug.Log($"Vertex count at layer {i} set to {network.GetLayerWidth(i)}");
        }

        for(int i = 0; i < network.depth - 1; i++) {
            int n = network.weights[i].GetShape()[0];
            int m = network.weights[i].GetShape()[1];
            float min = Mathf.Infinity;
            float max = Mathf.NegativeInfinity;
            for(int j = 0; j < n; j++) {
                for(int k = 0; k < m; k++) {
                    min = MathF.Min(min, network.weights[i].GetElement(j, k));
                    max = MathF.Max(max, network.weights[i].GetElement(j, k));
                }
            }

            float denom = max - min;
            if(denom != 0.0f) {
                for(int j = 0; j < n; j++) {
                    for(int k = 0; k < m; k++) {
                        float weight = (network.weights[i].GetElement(j, k) - min)/denom;
                        networkGraphic.SetLineWeight(i, j, k, weight);
                    }
                }
            } else {
                for(int j = 0; j < n; j++) {
                    for(int k = 0; k < m; k++) {
                        networkGraphic.SetLineWeight(i, j, k, 0.5f);
                    }
                }
            }
        }
    }


    void Update() {
        HighlightOver();
        HighlightSelect();
        UpdateUI();
    }

    private void HighlightOver() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            GameObject gameObject = hit.collider.gameObject;
            if (gameObject.GetComponent<UAgentCore>() != null && gameObject.layer == dLNum && gameObject != gameObjectMO && gameObject != Agent) {
                if(gameObjectMO != null && gameObjectMO != Agent) {
                    gameObjectMO.layer = dLNum;
                }

                gameObject.layer = hLNum;
                gameObjectMO = gameObject;
            }/* else {
                if (gameObjectMO != null) {
                    gameObjectMO.layer = dLNum;
                    gameObjectMO = null;
                }
            }*/
        }
    }

    private void HighlightSelect() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(ray, out hit)) {
                GameObject gameObject = hit.collider.gameObject;
                if(gameObject.GetComponent<UAgentCore>() != null && (gameObject.layer == dLNum || gameObject.layer == hLNum)) {
                    if (Agent != null) {
                        Agent.layer = dLNum;
                    }

                    gameObject.layer = hLNum;
                    Agent = gameObject;

                    AgentSensor = Agent.GetComponent<UAgentSensor>();
                    AgentSightCameraRI.texture = AgentSensor.tCam2DTex;
                    agentMetabolism = Agent.GetComponent<UAgentMetabolism>();
                    agentCombat = Agent.GetComponent<UAgentCombat>();
                    agentCognition = Agent.GetComponent<UAgentCognition>();

                    for(int i = 0; i < visualUIAgents.Count; i++) {
                        for(int j = 0; j < visualUIAgents.Count; j++) {
                            Destroy(visualUIAgents[i][j].gameObject);
                        }
                    }
                    visualGraphic.columns = AgentSensor.columns;
                    visualGraphic.rows = AgentSensor.rows;
                    visualGraphic.leftAngle = AgentSensor.leftSightAngle;
                    visualGraphic.rightAngle = AgentSensor.rightSightAngle;
                    visualUIAgents = new List<List<RawImage>>();
                    float maxRadius = Mathf.Min(visualGraphic.rectTransform.rect.width, visualGraphic.rectTransform.rect.height) * 0.5f;
                    float minRadius = maxRadius * visualGraphic.startRadius;
                    float startAngle = -AgentSensor.leftSightAngle + 90.0f;
                    float endAngle = AgentSensor.rightSightAngle + 90.0f;
                    for (int i = 0; i < AgentSensor.columns; i++) {
                        visualUIAgents.Add(new List<RawImage>());
                        for(int j = 0; j < AgentSensor.rows; j++) {
                            GameObject gb = new GameObject("Visual Agent Representation");
                            gb.transform.SetParent(visualGraphic.transform, false);

                            RawImage ri = gb.AddComponent<RawImage>();
                            RectTransform rt = ri.rectTransform;
                            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                            rt.pivot = new Vector2(0.5f, 0.5f);
                            float tRadius = (float) ((j * 2) + 1)/(visualGraphic.rows * 2);
                            float radius = Mathf.Lerp(minRadius, maxRadius, tRadius);
                            float tAngle = (float) ((i * 2) + 1)/(visualGraphic.columns * 2);
                            float angle = Mathf.Lerp(startAngle, endAngle, tAngle) * Mathf.Deg2Rad;
                            float x = radius * Mathf.Cos(angle);
                            float y = radius * Mathf.Sin(angle);
                            rt.anchoredPosition = new Vector2(x, y);
                            rt.sizeDelta = new Vector2(visualUIAgentSize, visualUIAgentSize);

                            gb.SetActive(false);
                            visualUIAgents[i].Add(ri);
                        }
                    }

                    audioGraphic.columns = AgentSensor.hearingSectors;
                    audioGraphic.rows = 1;
                    audioGraphic.sectort = new List<float>(new float[AgentSensor.hearingSectors]);
                    for (int i = 0; i < agentImages.Count; i++) {
                        agentImagesRI.Add(agentImages[i].GetComponent<RawImage>());
                        Renderer renderer = Agent.GetComponent<Renderer>();
                        agentImagesRI[i].color = renderer.material.color;
                        RectTransform rt = agentImagesRI[i].rectTransform;
                        float rmax = MathF.Max(Agent.transform.localScale.x, Agent.transform.localScale.z);
                        float rwidth = Agent.transform.localScale.x / rmax;
                        float rheight = Agent.transform.localScale.z / rmax;
                        Vector2 rv = new Vector2(rwidth, rheight);
                        agentImagesRI[i].rectTransform.sizeDelta = rv * originImageSize;
                    }
                }
            }/* else {
                if (Agent != null) {
                    Agent.layer = dLNum;
                    Agent = null;
                }
            }*/
        }
    }

    private bool IsAgent(GameObject gameObject) {
        return gameObject.GetComponent<UAgentCore>() != null;
    }

    private void UpdateUI(){
        if(Agent == null) {
            healthText.text = "N/A";
            staminaText.text = "N/A";
            satiationText.text = "N/A";
            hydrationText.text = "N/A";
            restText.text = "N/A";
        } else {
            healthText.text = agentCombat.health.ToString() + "/" + agentCombat.maxHealth.ToString();
            staminaText.text = agentMetabolism.stamina.ToString() + "/" + agentMetabolism.maxStamina.ToString() + "(" + agentMetabolism.staminaPerSecond.ToString() + " stamina/second)";
            satiationText.text = agentMetabolism.satiation.ToString() + "/" + agentMetabolism.maxSatiation.ToString() + "(" + agentMetabolism.hungerPerSecond.ToString() + " -satiation/second)";
            hydrationText.text = agentMetabolism.hydration.ToString() + "/" + agentMetabolism.maxHydration.ToString() + "(" + agentMetabolism.thirstPerSecond.ToString() + " -hydration/second)";
            restText.text = agentMetabolism.rest.ToString() + "/" + agentMetabolism.maxRest.ToString() + "(" + agentMetabolism.tirePerSecond.ToString() + " -rest/second)";

            for(int i = 0; i < AgentSensor.rows; i++) {
                for(int j = 0; j < AgentSensor.columns; j++) {
                    //3 is the first scale number
                    if (AgentSensor.sightData.GetElement(new int[] { i, j, 3}) != 0.0f) {
                        float r = AgentSensor.sightData.GetElement(new int[] { i, j, 0 });
                        float g = AgentSensor.sightData.GetElement(new int[] { i, j, 1 });
                        float b = AgentSensor.sightData.GetElement(new int[] { i, j, 2 });
                        float xscale = AgentSensor.sightData.GetElement(new int[] { i, j, 3 });
                        float yscale = AgentSensor.sightData.GetElement(new int[] { i, j, 4 });
                        float zscale = AgentSensor.sightData.GetElement(new int[] { i, j, 5 });

                        RawImage ri = visualUIAgents[i][j].GetComponent<RawImage>();
                        ri.color = new Color(r, g, b);
                        float rmax = MathF.Max(xscale, zscale);
                        float rwidth = xscale / rmax;
                        float rheight = zscale / rmax;
                        Vector2 rv = new Vector2(rwidth, rheight);
                        ri.rectTransform.sizeDelta  = rv * visualUIAgentSize;

                        visualUIAgents[i][j].gameObject.SetActive(true);
                    } else {
                        visualUIAgents[i][j].gameObject.SetActive(false);
                    }
                }
            }

            AgentSensor.tCam2DTex.Apply();

            for(int i = 0; i < AgentSensor.hearingSectors; i++) {
                audioGraphic.sectort[i] = Mathf.Clamp01(AgentSensor.hearingData[i]);
                audioGraphic.SetVerticesDirty();
            }
        }
    }
}
