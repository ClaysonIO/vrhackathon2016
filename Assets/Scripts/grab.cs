﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class grab : MonoBehaviour
{
    public LayerMask wallMask;
    public LayerMask furnitureMask;
    public LayerMask ceilingMask;
    public GameObject selectedObject = null;
    public  GameObject enteredObject = null;
    SteamVR_TrackedObject trackedObj;

    Vector3 lastPosition;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void FixedUpdate()
    {
        var device = SteamVR_Controller.Input((int)trackedObj.index);
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if(selectedObject == null) { 
                if(enteredObject != null)
                {
                    GrabObject(enteredObject);
                } else
                {
                    transform.FindChild("Laser").gameObject.SetActive(true);
                }
            }
        }
        if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (selectedObject) { 
                releaseObject(selectedObject);
            } else if(transform.FindChild("Laser").gameObject.active == true)
            {
                transform.FindChild("Laser").gameObject.SetActive(false);
                GrabObject(raycastFurniture());
            }
        }
        if (selectedObject != null)
        {
            lastPosition = transform.position;
        }

    }

    void GrabObject(GameObject whichObject)
    {
        if (whichObject.layer == LayerMask.NameToLayer("Furniture")){ 
        selectedObject = whichObject;
        switch (whichObject.tag)
        {
            case "furniture":
                pickupFurniture(whichObject);
                break;
            case "wallMounted":
                pickupWallMounted(whichObject);
                break;
            case "ceilingMounted":
                pickupCeilingMounted(whichObject);
                break;
        }
        } else if(whichObject.layer == LayerMask.NameToLayer("Menu"))
        {
            selectedObject = whichObject;
            pickupMenu(whichObject);
        } else if(whichObject.layer == LayerMask.NameToLayer("Catalogue"))
        {

        }
    }

    void releaseObject(GameObject whichObject)
    {
        if (whichObject.layer == LayerMask.NameToLayer("Furniture")){
            switch (whichObject.tag)
        {
            case "furniture":
                releaseFurniture(whichObject);

                break;
            case "wallMounted":
                releaseWallMounted(whichObject);
                break;
            case "ceilingMounted":
                releaseCeilingMounted(whichObject);
                break;
        }
    } else if(whichObject.layer == LayerMask.NameToLayer("Menu"))
        {
            releaseMenu(whichObject);
        } else if(whichObject.layer == LayerMask.NameToLayer("Catalogue"))
        {

        }
        selectedObject = null;
    }

    void pickupMenu(GameObject whichObject)
    {
        Destroy(whichObject.GetComponent<Rigidbody>());
        whichObject.transform.SetParent(transform);
    }
    void releaseMenu(GameObject whichObject)
    {
        whichObject.AddComponent<Rigidbody>();
        whichObject.GetComponent<Rigidbody>().isKinematic = true;
        whichObject.transform.SetParent(null);
    }

    void pickupFurniture(GameObject whichObject)
    {
        Destroy(whichObject.GetComponent<Rigidbody>());
        whichObject.transform.SetParent(transform);
    }
    void releaseFurniture(GameObject whichObject)
    {
        whichObject.AddComponent<Rigidbody>();
        whichObject.transform.SetParent(null);
        whichObject.GetComponent<Rigidbody>().velocity = (transform.position - lastPosition) / Time.deltaTime;
    }

    void pickupWallMounted(GameObject whichObject)
    {
        Destroy(whichObject.GetComponent<Rigidbody>());
        whichObject.transform.SetParent(transform);
    }
    void releaseWallMounted(GameObject whichObject)
    {
        whichObject.AddComponent<Rigidbody>();
        whichObject.GetComponent<Rigidbody>().isKinematic = true;
        whichObject.GetComponent<Rigidbody>().useGravity = false;
        whichObject.transform.SetParent(null);
        Vector3 wallPoint = findWallPoint(whichObject);
        Vector3 orientation = findWallNormal(whichObject);
        if (wallPoint != Vector3.zero && orientation != Vector3.zero)
        {
            StartCoroutine(moveObject(whichObject, whichObject.transform.position, wallPoint, whichObject.transform.eulerAngles, new Vector3(0f, orientation.y, 0f), .5f));
        }
    }

    void pickupCeilingMounted(GameObject whichObject)
    {
        Destroy(whichObject.GetComponent<Rigidbody>());
        whichObject.transform.SetParent(transform);
    }
    void releaseCeilingMounted(GameObject whichObject)
    {
        whichObject.AddComponent<Rigidbody>();
        whichObject.GetComponent<Rigidbody>().isKinematic = true;
        whichObject.GetComponent<Rigidbody>().useGravity = false;
        whichObject.transform.SetParent(null);
        Vector3 wallPoint = findCeilingPoint(whichObject);
        Vector3 orientation = findCeilingNormal(whichObject);
        if (wallPoint != Vector3.zero && orientation != Vector3.zero)
        {
            StartCoroutine(moveObject(whichObject, whichObject.transform.position, wallPoint, whichObject.transform.eulerAngles, new Vector3(0f, orientation.y, 0f), .5f));
        }
    }

    GameObject raycastFurniture()
    {
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit, 100f, furnitureMask);
        if (bHit)
        {
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    Vector3 findWallNormal(GameObject whichObject)
    {
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit, 100f, wallMask);
        if (bHit)
        {
            return hit.normal + hit.collider.transform.eulerAngles;
        }
        else
        {
            return Vector3.zero;
        }
    }

    Vector3 findWallPoint(GameObject whichObject)
    {
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit, 100f, wallMask);
        if (bHit)
        {
            return hit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }
    Vector3 findCeilingNormal(GameObject whichObject)
    {
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit, 100f, ceilingMask);
        if (bHit)
        {
            return hit.normal + hit.collider.transform.eulerAngles;
        }
        else
        {
            return Vector3.zero;
        }
    }

    Vector3 findCeilingPoint(GameObject whichObject)
    {
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit, 100f, ceilingMask);
        if (bHit)
        {
            return hit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }
    IEnumerator moveObject(GameObject whichObject, Vector3 startPoint, Vector3 endPoint, Vector3 startRotation, Vector3 endRotation, float howLong)
    {
        Quaternion startRot = Quaternion.Euler(startRotation);
        Quaternion endRot = Quaternion.Euler(endRotation);

        float startTime = Time.time;
        while(Time.time - startTime < howLong)
        {
            whichObject.transform.position = Vector3.Lerp(startPoint, endPoint, (Time.time - startTime) / howLong);
            whichObject.transform.rotation = Quaternion.Lerp(startRot, endRot, (Time.time - startTime) / howLong);
            yield return null;
        }
        whichObject.transform.position = endPoint;
        whichObject.transform.eulerAngles = endRotation;


    }
}
