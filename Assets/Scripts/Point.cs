using UnityEngine;
/// <summary>
///  A Point represents a vertex of open space, that is the space betweena floor and the ceiling, or the floor and the sky. 
///  Points represent the junction between up to six floor cells and six ceiling cells.
///  The lack of a cell indicates a wall between neighboring cells and their respective floor/ceiling cell.
/// </summary>
public class Point
{
    public Vector2 position;
    public PointType type {
        get;
        private set;
    }
    public VertexCoordinates coordinates;

    public GridChunk chunk;

    // Label
    public RectTransform uiRect;
    public TriCell[] floorCells;
    public TriCell[] ceilingCells;
    private Point[] neighbors;
    private Edge[] edges;

    public Point(PointType type, int x, int z) {
        this.coordinates = VertexCoordinates.FromOffsetCoordinates(x, z);
        this.position = VertexCoordinates.GetPos2DFromVertex(coordinates);
        this.type = type;
        neighbors = new Point[6];
        floorCells = new TriCell[6];
        ceilingCells = new TriCell[6];
        edges = new Edge[6];
    }

    public Point(PointType type, VertexCoordinates coordinates) {
        this.coordinates = coordinates;
        this.position = VertexCoordinates.GetPos2DFromVertex(coordinates);
        this.type = type;
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

    public void Refresh() {
        if (chunk) {
            chunk.Refresh();
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
        floorCells[(int)direction] = cell;
        // TODO set cells for other points? idk I dont think so bc it's covered in CreateCell
        // neighbors[(int)direction].edges[(int)direction.Opposite()] = edge;
    }
    #endregion
}

/// <summary>
/// A Horizontal Edge Point is one that lacks a N and S cell.
/// A Top Edge Point is one that lacks a N cell.
/// A Bottom Edge Point is one that lacks a S cell.
/// </summary>
public enum PointType { HorizontalEdge, TopEdge, BottomEdge, Center}