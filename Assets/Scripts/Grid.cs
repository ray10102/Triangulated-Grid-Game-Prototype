﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour
{
	public bool shouldLabelOnTriangle;

	public GridChunk chunkPrefab;
	public Text cellLabelPrefab;
	public int chunkCountX = 4, chunkCountZ = 3;

	public int defaultPointElevation;

	public Color defaultColor = Color.white;
	public Color touchedColor = Color.magenta;

	private GridPoint[] points;
	private GridCell[] cells;
	private GridChunk[] chunks;

	private int pointCountZ;
	private int pointCountX;
	private int cellCountX;
	private int cellCountZ;

	private List<GameObject> vertLabels;
	private List<GameObject> cellLabels;

	private bool showVertLabels;

	Canvas gridCanvas;

	void Awake() {
		gridCanvas = GetComponentInChildren<Canvas>();
		vertLabels = new List<GameObject>();
		cellLabels = new List<GameObject>();
		showVertLabels = true;
		pointCountX = chunkCountX * GridMetrics.chunkSizeX;
		pointCountZ = chunkCountZ * GridMetrics.chunkSizeZ;
		cellCountX = (pointCountX - 1) * 2;
		cellCountZ = pointCountZ - 1;

		cells = new GridCell[cellCountX * cellCountZ];

		GridMesh.SetHeight(pointCountZ);
		
		CreateChunks();
		CreatePoints();
		InitCells();
	}

	#region Getters
	public Point GetPointFromPosition(Vector3 position) {
		GridPoint point = GetGridPointFromPosition(position);
		return point.GetPointAtY(position.y);
	}

	public GridPoint GetGridPointFromPosition(Vector3 position) {
		position = transform.InverseTransformPoint(position);
		VertexCoordinates coordinates = VertexCoordinates.FromPosition(position);
		return points[GetPointIndexFromCoordinate(coordinates)];
	}

	public TriCell GetCellFromPosition(Vector3 position) {
		Point point = GetPointFromPosition(position);
		VertexCoordinates vert = point.coordinates;
		Vector2 center = point.position;
		// y intercept of triangle edge w positive slope
		float bPos = Util.FindIntercept2D(GridMetrics.SQRT_3, center);
		// "" w negative slope
		float bNeg = Util.FindIntercept2D(-GridMetrics.SQRT_3, center);

		float yPos = GridMetrics.SQRT_3 * position.x + bPos;
		float yNeg = -GridMetrics.SQRT_3 * position.x + bNeg;

		if (position.z > center.y) {
			if (yNeg > position.z) {
				return point.GetCell(CellDirection.NW);
			} else if (yPos > position.z) {
				return point.GetCell(CellDirection.NE);
			} else {
				return point.GetCell(CellDirection.N);
			}
		} else {
			if (yNeg < position.z) {
				return point.GetCell(CellDirection.SE);
			} else if (yPos < position.z) {
				return point.GetCell(CellDirection.SW);
			} else {
				return point.GetCell(CellDirection.S);
			}
		}
	}

	public Edge GetEdgeFromRaycast(RaycastHit hit, out int cellIndex) {
		// will be overriden, compiler complains without this
        cellIndex = 0;
        Vector3 position = hit.point;
		bool clickedCliff = false;
		if (Mathf.Approximately(Vector3.Angle(hit.normal, Vector3.up), 90f)) {
			clickedCliff = true;
		}
		Point point = GetPointFromPosition(position);
		VertexCoordinates vert = point.coordinates;
		Vector2 position2D = new Vector2(position.x, position.z);
		Vector2 center = VertexCoordinates.GetPos2DFromVertex(vert);
		// y intercept of triangle edge w positive slope
		float bPos = Util.FindIntercept2D(GridMetrics.SQRT_3, center);
		// "" w negative slope
		float bNeg = Util.FindIntercept2D(-GridMetrics.SQRT_3, center);
		// y intercept of line perpendicular to the edge with positive slope going through position
		float bPosPerp = Util.FindIntercept2D(-1 / GridMetrics.SQRT_3, position2D);
		// "" w negative slope ""
		float bNegPerp = Util.FindIntercept2D(1 / GridMetrics.SQRT_3, position2D);

		Vector2 closestPointPos = Util.FindIntersectionOfLines(GridMetrics.SQRT_3, -1 / GridMetrics.SQRT_3, bPos, bPosPerp);
		Vector2 closestPointNeg = Util.FindIntersectionOfLines(-GridMetrics.SQRT_3, 1 / GridMetrics.SQRT_3, bNeg, bNegPerp);
		float distPos = Vector2.Distance(closestPointPos, position2D);
		float distNeg = Vector2.Distance(closestPointNeg, position2D);
		float distHorizontal = Mathf.Abs(position2D.y - center.y);
		EdgeDirection direction;
		if (distPos < distNeg && distPos < distHorizontal) {
			// pos slope line is closest
			if (position.z > center.y) {
				direction = EdgeDirection.NE;
			} else {
				direction = EdgeDirection.SW;
			}
			if (!clickedCliff) {
				if (position.x < closestPointPos.x) {
					cellIndex = 0;
				} else {
					cellIndex = 1;
				}
			}
		} else if (distNeg < distHorizontal) {
			// neg slope line is closest
			if (position.z > center.y) {
				direction = EdgeDirection.NW;
			} else {
				direction = EdgeDirection.SE;
			}
			if (!clickedCliff) {
				if (position.x > closestPointNeg.x) {
					cellIndex = 0;
				} else {
					cellIndex = 1;
				}
			}
		} else {
			// horizontal line is closest
			if (position.x > center.x) {
				direction = EdgeDirection.E;
			} else {
				direction = EdgeDirection.W;
			}
			if (!clickedCliff) {
				if (position.z > center.y) {
					cellIndex = 0;
				} else {
					cellIndex = 1;
				}
			}
		}
		Edge result = point.GetEdge(direction);
		if (clickedCliff) {
			int lowIndex = result.LowSideIndex;
			int highIndex = lowIndex == 0 ? 1 : 0;
			float minTop = Mathf.Min(result.Corners[highIndex * 2].Position.y, result.Corners[highIndex * 2 + 1].Position.y);
			float maxBottom = Mathf.Max(result.Corners[lowIndex * 2].Position.y, result.Corners[lowIndex * 2 + 1].Position.y);
			float t = Mathf.InverseLerp(maxBottom, minTop, position.y);
			cellIndex = t < 0.5f ? lowIndex : highIndex;
		}
        return result;
    }

    public TriCell.CellCorner GetCornerFromPosition(Vector3 position) {
		TriCell cell = GetCellFromPosition(position);
		float closestDist = float.MaxValue;
		int closestCorner = int.MinValue;
		for (int i = 0; i < 3; i++) {
			Vector3 cornerPos = cell.corners[i].Position;
			float dist = Vector3.Distance(cornerPos, position);
			if (dist < closestDist) {
				closestDist = dist;
				closestCorner = i;
			}
		}
		return cell.corners[closestCorner];
	}
	#endregion

	#region Initialization
	private void CreateChunks() {
		chunks = new GridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				GridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}

	private void CreatePoints() {
		points = new GridPoint[pointCountZ * pointCountX];
		for (int z = 0, i = 0; z < pointCountZ; z++) {
			for (int x = 0; x < pointCountX; x++) {
				CreatePoint(x, z, i++);
			}
		}
	}

	private void InitCells() {
		for (int i = 0; i < cells.Length; i++) {
			InitCell(cells[i]);
		}
	}

	private void InitCell(GridCell cell) {
		// Right now, initializes points with 0 elevation. Later, should init w actual values
		if (cell != null) {
			cell.GetCell(0).SetElevation(0);
        } else {
			Debug.LogWarning("cell is null");
        }
	}

	private PointType GetPointType(int x, int z) {
		if ((x == pointCountX - 1 && (z & 1) == 1) || // rightmost point of odd  row
			(x == 0 && (z & 1) == 0)) { // leftmost point of even row
			return PointType.HorizontalEdge;
        }
		if (z == 0) {
			return PointType.BottomEdge;
        }
		if (z == pointCountZ - 1) {
			return PointType.TopEdge;
        }
		return PointType.Center;
	}

	private void CreatePoint(int x, int z, int i) {
		PointType type = GetPointType(x, z);
		GridPoint gridPoint = points[i] = new GridPoint(new Point(type, x, z));
		Point point = gridPoint.GetPoint(0);

		// Create cells
		if (type != PointType.HorizontalEdge) {
			if (type != PointType.TopEdge) {
				AxialCellCoordinates NCoordinates = point.coordinates.GetRelativeCellCoordinates(CellDirection.N);
				TriCell NCell = new TriCell(point, NCoordinates);
				int cellIndex = GetPointIndexFromCoordinate(NCoordinates);
				if (cells[cellIndex] == null) { 
					cells[cellIndex] = new GridCell(NCell);
					AddCellToChunk(x, z, cells[cellIndex]);
				} else {
					Debug.LogWarning("SOMETHING MIGHT BE WRONG??");
				}
				point.SetCell(CellDirection.N, NCell);
			}
			if (type != PointType.BottomEdge) {
				AxialCellCoordinates SCoordinates = point.coordinates.GetRelativeCellCoordinates(CellDirection.S);
				TriCell SCell = new TriCell(point, SCoordinates);
				int cellIndex = GetPointIndexFromCoordinate(SCoordinates);
				if (cells[cellIndex] == null) {
					cells[cellIndex] = new GridCell(SCell);
					AddCellToChunk(x, z, cells[cellIndex]);
				} else {
					Debug.LogWarning("SOMETHING MIGHT BE WRONG??");
				}
				point.SetCell(CellDirection.S, SCell);
			}
		}

		// Set neighbors and edges
		// This is quite possibly overcomplicated. Due for a refactoring.
		if (x > 0) {
			point.SetNeighbor(EdgeDirection.W, points[i - 1].GetPoint(0));
			// Can't set EW edge yet because those points' cells aren't initialized yet.
		}
		if (z > 0) {
			// Treat every other row differently
			if ((z & 1) == 0) { // Even row
				Point SEPoint = points[i - pointCountX].GetPoint(0);
				point.SetNeighbor(EdgeDirection.SE, SEPoint);
				point.SetCell(CellDirection.SE, SEPoint.GetCell(CellDirection.N));
				SEPoint.SetCell(CellDirection.NW, point.GetCell(CellDirection.S));
				// Neg slope edge
				point.SetEdge(EdgeDirection.SE,
					new Edge(EdgeOrientation.NWSE,
							 point.GetCell(CellDirection.SE),
							 point.GetCell(CellDirection.S)));
				// EW edge
				if (x > 0) {
					SEPoint.SetEdge(EdgeDirection.W,
						new Edge(EdgeOrientation.EW,
								 point.GetCell(CellDirection.S),
								 SEPoint.GetCell(CellDirection.SW)));
				}
				if (x > 0) {
					Point SWPoint = points[i - pointCountX - 1].GetPoint(0);
					point.SetNeighbor(EdgeDirection.SW, SWPoint);
					point.SetCell(CellDirection.SW, SWPoint.GetCell(CellDirection.N));
					SWPoint.SetCell(CellDirection.NE, point.GetCell(CellDirection.S));
					// Pos slope edge
					point.SetEdge(EdgeDirection.SW,
						new Edge(EdgeOrientation.SWNE,
								 point.GetCell(CellDirection.SW),
								 point.GetCell(CellDirection.S)));
				}
			} else { // Odd row
				Point SWPoint = points[i - pointCountX].GetPoint(0);
				point.SetNeighbor(EdgeDirection.SW, SWPoint);
				point.SetCell(CellDirection.SW, SWPoint.GetCell(CellDirection.N));
				SWPoint.SetCell(CellDirection.NE, point.GetCell(CellDirection.S));
				// Pos slope edge
				point.SetEdge(EdgeDirection.SW,
					new Edge(EdgeOrientation.SWNE,
							 point.GetCell(CellDirection.SW),
							 point.GetCell(CellDirection.S)));
				if (x < pointCountX - 1) {
					Point SEPoint = points[i - pointCountX + 1].GetPoint(0);
					point.SetNeighbor(EdgeDirection.SE, SEPoint);
					point.SetCell(CellDirection.SE, SEPoint.GetCell(CellDirection.N));
					SEPoint.SetCell(CellDirection.NW, point.GetCell(CellDirection.S));
					// Neg slope edge
					point.SetEdge(EdgeDirection.SE,
						new Edge(EdgeOrientation.NWSE,
								 point.GetCell(CellDirection.SE),
								 point.GetCell(CellDirection.S)));
					// EW edge
					if (x > 0) {
						SEPoint.SetEdge(EdgeDirection.W,
							new Edge(EdgeOrientation.EW,
									 point.GetCell(CellDirection.S),
									 SEPoint.GetCell(CellDirection.SW)));
					}
				}
			}
		}
		// Add Text Label
		Text vertLabel = Instantiate<Text>(cellLabelPrefab);
		vertLabel.rectTransform.anchoredPosition =
			new Vector2(point.position.x, point.position.y);
		vertLabel.text = point.coordinates.GetPerVertexTextLabel(shouldLabelOnTriangle);
		vertLabels.Add(vertLabel.gameObject);

		Text cellLabelNE = Instantiate<Text>(cellLabelPrefab);
		AxialCellCoordinates NECoordinate = point.coordinates.GetRelativeCellCoordinates(CellDirection.NE);
		cellLabelNE.rectTransform.anchoredPosition =
			new Vector2(point.position.x + GridMetrics.innerRadius, point.position.y + GridMetrics.triHeight * (NECoordinate.IsPositive ? 0.3f : 0.4f));
		cellLabelNE.color = Color.blue;
		cellLabelNE.text = NECoordinate.ToStringOnSeparateLines();
		cellLabels.Add(cellLabelNE.gameObject);

		Text cellLabelN = Instantiate<Text>(cellLabelPrefab);
		AxialCellCoordinates NCoordinate = point.coordinates.GetRelativeCellCoordinates(CellDirection.N);
		cellLabelN.rectTransform.anchoredPosition =
			new Vector2(point.position.x, point.position.y + GridMetrics.triHeight * (NCoordinate.IsPositive ? 0.3f : 0.4f));
		cellLabelN.color = Color.blue;
		cellLabelN.text = NCoordinate.ToStringOnSeparateLines();
		cellLabels.Add(cellLabelN.gameObject);

		vertLabel.rectTransform.SetParent(gridCanvas.transform, false);
		cellLabelNE.rectTransform.SetParent(gridCanvas.transform, false);
		cellLabelN.rectTransform.SetParent(gridCanvas.transform, false);
		cellLabelN.gameObject.SetActive(false);
		cellLabelNE.gameObject.SetActive(false);
	}

	private void AddCellToChunk(int x, int z, GridCell cell) {
		int chunkX = x / GridMetrics.chunkSizeX;
		int chunkZ = z / GridMetrics.chunkSizeZ;
		GridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		// int localX = x - chunkX * GridMetrics.chunkSizeX;
		// int localZ = z - chunkZ * GridMetrics.chunkSizeZ;
		chunk.AddCell(cell);
	}

	#endregion

	private int GetPointIndexFromCoordinate(VertexCoordinates coordinates) {
		return coordinates.X + coordinates.Z * pointCountX + coordinates.Z / 2;
	}

	private int GetPointIndexFromCoordinate(AxialCellCoordinates coordinates) {
		Vector2 offset = coordinates.GetOffsetCoodinates();
		return (int)offset.x + (int)offset.y * cellCountX;
	}

	public void ToggleLabels() {
		showVertLabels = !showVertLabels;
		foreach (GameObject label in cellLabels) {
			label.SetActive(!showVertLabels);
        }
		foreach (GameObject label in vertLabels) {
			label.SetActive(showVertLabels);
		}
	}
}