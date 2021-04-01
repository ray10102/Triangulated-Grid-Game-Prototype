using UnityEngine;
using System;

/// <summary>
/// Stores per cell data and has references to corner data.
/// </summary>
public class TriCell
{
    public TriOrientation orientation;
	// Counter clockwise corners starting from center
	public CellCorner[] corners;

	private Point point;
    private TriType type;

	public AxialCoordinates coordinates {
		get { return point.coordinates; }
	}

	public TriCell(Point point, TriOrientation orientation) {
        this.point = point;
        this.orientation = orientation;
		corners = new CellCorner[3];
		for (int i = 0; i < 3; i++) {
			corners[i] = new CellCorner(this, i);
		}
	}

	public Edge GetEdgeFromCorners(int c1, int c2) {
		if (c1 == c2) {
			throw new ArgumentException("Corner indicies must be different");
        }
		if (c1 > c2) {
			int temp = c1;
			c1 = c2;
			c2 = temp;
        }

		if (orientation == TriOrientation.Top) {
			if (c2 == 1) { // c1 must be 0
				return point.GetEdge(EdgeDirection.NW);
			} else if (c1 == 1 && c2 == 2) {
				Point NE = point.GetNeighbor(EdgeDirection.NE);
				if (NE != null) {
					return NE.GetEdge(EdgeDirection.W);
				}
				Point NW = point.GetNeighbor(EdgeDirection.NW);
				if (NW != null) {
					return NW.GetEdge(EdgeDirection.E);
				}
			}
			return point.GetEdge(EdgeDirection.NE);
        } else {
			if (c2 == 1) { // c1 must be 0
				return point.GetEdge(EdgeDirection.SE);
			} else if (c1 == 1 && c2 == 2) {
				Point SE = point.GetNeighbor(EdgeDirection.SE);
				if (SE != null) {
					return SE.GetEdge(EdgeDirection.W);
				}
				Point SW = point.GetNeighbor(EdgeDirection.SW);
				if (SW != null) {
					return SW.GetEdge(EdgeDirection.E);
				}
			}
			return point.GetEdge(EdgeDirection.SW);
		}
    }

	// Untested
	public TriCell GetNeighbor(CellDirection direction) {
		if ((orientation == TriOrientation.Top &&
			(direction == CellDirection.N || direction == CellDirection.SE || direction == CellDirection.SW)) || 
			(orientation == TriOrientation.Bottom &&
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
			if (orientation == TriOrientation.Top) {
				return point.GetNeighbor(EdgeDirection.NW);
            } else {
				return point.GetNeighbor(EdgeDirection.SE);
            }
        } else if (index == 2) {
			if (orientation == TriOrientation.Top) {
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

	private void UpdateType() {
		int minEl = Math.Min(Math.Min(corners[0].Elevation, corners[1].Elevation), corners[2].Elevation);
		int maxEl = Math.Max(Math.Max(corners[0].Elevation, corners[1].Elevation), corners[2].Elevation);
		int diff = maxEl - minEl;
		if (diff == 0) {
            SetType(TriType.Flat);
		} else if (diff < GridMetrics.slopeThresholds[0]) {
            SetType(TriType.NoCost);
		} else if (diff < GridMetrics.slopeThresholds[1]) {
            SetType(TriType.Cost);
		} else {
            SetType(TriType.NonTraversable);
		}
	}

	private void SetType(TriType value) {
		if (value != type) {
			type = value;
			foreach (CellCorner corner in corners) {
				corner.UpdateColor();
			}
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
		if (orientation == TriOrientation.Top) {
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

		private int elevation = int.MinValue;
		private Color color;
		private CellCorner prevCorner;
		private CellCorner nextCorner;

		public int Elevation {
			get {
				return elevation;
			}
			set {
				if (value == elevation) {
					return;
				}
				elevation = value;
				cell.UpdateType();
				cell.Refresh();
			}
		}

		public Vector3 Position {
			get {
				return cell.GetPoint(index).transform.position + Util.ElevationToVec3(Elevation);
            }
        }

		public Color Color {
			get {
				return color;
            }
		}

		public CellCorner PrevCorner {
			get { return prevCorner; }
			private set { 
				prevCorner = value;
				if (value != null && prevCorner.nextCorner == null) {
					prevCorner.NextCorner = this;
				}
			}
		}

		public CellCorner NextCorner {
			get { return nextCorner;  }
			private set { 
				nextCorner = value;
				if (value != null && nextCorner.prevCorner == null) {
					nextCorner.prevCorner = this;
				}
			}
		}

		public CellCorner(TriCell cell, int index) {
			this.cell = cell;
			this.index = index;
			// Because cells are created in order
			if (index > 0) {
				PrevCorner = cell.corners[index - 1];
			}
			if (index == 2) {
				NextCorner = cell.corners[0];
            }
		}

		public void UpdateColor() {
			if (cell.type == TriType.Flat) {
				color = Color.black * ((float)Elevation / (float)GridMetrics.maxElevation);
			} else if (cell.type == TriType.Cost) {
				// A yellow green, getting more yellow the steeper it is
				color = new Color(
					Mathf.InverseLerp((float)GridMetrics.slopeThresholds[0],
							   (float)GridMetrics.slopeThresholds[1],
							   (float)Elevation),
					1f,
					0f);
			} else {
				color = GridMetrics.colors[(int)cell.type];
			}
		}
	}
}

public enum TriOrientation { Top, Bottom }