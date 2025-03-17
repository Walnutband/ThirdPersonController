using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCamera : MonoBehaviour
{
    [SerializeField] private Transform playerTrans;
    [SerializeField] private Vector3 posDir;
    [SerializeField] private float distance;
    [SerializeField] private Vector3 posOffset;


    // Update is called once per frame
    void Update()
    {
        posDir = playerTrans.rotation * Vector3.back;
        transform.position = playerTrans.position + posDir * distance;
        transform.LookAt(playerTrans);
        transform.Translate(posOffset, Space.Self);
    }
}
