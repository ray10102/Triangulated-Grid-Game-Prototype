using UnityEngine;
/// <summary>
///  A Point represents a vertex of open space, that is the space betweena floor and the ceiling, or the floor and the sky. 
///  Points represent the junction between up to six floor cells and six ceiling cells.
///  The lack of a cell indicates a wall between neighboring cells and their respective floor/ceiling cell.
/// </summary>
public class Point
{
    public Vector2 position;
    public VertexCoordinates coordinates;
    public Vector2 ElevationExtents {
        get {
            if (extentsNeedUpdate) {
                RecalculateExtents();
            }
            return elevationExtents;
        }
    }
    private Vector2 elevationExtents;
    private bool extentsNeedUpdate;

    // Label
    public RectTransform uiRect;
    public TriCell[] floorCells;
    public TriCell[] ceilingCells;
    private Point[] neighbors;
    private Edge[] edges;

    public void UpdateExtents() {
        extentsNeedUpdate = true;
    }

    private void RecalculateExtents() {
        int maxElevation = int.MinValue;
        int minElevation = int.MaxValue;
        for (int i = 0; i < 6; i++) {
            TriCell.CellCorner corner = GetCornerOfCell((CellDirection)i);
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

    public Point(PointType type, int x, int z) {
        this.coordinates = VertexCoordinates.FromOffsetCoordinates(x, z);
        this.position = VertexCoordinates.GetPos2DFromVertex(coordinates);
        neighbors = new Point[6];
        floorCells = new TriCell[6];
        ceilingCells = new TriCell[6];
        edges = new Edge[6];
    }

    public Point(PointType type, VertexCoordinates coordinates) {
        this.coordinates = coordinates;
        this.position = VertexCoordinates.GetPos2DFromVertex(coordinates);
        neighbors = new Point[6];
        floorCells = new TriCell[6];
        ceilingCells = new TriCell[6];
        edges = new Edge[6];
        // TODO this does not belong to the point, should move elsewhere
        if (type != PointType.TopEdge) {
            SetCell(CellDirection.N, new TriCell(this, coordinates.GetRelativeCellCoordinates(CellDirection.N)));
        }
        if (type != PointType.BottomEdge) {
            SetCell(CellDirection.S, new TriCell(this, coordinates.GetRelativeCellCoordinates(CellDirection.S)));
        }
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
        neighbors[(int)direction].edges[(int)direction.Opposite()] = edge;
    }

    public TriCell GetCell(CellDirection direction) {
        return floorCells[(int)direction];
    }

    public void SetCell(CellDirection direction, TriCell cell) {
        if (cell != null) {
            floorCells[(int)direction] = cell;
            cell.SetPoint(direction.Opposite(), this);
            // TODO set cells for other points? idk I dont think so bc it's covered in CreateCell
            // neighbors[(int)direction].edges[(int)direction.Opposite()] = edge;
        }
    }

    public TriCell.CellCorner GetCornerOfCell(CellDirection direction) {
        TriCell cell = GetCell(direction);
        if (cell != null) {
            return cell.GetCorner(direction.Opposite());
        } else {
            return null;
        }
    }
    #endregion
}

/// <summary>
/// A Horizontal Edge Point is one that lacks a N and S cell.
/// A Top Edge Point is one that lacks a N cell.
/// A Bottom Edge Point is one that lacks a S cell.
/// </summary>
public enum PointType { HorizontalEdge, TopEdge, BottomEdge, Center}