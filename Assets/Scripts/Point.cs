using UnityEngine;
/// <summary>
///  A TriPair represents a vertex and the triangle cells above and below that vertex in the Z axis.
/// </summary>
public class Point : MonoBehaviour
{
    public AxialCoordinates coordinates;
    public TriCell[] cells;
    public GridChunk chunk;
    // Label
    public RectTransform uiRect;

    [SerializeField]
    private Point[] neighbors;
    [SerializeField]
    private Edge[] edges;

    private void Awake() {
        neighbors = new Point[6];
        cells = new TriCell[6];
        edges = new Edge[6];
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