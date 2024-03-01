using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Building : MonoBehaviour
{
    public Camera cam;
    public GameObject lastHitObject;
    private GameObject[] defencesArray;

    private void Start()
    {
        lastHitObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray.origin, ray.direction * 100, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject != lastHitObject) //change last object and change new object
            {
                lastHitObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                lastHitObject = hit.collider.gameObject;
                lastHitObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            } 
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
        }
        else
        {
            lastHitObject.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            Debug.DrawRay(ray.origin, ray.direction * 200.0f, Color.green);
        }
    }
}
