using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class MapEditor : MonoBehaviour
{
	public const float POINT_SELECT_RADIUS = GridMetrics.edgeLength * POINT_RADIUS_MULTIPLIER;
	public const float HEX_SELECT_RADIUS = GridMetrics.outerRadius / 3f * 2f;
	public const float CELL_SELECT_RADIUS = POINT_SELECT_RADIUS;
	public const float POINT_RADIUS_MULTIPLIER = 0.2f;
	public const float dragThreshold = 0.25f;

	public SelectMode selectMode;
	public InteractMode interactMode;
	public Color[] colors;
	public Grid hexGrid;

	public Text toggleSelectModeLabel;
	public Text toggleInteractModeLabel;
	public LineRenderer hoverLineRenderer;

	Color activeColor;
	int activeElevation = 0;

	private Vector3[] pointArray;
	private float clickStart;
	private bool isClicking;
	private bool isDragging {
		get {
			return isClicking && Time.time - clickStart > dragThreshold;
        }
    }

	private bool isMouseOnScreen {
		get {
			Vector3 mousePos = Input.mousePosition;
			return mousePos.x > 0 && mousePos.y > 0 && mousePos.x < Screen.width && mousePos.y < Screen.height;
        }
    }

	void Awake() {
		SelectColor(0);
		// Max of 12 points needed to render a hover (hexagon with a cliff on every edge)
		pointArray = new Vector3[12];
	}

	void Update() {
		// Mouse hover
		if (isMouseOnScreen) {
			HandleHover();
		}

		// Clicking
		if (Input.GetMouseButton(0) &&
			!EventSystem.current.IsPointerOverGameObject() &&
			!isClicking) {
			isClicking = true;
			clickStart = Time.time;
			HandleClick();
		} else if (!Input.GetMouseButton(0)) {
			isClicking = false;
        }

	}

	private void HandleHover() {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			switch (selectMode) {
				case SelectMode.Tri:
					HandleTriHover(hexGrid.GetCellFromPosition(hit.point));
					break;
				case SelectMode.Hex:
					HandleHexHover(hexGrid.GetPointFromPosition(hit.point));
					break;
				case SelectMode.Point:
					HandlePointHover(hexGrid.GetPointFromPosition(hit.point));
					break;
				case SelectMode.Edge:
					HandleEdgeHover(hexGrid.GetEdgeFromPosition(hit.point));
					break;
				case SelectMode.Multi:
					HandleMultiHover(hit);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	void HandleClick() {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			switch (interactMode) {
				case InteractMode.Edit:
					switch (selectMode) {
						case SelectMode.Tri:
							EditTri(hexGrid.GetCellFromPosition(hit.point));
							break;
						case SelectMode.Hex:
							EditHex(hexGrid.GetPointFromPosition(hit.point));
							break;
						case SelectMode.Point:
							EditPoint(hexGrid.GetPointFromPosition(hit.point));
							break;
						case SelectMode.Edge:
							EditEdge(hexGrid.GetEdgeFromPosition(hit.point));
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
				case InteractMode.Inspect:
					if (selectMode == SelectMode.Edge) {
						hexGrid.GetEdgeFromPosition(hit.point);
                    }
					break;
			}
		}
	}

    #region Edit Handlers

    void EditEdge(Edge edge) {
		
    }

	void EditTri(TriCell tri) {
		tri.SetElevation(activeElevation);
	}

	void EditPoint(Point point) {
		// point.Refresh();
    }

	void EditHex(Point point) {
		// don't do it this way, editing the tris in the hex should trigger the refreshes.
		// point.RefreshHex();
	}

    #endregion

    #region Hover Handlers

	private void HandleMultiHover(RaycastHit hit) {
		Debug.Log("Normal: " + hit.normal.ToString());
		Debug.Log("Angle: " + Vector3.Angle(hit.normal, Vector3.up).ToString());
		Debug.Log("IsEdge: " + Mathf.Approximately(Vector3.Angle(hit.normal, Vector3.up), 90f).ToString());
		if (Mathf.Approximately(Vector3.Angle(hit.normal, Vector3.up), 90f)) {
			HandleEdgeHover(hexGrid.GetEdgeFromPosition(hit.point));
		} else {
			Vector2 hitXZ = Util.XZ(hit.point);
			Point point = hexGrid.GetPointFromPosition(hit.point);
			float distFromPoint = Vector2.Distance(
				Util.XZ(point.transform.position), hitXZ);
			if (distFromPoint < POINT_SELECT_RADIUS) {
				HandlePointHover(point);
			} else if (distFromPoint < HEX_SELECT_RADIUS) {
				HandleHexHover(point);
			} else {
				TriCell cell = hexGrid.GetCellFromPosition(hit.point);
				if (Vector2.Distance(cell.centerXZ, hitXZ) < CELL_SELECT_RADIUS) {
					HandleTriHover(cell);
                } else {
					HandleEdgeHover(hexGrid.GetEdgeFromPosition(hit.point));
                }
			}
		}
	}

	private void HandleTriHover(TriCell cell) {
		hoverLineRenderer.loop = true;
		hoverLineRenderer.positionCount = 3;
		for (int i = 0; i < 3; i++) {
			pointArray[i] = cell.corners[i].Position;
		}
		hoverLineRenderer.SetPositions(pointArray);
    }

	private void HandlePointHover(Point point) {
		int minElevation = int.MaxValue;
		int maxElevation = int.MinValue;
		for(int i = 0; i < 6; i++) {
			CellDirection direction = (CellDirection)i;
			int elevation = point.GetCell(direction).corners[direction.GetCenterVertIndexForCell()].Elevation;
			if (elevation < minElevation) {
				minElevation = elevation;
            }
			if (elevation > maxElevation) {
				maxElevation = elevation;
            }
        }
		if (minElevation != maxElevation) { // draw vertical line
			hoverLineRenderer.loop = false;
			hoverLineRenderer.positionCount = 2;
			pointArray[0] = point.transform.position + Util.ElevationToVec3(minElevation);
			pointArray[1] = point.transform.position + Util.ElevationToVec3(maxElevation);
		} else { // Draw hex around point
			hoverLineRenderer.loop = false;
			hoverLineRenderer.positionCount = 12;
			Vector3 center = point.transform.position + Util.ElevationToVec3(minElevation);
			pointArray[0] = point.GetCell(CellDirection.N).corners[1].Position;
			pointArray[1] = point.GetCell(CellDirection.N).corners[2].Position;
			pointArray[2] = point.GetCell(CellDirection.NE).corners[0].Position;
			pointArray[3] = point.GetCell(CellDirection.NE).corners[1].Position;
			pointArray[4] = point.GetCell(CellDirection.SE).corners[2].Position;
			pointArray[5] = point.GetCell(CellDirection.SE).corners[0].Position;
			pointArray[6] = point.GetCell(CellDirection.S).corners[1].Position;
			pointArray[7] = point.GetCell(CellDirection.S).corners[2].Position;
			pointArray[8] = point.GetCell(CellDirection.SW).corners[0].Position;
			pointArray[9] = point.GetCell(CellDirection.SW).corners[1].Position;
			pointArray[10] = point.GetCell(CellDirection.NW).corners[2].Position;
			pointArray[11] = point.GetCell(CellDirection.NW).corners[0].Position;
			for (int i = 0; i < 12; i++) {
				Vector3 diff = pointArray[i] - center;
				pointArray[i] = center + diff * POINT_RADIUS_MULTIPLIER;
            }
		}
		hoverLineRenderer.SetPositions(pointArray);
	}

	private void HandleHexHover(Point point) {
		hoverLineRenderer.loop = true;
		hoverLineRenderer.positionCount = 12;
		pointArray[0] = point.GetCell(CellDirection.N).corners[1].Position;
		pointArray[1] = point.GetCell(CellDirection.N).corners[2].Position;
		pointArray[2] = point.GetCell(CellDirection.NE).corners[0].Position;
		pointArray[3] = point.GetCell(CellDirection.NE).corners[1].Position;
		pointArray[4] = point.GetCell(CellDirection.SE).corners[2].Position;
		pointArray[5] = point.GetCell(CellDirection.SE).corners[0].Position;
		pointArray[6] = point.GetCell(CellDirection.S).corners[1].Position;
		pointArray[7] = point.GetCell(CellDirection.S).corners[2].Position;
		pointArray[8] = point.GetCell(CellDirection.SW).corners[0].Position;
		pointArray[9] = point.GetCell(CellDirection.SW).corners[1].Position;
		pointArray[10] = point.GetCell(CellDirection.NW).corners[2].Position;
		pointArray[11] = point.GetCell(CellDirection.NW).corners[0].Position;
		hoverLineRenderer.SetPositions(pointArray);
	}

	private void HandleEdgeHover(Edge edge) {
		hoverLineRenderer.loop = true;
		hoverLineRenderer.positionCount = 4;
		pointArray[0] = edge.Corners[0].Position;
		pointArray[1] = edge.Corners[1].Position;
		pointArray[2] = edge.Corners[3].Position;
		pointArray[3] = edge.Corners[2].Position;
		hoverLineRenderer.SetPositions(pointArray);
	}

	#endregion

	public void SelectColor(int index) {
		activeColor = colors[index];
	}

	public void ChangeSelectMode() {
		selectMode = (SelectMode) (((int)selectMode + 1) % Enum.GetValues(typeof(SelectMode)).Length);
		switch (selectMode) {
			case SelectMode.Tri:
				toggleSelectModeLabel.text = "Tri Select";
				break;
			case SelectMode.Hex:
				toggleSelectModeLabel.text = "Hex Select";
				break;
			case SelectMode.Point:
				toggleSelectModeLabel.text = "Point Select";
				break;
			case SelectMode.Edge:
				toggleSelectModeLabel.text = "Edge Select";
				break;
			case SelectMode.Multi:
				toggleSelectModeLabel.text = "Multi Select";
				break;
		}
    }

	public void ChangeInteractMode() {
		interactMode = (InteractMode)(((int)interactMode + 1) % 2);
		switch (interactMode) {
			case InteractMode.Edit:
				toggleInteractModeLabel.text = "Edit";
				break;
			case InteractMode.Inspect:
				toggleInteractModeLabel.text = "Inspect";
				break;
		}
	}

	public void SetElevation(float elevation) {
		activeElevation = (int)elevation;
	}
}

public enum SelectMode { Tri, Point, Edge, Hex, Multi }
public enum InteractMode { Edit, Inspect } 