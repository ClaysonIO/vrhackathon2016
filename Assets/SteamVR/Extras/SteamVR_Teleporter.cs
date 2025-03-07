﻿using UnityEngine;
using System.Collections;

public class SteamVR_Teleporter : MonoBehaviour
{
    public enum TeleportType
    {
        TeleportTypeUseTerrain,
        TeleportTypeUseCollider,
        TeleportTypeUseZeroY
    }

    public bool teleportOnClick = true;
    public TeleportType teleportType = TeleportType.TeleportTypeUseZeroY;
    Transform reference;
    SteamVR_TrackedObject trackedObj;

    // Use this for initialization
    void Start ()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        Transform eyeCamera = GameObject.FindObjectOfType<SteamVR_Camera>().GetComponent<Transform>();
        // The referece point for the camera is two levels up from the SteamVR_Camera
        reference = eyeCamera.parent.parent;

        if (teleportType == TeleportType.TeleportTypeUseTerrain)
        {
            // Start the player at the level of the terrain
            reference.position = new Vector3(reference.position.x, Terrain.activeTerrain.SampleHeight(reference.position), reference.position.z);
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Grip))
        {
            transform.FindChild("Laser").gameObject.SetActive(true);
        }
        if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Grip))
        {
            transform.FindChild("Laser").gameObject.SetActive(false);
            DoClick();
        }

    }

    void DoClick()
    {
        if (teleportOnClick)
        {
            // Teleport
            float refY = reference.position.y;

            Plane plane = new Plane(Vector3.up, -refY);
            Ray ray = new Ray(this.transform.position, transform.forward);

            bool hasGroundTarget = false;
            float dist = 0f;
            if (teleportType == TeleportType.TeleportTypeUseCollider)
            {
                RaycastHit hitInfo;
                TerrainCollider tc = Terrain.activeTerrain.GetComponent<TerrainCollider>();
                hasGroundTarget = tc.Raycast(ray, out hitInfo, 1000f);
                dist = hitInfo.distance;
            }
            else if (teleportType == TeleportType.TeleportTypeUseCollider)
            {
                RaycastHit hitInfo;
                Physics.Raycast(ray, out hitInfo);
                dist = hitInfo.distance;
            }
            else
            {
                hasGroundTarget = plane.Raycast(ray, out dist);
            }
            if (hasGroundTarget)
            {
                Vector3 newPos = ray.origin + ray.direction * dist - new Vector3(reference.GetChild(0).localPosition.x, 0f, reference.GetChild(0).localPosition.z);

                reference.position = newPos;
            }
        }
    }
}
