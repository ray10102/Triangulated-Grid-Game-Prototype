using System;
using UnityEngine;

[System.Serializable]
public struct VertexCoordinates
{
	public int X {
		get {
			return x;
		}
	}
	public int Z {
		get {
			return z;
		}
	}
	public int Y {
		get {
			return -X - Z;
		}
	}

	[SerializeField]
	private int x, z;

	// Odd X coordinates typically require special attention
	public bool IsOdd {
		get {
			return z % 2 == 1;
		}
	}

	public VertexCoordinates(int x, int z) {
		this.x = x;
		this.z = z;
	}

	#region Static Coordinate System Converters
	public static VertexCoordinates FromOffsetCoordinates(int x, int z) {
		return new VertexCoordinates(x - z / 2, z);
	}

	public static VertexCoordinates FromPosition(Vector3 position) {
		float x = position.x / (GridMetrics.innerRadius * 2f);
		float y = -x;

		float offset = position.z / (GridMetrics.outerRadius * 3f);
		x -= offset;
		y -= offset;

		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(-x - y);

		if (iX + iY + iZ != 0) {
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(-x - y - iZ);

			if (dX > dY && dX > dZ) {
				iX = -iY - iZ;
			} else if (dZ > dY) {
				iZ = -iX - iY;
			}
		}

		return new VertexCoordinates(iX, iZ);
	}

	public static Vector2 GetPos2DFromVertex(VertexCoordinates coordinates) {
		Vector2 position;
		position.x = (coordinates.X + coordinates.Z * 0.5f) * (GridMetrics.innerRadius * 2f);
		position.y = coordinates.Z * (GridMetrics.outerRadius * 1.5f);
		return position;
	}
	#endregion

	public AxialCellCoordinates GetRelativeCellCoordinates(CellDirection dir) {
		return new AxialCellCoordinates(this, dir);
    }

	#region GetRelativeCoordinates
	public VertexCoordinates GetRelativeCoordinates(Vector3 relativePosition) {
		return new VertexCoordinates(this.x + (int)relativePosition.y + (int)relativePosition.z, this.z + (int)relativePosition.x - (int)relativePosition.y);
	}

	public VertexCoordinates GetRelativeCoordinates(EdgeDirection direction) {
		switch (direction) {
			case EdgeDirection.NE:
				return GetUpperRight();
			case EdgeDirection.E:
				return GetRight();
			case EdgeDirection.SE:
				return GetLowerRight();
			case EdgeDirection.SW:
				return GetLowerLeft();
			case EdgeDirection.W:
				return GetLeft();
			case EdgeDirection.NW:
				return GetUpperLeft();
			default: 
				throw new ArgumentOutOfRangeException();
		}
    }

    public VertexCoordinates GetUpperRight() {
		return GetRelativeCoordinates(new Vector3(1, 0, 0));
	}

	public VertexCoordinates GetUpperLeft() {
		return GetRelativeCoordinates(new Vector3(0, -1, 0));
	}
	public VertexCoordinates GetRight() {
		return GetRelativeCoordinates(new Vector3(0, 0, 1));
	}

	public VertexCoordinates GetLeft() {
		return GetRelativeCoordinates(new Vector3(0, 0, -1));
	}

	public VertexCoordinates GetLowerRight() {
		return GetRelativeCoordinates(new Vector3(0, 1, 0));
	}

	public VertexCoordinates GetLowerLeft() {
		return GetRelativeCoordinates(new Vector3(-1, 0, 0));
	}
	#endregion

    #region String Methods

	public override string ToString() {
		return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
	}

	/// <param name="printPerTriangle">Whether the text label should display over the vertex or twice per triangle.</param>
	public string GetPerVertexTextLabel(bool printPerTriangle) {
		string result = X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
		if (printPerTriangle) {
			result = result + "\n\n" + result;
        }
		return result;
	}

	public string ToStringOnSeparateLines() {
		return X.ToString() + "\n" + Z.ToString();
	}
    #endregion
}