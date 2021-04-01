using UnityEngine;

public static class GridMetrics
{
	public const float elevationStep = 5f;
	public const float outerRadius = 5f;
	public const float innerRadius = outerRadius * 0.866025404f;
	public const float edgeLength = 2f * innerRadius;
	public const float triHeight = outerRadius * 1.5f;
	public const float SQRT_3 = 1.73205080757f;
	public const int chunkSizeX = 5, chunkSizeZ = 5;
	// colors correspond to TriType enum
	public static Color[] colors = {
		Color.white,  // flat
		Color.cyan,   // cliff
		Color.green,  // slope 1: no traversal cost
		Color.yellow, // slope 2: incurs traversal cost
		Color.red,    // slope 3: requires traversal method
	};

	// Elevation difference thresholds for defining slope types (aka colors). 
	// slope 1: diff < 2
	// slope 2: 2 < diff < 5
	// slope 3: diff > 6
	public static int[] slopeThresholds = {
		2, 5
	};

	private static Vector3[] corners = {
		// NE
		new Vector3(innerRadius, 0f, triHeight),
		// E
		new Vector3(edgeLength, 0f, 0f),
		// SE
		new Vector3(innerRadius, 0f, -triHeight),
		// SW
		new Vector3(-innerRadius, 0f, -triHeight),
		// W
		new Vector3(-edgeLength, 0f, 0f),
		// NW
		new Vector3(-innerRadius, 0f, triHeight)
	};

	public static Vector3 GetCorner(EdgeDirection direction) {
		return corners[(int)direction];
	}
}

public enum TriType { Flat, Cliff, NoCost, Cost, NonTraversable }