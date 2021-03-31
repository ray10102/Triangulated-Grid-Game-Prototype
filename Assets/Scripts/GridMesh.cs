using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridMesh : MonoBehaviour
{
	// Temporary buffers
	static List<Vector3> vertices = new List<Vector3>();
	static List<int> triangles = new List<int>();
	static List<Color> colors = new List<Color>();

	Mesh hexMesh;
	MeshCollider meshCollider;

	private static int height = int.MinValue;

	void Awake() {
		GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
		meshCollider = gameObject.AddComponent<MeshCollider>();
		hexMesh.name = "Hex Mesh";
	}

	public static void SetHeight(int height) {
		GridMesh.height = height;
	}

	public void Triangulate(Point[] cells) {
		hexMesh.Clear();
		vertices.Clear();
		triangles.Clear();
		colors.Clear();
		for (int i = 0; i < cells.Length; i++) {
			Triangulate(cells[i]);
		}
		hexMesh.vertices = vertices.ToArray();
		hexMesh.triangles = triangles.ToArray();
		hexMesh.colors = colors.ToArray();
		hexMesh.RecalculateNormals();

		meshCollider.sharedMesh = hexMesh;
	}

	private void Triangulate(Point point) {
		if (height < 0) {
            Debug.LogError("Forgot to set grid height ya dingus");
        }
		Vector3 center = point.transform.localPosition;
		if (point.coordinates.Z < height - 1) {
			AddTriangle(center + Util.ElevationToVec3(point.GetCell(CellDirection.N).corners[0].Elevation),
				center + GridMetrics.GetCorner(EdgeDirection.NW) + Util.ElevationToVec3(point.GetCell(CellDirection.N).corners[1].Elevation),
				center + GridMetrics.GetCorner(EdgeDirection.NE) + Util.ElevationToVec3(point.GetCell(CellDirection.N).corners[2].Elevation));
			AddTriangleColor(point.GetCell(CellDirection.N));
		}
		if (point.coordinates.Z > 0) {
			AddTriangle(center + Util.ElevationToVec3(point.GetCell(CellDirection.S).corners[0].Elevation),
				center + GridMetrics.GetCorner(EdgeDirection.SE) + Util.ElevationToVec3(point.GetCell(CellDirection.S).corners[1].Elevation),
				center + GridMetrics.GetCorner(EdgeDirection.SW) + Util.ElevationToVec3(point.GetCell(CellDirection.S).corners[2].Elevation));
			AddTriangleColor(point.GetCell(CellDirection.S));
		}

		// Triangulate cliffs
		foreach (EdgeOrientation direction in (EdgeOrientation[])Enum.GetValues(typeof(EdgeOrientation))) {
			// There are always half as many edge orientations as edge directions. Technically hacky but implicitly enforced by invariant.
			// Effectively only triangulates NE, E, and SE edge cliffs to avoid double triangulation
			Edge edge = point.GetEdge((EdgeDirection)direction);
			if (edge != null && edge.IsCliff) {
				Vector3 corner = GridMetrics.GetCorner((EdgeDirection)direction);
				// Because we are looking at only E edges, xz always corresponds to center, yw to corner
				Vector4 elevations = edge.Elevations;
				Color[] colors = edge.Colors;
				if (elevations.x != elevations.z) {
					if (elevations.y != elevations.w) { // Both vertices different
						// Vertex order depends on which side is lower.
						// TODO update this to use existing vertices instead of adding new ones
						// fix EW edges
						if (elevations.x < elevations.z) {
							// Top tri lower
							AddTriangle(center + Util.ElevationToVec3((int)elevations.x),
								center + corner + Util.ElevationToVec3((int)elevations.y),
								center + Util.ElevationToVec3((int)elevations.z));
							AddTriangleColor(colors[0], colors[1], colors[2]);
							AddTriangle(center + corner + Util.ElevationToVec3((int)elevations.y),
								center + corner + Util.ElevationToVec3((int)elevations.w),
								center + Util.ElevationToVec3((int)elevations.z));
							AddTriangleColor(colors[1], colors[3], colors[2]);
						} else {
							// Top tri higher
							AddTriangle(center + corner + Util.ElevationToVec3((int)elevations.y),
								center + Util.ElevationToVec3((int)elevations.z),
								center + Util.ElevationToVec3((int)elevations.x));
							AddTriangleColor(colors[1], colors[2], colors[0]);
							AddTriangle(center + Util.ElevationToVec3((int)elevations.z),
								center + corner + Util.ElevationToVec3((int)elevations.y),
								center + corner + Util.ElevationToVec3((int)elevations.w));
							AddTriangleColor(colors[2], colors[1], colors[3]);
						}

					} else { // Only top vertex different
						Debug.Log("top vert");
						AddTriangle(center + Util.ElevationToVec3((int)elevations.x),
							center + Util.ElevationToVec3((int)elevations.z),
							corner + Util.ElevationToVec3((int)elevations.y));
					}
				} else if (elevations.y != elevations.w) { // Only bottom vertex different
					Debug.Log("bottom vert");
					AddTriangle(corner + Util.ElevationToVec3((int)elevations.y),
							corner + Util.ElevationToVec3((int)elevations.w),
							center + Util.ElevationToVec3((int)elevations.z));
				}
			}
		}
	}

	private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	private void AddTriangleColor(TriCell cell) {
		colors.Add(cell.corners[0].Color);
		colors.Add(cell.corners[1].Color);
		colors.Add(cell.corners[2].Color);
	}

	private void AddTriangleColor(Color c1, Color c2, Color c3) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}
}