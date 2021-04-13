using UnityEngine;
using System;

/// <summary>
///  A Point represents a vertex of open space, that is the space betweena floor and the ceiling, or the floor and the sky. 
///  Points represent the junction between up to six floor cells and six ceiling cells.
///  The lack of a cell indicates a wall between neighboring cells and their respective floor/ceiling cell.
/// </summary>
public class Point
{
    public Vector2 position;
    public GridPoint gridPoint;

    public Vector2 ElevationExtents {
        get {
            if (extentsNeedUpdate) {
                RecalculateExtents();
            }
            return elevationExtents;
        }
    }
    public VertexCoordinates coordinates {
        get {
            return gridPoint.coordinates;
        }
    }

    private Vector2 elevationExtents;
    private bool extentsNeedUpdate;

    // Label
    public RectTransform uiRect;
    public TriCell[] cells;
    private Point[] neighbors;
    private Edge[] edges;

    public void UpdateExtents() {
        extentsNeedUpdate = true;
    }

    private void RecalculateExtents() {
        int maxElevation = int.MinValue;
        int minElevation = int.MaxValue;
        for (int i = 0; i < cells.Length; i++) {
            TriCell.CellCorner corner = GetCornerOfCell(i);
            if (corner != null) {
                int elevation = corner.Elevation;
                if (elevation < minElevation) {
                    minElevation = elevation;
                }
                if (elevation > maxElevation) {
                    maxElevation = elevation;
                }
            }
        }
        elevationExtents = new Vector2(minElevation, maxElevation);
        extentsNeedUpdate = false;
    }

    public Point(PointType type, int x, int z) : this(type, VertexCoordinates.FromOffsetCoordinates(x, z)) { }

    public Point(PointType type, VertexCoordinates coordinates) {
        this.position = VertexCoordinates.GetPos2DFromVertex(coordinates);
        neighbors = new Point[6];
        cells = new TriCell[12];
        edges = new Edge[6];
    }

    #region Neighbors Cells and Edges
    public Point GetNeighbor(EdgeDirection direction) {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(EdgeDirection direction, Point cell) {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public Edge GetEdge(EdgeDirection direction) {
        return edges[(int)direction];
    }

    public void SetEdge(EdgeDirection direction, Edge edge) {
        edges[(int)direction] = edge;
        GetNeighbor(direction).edges[(int)direction.Opposite()] = edge; // bypass SetEdge to avoid infinite loop
    }

    public TriCell GetCell(CellDirection direction, Vector3 normal) {
        TriangleType type = Util.GetTriTypeFromNormal(normal);
        return GetCell(direction, type);
    }

    public TriCell GetCell(CellDirection direction, TriangleType type) {
        if (type == TriangleType.Edge) {
            throw new NotImplementedException("GetCell currently not handling edge types");
        }
        return cells[GetCellIndex(direction, type)];
    }

    public void SetCell(CellDirection direction, TriCell cell, TriangleType type) {
        if (cell != null) {
            cells[GetCellIndex(direction, type)] = cell;
        } else {
            throw new ArgumentException("null cell: " + gridPoint.coordinates.ToString() + ", " + direction.ToString());
        }
    }

    public TriCell.CellCorner GetCornerOfCell(int cellIndex) {
        TriCell cell = cells[cellIndex];
        if (cell != null) {
            return cell.GetCorner(GetDirectionFromIndex(cellIndex).Opposite());
        } else {
            return null;
        }
    }
    #endregion

    private int GetCellIndex(CellDirection direction, TriangleType type) {
        return (int)direction + (type == TriangleType.Ceiling ? 6 : 0);
    }

    private CellDirection GetDirectionFromIndex(int cellIndex) {
        return (CellDirection)(cellIndex - (cellIndex > 5 ? 6 : 0));
    }
}

/// <summary>
/// A Horizontal Edge Point is one that lacks a N and S cell.
/// A Top Edge Point is one that lacks a N cell.
/// A Bottom Edge Point is one that lacks a S cell.
/// </summary>
public enum PointType { HorizontalEdge, TopEdge, BottomEdge, Center}