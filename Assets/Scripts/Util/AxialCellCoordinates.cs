using System;
using System.Collections.Generic;
using UnityEngine;

public struct AxialCellCoordinates
{
	public int X {
		get;
		private set;
	}
	public int Z {
		get;
		private set;
	}
	public int Y {
		get;
		private set;
	}

	// Positive triangles sum to 1, negative triangles sum to 0
	public bool IsPositive {
		get {
			return (X + Y + Z) == 1;
		}
	}

	public List<AxialCellCoordinates> coordinateContainer;

	public AxialCellCoordinates(int x, int y, int z) {
		X = x;
		Y = y;
		Z = z;
		coordinateContainer = new List<AxialCellCoordinates>();
	}

	public AxialCellCoordinates(VertexCoordinates vert, CellDirection dir) {
		coordinateContainer = new List<AxialCellCoordinates>();
		switch (dir) {
			case CellDirection.N:
				X = vert.X - 1;
				Y = vert.Y + 1;
				Z = vert.Z;
				break;
			case CellDirection.NE:
				X = vert.X;
				Y = vert.Y + 1;
				Z = vert.Z;
				break;
			case CellDirection.SE:
				X = vert.X;
				Y = vert.Y + 1;
				Z = vert.Z - 1;
				break;
			case CellDirection.S:
				X = vert.X;
				Y = vert.Y + 2;
				Z = vert.Z - 1;
				break;
			case CellDirection.SW:
				X = vert.X - 1;
				Y = vert.Y + 2;
				Z = vert.Z - 1;
				break;
			case CellDirection.NW:
				X = vert.X - 1;
				Y = vert.Y + 2;
				Z = vert.Z;
				break;
			default:
				throw new ArgumentException();
		}
	}

    #region Coordinate selection
	public List<AxialCellCoordinates> GetCoordinatesInDirection(int direction, int distance) {
		coordinateContainer.Clear();
		AxialCellCoordinates last = this;
		if ((direction & 1) == 0) { // In cell direction
			AxialCellCoordinates next;
			for (int i = 0; i < distance; i++) {
				next = last.StepToward(((GridDirection)direction).ToCellDirection());
				coordinateContainer.Add(next);
				last = next;
			}
		} else { // In edge direction
			AxialCellCoordinates next;
			for (int i = 0; i < distance; i++) {
				next = last.StepToward(last.GetCellDirectionForEdgeDirection(((GridDirection)direction).ToEdgeDirection()));
				coordinateContainer.Add(next);
				last = next;
			}
		}
		return coordinateContainer;
	}
	#endregion

	#region Distance
	public static int ManhattanDistance(AxialCellCoordinates first, AxialCellCoordinates second) {
		return first.ManhattanDistanceTo(second);
	}

	public static Vector3 ComponentwiseDistance(AxialCellCoordinates first, AxialCellCoordinates second) {
		return first.ComponentwiseDistance(second);
    }

	public static int RadialDistance(AxialCellCoordinates first, AxialCellCoordinates second) {
		return first.RadialDistance(second);
    }

	public int ManhattanDistanceTo(AxialCellCoordinates other) {
		return Mathf.Abs(X - other.X) + Mathf.Abs(Y - other.Y) + Mathf.Abs(Z - other.Z);
	}

	public Vector3 ComponentwiseDistance(AxialCellCoordinates other) {
		return new Vector3(Mathf.Abs(X - other.X), Mathf.Abs(Y - other.Y), Mathf.Abs(Z - other.Z));
    }

	/// <summary>
	/// Radial distance returns the max component of componentwise distance.
	/// </summary>
	public int RadialDistance(AxialCellCoordinates other) {
		return Mathf.Max(Mathf.Max(Mathf.Abs(X - other.X), Mathf.Abs(Y - other.Y)), Mathf.Abs(Z - other.Z));
    }

	// TODO needs testing
	public Vector2 GetOffsetCoodinates() {
		return new Vector2((X + Z / 2) * 2 + (IsPositive ? 0 : 1) + (Z % 2 == 1 ? 1 : 0), Z);
    }

	public UVLCellCoordinates ToUVL() {
		return new UVLCellCoordinates(X, Z, X + Y + Z);
    }

	public ContinuousCellCoordinates ToCCS() {
		throw new NotImplementedException();
	}
	#endregion

	#region Coordinate arithmetic
	public AxialCellCoordinates Plus(Vector3 amt) {
		return new AxialCellCoordinates(X + (int)amt.x, Y + (int)amt.y, Z + (int)amt.z);
	}

	public AxialCellCoordinates StepToward(CellDirection dir) {
		// Cross edge
		if ((this.IsPositive && (dir == CellDirection.NE || dir == CellDirection.NW || dir == CellDirection.S)) ||
			(!this.IsPositive && (dir == CellDirection.SE || dir == CellDirection.SW || dir == CellDirection.N))) {
			return this.Plus(AcrossEdge(dir));
        } else {
			return this.Plus(AcrossCorner(dir));
        }
	}

