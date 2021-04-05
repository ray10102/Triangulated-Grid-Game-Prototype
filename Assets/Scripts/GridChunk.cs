using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridChunk : MonoBehaviour
{
	Point[] points;

	GridMesh hexMesh;

	void Awake() {
		hexMesh = GetComponentInChildren<GridMesh>();

		points = new Point[GridMetrics.chunkSizeX * GridMetrics.chunkSizeZ];
	}

    private void LateUpdate() {
		hexMesh.Triangulate(points);
		enabled = false;
    }

    public void Refresh() {
		enabled = true;
	}

	public void AddPoint(int index, Point point) {
		points[index] = point;
		point.chunk = this;
	}
}
