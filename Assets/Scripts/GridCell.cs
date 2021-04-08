using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A GridCell is a container for all TriCells with the same coordinates. 
/// Cells are stored in order from lowest to highest, and thus alternating between floors and ceilings.
/// A GridCell always contains an odd number of cells, since every cell must start and end with a floor.
/// </summary>
public class GridCell
{
    public GridChunk chunk;
    public int count {
        get {
            return cells.Count;
        }
    }

    // A grid space will always have an odd number of cells, since no space can end in a ceiling
    private List<TriCell> cells;

    public GridCell(TriCell cell) {
        cells = new List<TriCell>();
        cells.Add(cell);
        cell.gridCell = this;
    }

    public TriCell GetCell(int index) {
        return cells[index];
    }

    public TriCell GetFloor(int index) {
        if (index * 2 > cells.Count - 1) {
            throw new ArgumentOutOfRangeException();
        }
        return cells[index * 2];
    }
    public TriCell GetCeiling(int index) {
        if (index * 2 + 1 > cells.Count - 1) {
            throw new ArgumentOutOfRangeException();
        }
        return cells[index * 2 + 1];
    }

    public TriCell GetCellAtPoint(Vector3 point) {
        // In the future, it might be useful to do this with binary search
        float smallestDiff = float.MaxValue;
        int bestMatch = int.MinValue;
        for (int i = 0; i < cells.Count; i++) {
            for (int j = 0; j < cells[i].corners.Length; j++) {
                float diff = Vector3.Distance(cells[i].corners[j].Position, point);
                if (diff < smallestDiff) {
                    bestMatch = i;
                }
            }
        }
        return cells[bestMatch];
    }

    public TriCell GetCellAtPointWithNormal(Vector3 point, Vector3 normal) {
        int offset = 0;
        if (Vector3.Angle(normal, Vector3.up) > 90f) { // ceiling
            offset = 1;
        }

        float smallestDiff = float.MaxValue;
        int bestMatch = int.MinValue;
        
        for (int i = 0; i < (offset == 1 ? cells.Count / 2 : cells.Count / 2 + 1); i++) {
            for (int j = 0; j < cells[i].corners.Length; j++) {
                float diff = Vector3.Distance(cells[i * 2 + offset].corners[j].Position, point);
                if (diff < smallestDiff) {
                    bestMatch = i * 2 + offset;
                }
            }
        }
        return cells[bestMatch];
    }
}
