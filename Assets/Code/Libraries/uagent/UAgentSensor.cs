//==============================================================================
// Filename: UAgentSensor.cs
// Author: Aaron Thompson
// Date Created: 8/10/2020
// Last Updated: 1/17/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lmath;
//------------------------------------------------------------------------------
public class UAgentSensor : MonoBehaviour {
// VARIABLES
//------------------------------------------------------------------------------
	//AGENT COMPONENT(s)
	public UAgentCore uagentCore;

	//DATA
	public static int sightVectorSize = 6;
	public Tensor<float> sightData;

	//SIGHT SETTING(s)
	public bool hasSight = true;
	public float minSightRadius = 0.0f;
	public float maxSightRadius = 1.0f;
	[Range(0.0f, 360.0f)]
	public float offsetSightAngle = 0.0f;
	[Range(0.0f, 180.0f)]
	public float leftSightAngle = 45.0f;
	public float oLSAngle { get { return leftSightAngle - offsetSightAngle; } }
	[Range(0.0f, 180.0f)]
	public float rightSightAngle = 45.0f;
	public float oRSAngle { get { return rightSightAngle + offsetSightAngle; } }
	public int columns = 1;
	public int rows = 1;
	[Range(0.001f, 2.0f)]
	public float sightUpdateDeltaTime = 0.25f;

	//AUDITORY SETTING(s)
	public bool hasHearing = true;

	//DEBUG SETTING(s)
	public bool debugEnabled = false;
	public int debugDrawSightDiv = 16;
	public Color debugDrawSightColor = Color.white;


// MONOBEHAVIOR METHODS
//------------------------------------------------------------------------------
	void Awake (){
		sightData = Tensor<float>.Zeros(new int[] { rows, columns, sightVectorSize });
	}

	void Start (){
		StartCoroutine(IESight());
	}
	
	void Update (){
		if (hasSight) {
			if(debugEnabled){
				DebugDrawSight();
			}
		}

		if (hasHearing) {

		}
	}

// SIGHT
//------------------------------------------------------------------------------
	private void UpdateSightData() {
		if(sightData.GetShape()[0] != rows || sightData.GetShape()[1] != columns) {
			sightData = Tensor<float>.Zeros(new int[] { rows, columns, sightVectorSize });
		}
	}

	IEnumerator IESight() {
		while(true) {
			Vector<int> point = Vector<int>.Zeros(2);
			Matrix<int> count = Matrix<int>.Zeros(rows, columns);

			UpdateSightData();
			sightData.Fill(0.0f);

			for(int i = 0; i < uagentCore.uagentManager.agents.Count; i++) {
				GameObject gb = uagentCore.uagentManager.agents[i];
				if(gb == null) {
					continue;
                }

				point = GetSightPoint(gb);
				if (point != null) {
					Renderer renderer = gb.GetComponent<Renderer>();
					count[point[0], point[1]] += 1;

					if (renderer != null) {
						for (int j = 0; j < 3; j++) {
							float value = sightData.GetElement(new int[] { point[0], point[1], j });
							value += renderer.material.color[j];
							sightData.SetElement(value, new int[] { point[0], point[1], j });
						}

						for (int j = 3; j < 6; j++) {
							float value = sightData.GetElement(new int[] { point[0], point[1], j });
							value += gb.transform.localScale[j - 3];
							sightData.SetElement(value, new int[] { point[0], point[1], j });
						}
					}

				}
			}

			for(int i = 0; i < rows; i++) {
				for(int j = 0; j < columns; j++) {
					if(count[i, j] > 1) {
						for(int k = 0; k < sightVectorSize; k++) {
							float value = sightData.GetElement(new int[] { i, j, k });
							value /= count[i, j];
							sightData.SetElement(value, new int[] { i, j, k });
						}
					}
				}
			}

			yield return new WaitForSeconds(sightUpdateDeltaTime);
		}
	}

	private Vector<int> GetSightPoint(GameObject gb) {
		Vector2 origin = new Vector2(transform.position.x, transform.position.z);
		Vector2 foward = new Vector2(transform.forward.x, transform.forward.z);
		Vector2 target = new Vector2(gb.transform.position.x, gb.transform.position.z);

		float distance = Vector2.Distance(origin, target);
		float angle = Vector2.SignedAngle(foward.normalized, (target - origin).normalized) * -1;

		if (((distance < maxSightRadius) && (distance > minSightRadius)) && ((angle > oLSAngle * -1) && (angle < oRSAngle))) {
			int column = Mathf.FloorToInt(columns * ((angle + oLSAngle) / (oLSAngle + oRSAngle)));
			int row = Mathf.FloorToInt(rows * ((distance - minSightRadius) / (maxSightRadius - minSightRadius)));
			row = Clamp<int>(row, 0, rows);
			column = Clamp<int>(column, 0, columns);
			return new Vector<int>(new int[] { row, column });
		}
		
		return null;
	}

