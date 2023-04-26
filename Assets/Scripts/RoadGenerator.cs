using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[System.Serializable]
public struct RoadNode {
	public Vector3 position;
	public Vector3 anchor1, anchor2;
	public bool gap;
	// any node with the gap flag enabled has its anchors unused.
	// TODO: have spline function join end to start when end's .gap is false..
}

public class RoadGenerator : MonoBehaviour {
	public List<RoadNode> nodes = new List<RoadNode>();
	
	public SplineType splineType = SplineType.Bezier;
	
	// cheap way to be able to make "button" (call stuff from inspector)
	
	[ContextMenu("Make Spline C^1 Continuous")]
	void MakeC1Continuous() {
		// this literally just makes all the joins into "mirrored" joins
		// it just averages out the two you have after
		//
		// https://youtu.be/jvPPXbo87ds?t=1180
		RoadNode[] theNodes = nodes.ToArray();
		for (int i = 1; i < nodes.Count - 1; i++) {
			theNodes[i].anchor1 = 2f * theNodes[i].position - theNodes[i - 1].anchor2;
		}
		nodes = new List<RoadNode>(theNodes);
	}
	
	void Start() { GenerateRoad(); }
	
	void Update() {
		
	}
	
	public Vector3[] GetVertexArray() {
		int vertexCount = (nodes.Count - 1) * 3 + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		
		for (int i = 0; i < nodes.Count; i++) {
			vertices[i * 3] = nodes[i].position;
			if (i >= nodes.Count - 1) continue;
			
			vertices[i * 3 + 1] = nodes[i].anchor1;
			vertices[i * 3 + 2] = nodes[i].anchor2;
		}
		
		return vertices;
	}
	
	public void GenerateRoad() {
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		MeshCollider meshCollider = GetComponent<MeshCollider>();
		Mesh mesh = new Mesh();
		
		
		
		meshFilter.mesh = mesh;
		meshCollider.sharedMesh = mesh;
	}
}