	/// <summary>
	/// Translates an edge direction into the cell direction of the next cell based
	/// on whether this one is a positive or negative cell.
	/// </summary>
	private CellDirection GetCellDirectionForEdgeDirection(EdgeDirection dir) {
		switch (dir) {
			case EdgeDirection.E:
				return IsPositive ? CellDirection.NE : CellDirection.SE;
			case EdgeDirection.W:
				return IsPositive ? CellDirection.NW : CellDirection.SW;
			case EdgeDirection.SE:
				return IsPositive ? CellDirection.S : CellDirection.SE;
			case EdgeDirection.NW:
				return IsPositive ? CellDirection.NW : CellDirection.N;
			case EdgeDirection.SW:
				return IsPositive ? CellDirection.S : CellDirection.SW;
			case EdgeDirection.NE:
				return IsPositive ? CellDirection.NE : CellDirection.N;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private static Vector3 AcrossCorner(CellDirection dir) {
		switch (dir) {
			case CellDirection.N:
				return new Vector3(-1, -1, 1);
			case CellDirection.S:
				return new Vector3(1, 1, -1);
			case CellDirection.SE:
				return new Vector3(1, -1, -1);
			case CellDirection.NW:
				return new Vector3(-1, 1, 1);
			case CellDirection.SW:
				return new Vector3(-1, 1, -1);
			case CellDirection.NE:
				return new Vector3(1, -1, 1);
			default:
				throw new ArgumentOutOfRangeException();
		}
    }

	public static Vector3 AcrossEdge(CellDirection dir) {
		switch (dir) {
			case CellDirection.N:
				return new Vector3(0, 0, 1);
			case CellDirection.S:
				return new Vector3(0, 0, -1);
			case CellDirection.SE:
				return new Vector3(1, 0, 0);
			case CellDirection.NW:
				return new Vector3(-1, 0, 0);
			case CellDirection.SW:
				return new Vector3(0, 1, 0);
			case CellDirection.NE:
				return new Vector3(0, -1, 0);
			default:
				throw new ArgumentOutOfRangeException();
        }
    }
	#endregion

	#region Static Coordinate System Converters
	public static AxialCellCoordinates FromOffsetCoordinates(int x, int z) {
		int newX = x / 2  - z / 2;
		int newY = -newX - z;
		if (x % 2 == 0) {
			newY++;
        }
		return new AxialCellCoordinates(newX, newY , z);
	}

	public static AxialCellCoordinates FromPosition(Vector3 position) {
		float z = position.z / GridMetrics.triHeight;
		float x = (position.x - (GridMetrics.innerRadius * Mathf.Floor(z))) / GridMetrics.edgeLength;
		bool isLeft = z < (-GridMetrics.triHeight / GridMetrics.edgeLength) * x + GridMetrics.triHeight;
		int y = isLeft ? -(int)x - (int)z + 1 : -(int)x - (int)z;
		return new AxialCellCoordinates((int)x, y, (int)z);
	}
	#endregion

	#region GetRelativeCoordinates
	public ContinuousCellCoordinates GetRelativeCoordinates(Vector3 relativePosition) {
		return new ContinuousCellCoordinates(X + (int)relativePosition.x, Y + (int)relativePosition.y, Z + (int)relativePosition.z);
	}

	public ContinuousCellCoordinates GetRelativeCoordinates(CellDirection direction) {
		switch (direction) {
			case CellDirection.NE:
				return GetUpperRight();
			case CellDirection.N:
				return GetUpper();
			case CellDirection.SE:
				return GetLowerRight();
			case CellDirection.SW:
				return GetLowerLeft();
			case CellDirection.S:
				return GetLower();
			case CellDirection.NW:
				return GetUpperLeft();
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public ContinuousCellCoordinates GetUpperRight() {
		return GetRelativeCoordinates(new Vector3(0, -1, -1));
	}

	public ContinuousCellCoordinates GetUpperLeft() {
		return GetRelativeCoordinates(new Vector3(-1, -1, 0));
	}
	public ContinuousCellCoordinates GetUpper() {
		return GetRelativeCoordinates(new Vector3(1, 0, 1));
	}

	public ContinuousCellCoordinates GetLower() {
		return GetRelativeCoordinates(new Vector3(-1, 0, -1));
	}

	public ContinuousCellCoordinates GetLowerRight() {
		return GetRelativeCoordinates(new Vector3(1, 1, 0));
	}

	public ContinuousCellCoordinates GetLowerLeft() {
		return GetRelativeCoordinates(new Vector3(0, 1, 1));
	}
	#endregion

	#region String Methods
	public override string ToString() {
		return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
	}

	/// <param name="printPerTriangle">Whether the text label should display over the vertex or twice per triangle.</param>
	public string ToStringOnSeparateLines() {
		Vector2 offset = GetOffsetCoodinates();
		return offset.x.ToString() + "\n" + offset.y.ToString();
		string result = X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
		return result;
	}
	#endregion
}
