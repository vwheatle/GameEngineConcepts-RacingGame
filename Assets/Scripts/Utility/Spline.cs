using System;
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
	private static readonly float[][] bezierMatrix = new float[][] {
		new float[] {  1f,  0f,  0f,  0f, },
		new float[] { -3f,  3f,  0f,  0f, },
		new float[] {  3f, -6f,  3f,  0f, },
		new float[] { -1f,  3f, -3f,  1f, },
	};
	// private static readonly float[][] catmullRomMatrix = new float[][] {
	// 	new float[] {  0/2f,  2/2f,  0/2f,  0/2f, },
	// 	new float[] { -1/2f,  3/2f,  1/2f,  0/2f, },
	// 	new float[] {  2/2f, -5/2f,  4/2f, -1/2f, },
	// 	new float[] { -1/2f,  3/2f, -3/2f,  1/2f, },
	// };
	private static readonly float[][] bSplineMatrix = new float[][] {
		new float[] {  1/6f,  4/6f,  1/6f,  0/6f, },
		new float[] { -3/6f,  0/6f,  3/6f,  0/6f, },
		new float[] {  3/6f, -6/6f,  3/6f,  0/6f, },
		new float[] { -1/6f,  3/6f, -3/6f,  1/6f, },
	};
	
	private static ref readonly float[][] GetCharacteristicMatrix(SplineType type) {
		switch (type) {
			case SplineType.Bezier: return ref bezierMatrix;
			// case SplineType.CatmullRom: return ref catmullRomMatrix;
			case SplineType.BSpline: return ref bSplineMatrix;
			default: // please follow the types, ok?
				throw new System.Exception("invalid splinetype");
		}
	}
	
	private static Vector4 GetTVector(ResultType type, float t) {
		return type switch {
			ResultType.Position => new Vector4(1f, t, t*t, t*t*t),
			ResultType.Tangent => new Vector4(0f, 1f, 2f*t, 3f*t*t),
			_ => Vector4.negativeInfinity,
		};
	}
	
	private static Vector3 MultiplyMatrix(Vector4 tVals, float[][] mat, Vector3[] pts) {
		return
			tVals[0] * ( mat[0][0]*pts[0] + mat[0][1]*pts[1] + mat[0][2]*pts[2] + mat[0][3]*pts[3] ) +
			tVals[1] * ( mat[1][0]*pts[0] + mat[1][1]*pts[1] + mat[1][2]*pts[2] + mat[1][3]*pts[3] ) +
			tVals[2] * ( mat[2][0]*pts[0] + mat[2][1]*pts[1] + mat[2][2]*pts[2] + mat[2][3]*pts[3] ) +
			tVals[3] * ( mat[3][0]*pts[0] + mat[3][1]*pts[1] + mat[3][2]*pts[2] + mat[3][3]*pts[3] );
	}
	
	public static IEnumerable<Vector3> GetPoints(
		Vector3[] points,
		int pointsPerKnot = 32,
		bool loop = false,
		SplineType type = SplineType.Bezier,
		ResultType resType = ResultType.Position
	) {
		if (!loop && points.Length % 3 != 1) throw new System.Exception("non-loop splines must omit control points past end!");
		if ( loop && points.Length % 3 != 0) throw new System.Exception("loop splines must have control points past end!");
		
		float[][] matrix = GetCharacteristicMatrix(type);
		
		int pointsLength3 = points.Length / 3;
		int startIteration = 0;
		if (loop) {
			// pointsLength3 should already be correct.
			// startIteration = 1; // redundant vertices :(
		}
		for (int p = 0; p < pointsLength3; p++) {
			int knotIndex = p * 3;
			for (int i = startIteration; i <= pointsPerKnot; i++) {
				float t = i / (float)pointsPerKnot;
				
				Vector4 tVals = GetTVector(resType, t);
				
				Vector3 nextVertex = MultiplyMatrix(
					tVals, matrix,
					new Vector3[4] {
						points[knotIndex + 0], points[knotIndex + 1],
						points[knotIndex + 2], points[(knotIndex + 3) % points.Length]
					}
				);
				
				// todo: gross
				if (resType == ResultType.Tangent) nextVertex.Normalize();
				
				yield return nextVertex;
			}
			startIteration = 1;
		}
	}
	
	public static Vector3 GetPoint(
		ref Vector3[] points, float u, bool loop = false,
		SplineType type = SplineType.Bezier,
		ResultType resType = ResultType.Position
	) {
		// we're given a u value, which...
		// well, look at the video i'm referencing.
		// https://youtu.be/jvPPXbo87ds?t=620
		// that big long line at the top is the u value's range
		// and you see the fractional part is the t value, and the
		// integer part selects which knot and yeah.
		
		// number of knots the points array represents.
		// each knot is represented by 3 points in the array,
		// one start point and two control points.
		// (the knot's end point is the next knot's start point)
		// (the last knot only stores a start point, and thus
		//  is not visible when integer-dividing by 3, so we
		//  have to round up by adding 1.)
		int totalKnots = (points.Length / 3) + 1;
		
		// which knot we're currently referring to.
		int knotUnclamped = Mathf.FloorToInt(u);
		
		// which knot we're currently referring to,
		// but clamped to the range of knots given in the points array.
		// anything outside will be extrapolated from the existing points.
		int knot = Mathf.Clamp(knotUnclamped, 0, totalKnots - 1);
		// helper to index into the points array
		int knotIndex = knot * 3;
		
		// the familiar t value. used for calculating
		// the bezier curve between two knots
		float t = u % 1f;
		// allow t value to go past start/end of knots
		//  (for fun!!)
		if (knotUnclamped > knot) t = u - (float)knot;
		if (knotUnclamped < knot) t = u;
		
		Vector4 tVals = GetTVector(resType, t);
		float[][] matrix = GetCharacteristicMatrix(type);
		
		Vector3[] curveVertices = new Vector3[4] {
			points[knotIndex + 0], points[knotIndex + 1],
			points[knotIndex + 2], points[(knotIndex + 3) % points.Length]
		};
		Vector3 nextVertex = MultiplyMatrix(tVals, matrix, curveVertices);
		
		// todo: gross
		if (resType == ResultType.Tangent) nextVertex.Normalize();
		
		return nextVertex;
	}
}
