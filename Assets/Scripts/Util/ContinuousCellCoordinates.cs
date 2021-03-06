using System;
using UnityEngine;

public struct ContinuousCellCoordinates
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

	// Positive triangles sum to 1, negative triangles sum to -1
	public bool IsPositive {
		get {
			return (X + Y + Z) == 1;
		}
	}

	public ContinuousCellCoordinates(int x, int y, int z) {
		X = x;
		Y = y;
		Z = z;
    }

	public ContinuousCellCoordinates(VertexCoordinates vert, CellDirection dir) {
		X = vert.X;
		Y = vert.Y;
		Z = vert.Z;
		switch (dir) {
			case CellDirection.N:
				Y = Y - 1;
				break;
			case CellDirection.NE:
				X = X + 1;
				break;
			case CellDirection.SE:
				Z = Z - 1;
				break;
			case CellDirection.S:
				Y = Y + 1;
				break;
			case CellDirection.SW:
				X = X - 1;
				break;
			case CellDirection.NW:
				Z = Z + 1;
				break;
		}
	}

	public ContinuousCellCoordinates(VertexCoordinates v1, VertexCoordinates v2, VertexCoordinates v3) {
		if (v1.X == v2.X) {
			X = v1.X;
        } else if (v2.X == v3.X) {
			X = v2.X;
        } else if (v3.X == v1.X) {
			X = v3.X;
        } else {
			throw new ArgumentException("Invalid vertex coordinates: No coordinates share X");
        }

		if (v1.Y == v2.Y) {
			Y = v1.Y;
		} else if (v2.Y == v3.Y) {
			Y = v2.Y;
		} else if (v3.Y == v1.Y) {
			Y = v3.Y;
		} else {
			throw new ArgumentException("Invalid vertex coordinates: No coordinates share Y");
		}

		if (v1.Z == v2.Z) {
			Z = v1.Z;
		} else if (v2.Z == v3.Z) {
			Z = v2.Z;
		} else if (v3.Z == v1.Z) {
			Z = v3.Z;
		} else {
			throw new ArgumentException("Invalid vertex coordinates: No coordinates share Z");
		}
	}

	public static int ManhattanDistance(ContinuousCellCoordinates first, ContinuousCellCoordinates second) {
		return first.ManhattanDistanceTo(second);
    }

	public int ManhattanDistanceTo(ContinuousCellCoordinates other) {
		throw new NotImplementedException();
	}

	#region Static Coordinate System Converters
	public static ContinuousCellCoordinates FromOffsetCoordinates(int x, int z) {
		throw new NotImplementedException();
	}

	public static ContinuousCellCoordinates FromPosition(Vector3 position) {
		throw new NotImplementedException();
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
		string result = X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
		return result;
	}
	#endregion
}
