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
			return tri / (float)(knots.Count * splineIterationsPerKnot * 6);
		} else {
			return tri / (float)(((knots.Count - 1) * splineIterationsPerKnot * 6) - 1);
		}
	}
	
	public int AddRoadSegment(
		ref List<Vector3> meshVertices,
		ref List<Vector3> meshNormals,
		ref List<Vector3> meshUVs,
		Vector3 position,
		Quaternion rotation
	) {
		Vector3[] vertices = new Vector3[] {
			Vector3.left  * roadWidth / 2f,
			Vector3.right * roadWidth / 2f,
			Vector3.down  * roadHeight
		};
		foreach (Vector3 vertex in vertices)
			meshVertices.Add(position + rotation * vertex);
		// mid-refactoring, sorry!!
		
		return 0;
	}
	
	private static int[] RoadSegmentConnection(int prevIndex, int nextIndex) =>
		new int[] {
			// Top face
			prevIndex + 0, nextIndex + 0, prevIndex + 1,
			prevIndex + 1, nextIndex + 0, nextIndex + 1,
			
			// Left face
			prevIndex + 1, nextIndex + 1, prevIndex + 2,
			prevIndex + 2, nextIndex + 1, nextIndex + 2,
			
			// Right face
			prevIndex + 0, prevIndex + 2, nextIndex + 0,
			nextIndex + 0, prevIndex + 2, nextIndex + 2,
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
		
		var thing = Spline.GetPoints(vertices, splineIterationsPerKnot, loop, splineType, ResultType.Position)
			.Zip(Spline.GetPoints(vertices, splineIterationsPerKnot, loop, splineType, ResultType.Tangent), (a, b) => (a, b));
		foreach ((Vector3 pos, Vector3 tan) in thing) {
			if (meshVertices.Count > 0) {
				meshIndices.AddRange(
					RoadSegmentConnection(meshVertices.Count - 3, meshVertices.Count)
				);
			}
			
			// makes a quaternion such that
			// look * Vector3.forward = tan
			// and it'll always be up
			Quaternion look = Quaternion.LookRotation(tan, Vector3.up);
			
			Vector3[] segVertices = new Vector3[] {
				Vector3.left  * roadWidth / 2f,
				Vector3.right * roadWidth / 2f,
				Vector3.down  * roadHeight
			};
			
			foreach (Vector3 vert in segVertices)
				meshVertices.Add(pos + look * vert);
			
			Vector3 normal = look * Vector3.up;
			meshNormals.Add(Vector3.Slerp(normal, look * Vector3.left, 1/4f));
			meshNormals.Add(Vector3.Slerp(normal, look * Vector3.right, 1/4f));
			meshNormals.Add(-normal);
			
			float v = meshUVs.Count / 3f;
			float vv = (v / splineIterationsPerKnot) * 12f;
			meshUVs.Add(new Vector2(0f, vv));
			meshUVs.Add(new Vector2(1f, vv));
			meshUVs.Add(new Vector2(0.5f, vv));
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
		float u = ((progress % 1f) + 1f) % 1f * knots.Count;
		Vector3[] vertices = GetVertexArray();
		return (
			transform.position + Spline.GetPoint(ref vertices, u, loop, splineType, ResultType.Position),
			Spline.GetPoint(ref vertices, u, loop, splineType, ResultType.Tangent)
		);
	}
}
