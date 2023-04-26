using System.Collections.Generic;
using UnityEngine;

public enum SplineType {
	Bezier,
	// CatmullRom, // I forgot it was actually difficult to implement cuz you have to make ghost knots..
	//             // plus i'm not gonna use it anyway
	BSpline
}

public enum ResultType {
	Position,
	Tangent
}

public class Spline {
	// please index with [y][x]
	public static readonly float[][] bezierMatrix = new float[][] {
		new float[] {  1f,  0f,  0f,  0f, },
		new float[] { -3f,  3f,  0f,  0f, },
		new float[] {  3f, -6f,  3f,  0f, },
		new float[] { -1f,  3f, -3f,  1f, },
	};
	// public static readonly float[][] catmullRomMatrix = new float[][] {
	// 	new float[] {  0/2f,  2/2f,  0/2f,  0/2f, },
	// 	new float[] { -1/2f,  3/2f,  1/2f,  0/2f, },
	// 	new float[] {  2/2f, -5/2f,  4/2f, -1/2f, },
	// 	new float[] { -1/2f,  3/2f, -3/2f,  1/2f, },
	// };
	public static readonly float[][] bSplineMatrix = new float[][] {
		new float[] {  1/6f,  4/6f,  1/6f,  0/6f, },
		new float[] { -3/6f,  0/6f,  3/6f,  0/6f, },
		new float[] {  3/6f, -6/6f,  3/6f,  0/6f, },
		new float[] { -1/6f,  3/6f, -3/6f,  1/6f, },
	};
	
	public static ref readonly float[][] GetCharacteristicMatrix(SplineType type) {
		switch (type) {
			case SplineType.Bezier: return ref bezierMatrix;
			// case SplineType.CatmullRom: return ref catmullRomMatrix;
			case SplineType.BSpline: return ref bSplineMatrix;
			default: // please follow the types, ok?
				throw new System.Exception("invalid splinetype");
		}
	}
	
	public static Vector4 GetTVector(ResultType type, float t) {
		return type switch {
			ResultType.Position => new Vector4(1f, t, t*t, t*t*t),
			ResultType.Tangent => new Vector4(0f, 1f, 2f*t, 3f*t*t),
			_ => Vector4.negativeInfinity,
		};
	}
	
	public static Vector3 MultiplyMatrix(Vector4 tVals, float[][] mat, Vector3[] pts) {
		return
			tVals[0] * ( mat[0][0]*pts[0] + mat[0][1]*pts[1] + mat[0][2]*pts[2] + mat[0][3]*pts[3] ) +
			tVals[1] * ( mat[1][0]*pts[0] + mat[1][1]*pts[1] + mat[1][2]*pts[2] + mat[1][3]*pts[3] ) +
			tVals[2] * ( mat[2][0]*pts[0] + mat[2][1]*pts[1] + mat[2][2]*pts[2] + mat[2][3]*pts[3] ) +
			tVals[3] * ( mat[3][0]*pts[0] + mat[3][1]*pts[1] + mat[3][2]*pts[2] + mat[3][3]*pts[3] );
	}
	
	public static IEnumerable<Vector3> CalculateSpline(
		SplineType type, Vector3[] points,
		int iterationsPerKnot = 32,
		ResultType resType = ResultType.Position
	) {
		// control points must be joined end-to end
		if (points.Length % 3 != 1) throw new System.Exception("points not valid!!");
		
		float[][] matrix = GetCharacteristicMatrix(type);
		
		int pointsLength3 = points.Length / 3;
		int startIteration = 0;
		for (int p = 0; p < pointsLength3; p++) {
			int knotIndex = p * 3;
			for (int i = startIteration; i <= iterationsPerKnot; i++) {
				float t = i / (float)iterationsPerKnot;
				
				Vector4 tVals = GetTVector(resType, t);
				
				Vector3 nextVertex = MultiplyMatrix(
					tVals, matrix,
					new Vector3[4] {
						points[knotIndex + 0], points[knotIndex + 1],
						points[knotIndex + 2], points[knotIndex + 3]
					}
				);
				
				// todo: gross
				if (resType == ResultType.Tangent) nextVertex.Normalize();
				
				yield return nextVertex;
			}
			startIteration = 1;
		}
	}
}
