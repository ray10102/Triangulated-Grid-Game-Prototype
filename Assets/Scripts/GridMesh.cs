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

	public void Triangulate(IEnumerable<GridCell> cells) {
		hexMesh.Clear();
		vertices.Clear();
		triangles.Clear();
		colors.Clear();
		foreach(GridCell cell in cells) {
			Triangulate(cell);
        }
		hexMesh.vertices = vertices.ToArray();
		hexMesh.triangles = triangles.ToArray();
		hexMesh.colors = colors.ToArray();
		hexMesh.RecalculateNormals();
		meshCollider.sharedMesh = hexMesh;
	}

	private void Triangulate(GridCell cell) {
		for (int i = 0; i < cell.count; i++) {
			Triangulate(cell.GetCell(i));
        }
    }

	private void Triangulate(TriCell cell) {
		AddTriangle(cell);
		AddTriangleColor(cell);
		if (cell.IsPositive) {
			Edge[] edges = cell.GetEdges();
			foreach (Edge edge in edges) {
				if (edge != null && edge.IsCliff) {
					if (edge.Corners[0].Elevation != edge.Corners[2].Elevation) {
						if (edge.Corners[1].Elevation != edge.Corners[3].Elevation) {
							// Both vertices different
							// Vertex order depends on which side is lower.
							if (edge.Corners[0].Elevation < edge.Corners[2].Elevation) {
								// Top tri lower
								AddTriangle(edge, 0, 2, 1);
								AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
								AddTriangle(edge, 1, 2, 3);
								AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
							} else {
								// Top tri higher
								AddTriangle(edge, 1, 0, 2);
								AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
								AddTriangle(edge, 2, 3, 1);
								AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
							}
						} else {
							if (edge.Corners[0].Elevation < edge.Corners[2].Elevation) { // tri facing top cell
								AddTriangle(edge, 1, 0 , 2);
								AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
							} else { // tri facing bottom cell
								AddTriangle(edge, 1, 0, 2);
								AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
							}
						}
					} else if (edge.Corners[1].Elevation != edge.Corners[3].Elevation) { // Only center vertex different
						if (edge.Corners[1].Elevation < edge.Corners[3].Elevation) { // tri facing top cell
							AddTriangle(edge, 1, 0, 3);
							AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
						} else { // tri facing bottom cell
							AddTriangle(edge, 1, 0, 3);
							AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
						}
					}
				}
			}
        }
    }

	/// <summary>
	/// Triangulates top and bottom tris and NE, E, and SE edge cliffs for each point (if applicable).
	/// Deprecated. replacing with triangulation per cell, which is much more readable and requires far fewer checks on whether each cell needs to exist.
	/// </summary>
	/// <param name="point"></param>
	private void Triangulate(Point point) {
		throw new NotImplementedException("Triangulate based on cells instead of points pls");
		Vector3 center = Util.ToVec3(point.position);

		

		/* Point type is now held in GridPoint
		 * I'm commenting this out so the code still compiles without deleting the code entirely.
		 * 
		if (point.type != PointType.HorizontalEdge) {
			if (point.type != PointType.TopEdge) { // Top Triangle
				AddTriangle(center + Util.ElevationToVec3(point.GetCell(CellDirection.N).corners[0].Elevation),
					center + GridMetrics.GetCorner(EdgeDirection.NW) + Util.ElevationToVec3(point.GetCell(CellDirection.N).corners[1].Elevation),
					center + GridMetrics.GetCorner(EdgeDirection.NE) + Util.ElevationToVec3(point.GetCell(CellDirection.N).corners[2].Elevation));
				AddTriangleColor(point.GetCell(CellDirection.N));
			}
			if (point.type != PointType.BottomEdge) { // Bottom Triangle
				AddTriangle(center + Util.ElevationToVec3(point.GetCell(CellDirection.S).corners[0].Elevation),
					center + GridMetrics.GetCorner(EdgeDirection.SE) + Util.ElevationToVec3(point.GetCell(CellDirection.S).corners[1].Elevation),
					center + GridMetrics.GetCorner(EdgeDirection.SW) + Util.ElevationToVec3(point.GetCell(CellDirection.S).corners[2].Elevation));
				AddTriangleColor(point.GetCell(CellDirection.S));
			}
		}
		*/

		// Triangulate cliffs
		foreach (EdgeOrientation direction in (EdgeOrientation[])Enum.GetValues(typeof(EdgeOrientation))) {
			// There are always half as many edge orientations as edge directions. Technically hacky but implicitly enforced by invariant.
			// Effectively only triangulates NE, E, and SE edge cliffs to avoid double triangulation
			Edge edge = point.GetEdge((EdgeDirection)direction);
			if (edge != null && edge.IsCliff) {
				Vector3 corner = GridMetrics.GetCorner((EdgeDirection)direction);
				// Because we are looking at only E edges, xz always corresponds to center, yw to corner
				if (edge.Corners[0].Elevation != edge.Corners[2].Elevation) {
					if (edge.Corners[1].Elevation != edge.Corners[3].Elevation) { 
						// Both vertices different
						// Vertex order depends on which side is lower.
						if (edge.Corners[0].Elevation < edge.Corners[2].Elevation) {
							// Top tri lower
							AddTriangle(center + corner + Util.ElevationToVec3(edge.Corners[0].Elevation),
								center + corner + Util.ElevationToVec3(edge.Corners[2].Elevation),
								center + Util.ElevationToVec3(edge.Corners[1].Elevation));
							// AddTriangleColor(edge.Corners[0].Color, edge.Corners[2].Color, edge.Corners[1].Color);
							AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
							AddTriangle(center + Util.ElevationToVec3((int)edge.Corners[1].Elevation),
								center + corner + Util.ElevationToVec3((int)edge.Corners[2].Elevation),
								center + Util.ElevationToVec3((int)edge.Corners[3].Elevation));
							// AddTriangleColor(edge.Corners[1].Color, edge.Corners[2].Color, edge.Corners[3].Color);
							AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
						} else {
							// Top tri higher
							AddTriangle(center + Util.ElevationToVec3((int)edge.Corners[1].Elevation),
								center + corner + Util.ElevationToVec3((int)edge.Corners[0].Elevation),
								center + corner + Util.ElevationToVec3((int)edge.Corners[2].Elevation));
							// AddTriangleColor(edge.Corners[1].Color, edge.Corners[0].Color, edge.Corners[2].Color);
							AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
							AddTriangle(center + corner + Util.ElevationToVec3((int)edge.Corners[2].Elevation),
								center + Util.ElevationToVec3((int)edge.Corners[3].Elevation),
								center + Util.ElevationToVec3((int)edge.Corners[1].Elevation));
							// AddTriangleColor(edge.Corners[2].Color, edge.Corners[3].Color, edge.Corners[1].Color);
							AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
						}
					} else { // Only corner vertex different
						if (edge.Corners[0].Elevation < edge.Corners[2].Elevation) { // tri facing top cell
							AddTriangle(center + Util.ElevationToVec3((int)edge.Corners[1].Elevation),
								center + corner + Util.ElevationToVec3((int)edge.Corners[0].Elevation),
								center + corner + Util.ElevationToVec3((int)edge.Corners[2].Elevation));
							AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
						} else { // tri facing bottom cell
							AddTriangle(center + Util.ElevationToVec3((int)edge.Corners[1].Elevation),
								center + corner + Util.ElevationToVec3((int)edge.Corners[0].Elevation),
								center + corner + Util.ElevationToVec3((int)edge.Corners[2].Elevation));
							AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
						}
					}
				} else if (edge.Corners[1].Elevation != edge.Corners[3].Elevation) { // Only center vertex different
					if (edge.Corners[1].Elevation < edge.Corners[3].Elevation) { // tri facing top cell
						AddTriangle(center + Util.ElevationToVec3((int)edge.Corners[1].Elevation),
							center + corner + Util.ElevationToVec3((int)edge.Corners[0].Elevation),
							center + Util.ElevationToVec3((int)edge.Corners[3].Elevation));
						AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
					} else { // tri facing bottom cell
						AddTriangle(center + Util.ElevationToVec3((int)edge.Corners[1].Elevation),
							center + corner + Util.ElevationToVec3((int)edge.Corners[0].Elevation),
							center + Util.ElevationToVec3((int)edge.Corners[3].Elevation));
						AddTriangleColor(GridMetrics.colors[(int)TriType.Cliff]);
					}
				}
			}
		}
	}

	private void AddTriangle(TriCell cell) {
		AddTriangle(cell.corners[0].Position, cell.corners[1].Position, cell.corners[2].Position);
    }

	private void AddTriangle(Edge edge, int c1, int c2, int c3) {
		AddTriangle(
			edge.Corners[c1].Position,
			edge.Corners[c2].Position,
			edge.Corners[c3].Position);
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

	private void AddTriangleColor(Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}
}