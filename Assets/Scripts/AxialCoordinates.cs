using System;
using UnityEngine;

[System.Serializable]
public struct AxialCoordinates
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

	public Vector2 UpperCoordinates2D { get; private set; }
	public Vector2 LowerCoordinates2D { get; private set; }

	[SerializeField]
	private int x, z;

	// Odd X coordinates typically require special attention
	public bool IsOdd {
		get {
			return z % 2 == 1;
		}
	}

	public AxialCoordinates(int x, int z) {
		bool isOdd = z % 2 == 1;
		this.x = x;
		this.z = z;
		UpperCoordinates2D = new Vector2(isOdd ? x * 2 + 1 : x * 2, z + 1);
		LowerCoordinates2D = new Vector2(isOdd ? x * 2 + 1 : x * 2, z);
	}

	#region Static Coordinate System Converters
	public static AxialCoordinates FromOffsetCoordinates(int x, int z) {
		return new AxialCoordinates(x - z / 2, z);
	}

	public static AxialCoordinates FromPosition(Vector3 position) {
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

		return new AxialCoordinates(iX, iZ);
	}

	public static Vector2 GetCenterFromAxial(AxialCoordinates coordinates) {
		Vector2 position;
		position.x = (coordinates.X + coordinates.Z * 0.5f) * (GridMetrics.innerRadius * 2f);
		position.y = coordinates.Z * (GridMetrics.outerRadius * 1.5f);
		return position;
	}
	#endregion

	#region GetRelativeCoordinates
	public AxialCoordinates GetRelativeCoordinates(Vector3 relativePosition) {
		return new AxialCoordinates(this.x + (int)relativePosition.y + (int)relativePosition.z, this.z + (int)relativePosition.x - (int)relativePosition.y);
	}

	public AxialCoordinates GetRelativeCoordinates(EdgeDirection direction) {
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

    public AxialCoordinates GetUpperRight() {
		return GetRelativeCoordinates(new Vector3(1, 0, 0));
	}

	public AxialCoordinates GetUpperLeft() {
		return GetRelativeCoordinates(new Vector3(0, -1, 0));
	}
	public AxialCoordinates GetRight() {
		return GetRelativeCoordinates(new Vector3(0, 0, 1));
	}

	public AxialCoordinates GetLeft() {
		return GetRelativeCoordinates(new Vector3(0, 0, -1));
	}

	public AxialCoordinates GetLowerRight() {
		return GetRelativeCoordinates(new Vector3(0, 1, 0));
	}

	public AxialCoordinates GetLowerLeft() {
		return GetRelativeCoordinates(new Vector3(-1, 0, 0));
	}
	#endregion

    #region String Methods
    public string GetPerTriangleTextLabel() {
        return UpperCoordinates2D.x.ToString() + "\n" + UpperCoordinates2D.y.ToString() + "\n\n\n" + LowerCoordinates2D.x.ToString() + "\n" + LowerCoordinates2D.y.ToString();
    }

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