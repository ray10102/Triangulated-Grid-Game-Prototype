using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Stores per cell data and has references to corner data.
/// </summary>
public class TriCell
{
	public GridCell gridCell;

	// Clockwise corners starting from center
	public CellCorner[] corners;
	public Vector2 centerXZ {
		get {
			return points[0].position + (IsPositive ? -1 : 1) * Vector2.up * GridMetrics.outerRadius;
        }
    }
	public AxialCellCoordinates coordinates {
		get {
			return gridCell.coordinates;
		}
    }
	public bool IsPositive {
		get {
			return gridCell.coordinates.IsPositive;
        }
    }

	// Ok in hindsight I'm p sure I'll never use this but I'm not gonna delete it now
	// TODO remove this if it never gets used
	public float AvgY {
		get {
			if (dirtyElevation) {
				RecalculateElevationValues();
            }
			return avgY;
		}
    }
	private float avgY;

	public TriType Type {
		get {
			if (dirtyElevation) {
				RecalculateElevationValues();
			}
			return type;
		}
	}
	private TriType type;
	public Vector3 Normal {
		get {
			if (dirtyElevation) {
				RecalculateElevationValues();
			}
			return normal;
		}
	}
	private Vector3 normal;
	private bool dirtyElevation;

	/// <summary>
	/// The points that form this cell, i.e., the points of this cell's corners.
	/// </summary>
	private Point[] points;
	/// <summary>
	/// The edges that form this cell, i.e., the edges between this cell's corners.
	/// </summary>
	private Edge[] edges;
	private TriCell[] neighbors;

	private static List<TriCell> triCellListTemp;

	public TriCell(Point point, AxialCellCoordinates coordinates) {
        points = new Point[3];
		neighbors = new TriCell[12];
		edges = new Edge[3];
		corners = new CellCorner[3];
		for (int i = 0; i < 3; i++) {
			corners[i] = new CellCorner(this, i);
		}
		type = (TriType)int.MaxValue; // set invalid type to trigger evaluation of type
	}

    #region Getters

    /// <summary>
    /// Gets the edge between this cell and the cell in the direction given.
    /// </summary>
    /// <param name="dir">The direction of the other cell</param>
    /// <returns></returns>
    public Edge GetEdgeBetween(CellDirection dir) {
		switch (dir) {
			// Neg cells
			case CellDirection.N:
				return points[1].GetEdge(EdgeDirection.E);
			case CellDirection.SE:
				return points[0].GetEdge(EdgeDirection.NE);
			case CellDirection.SW:
				return points[0].GetEdge(EdgeDirection.NW);
			// Pos cells
			case CellDirection.S:
				return points[1].GetEdge(EdgeDirection.W);
			case CellDirection.NW:
				return points[0].GetEdge(EdgeDirection.SW);
			case CellDirection.NE:
				return points[0].GetEdge(EdgeDirection.SE);
			default:
				throw new ArgumentException();
        }
    }

	public Edge[] GetEdges() {
		if (IsPositive) {
			edges[0] = GetEdgeBetween(CellDirection.S);
			edges[1] = GetEdgeBetween(CellDirection.NW);
			edges[2] = GetEdgeBetween(CellDirection.NE);
        } else {
			edges[0] = GetEdgeBetween(CellDirection.N);
			edges[1] = GetEdgeBetween(CellDirection.SE);
			edges[2] = GetEdgeBetween(CellDirection.SW);
		}
		return edges;
	}

	public Edge GetEdgeFromCorners(int c1, int c2) {
		throw new NotImplementedException("Not implemented this bc it's never used");
		/*
		if (c1 == c2) {
			throw new ArgumentException("Corner indicies must be different");
        }
		if (c1 > c2) {
			int temp = c1;
			c1 = c2;
			c2 = temp;
        }

		if (!coordinates.IsPositive) {
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
		}*/
    }

	public TriCell GetNeighbor(GridDirection direction) {
		return neighbors[(int)direction];
    }

	public void SetNeighbor(GridDirection direction, TriCell cell) {
		neighbors[(int)direction] = cell;
    }

	public TriCell[] GetNeighbors() {
		return neighbors;
    }

	// Retrieves a temporary list of the neighbors matching the specified degree. This list will be cleared and repopulated every time this is called, so it must be consumed immediately or copied.
	public List<TriCell> GetNeighbors(params NeighborDegree[] degrees) {
		if (triCellListTemp == null) {
			triCellListTemp = new List<TriCell>();
        }
		triCellListTemp.Clear();
		bool isPositive = IsPositive;
		foreach(NeighborDegree degree in degrees) {
			switch (degree) {
				case NeighborDegree.Primary:
					for (int i = 0; i < 3; i++) { // every fourth neighbor starting w 2 or 0
                        triCellListTemp.Add(neighbors[(isPositive ? 2 : 0) + i * 4]);
					}
					break;
				case NeighborDegree.Secondary:
					for (int i = 0; i < 3; i++) { // every fourth neighbor starting w 2 or 0
						triCellListTemp.Add(neighbors[(isPositive ? 0 : 2) + i * 4]);
					}
					break;
				case NeighborDegree.Tertiary:
					for (int i = 0; i < 6; i++) { // every other neighbor starting with 1
						triCellListTemp.Add(neighbors[1 + i * 2]);
					}
					break;
				default:
					throw new ArgumentException();
			}
        }
		return triCellListTemp;
	}

	public Point GetPoint(int index) {
		if (points[index] == null) {
			Debug.LogError(coordinates.ToStringOnSeparateLines() + ", " + index.ToString());
        }
		return points[index];
    }

