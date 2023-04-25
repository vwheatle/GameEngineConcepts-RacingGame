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
	// note that the last node in a chain has its anchors unused.
}

public class RoadGenerator : MonoBehaviour {
	public List<RoadNode> nodes = new List<RoadNode>();
	
	public SplineType splineType = SplineType.Bezier;
	
	// cheap way to be able to make "button" (call stuff from inspector)
	
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
