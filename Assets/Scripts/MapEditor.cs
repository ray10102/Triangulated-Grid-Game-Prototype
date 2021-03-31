using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class MapEditor : MonoBehaviour
{
	public SelectMode selectMode;
	public InteractMode interactMode;
	public Color[] colors;
	public Grid hexGrid;

	public Text toggleSelectModeLabel;
	public Text toggleInteractModeLabel;

	Color activeColor;
	int activeElevation = 0;

	void Awake() {
		SelectColor(0);
	}

	void Update() {
		if (Input.GetMouseButton(0) &&
			!EventSystem.current.IsPointerOverGameObject()) {
			HandleInput();
		}
	}

	void HandleInput() {
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

	void EditEdge(Edge edge) {
		
    }

	void EditTri(TriCell tri) {
		tri.SetColor(activeColor);
		tri.SetElevation(activeElevation);
	}

	void EditPoint(Point point) {
		// point.Refresh();
    }

	void EditHex(Point point) {
		// don't do it this way, editing the tris in the hex should trigger the refreshes.
		// point.RefreshHex();
	}

	public void SelectColor(int index) {
		activeColor = colors[index];
	}

	public void ChangeSelectMode() {
		selectMode = (SelectMode) (((int)selectMode + 1) % 3);
		switch (selectMode) {
			case SelectMode.Tri:
				toggleSelectModeLabel.text = "Tri Mode";
				break;
			case SelectMode.Hex:
				toggleSelectModeLabel.text = "Hex Mode";
				break;
			case SelectMode.Point:
				toggleSelectModeLabel.text = "Point Mode";
				break;
			case SelectMode.Edge:
				toggleSelectModeLabel.text = "Edge Mode";
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

public enum SelectMode { Tri, Point, Edge, Hex }
public enum InteractMode { Edit, Inspect } 