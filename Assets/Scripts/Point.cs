using UnityEngine;
/// <summary>
///  A Point represents the 2D location of the vertex between 6 cells, the center of a hex.
/// </summary>
public class Point
{
    public Vector2 position;
    public PointType type {
        get;
        private set;
    }
    public VertexCoordinates coordinates;
    public TriCell[] cells;
    public TriCell[] ceilingCells;
    public GridChunk chunk;
    // Label
    public RectTransform uiRect;

    private Point[] neighbors;
    private Edge[] edges;

    public Point(PointType type, int x, int z) {
        this.coordinates = VertexCoordinates.FromOffsetCoordinates(x, z);
        this.position = VertexCoordinates.GetPos2DFromVertex(coordinates);
        this.type = type;
        neighbors = new Point[6];
        cells = new TriCell[6];
        edges = new Edge[6];
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
        return cells[(int)direction];
    }

    public void SetCell(CellDirection direction, TriCell cell) {
        cells[(int)direction] = cell;
        // TODO set cells for other points? idk I dont think so bc it's covered in CreateCell
        // neighbors[(int)direction].edges[(int)direction.Opposite()] = edge;
    }
    #endregion
}

public enum PointType { HorizontalEdge, TopEdge, BottomEdge, Center}