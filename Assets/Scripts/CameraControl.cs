﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float stickMinZoom, stickMaxZoom;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float rotationSpeed;

    public Grid grid;

    private Transform swivel, stick;
    private float zoom = 1f;
    private float rotationAngle;

    private void Awake() {
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    private void Update() {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f) {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f) {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f) {
            AdjustPosition(xDelta, zDelta);
        }
    }

    private void AdjustZoom(float delta) {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    private void AdjustPosition(float xDelta, float zDelta) {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = 
            Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * 
            damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }

    private Vector3 ClampPosition(Vector3 position) {
        // TODO: check if this is off by a half a cell in x and a cell in y. See Hex Map tutorial part 5
        float xMax =
			grid.chunkCountX * GridMetrics.chunkSizeX *
			(2f * GridMetrics.innerRadius);
		position.x = Mathf.Clamp(position.x, 0f, xMax);

		float zMax =
			grid.chunkCountZ * GridMetrics.chunkSizeZ *
			(1.5f * GridMetrics.outerRadius);
		position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }

    private void AdjustRotation(float delta) {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f) {
            rotationAngle += 360f;
        } else if (rotationAngle >= 360f) {
            rotationAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }
}
