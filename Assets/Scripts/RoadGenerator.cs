using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RoadKnot {
	public Vector3 position;
	public Vector3 anchor1, anchor2;
}

public class RoadGenerator : MonoBehaviour {
	public List<RoadKnot> knots = new List<RoadKnot>();
	
	public SplineType splineType = SplineType.Bezier;
	public int splineIterationsPerKnot = 32;
	
	public float roadWidth = 16f;
	public float roadHeight = 6f;
	
	public float wallHeight = 1.5f;
	
	public bool loop = false;
	
	private int meshLastSegmentIndex;
	
	[ContextMenu("Make Spline C^1 Continuous")]
	void MakeC1Continuous() {
		// this literally just makes all the joins into "mirrored" joins
		// https://youtu.be/jvPPXbo87ds?t=1180
		RoadKnot[] theNodes = knots.ToArray();
		for (int i = 1; i < knots.Count; i++) {
			theNodes[i].anchor1 = 2f * theNodes[i].position - theNodes[i - 1].anchor2;
		}
		if (loop) {
			theNodes[0].anchor1 = 2f * theNodes[0].position - theNodes[theNodes.Length - 1].anchor2;
		}
		knots = new List<RoadKnot>(theNodes);
	}
	
	[ContextMenu("Rotate Nodes")]
	void RotateNodes() {
		RoadKnot zero = knots[0];
		knots.RemoveAt(0);
		knots.Add(zero);
	}
	
	void Start() { GenerateRoad(); }
	
	public Vector3[] GetVertexArray() {
		int vertexCount = loop
			? (knots.Count * 3)
			: ((knots.Count - 1) * 3 + 1);
		Vector3[] vertices = new Vector3[vertexCount];
		
		for (int i = 0; i < knots.Count; i++) {
			vertices[i * 3] = knots[i].position;
			if (!loop && i >= knots.Count - 1) continue;
			
			vertices[i * 3 + 1] = knots[i].anchor1;
			vertices[i * 3 + 2] = knots[i].anchor2;
		}
		
		return vertices;
	}
	
	public float GetProgressFromTriangleIndex(int tri) {
		if (loop) {
			return tri / (float)(knots.Count * splineIterationsPerKnot * 10);
		} else {
			return tri / (float)(((knots.Count - 1) * splineIterationsPerKnot * 10) - 1);
		}
	}
	
	public int AddRoadSegment(
		ref List<Vector3> meshVertices,
		ref List<Vector3> meshNormals,
		ref List<Vector2> meshUVs,
		Vector3 position,
		Quaternion rotation
	) {
		int startIndex = meshVertices.Count;
		
		float roadWidth2 = roadWidth / 2f;
		Vector3[] vertices = new Vector3[] {
			Vector3.left  * roadWidth2,
			Vector3.right * roadWidth2,
			Vector3.down  * roadHeight,
			Vector3.left  * roadWidth2 + Vector3.up * wallHeight,
			Vector3.right * roadWidth2 + Vector3.up * wallHeight
		};
		foreach (Vector3 vertex in vertices)
			meshVertices.Add(position + rotation * vertex);
		// mid-refactoring, sorry!!
		
		return startIndex;
	}
	
	private static int[] RoadSegmentConnection(int prevIndex, int nextIndex) =>
		// These faces have to be ordered in a specific way because Unity uses
		// the winding order of the triangle to determine its normal.
		new int[] {
			// Top face
			prevIndex + 0, nextIndex + 0, prevIndex + 1, // a0 b0 a1
			prevIndex + 1, nextIndex + 0, nextIndex + 1, // a1 b0 b1
			
			// Left face
			prevIndex + 0, prevIndex + 2, nextIndex + 0, // a0 a2 b0
			nextIndex + 0, prevIndex + 2, nextIndex + 2, // b0 a2 b2
			
			// Right face
			prevIndex + 1, nextIndex + 1, prevIndex + 2, // a1 b1 a2
			prevIndex + 2, nextIndex + 1, nextIndex + 2, // a2 b1 b2
			
			// Left wall
			prevIndex + 3, nextIndex + 0, prevIndex + 0, // a3 b0 a0
			prevIndex + 3, nextIndex + 3, nextIndex + 0, // a3 b3 b0
			
			// Right wall
			prevIndex + 4, prevIndex + 1, nextIndex + 1, // a4 a1 b1
			nextIndex + 4, prevIndex + 4, nextIndex + 1, // b4 a4 b1
		};
	
