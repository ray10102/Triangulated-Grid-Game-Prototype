using UnityEngine;
using System;

public class Edge
{
    public EdgeOrientation direction;
    // cells[0] is the top cell and cells[1] is the bottom cell.
    public TriCell[] cells;

    /// <summary>
    /// Checks if all corresponding vertices match. If they don't, then this edge features a cliff.
    /// </summary>
    public bool IsCliff {
        get {
            return cells[0] != null && cells[1] != null
                && (Corners[0].Elevation != Corners[2].Elevation
                 || Corners[1].Elevation != Corners[3].Elevation);
        }
    }

    public TriCell.CellCorner[] Corners {
        get {
            if (corners == null) {
                corners = new TriCell.CellCorner[4];
                for(int i = 0; i < vertexIndices.Length; i++) {
                    corners[i] = cells[i / 2].corners[vertexIndices[i]];
                }
            }
            return corners;
        }
    }

    public int LowSideIndex {
        get {
            if (Corners[0].Elevation < Corners[2].Elevation || Corners[1].Elevation < Corners[3].Elevation) {
                return 0;
            } else {
                // Will return 1 if this edge is not a cliff. Only use this on actual cliffs.
                return 1;
            }
        }
    }

    private TriCell.CellCorner[] corners;
    private int[] vertexIndices;

    public Edge(EdgeOrientation direction, TriCell topCell, TriCell bottomCell) {
        this.direction = direction;
        vertexIndices = direction.GetVertexIndices();
        cells = new TriCell[2];
        cells[0] = topCell;
        cells[1] = bottomCell;
    }
}