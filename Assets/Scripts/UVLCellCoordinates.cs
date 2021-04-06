using System;
using UnityEngine;

public struct UVLCellCoordinates
{
	public int U {
		get;
		private set;
	}
	public int V {
		get;
		private set;
	}
	// 1 for positive (left) tri, 0 for neg (right)
	public int L {
		get;
		private set;
	}

	// Positive triangles are left triangles
	public bool IsPositive {
		get {
			return L == 1;
		}
	}

	public UVLCellCoordinates(int u, int v, int r) {
		U = u;
		V = v;
		L = r;
	}

	public UVLCellCoordinates(VertexCoordinates vert, CellDirection dir) {
		switch (dir) {
			case CellDirection.N:
				U = vert.X - 1;
				V = vert.Z;
				L = 0;
				break;
			case CellDirection.NE:
				U = vert.X;
				V = vert.Z;
				L = 1;
				break;
			case CellDirection.NW:
				U = vert.X - 1;
				V = vert.Z;
				L = 1;
				break;
			case CellDirection.S:
				U = vert.X;
				V = vert.Z - 1;
				L = 1;
				break;
			case CellDirection.SE:
				U = vert.X;
				V = vert.Z - 1;
				L = 0;
				break;
			case CellDirection.SW:
				U = vert.X - 1;
				V = vert.Z - 1;
				L = 0;
				break;
			default:
				throw new ArgumentException();
		}
    }

	public Vector2 GetOffsetCoodinates() {
		return new Vector2((U + V / 2) * 2 + (IsPositive ? 0 : 1), V);
	}

	public AxialCellCoordinates ToAxial() {
		int Y = -U - V + L;
		return new AxialCellCoordinates(U, Y, V);
	}

	public ContinuousCellCoordinates ToCCS() {
		throw new NotImplementedException();
	}

	#region Static Coordinate System Converters
	public static UVLCellCoordinates FromOffsetCoordinates(int x, int z) {
		return new UVLCellCoordinates(x / 2 - z / 2, z, x % 2);
	}

	public static UVLCellCoordinates FromPosition(Vector3 position) {
		float y = position.z / GridMetrics.triHeight;
		float x = (position.x - (GridMetrics.innerRadius * Mathf.Floor(y))) / GridMetrics.edgeLength;
		bool isLeft = y < (-GridMetrics.triHeight / GridMetrics.edgeLength) * x + GridMetrics.triHeight;
		return new UVLCellCoordinates((int)x, (int)y, isLeft ? 1 : 0);
	}
	#endregion

	#region GetRelativeCoordinates
	public UVLCellCoordinates GetRelativeCoordinates(Vector3 relativePosition) {
		return new UVLCellCoordinates(U + (int)relativePosition.x, V + (int)relativePosition.y, L + (int)relativePosition.z);
	}

	public UVLCellCoordinates GetRelativeCoordinates(CellDirection direction) {
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

	public UVLCellCoordinates GetUpperRight() {
		return GetRelativeCoordinates(new Vector3(0, 0, -1));
	}

	public UVLCellCoordinates GetUpperLeft() {
		return GetRelativeCoordinates(new Vector3(-1, 0, -1));
	}
	
	public UVLCellCoordinates GetLower() {
		return GetRelativeCoordinates(new Vector3(0, -1, -1));
	}

	public UVLCellCoordinates GetUpper() {
		return GetRelativeCoordinates(new Vector3(0, 1, 1));
	}

	public UVLCellCoordinates GetLowerRight() {
		return GetRelativeCoordinates(new Vector3(1, 0, 1));
	}

	public UVLCellCoordinates GetLowerLeft() {
		return GetRelativeCoordinates(new Vector3(0, 0, 1));
	}
	#endregion

	#region String Methods
	public override string ToString() {
		return "(" + U.ToString() + ", " + V.ToString() + ", " + L.ToString() + ")";
	}

	/// <param name="printPerTriangle">Whether the text label should display over the vertex or twice per triangle.</param>
	public string ToStringOnSeparateLines() {
		return U.ToString() + "\n" + V.ToString() + "\n" + L.ToString();
	}
	#endregion
}