	public void GenerateRoad() {
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		MeshCollider meshCollider = GetComponent<MeshCollider>();
		Mesh mesh = new Mesh();
		
		Vector3[] vertices = GetVertexArray();
		
		List<Vector3> meshVertices = new List<Vector3>();
		List<int> meshIndices = new List<int>();
		List<Vector3> meshNormals = new List<Vector3>();
		List<Vector2> meshUVs = new List<Vector2>();
		
		// { // Start cap
		// 	meshIndices.Add(0);
		// 	meshIndices.Add(1);
		// 	meshIndices.Add(2);
		// }
		
		int lastSegment = -1;
		var thing = Spline.GetPoints(vertices, splineIterationsPerKnot, loop, splineType, ResultType.Position)
			.Zip(Spline.GetPoints(vertices, splineIterationsPerKnot, loop, splineType, ResultType.Tangent), (a, b) => (a, b));
		foreach ((Vector3 pos, Vector3 tan) in thing) {
			if (lastSegment >= 0) {
				meshIndices.AddRange(
					RoadSegmentConnection(lastSegment, meshVertices.Count)
				);
			}
			
			// makes a quaternion such that
			// look * Vector3.forward = tan
			// and it'll always be up
			Quaternion look = Quaternion.LookRotation(tan, Vector3.up);
			
			lastSegment = AddRoadSegment(
				ref meshVertices,
				ref meshNormals,
				ref meshUVs,
				pos, look
			);
			
			Vector3 normal = look * Vector3.up;
			meshNormals.Add(Vector3.Slerp(normal, look * Vector3.left, 1/4f));
			meshNormals.Add(Vector3.Slerp(normal, look * Vector3.right, 1/4f));
			meshNormals.Add(-normal);
			meshNormals.Add(Vector3.Slerp(normal, look * Vector3.left, 1/4f));
			meshNormals.Add(Vector3.Slerp(normal, look * Vector3.right, 1/4f));
			
			float v = meshUVs.Count / 3f;
			float vv = (v / splineIterationsPerKnot) * 12f;
			meshUVs.Add(new Vector2(0f, vv));
			meshUVs.Add(new Vector2(1f, vv));
			meshUVs.Add(new Vector2(0.5f, vv));
			meshUVs.Add(new Vector2(0f, vv));
			meshUVs.Add(new Vector2(1f, vv));
		}
		
		// { // End cap
		// 	int startIndex = meshVertices.Count - 3;
		// 	meshIndices.Add(startIndex + 2);
		// 	meshIndices.Add(startIndex + 1);
		// 	meshIndices.Add(startIndex + 0);
		// }
		
		mesh.SetVertices(meshVertices);
		mesh.SetUVs(0, meshUVs);
		mesh.SetNormals(meshNormals);
		mesh.SetIndices(meshIndices, MeshTopology.Triangles, 0);
		
		meshFilter.mesh = mesh;
		meshCollider.sharedMesh = mesh;
	}
	
	// progress is from 0 to 1. I don't want you to have to think about knots.
	public (Vector3, Vector3) GetPositionTangentPairAt(float progress) {
		float u = (((progress % 1f) + 1f) % 1f) * knots.Count;
		Vector3[] vertices = GetVertexArray();
		return (
			transform.position + Spline.GetPoint(ref vertices, u, loop, splineType, ResultType.Position),
			Spline.GetPoint(ref vertices, u, loop, splineType, ResultType.Tangent)
		);
	}
}