	public CellCorner GetCorner(CellDirection dir) {
		TriangleType type = Util.GetTriTypeFromNormal(Normal);
		if (ValidateCornerDirection(dir)) {
			switch (dir) {
				case CellDirection.N:
					return corners[0];
				case CellDirection.S:
					return corners[0];
				case CellDirection.SE:
					return type == TriangleType.Floor ? corners[1] : corners[2];
				case CellDirection.NW:
					return type == TriangleType.Floor ? corners[1] : corners[2];
				case CellDirection.SW:
					return type == TriangleType.Floor ? corners[2] : corners[1];
				case CellDirection.NE:
					return type == TriangleType.Floor ? corners[2] : corners[1];
				default:
					throw new ArgumentOutOfRangeException();
            }
        }
		throw new ArgumentException();
    }

	#endregion

	#region Setters
	public void SetPoint(CellDirection dir, Point point) {
		if (point == null) {
			throw new ArgumentException("Trying to connect an invalid point to this cell: " + coordinates.ToString());
        }
		TriangleType type = Util.GetTriTypeFromNormal(Normal);
		point.SetCell(dir.Opposite(), this, type);
		if (type == TriangleType.Floor && (dir == CellDirection.NW || dir == CellDirection.SE) || 
			type == TriangleType.Ceiling && (dir == CellDirection.NE || dir == CellDirection.SW)) {
			points[1] = point;
        } else if (type == TriangleType.Floor && (dir == CellDirection.NE || dir == CellDirection.SW) ||
			type == TriangleType.Ceiling && (dir == CellDirection.NW || dir == CellDirection.SE)) {
			points[2] = point;
        } else {
			points[0] = point;
        }
    }

	public void SetElevation(int elevation) {
		foreach(CellCorner corner in corners) {
			corner.Elevation = elevation;
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

    #endregion

    #region Util
	/// <summary>
	/// Returns the XZ manhattan distance to the other cell's cordinates
	/// </summary>
	public int ManhattanDistanceTo(TriCell other) {
		return coordinates.ManhattanDistanceTo(other.gridCell.coordinates);
    }

	/// <summary>
	/// Returns the XZ componentwise distance to the other cell's cordinates
	/// </summary>
	public Vector3 ComponentwiseDistanceTo(TriCell other) {
		return coordinates.ComponentwiseDistance(other.gridCell.coordinates);
	}

	/// <summary>
	/// Returns the XZ radial distance to the other cell's cordinates
	/// </summary>
	public int RadialDistanceTo(TriCell other) {
		return coordinates.RadialDistance(other.gridCell.coordinates);
	}
	#endregion

	/// <summary>
	/// Marks elevation change to defer recalculation of elevation dependent values until after all elevations have been set
	/// </summary>
	private void DirtyElevation() {
		dirtyElevation = true;
    }

	private void RecalculateElevationValues() {
		// Normal 
		Vector3 u = corners[1].Position - corners[0].Position;
		Vector3 v = corners[2].Position - corners[1].Position;
		normal = new Vector3(
			u.y * v.z - u.z * v.y,
			u.z * v.x - u.x * v.z,
			u.x * v.y - u.y * v.x);

		// Type
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

		// AvgY
		int sumY = 0;
		for (int i = 0; i < corners.Length; i++) {
			sumY = sumY + corners[i].Elevation;
        }
		avgY = sumY / 3f * GridMetrics.elevationStep;

		// reset dirty elevation flag
		dirtyElevation = false;
	}

	private void Refresh() {
		gridCell.chunk.Refresh();
		// Update the neighboring points that render affected edges
		if (!IsPositive) { // Positive cells generally handle cliff triangulation, so negative cells must refresh neighbors
			List<TriCell> neighbors = GetNeighbors(NeighborDegree.Primary);
			foreach (TriCell neighbor in neighbors) {
				if (neighbor != null) {
					neighbor.Refresh();
				}
            }
		}
	}

	private bool ValidateCellDirection(CellDirection direction) {
		return ((!IsPositive &&
			(direction == CellDirection.N || direction == CellDirection.SE || direction == CellDirection.SW)) ||
			(IsPositive &&
			(direction == CellDirection.S || direction == CellDirection.NE || direction == CellDirection.NW)));
	}
	private bool ValidateCornerDirection(CellDirection direction) {
		return ((IsPositive &&
			(direction == CellDirection.N || direction == CellDirection.SE || direction == CellDirection.SW)) ||
			(!IsPositive &&
			(direction == CellDirection.S || direction == CellDirection.NE || direction == CellDirection.NW)));
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
				cell.DirtyElevation();
				cell.GetPoint(index).UpdateExtents();
				cell.Refresh();
			}
		}
		public Vector3 Position {
			get {
				return Util.ToVec3(cell.GetPoint(index).position) + Util.ElevationToVec3(Elevation);
            }
        }
		public Color Color {
			get {
				if (cell.dirtyElevation) {
					cell.RecalculateElevationValues();
                }
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

		/// <summary>
		/// Returns the corner opposite to the edge containing this corner and the other.
		/// </summary>
		/// <param name="other">The other corner in this edge</param>
		/// <returns>The corner opposite to the edge containing this corner and the other.</returns>
		public CellCorner GetOppositeCorner(CellCorner other) {
			if (other.Equals(prevCorner)) {
				return nextCorner;
            } else if (other.Equals(nextCorner)) {
				return prevCorner;
            } else {
				throw new ArgumentException("This corner does not share an edge with the other corner");
            }
        }

		public void UpdateColor() {
			if (cell.type == TriType.Flat) {
				color = Color.white * (1f - ((float)Elevation / (float)GridMetrics.maxElevation));
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

public enum NeighborDegree { Primary, Secondary, Tertiary }