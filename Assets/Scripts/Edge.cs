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
            Vector4 indicies = direction.GetVertexIndices();
            return  cells[0] != null && cells [1] != null 
                && (cells[0].corners[(int)indicies.x].Elevation != cells[1].corners[(int)indicies.z].Elevation
                 || cells[0].corners[(int)indicies.y].Elevation != cells[1].corners[(int)indicies.w].Elevation);
        }
    }

    public Vector4 Elevations {
        get {
            Vector4 indicies = direction.GetVertexIndices();
            return new Vector4(cells[0].corners[(int)indicies.x].Elevation,
                               cells[0].corners[(int)indicies.y].Elevation, 
                               cells[1].corners[(int)indicies.z].Elevation, 
                               cells[1].corners[(int)indicies.w].Elevation);
        }
    }

    public Color[] Colors {
        get {
            Vector4 indicies = direction.GetVertexIndices();
            return new Color[] {cells[0].corners[(int)indicies.x].Color,
                                cells[0].corners[(int)indicies.y].Color,
                                cells[1].corners[(int)indicies.z].Color,
                                cells[1].corners[(int)indicies.w].Color };
        }
    }

    public Edge(EdgeOrientation direction, TriCell topCell, TriCell bottomCell) {
        this.direction = direction;
        cells = new TriCell[2];
        cells[0] = topCell;
        cells[1] = bottomCell;
    }
}