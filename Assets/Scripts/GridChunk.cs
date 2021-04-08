using System;
using System.Collections.Generic;
using UnityEngine;

public class GridChunk : MonoBehaviour
{
	// In the future, this may need to become an array. 
	// For now, we never access cells directly, and each chunk may have a different number of cells in it, so we're gonna use a list.
	List<GridCell> cells;

	GridMesh hexMesh;

	void Awake() {
		hexMesh = GetComponentInChildren<GridMesh>();
		cells = new List<GridCell>();
	}

    private void LateUpdate() {
		hexMesh.Triangulate(cells);
		enabled = false;
    }

    public void Refresh() {
		enabled = true;
	}

	public void AddCell(int index, GridCell cell) {
		throw new NotImplementedException();
		cells[index] = cell;
		cell.chunk = this;
	}

	public void AddCell(GridCell cell) {
		cells.Add(cell);
		cell.chunk = this;
	}
}
