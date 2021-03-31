using UnityEngine;
using System;

/// <summary>
/// Stores per cell data and has references to corner data.
/// </summary>
public class TriCell
{
    public TriType type;
	// Counter clockwise corners starting from center
	public CellCorner[] corners;

	private Point point;

	public AxialCoordinates coordinates {
		get { return point.coordinates; }
	}

	public TriCell(Point point, TriType type) {
        this.point = point;
        this.type = type;
		corners = new CellCorner[3];
		for (int i = 0; i < 3; i++) {
			corners[i] = new CellCorner(this, i);
		}
	}

	// Untested
	public TriCell GetNeighbor(CellDirection direction) {
		if ((type == TriType.Top &&
			(direction == CellDirection.N || direction == CellDirection.SE || direction == CellDirection.SW)) || 
			(type == TriType.Bottom &&
			(direction == CellDirection.S || direction == CellDirection.NE || direction == CellDirection.NW))) {
				return GetNeighborHelper(direction);
		} else {
			throw new ArgumentException();
		}
    }

	public Point GetPoint(int index) {
		// Center
		if (index == 0) {
			return point;
        } else if (index == 1) {
			if (type == TriType.Top) {
				return point.GetNeighbor(EdgeDirection.NW);
            } else {
				return point.GetNeighbor(EdgeDirection.SE);
            }
        } else if (index == 2) {
			if (type == TriType.Top) {
				return point.GetNeighbor(EdgeDirection.NE);
			} else {
				return point.GetNeighbor(EdgeDirection.SW);
			}
		} else {
			throw new ArgumentOutOfRangeException();
        }
    }

	public void SetElevation(int elevation) {
		foreach(CellCorner corner in corners) {
			corner.Elevation = elevation;
        }
    }

	public void SetColor(Color color) {
		foreach(CellCorner corner in corners) {
			corner.Color = color;
        }
    }

	private TriCell GetNeighborHelper(CellDirection direction) {
		EdgeDirection edge1, edge2;
		CellDirection cell1, cell2;
		direction.GetRelativeCellDirections(out edge1, out edge2, out cell1, out cell2);
		Point p1 = point.GetNeighbor(edge1);
		Point p2 = point.GetNeighbor(edge2);
		if (p1 != null) {
			return p1.GetCell(cell1);
		} else if (p2 != null) {
			return p2.GetCell(cell2);
		} else {
			throw new ArgumentOutOfRangeException("No neighboring cell found");
		}
	}

	private void Refresh() {
		point.Refresh();
		// Update the neighboring points that render affected edges
		if (type == TriType.Top) {
			Point NW = point.GetNeighbor(EdgeDirection.NW);
			if (NW != null) {
				NW.Refresh();
			}
		} else {
			Point SW = point.GetNeighbor(EdgeDirection.SW);
			if (SW != null) {
				SW.Refresh();
			}
		}
	}

	public class CellCorner
	{
		private int index;
		private TriCell cell;

		private int elevation;
		private Color color;

		public int Elevation {
			get {
				return elevation;
			}
			set {
				if (value == elevation) {
					return;
				}
				elevation = value;
				cell.Refresh();
			}
		}

		public Color Color {
			get {
				return color;
            }
			set {
				if (value == color) {
					return;
                }
				color = value;
				cell.Refresh();
            }
        }

		public Vector3 position {
			get {
				return cell.GetPoint(index).transform.position + Util.ElevationToVec3(Elevation);
            }
        }

		public CellCorner(TriCell cell, int index) {
			this.cell = cell;
			this.index = index;
		}
	}
}

public enum TriType { Top, Bottom }