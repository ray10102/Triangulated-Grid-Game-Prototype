using UnityEngine;
using System;

public enum EdgeDirection
{
	NE, E, SE, SW, W, NW
}

/// <summary>
/// Represents both cells' directions relative to a point and cells' directions relative to each other.
/// </summary>
public enum CellDirection
{
	N, NE, SE, S, SW, NW
}

public enum EdgeOrientation { SWNE, EW, NWSE }

public static class EdgeOrientationExtensions
{
    /// <summary>
    /// Translates an edge diection into vertex indices for that edge's adjacent cells.
    /// x corresponds to z, y corresponds to w. xy represent the vertex indices of the top cell, while zw represent those of the bottom cell.
    /// Indices are returned in left to right order.
    /// </summary>
    public static Vector4 GetVertexIndices(this EdgeOrientation direction) {
        switch (direction) {
            case EdgeOrientation.EW:
                return new Vector4(1, 2, 2, 1);
            case EdgeOrientation.NWSE:
                return new Vector4(1, 0, 0, 1);
            case EdgeOrientation.SWNE:
                return new Vector4(2, 0, 0, 2);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public static class EdgeDirectionExtensions
{
	public static EdgeDirection Opposite(this EdgeDirection direction) {
		return (int)direction < 3 ? (direction + 3) : (direction - 3);
	}
}

public static class CellDirectionExtensions
{
	public static CellDirection Opposite(this CellDirection direction) {
		return (int)direction < 3 ? (direction + 3) : (direction - 3);
	}
    
    /// <summary>
    /// Returns the two possible directions to a cell's neighbor and the position of the cell relative to that point.
    /// </summary>
    public static void GetRelativeCellDirections(this CellDirection direction, out EdgeDirection e1, out EdgeDirection e2, out CellDirection c1, out CellDirection c2) {
        switch (direction) {
            case CellDirection.N:
                e1 = EdgeDirection.NE;
                e2 = EdgeDirection.NW;
                c1 = CellDirection.NW;
                c2 = CellDirection.NE;
                return;
            case CellDirection.SE:
                e1 = EdgeDirection.NE;
                e2 = EdgeDirection.E;
                c1 = CellDirection.S;
                c2 = CellDirection.NW;
                return;
            case CellDirection.SW:
                e1 = EdgeDirection.NW;
                e2 = EdgeDirection.W;
                c1 = CellDirection.S;
                c2 = CellDirection.NE;
                return;
            case CellDirection.NE:
                e1 = EdgeDirection.E;
                e2 = EdgeDirection.SE;
                c1 = CellDirection.SW;
                c2 = CellDirection.N;
                return;
            case CellDirection.NW:
                e1 = EdgeDirection.W;
                e2 = EdgeDirection.SW;
                c1 = CellDirection.SE;
                c2 = CellDirection.N;
                return;
            case CellDirection.S:
                e1 = EdgeDirection.SE;
                e2 = EdgeDirection.SW;
                c1 = CellDirection.NW;
                c2 = CellDirection.NE;
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}