	private void DebugDrawSight(){
		Vector2 position2D = new Vector2(transform.position.x, transform.position.z);
		Vector2 forward2D = new Vector2(transform.forward.x, transform.forward.z).normalized;
		float angle = -1 * (oRSAngle);
		float angleAdjust = (rightSightAngle + leftSightAngle) / debugDrawSightDiv;
		float tColumn = 0.0f;
		float tColumnAdjust = 1.0f / columns;
		float tRow = 0.0f;
		float tRowAdjust = 1.0f / rows;


		//Radius Line(s)
		Vector2 minForward2D = (forward2D * minSightRadius);
		Vector2 maxForward2D = (forward2D * maxSightRadius);

		//Left Radius Line
		Vector2 lA = position2D + Rotate(minForward2D, oLSAngle);
		Vector2 lB = position2D + Rotate(maxForward2D, oLSAngle);
		Vector3 lStart = new Vector3(lA.x, transform.position.y, lA.y);
		Vector3 lEnd = new Vector3(lB.x, transform.position.y, lB.y);
		Debug.DrawLine(lStart, lEnd, debugDrawSightColor);

		//Right Radius Line
		Vector2 rA = position2D + Rotate(minForward2D, -1 * (oRSAngle));
		Vector2 rB = position2D + Rotate(maxForward2D, - 1 * (oRSAngle));
		Vector3 rStart = new Vector3(rA.x, transform.position.y, rA.y);
		Vector3 rEnd = new Vector3(rB.x, transform.position.y, rB.y);
		Debug.DrawLine(rStart, rEnd, debugDrawSightColor);

		//Inbetween Radius Line(s)
		for (int i = 0; i < columns - 1; i++) {
			tColumn += tColumnAdjust;
			float tAngle = Mathf.Lerp(oLSAngle, -1 * (oRSAngle), tColumn);
			Vector2 columnA = position2D + Rotate(minForward2D, tAngle);
			Vector2 columnB = position2D + Rotate(maxForward2D, tAngle);
			Vector3 columnStart = new Vector3(columnA.x, transform.position.y, columnA.y);
			Vector3 columnEnd = new Vector3(columnB.x, transform.position.y, columnB.y);
			Debug.DrawLine(columnStart, columnEnd, debugDrawSightColor);
		}

		//Circumference Line(s)
		for (int i = 0; i < debugDrawSightDiv; i++) {
			//Inner Line
			Vector2 iA = position2D + Rotate(minForward2D, angle);
			Vector2 iB = position2D + Rotate(minForward2D, angle + angleAdjust);
			Vector3 iStart = new Vector3(iA.x, transform.position.y, iA.y);
			Vector3 iEnd = new Vector3(iB.x, transform.position.y, iB.y);
			Debug.DrawLine(iStart, iEnd, debugDrawSightColor);

			//Outer Line
			Vector2 oA = position2D + Rotate(maxForward2D, angle);
			Vector2 oB = position2D + Rotate(maxForward2D, angle + angleAdjust);
			Vector3 oStart = new Vector3(oA.x, transform.position.y, oA.y);
			Vector3 oEnd = new Vector3(oB.x, transform.position.y, oB.y);
			Debug.DrawLine(oStart, oEnd, debugDrawSightColor);

			for(int j = 0; j < rows - 1; j++) {
				tRow += tRowAdjust;
				Vector3 rowStart = Vector3.Lerp(iStart, oStart, tRow);
				Vector3 rowEnd = Vector3.Lerp(iEnd, oEnd, tRow);
				Debug.DrawLine(rowStart, rowEnd, debugDrawSightColor);
			}

			angle += angleAdjust;
			tRow = 0.0f;
		}
	}
// AUDITORY
//------------------------------------------------------------------------------

// HELPER FUNCTION(s)
//------------------------------------------------------------------------------
	private static Vector2 Rotate(Vector2 v, float degrees){
		float radians = degrees * Mathf.Deg2Rad;
		float sin = Mathf.Sin(radians);
		float cos = Mathf.Cos(radians);
		return new Vector2((cos * v.x) - (sin * v.y), (sin * v.x) + (cos * v.y));
	}

	private static T Clamp<T>(T e, T min, T max) where T : System.IComparable<T> {
		if (e.CompareTo(min) < 0) {
			return min;
		} else if (e.CompareTo(max) > 0) {
			return max;
		} else {
			return e;
		}
	}
}
//==============================================================================
//==============================================================================
