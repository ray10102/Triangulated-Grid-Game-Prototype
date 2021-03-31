using UnityEngine;

public static class GridMetrics
{
	public const float elevationStep = 5f;
	public const float outerRadius = 5f;
	public const float innerRadius = outerRadius * 0.866025404f;
	public const float SQRT_3 = 1.73205080757f;
	public const int chunkSizeX = 5, chunkSizeZ = 5;

	private static Vector3[] corners = {
		// NE
		new Vector3(innerRadius, 0f, outerRadius * 1.5f),
		// E
		new Vector3(innerRadius * 2f, 0f, 0f),
		// SE
		new Vector3(innerRadius, 0f, -outerRadius * 1.5f),
		// SW
		new Vector3(-innerRadius, 0f, -outerRadius * 1.5f),
		// W
		new Vector3(-innerRadius * 2f, 0f, 0f),
		// NW
		new Vector3(-innerRadius, 0f, outerRadius * 1.5f)
	};

	public static Vector3 GetCorner(EdgeDirection direction) {
		return corners[(int)direction];
	}
}