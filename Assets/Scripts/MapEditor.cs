using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapEditor : MonoBehaviour
{
	public const float POINT_SELECT_RADIUS = GridMetrics.edgeLength * POINT_RADIUS_MULTIPLIER;
	public const float CORNER_SELECT_RADIUS = GridMetrics.outerRadius / 3f;
	public const float HEX_SELECT_RADIUS = GridMetrics.outerRadius / 3f * 2f;
	public const float CELL_SELECT_RADIUS = GridMetrics.outerRadius / 3f;
	public const float EDGE_DIRECTION_INDICATOR_RADIUS = GridMetrics.outerRadius * 0.25f;
	public const float POINT_RADIUS_MULTIPLIER = 0.2f;
	public const float dragThreshold = 0.25f;

	public SelectedComponent selectMode;
	public SelectInteraction interactionMode;
	public Color[] colors;
	public Grid hexGrid;

	// Hover indicator
	[SerializeField]
	private LineRenderer hoverLineRenderer;

	[SerializeField]
	private Dropdown selectModeDropdown;
	[SerializeField]
	private Dropdown interactModeDropdown;

	// Labels
	[SerializeField]
	private Text toggleInteractModeLabel;
	[SerializeField]
	private Text elevationLabel;
	[SerializeField]
	private Button pointVsCornerButton;
	private Text pointVsCornerLabel;

	// Editing Toggles
	[SerializeField]
	private Toggle editElevationToggle;
	[SerializeField]
	private Toggle editEdgeTypeToggle;

	private int activeElevation = 0;
	private bool isEditingElevation;
	private bool isEditingEdgeType;

	private Vector3[] pointArray;
	// if false, corners
	private bool isMultiSelectingPoints;
	private float clickStart;
	private Vector3 clickStartPos;
	private bool isClicking;
	private bool isDragging {
		get {
			return isClicking && Time.time - clickStart > dragThreshold;
        }
    }

	public static List<TriCell> selectedCells;

	private bool isMouseOnScreen {
		get {
			Vector3 mousePos = Input.mousePosition;
			return mousePos.x > 0 && mousePos.y > 0 && mousePos.x < Screen.width && mousePos.y < Screen.height;
        }
    }

	void Awake() {
		// Max of 12 points needed to render a hover (hexagon with a cliff on every edge)
		pointArray = new Vector3[12];
		if (pointVsCornerButton) {
			pointVsCornerLabel = pointVsCornerButton.GetComponentInChildren<Text>();
        }
		selectedCells = new List<TriCell>();
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
			if (isClicking) {
				// End drag
				if (isDragging) {

                }
				isClicking = false;
			}
        }
	}

	void HandleClick() {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			/* Vertical raycast to detect number of floors
			 * Commenting out for later use
			Vector3 hitPoint = hit.point;
			Ray verticalRay = new Ray(new Vector3(hit.point.x, -1f, hit.point.z), Vector3.up);
			RaycastHit hit2;
			if (Physics.Raycast(verticalRay, out hit2)) {
				Debug.Log("Vertical hit");
            } */
			clickStartPos = hit.point;
			/*
			if (Physics.Raycast(inputRay, out hit)) {
				switch (interactMode) {
					case InteractMode.Edit:
						HandleEdit(hit);
						break;
					case InteractMode.Inspect:
						break;
				}
			}
			*/
		}
	}

    #region Tools

    #endregion

    #region Edit Handlers

    private void HandleEdit(RaycastHit hit) {
		switch (selectMode) {
			case SelectedComponent.Tri:
				HandleTriEdit(hexGrid.GetCellFromPosition(hit.point));
				break;
			case SelectedComponent.Hex:
				HandleHexEdit(hexGrid.GetPointFromPosition(hit.point));
				break;
			case SelectedComponent.Point:
				HandlePointEdit(hexGrid.GetPointFromPosition(hit.point));
				break;
			case SelectedComponent.Edge:
				HandleEdgeEdit(hit);
				break;
			case SelectedComponent.Corner:
				HandleCornerEdit(hexGrid.GetCornerFromPosition(hit.point));
				break;
			case SelectedComponent.Multi:
				HandleMultiEdit(hit);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void HandleMultiEdit(RaycastHit hit) {
		if (Mathf.Approximately(Vector3.Angle(hit.normal, Vector3.up), 90f)) {
			HandleEdgeEdit(hit);
		} else {
			Vector2 hitXZ = Util.XZ(hit.point);
			Point point = hexGrid.GetPointFromPosition(hit.point);
			float distFromPoint = Vector2.Distance(
				point.position, hitXZ);
			if (isMultiSelectingPoints && distFromPoint < POINT_SELECT_RADIUS) {
				HandlePointEdit(point);
			} else if (!isMultiSelectingPoints && distFromPoint < CORNER_SELECT_RADIUS) {
				HandleCornerEdit(hexGrid.GetCornerFromPosition(hit.point));
			} else if (distFromPoint < HEX_SELECT_RADIUS) {
				HandleHexEdit(point);
			} else {
				TriCell cell = hexGrid.GetCellFromPosition(hit.point);
				if (Vector2.Distance(cell.centerXZ, hitXZ) < CELL_SELECT_RADIUS) {
					HandleTriEdit(cell);
				} else {
					HandleEdgeEdit(hit);
				}
			}
		}
	}

	private void HandleCornerEdit(TriCell.CellCorner corner) {
		if (isEditingElevation) {
			corner.Elevation = activeElevation;
        }
    }

	private void HandleEdgeEdit(RaycastHit hit) {
		int cellIndex;
		Edge edge = hexGrid.GetEdgeFromRaycast(hit, out cellIndex);
		if (isEditingElevation) {
			edge.Corners[cellIndex * 2].Elevation = activeElevation;
			edge.Corners[cellIndex * 2 + 1].Elevation = activeElevation;
		}
		if (isEditingEdgeType) {
			if (edge.IsCliff) {
				int otherCell = (cellIndex + 1) % 2;
				edge.Corners[cellIndex * 2].Elevation = edge.Corners[otherCell * 2].Elevation;
				edge.Corners[cellIndex * 2 + 1].Elevation = edge.Corners[otherCell * 2 + 1].Elevation;
			} else {
				TriCell.CellCorner c1 = edge.Corners[cellIndex * 2];
				TriCell.CellCorner c2 = edge.Corners[cellIndex * 2 + 1];
				TriCell.CellCorner c3 = c1.GetOppositeCorner(c2);
				c1.Elevation = c3.Elevation;
				c2.Elevation = c3.Elevation;
			}
		}
	}

	private void HandleTriEdit(TriCell tri) {
		if (isEditingElevation) {
			tri.SetElevation(activeElevation);
		}
	}

	private void HandlePointEdit(Point point) {
		if (isEditingElevation) {

		}
	}

	private void HandleHexEdit(Point point) {
		if (isEditingElevation) {

		}
	}

    #endregion

    #region Selection Handlers
	
	private void HandleLineSelect(RaycastHit hit) {
		switch (selectMode) {
			case SelectedComponent.Tri:
				HandleTriLineSelect(hit.point);
				break;
			case SelectedComponent.Hex:
				HandleHexLineSelect(hit.point);
				break;
			case SelectedComponent.Point:
				break;
			case SelectedComponent.Edge:
				break;
			case SelectedComponent.Corner:
				break;
			case SelectedComponent.Multi:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void HandleHexLineSelect(Vector3 dragEnd) {

    }

	private void HandleTriLineSelect(Vector3 dragEnd) {
		Vector2 dragDirection = Util.XZ(clickStartPos) - Util.XZ(dragEnd);
		float angle = Vector2.SignedAngle(Vector2.up, dragDirection);
		// 0 is down, negative left, positive right
		int direction = (int) (angle / 30f);
		Debug.Log(direction);
	}

	#endregion

	#region Hover Handlers

	private void HandleHover() {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			if (isDragging) {
				switch (interactionMode) {
					case SelectInteraction.Freehand:
						
						break;
					case SelectInteraction.Line:
						HandleLineSelect(hit);
						break;
					case SelectInteraction.Area:
						break;
                }
			} else {
				switch (selectMode) {
					case SelectedComponent.Tri:
						HandleTriHover(hexGrid.GetCellFromPosition(hit.point));
						break;
					case SelectedComponent.Hex:
						HandleHexHover(hexGrid.GetPointFromPosition(hit.point));
						break;
					case SelectedComponent.Point:
						HandlePointHover(hexGrid.GetPointFromPosition(hit.point));
						break;
					case SelectedComponent.Edge:
						HandleEdgeHover(hit);
						break;
					case SelectedComponent.Corner:
						HandleCornerHover(hexGrid.GetCornerFromPosition(hit.point));
						break;
					case SelectedComponent.Multi:
						HandleMultiHover(hit);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	private void HandleMultiHover(RaycastHit hit) {
		if (Mathf.Approximately(Vector3.Angle(hit.normal, Vector3.up), 90f)) {
			HandleEdgeHover(hit);
		} else {
			Vector2 hitXZ = Util.XZ(hit.point);
			Point point = hexGrid.GetPointFromPosition(hit.point);
			float distFromPoint = Vector2.Distance(
				point.position, hitXZ);
			if (isMultiSelectingPoints && distFromPoint < POINT_SELECT_RADIUS) {
				HandlePointHover(point);
			} else if (!isMultiSelectingPoints && distFromPoint < CORNER_SELECT_RADIUS) {
				HandleCornerHover(hexGrid.GetCornerFromPosition(hit.point));
			} else if (distFromPoint < HEX_SELECT_RADIUS) {
				HandleHexHover(point);
			} else {
				TriCell cell = hexGrid.GetCellFromPosition(hit.point);
				if (Vector2.Distance(cell.centerXZ, hitXZ) < CELL_SELECT_RADIUS) {
					HandleTriHover(cell);
				} else {
					HandleEdgeHover(hit);
				}
			}
		}
	}

	private void HandleCornerHover(TriCell.CellCorner corner) {
		hoverLineRenderer.loop = true;
		hoverLineRenderer.positionCount = 3;
		pointArray[0] = corner.Position;
		pointArray[1] = pointArray[0] + (corner.PrevCorner.Position - pointArray[0]) * 0.5f;
		pointArray[2] = pointArray[0] + (corner.NextCorner.Position - pointArray[0]) * 0.5f;
		hoverLineRenderer.SetPositions(pointArray);
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
			Vector3 pos = Util.ToVec3(point.position);
			pointArray[0] = pos + Util.ElevationToVec3(minElevation);
			pointArray[1] = pos + Util.ElevationToVec3(maxElevation);
		} else { // Draw hex around point
			hoverLineRenderer.loop = false;
			hoverLineRenderer.positionCount = 12;
			Vector3 center = Util.ToVec3(point.position) + Util.ElevationToVec3(minElevation);
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

	private void HandleEdgeHover(RaycastHit hit) {
		int cellIndex;
		Edge edge = hexGrid.GetEdgeFromRaycast(hit, out cellIndex);
		hoverLineRenderer.loop = true;
		hoverLineRenderer.positionCount = 5;
		int offset = 0;
		// This is really ugly but more readable than doing it cleaner imo
		pointArray[0] = edge.Corners[0].Position;
		if (cellIndex == 0) {
			TriCell.CellCorner c1 = edge.Corners[0];
			TriCell.CellCorner c2 = edge.Corners[1];

			Vector3 half = c2.Position + (c1.Position - c2.Position) * 0.5f;
			pointArray[1] = half + (c1.GetOppositeCorner(c2).Position - half).normalized * EDGE_DIRECTION_INDICATOR_RADIUS;
			offset++;
        }
		pointArray[1 + offset] = edge.Corners[1].Position;
		pointArray[2 + offset] = edge.Corners[3].Position;
		if (cellIndex == 1) {
			TriCell.CellCorner c1 = edge.Corners[2];
			TriCell.CellCorner c2 = edge.Corners[3];

			Vector3 half = c2.Position + (c1.Position - c2.Position) * 0.5f;
			pointArray[3] = half + (c1.GetOppositeCorner(c2).Position - half).normalized * EDGE_DIRECTION_INDICATOR_RADIUS;
			offset++;
		}
		pointArray[3 + offset] = edge.Corners[2].Position;
		hoverLineRenderer.SetPositions(pointArray);
	}

    #endregion

    #region Map editor UI handlers

	public void HandleSelectModeChange() {
		selectMode = (SelectedComponent)selectModeDropdown.value;
		if (selectMode == SelectedComponent.Multi) {
			pointVsCornerButton.gameObject.SetActive(true);
		} else {
			pointVsCornerButton.gameObject.SetActive(false);
		}
	}

	public void HandleInteractModeChange() {
		interactionMode = (SelectInteraction)interactModeDropdown.value;
    }

	public void HandlePointVsCornerMode() {
		isMultiSelectingPoints = !isMultiSelectingPoints;
		if (isMultiSelectingPoints) {
			pointVsCornerLabel.text = "Points";
		} else {
			pointVsCornerLabel.text = "Corners";
		}
	}

	public void HandleEditEdgeTypeToggle() {
		isEditingEdgeType = editEdgeTypeToggle.isOn;
	}

	public void HandleEditElevationToggle() {
		isEditingElevation = editElevationToggle.isOn;
    }

	public void SetElevation(float elevation) {
		activeElevation = (int)elevation;
		elevationLabel.text = activeElevation.ToString();
	}

    #endregion
}

public enum SelectedComponent { Tri, Point, Edge, Hex, Corner, Multi }
public enum SelectInteraction { Line, Area, Freehand }
