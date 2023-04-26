using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadGenerator))]
[CanEditMultipleObjects]
public class RoadHandle : Editor {
	protected virtual void OnSceneGUI() {
		RoadGenerator the = (RoadGenerator)target;
		
		if (the.nodes == null) return;
		if (the.nodes.Count <= 0) return;

		EditorGUI.BeginChangeCheck();
		RoadNode[] newNodes = the.nodes.ToArray();
		for (int i = 0; i < newNodes.Length; i++) {
			Handles.color = Color.white;
			newNodes[i].position = Handles.PositionHandle(newNodes[i].position + the.transform.position, Quaternion.identity) - the.transform.position;
			
			if (i < newNodes.Length - 1) {
				newNodes[i].anchor1 = Handles.PositionHandle(newNodes[i].anchor1 + the.transform.position, Quaternion.identity) - the.transform.position;
				newNodes[i].anchor2 = Handles.PositionHandle(newNodes[i].anchor2 + the.transform.position, Quaternion.identity) - the.transform.position;
				
				Handles.color = Color.cyan;
				Handles.DrawLine(newNodes[i].position + the.transform.position, newNodes[i].anchor1 + the.transform.position);
				Handles.DrawLine(newNodes[i].anchor2 + the.transform.position, newNodes[i + 1].position + the.transform.position);
				
				Handles.color = Color.yellow;
				Handles.DrawLine(the.transform.position + newNodes[i].position, newNodes[i + 1].position + the.transform.position);
			}
			
			Handles.color = Color.green;
			Handles.Label(newNodes[i].position + the.transform.position, i.ToString());
			
			if (i < newNodes.Length - 1) {
				Handles.Label(newNodes[i].anchor1 + the.transform.position, $"{i}a1");
				Handles.Label(newNodes[i].anchor2 + the.transform.position, $"{i}a2");
			}
		}
		bool anything = EditorGUI.EndChangeCheck();
		
		if (anything) {
			Undo.RecordObject(the, "Move Control Point");
			the.nodes = new List<RoadNode>(newNodes);
		}
		
		Vector3[] vertices = the.GetVertexArray();
		int iterations = Mathf.Max(1, Mathf.Min(16, the.splineIterationsPerKnot));
		var thing = Spline.CalculateSpline(the.splineType, vertices, iterations, ResultType.Position)
			.Zip(Spline.CalculateSpline(the.splineType, vertices, iterations, ResultType.Tangent), (a, b) => (a, b));
		
		(Vector3 lastPos, Vector3 lastTan) = thing.Take(1).First();
		foreach ((Vector3 pos, Vector3 tan) in thing.Skip(1)) {
			Handles.color = Color.white;
			Handles.DrawLine(the.transform.position + lastPos, the.transform.position + pos);
			// Handles.color = Color.cyan;
			// Handles.DrawLine(the.transform.position + pos, the.transform.position + pos + tan);
			
			// visualize local rotation
			Quaternion look = Quaternion.LookRotation(tan);
			
			Handles.color = Color.green;
			Handles.DrawLine(the.transform.position + pos, the.transform.position + pos + (look * Vector3.up));
			Handles.color = Color.red;
			Handles.DrawLine(the.transform.position + pos, the.transform.position + pos + (look * Vector3.left  * the.roadWidth / 2));
			Handles.DrawLine(the.transform.position + pos, the.transform.position + pos + (look * Vector3.right * the.roadWidth / 2));
			
			(lastPos, lastTan) = (pos, tan);
		}
		
	}
}
