using UnityEngine;
using System;

public enum EdgeDirection
{
	NW, W, SW, SE, E, NE
}

/// <summary>
/// Represents both cells' directions relative to a point and cells' directions relative to each other.
/// </summary>
public enum CellDirection
{
	N, NW, SW, S, SE, NE
}

public enum EdgeOrientation { SWNE, EW, NWSE }

public enum GridDirection
{
    N, NNW, NWW, W, SWW, SSW, S, SSE, SEE, E, NEE, NNE
}

public static class EdgeOrientationExtensions
{
    /// <summary>
    /// Translates an edge diection into vertex indices for that edge's adjacent cells.
    /// x corresponds to z, y corresponds to w. xy represent the vertex indices of the top cell, while zw represent those of the bottom cell.
    /// Indices are returned in left to right order.
    /// </summary>
    public static int[] GetVertexIndices(this EdgeOrientation direction) {
        switch (direction) {
            case EdgeOrientation.EW:
                return new int[4] { 1, 2, 2, 1 };
            case EdgeOrientation.NWSE:
                return new int[4] { 0, 1, 1, 0 };
            case EdgeOrientation.SWNE:
                return new int[4] { 2, 0, 0, 2 };
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

    public static GridDirection ToGridDirection(this EdgeDirection direction) {
        return (GridDirection)((int)direction * 2 + 1);
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

    public static int GetCenterVertIndexForCell(this CellDirection direction) {
        switch (direction) {
            case CellDirection.N:
                return 0;
            case CellDirection.NE:
                return 2;
            case CellDirection.SE:
                return 1;
            case CellDirection.S:
                return 0;
            case CellDirection.SW:
                return 2;
            case CellDirection.NW:
                return 1;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static GridDirection ToGridDirection(this CellDirection direction) {
        return (GridDirection) ((int)direction * 2);
    }
}

public static class GridDirectionExtensions
{
    public static EdgeDirection ToEdgeDirection(this GridDirection direction) {
        if (((int)direction & 1) == 0) { // even GridDirection, aka a CellDirection
            throw new ArgumentException();
        }
        return (EdgeDirection)((int)direction / 2);
    }

    public static CellDirection ToCellDirection (this GridDirection direction) {
        if (((int)direction & 1) == 1) { // even GridDirection, aka an EdgeDirection
            throw new ArgumentException();
        }
        return (CellDirection)((int)direction / 2);
    }

    public static GridDirection Opposite(this GridDirection direction) {
        return (int)direction < 6 ? (direction + 6) : (direction - 6);
    }
}