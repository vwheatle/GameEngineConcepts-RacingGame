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
	public int splineIterationsPerKnot = 32;
	
	public float roadWidth = 16f;
	public float roadHeight = 6f;
	
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
		
		Vector3[] vertices = GetVertexArray();
		
		List<Vector3> meshVertices = new List<Vector3>();
		List<int> meshIndices = new List<int>();
		List<Vector3> meshNormals = new List<Vector3>();
		List<Vector2> meshUVs = new List<Vector2>();
		
		meshIndices.Add(0);
		meshIndices.Add(1);
		meshIndices.Add(2);
		
		var thing = Spline.CalculateSpline(splineType, vertices, splineIterationsPerKnot, ResultType.Position)
			.Zip(Spline.CalculateSpline(splineType, vertices, splineIterationsPerKnot, ResultType.Tangent), (a, b) => (a, b));
		foreach ((Vector3 pos, Vector3 tan) in thing) {
			int startIndex = meshVertices.Count - 3;
			if (meshVertices.Count > 0) {
				int[][] triangles = new int[][] {
					new int[] { 0, 3, 1 }, // Top face
					new int[] { 1, 3, 4 },
					new int[] { 1, 4, 2 }, // Left face
					new int[] { 2, 4, 5 },
					new int[] { 0, 2, 3 }, // Right face
					new int[] { 3, 2, 5 },
				};
				
				foreach (int[] tri in triangles)
					foreach (int i in tri)
						meshIndices.Add(startIndex + i);
			}
			
			// makes a quaternion such that
			// look * Vector3.forward = tan
			// and it'll always be up
			Quaternion look = Quaternion.LookRotation(tan);
			
			meshVertices.Add(pos + (look * Vector3.left * roadWidth / 2));
			meshVertices.Add(pos + (look * Vector3.right * roadWidth / 2));
			meshVertices.Add(pos + (look * Vector3.down * roadHeight));
			
			Vector3 normal = look * Vector3.up;
			meshNormals.Add(Vector3.Slerp(normal, look * Vector3.left, 0.1f));
			meshNormals.Add(Vector3.Slerp(normal, look * Vector3.right, 0.1f));
			meshNormals.Add(-normal);
			
			float v = (meshUVs.Count / 3) % 2;
			meshUVs.Add(new Vector2(0f, v));
			meshUVs.Add(new Vector2(1f, v));
			meshUVs.Add(new Vector2(0.5f, v));
		}
		
		{
			int startIndex = meshVertices.Count - 3;
			meshIndices.Add(startIndex + 2);
			meshIndices.Add(startIndex + 1);
			meshIndices.Add(startIndex + 0);
		}
		
		// foreach (var v in meshVertices) Debug.Log(v);
		// foreach (var v in meshIndices) Debug.Log(v);
		
		mesh.SetVertices(meshVertices);
		mesh.SetUVs(0, meshUVs);
		mesh.SetNormals(meshNormals);
		// mesh.RecalculateNormals();
		mesh.SetIndices(meshIndices, MeshTopology.Triangles, 0);
		
		meshFilter.mesh = mesh;
		meshCollider.sharedMesh = mesh;
	}
}
