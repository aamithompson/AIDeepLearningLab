//==============================================================================
// Filename: SpeciesManager.cs
// Author: Aaron Thompson
// Date Created: 2/19/2021
// Last Updated: 7/18/2022
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lmath;

public class SpeciesManager : MonoBehaviour {
	public List<Species> species;
	public List<GameObject> groups;

	public Dictionary<string, int> speciesDict;

    void Start() {
		Initialize();
    }

    public void Initialize() {
		if (groups != null) {
			ClearCreatures();
		}

		speciesDict = new Dictionary<string, int>();
		groups = new List<GameObject>();

		for(int i = 0; i < species.Count; i++) {
			//Names
			speciesDict.Add(species[i].name, i);

			//Group
			GameObject group = Instantiate(new GameObject());
			group.transform.position = transform.position;
			group.transform.parent = transform;
			groups.Add(group);
        }
    }

    public GameObject InstantiateCreature(int i) {
		int n = species[i].presets.Count;
		Vector values = Vector.Random(Vector.Zeros(n), Vector.Ones(n));
		values += new Vector(species[i].bias.ToArray());

		float sum = 0;
		for(int j = 0; j < n; j++) {
			sum += values[j];
        }
		values.Scale(1 / sum);

		GameObject gameObject = Instantiate(species[i].presets[0]);
		Renderer renderer = gameObject.GetComponent<Renderer>();
		Renderer presetRenderer = species[i].presets[0].GetComponent<Renderer>();
		Vector3 presetScale = species[i].presets[0].transform.localScale * values[0];
		Color presetColor = presetRenderer.sharedMaterial.color * values[0];
		gameObject.transform.localScale = presetScale;
		renderer.material.color = presetColor;
		for (int j = 1; j < n; j++) {
			presetRenderer = species[i].presets[j].GetComponent<Renderer>();
			presetScale = species[i].presets[j].transform.localScale * values[j];
			presetColor = presetRenderer.sharedMaterial.color * values[j];

			gameObject.transform.localScale = gameObject.transform.localScale + presetScale;
			renderer.material.color = renderer.material.color + presetColor;
		}

		gameObject.AddComponent<UAgentCore>();
		gameObject.transform.parent = groups[i].transform;
		gameObject.name = gameObject.name.Replace("(Clone)", "").Trim();

		return gameObject;
	}

	public GameObject InstantiateCreature(int i, Vector3 position) {
		GameObject gameObject = InstantiateCreature(i);

		gameObject.transform.position = position;

		return gameObject;
    }

	public void InstantiateCreatureGroup(int i, int n, Vector3 cornerA, Vector3 cornerB) {
		for(int j = 0; j < n; j++) {
			float x = Random.Range(cornerA.x, cornerB.x);
			float y = Random.Range(cornerA.y, cornerB.y);
			float z = Random.Range(cornerA.z, cornerB.z);
			Vector3 position = new Vector3(x, y, z);

			InstantiateCreature(i, position);
        }
	}

	public void InstantiateCreatureGroup(Vector n, Vector3 cornerA, Vector3 cornerB) {
		for(int i = 0; i < n.length; i++) {
			InstantiateCreatureGroup(i, (int)n[i], cornerA, cornerB);
        }
    }

	public void ClearCreatures() {
		foreach(Transform group in transform) {
			foreach(Transform child in group) {
				Destroy(child.gameObject);
			}
        }
    }

	[System.Serializable]
	public class Species{
		public string name;
		public List<GameObject> presets;
		public List<float> bias;
	}
}
//==============================================================================
//==============================================================================
