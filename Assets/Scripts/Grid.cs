using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour
{
	public bool shouldLabelOnTriangle;

	public GridChunk chunkPrefab;
	public Point pointPrefab;
	public Text cellLabelPrefab;
	public int chunkCountX = 4, chunkCountZ = 3;

	public int defaultPointElevation;

	public Color defaultColor = Color.white;
	public Color touchedColor = Color.magenta;

	private Point[] points;
	private GridChunk[] chunks;

	private int cellCountZ;
	private int cellCountX;

	void Awake() {

		cellCountX = chunkCountX * GridMetrics.chunkSizeX;
		cellCountZ = chunkCountZ * GridMetrics.chunkSizeZ;

		GridMesh.SetHeight(cellCountZ);

		CreateChunks();
		CreatePoints();
		InitPoints();
	}

	#region Getters
	public Point GetPointFromPosition(Vector3 position) {
		position = transform.InverseTransformPoint(position);
		AxialCoordinates coordinates = AxialCoordinates.FromPosition(position);
		return points[GetPointIndexFromCoordinate(coordinates)];
	}

	public Point GetPointFromTri(TriCell cell) {
		return points[GetPointIndexFromCoordinate(cell.coordinates)];
	}

	public TriCell GetCellFromPosition(Vector3 position) {
		Point point = GetPointFromPosition(position);
		AxialCoordinates vert = point.coordinates;
		Vector2 center = Util.XZ(point.transform.position);
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

	public Edge GetEdgeFromPosition(Vector3 position) {
		Point point = GetPointFromPosition(position);
		AxialCoordinates vert = point.coordinates;
		Vector2 position2D = new Vector2(position.x, position.z);
		Vector2 center = AxialCoordinates.GetCenterFromAxial(vert);
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
		Debug.Log(distPos + ", " + distNeg + ", " + distHorizontal);
		EdgeDirection direction;
		if (distPos < distNeg && distPos < distHorizontal) {
			// pos slope line is closest
			if (position.z > center.y) {
				direction = EdgeDirection.NE;
			} else {
				direction = EdgeDirection.SW;
			}
		} else if (distNeg < distHorizontal) {
			// neg slope line is closest
			if (position.z > center.y) {
				direction = EdgeDirection.NW;
			} else {
				direction = EdgeDirection.SE;
			}
		} else {
			// horizontal line is closest
			if (position.x > center.x) {
				direction = EdgeDirection.E;
			} else {
				direction = EdgeDirection.W;
			}
		}
		return point.GetEdge(direction);
	}
	#endregion

	private void CreateChunks() {
		chunks = new GridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				GridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}

    #region Initialization

    private void CreatePoints() {
		points = new Point[cellCountZ * cellCountX];
		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreatePoint(x, z, i++);
			}
		}
	}

	private void InitPoints() {
		for (int i = 0; i < points.Length; i++) {
			InitPoint(points[i]);
        }
    }

	private void InitPoint(Point point) {
		point.GetCell(CellDirection.N).SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
		point.GetCell(CellDirection.S).SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
		point.GetCell(CellDirection.N).SetElevation(0);// Random.Range(0, 5));
		point.GetCell(CellDirection.S).SetElevation(0);// Random.Range(0, 5));
	}

	private void CreatePoint(int x, int z, int i) {
		Point point = points[i] = Instantiate<Point>(pointPrefab);
		point.coordinates = AxialCoordinates.FromOffsetCoordinates(x, z);
		Vector2 center2D = AxialCoordinates.GetCenterFromAxial(point.coordinates);
		point.transform.localPosition = new Vector3(center2D.x, 0f, center2D.y);
		point.SetCell(CellDirection.N, new TriCell(point, TriType.Top));
		point.SetCell(CellDirection.S, new TriCell(point, TriType.Bottom));
		point.name = "Cell " + point.coordinates.ToString();
		// Set neighbors and edges
		if (x > 0) {
			point.SetNeighbor(EdgeDirection.W, points[i - 1]);
			// Can't set EW edge yet because those points' cells aren't initialized yet.
		}
		if (z > 0) {
			// Treat every other row differently
			if ((z & 1) == 0) {
				Point SEPoint = points[i - cellCountX];
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
					Point SWPoint = points[i - cellCountX - 1];
					point.SetNeighbor(EdgeDirection.SW, SWPoint);
					point.SetCell(CellDirection.SW, SWPoint.GetCell(CellDirection.N));
					SWPoint.SetCell(CellDirection.NE, point.GetCell(CellDirection.S));
					// Pos slope edge
					point.SetEdge(EdgeDirection.SW,
						new Edge(EdgeOrientation.SWNE,
								 point.GetCell(CellDirection.SW),
								 point.GetCell(CellDirection.S)));
				}
			} else {
				Point SWPoint = points[i - cellCountX];
				point.SetNeighbor(EdgeDirection.SW, SWPoint);
				point.SetCell(CellDirection.SW, SWPoint.GetCell(CellDirection.N));
				SWPoint.SetCell(CellDirection.NE, point.GetCell(CellDirection.S));
				// Pos slope edge
				point.SetEdge(EdgeDirection.SW,
					new Edge(EdgeOrientation.SWNE,
							 point.GetCell(CellDirection.SW),
							 point.GetCell(CellDirection.S)));
				if (x < cellCountX - 1) {
					Point SEPoint = points[i - cellCountX + 1];
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
						Debug.Log("S cell: " + (point.GetCell(CellDirection.S) != null).ToString());
						Debug.Log("SE SW cell: " + (SEPoint.GetCell(CellDirection.SW) != null).ToString());
						SEPoint.SetEdge(EdgeDirection.W,
							new Edge(EdgeOrientation.EW,
									 point.GetCell(CellDirection.S),
									 SEPoint.GetCell(CellDirection.SW)));
                    }
				}
			}
		}
		// Add Text Label
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition =
			new Vector2(center2D.x, center2D.y + 0.5f);
		label.text = point.coordinates.GetPerVertexTextLabel(shouldLabelOnTriangle);
		point.uiRect = label.rectTransform;

		AddPointToChunk(x, z, point);
	}

	private void AddPointToChunk(int x, int z, Point point) {
		int chunkX = x / GridMetrics.chunkSizeX;
		int chunkZ = z / GridMetrics.chunkSizeZ;
		GridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * GridMetrics.chunkSizeX;
		int localZ = z - chunkZ * GridMetrics.chunkSizeZ;
		chunk.AddPoint(localX + localZ * GridMetrics.chunkSizeX, point);
	}

    #endregion

    private int GetPointIndexFromCoordinate(AxialCoordinates coordinates) {
		return coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
	}
}