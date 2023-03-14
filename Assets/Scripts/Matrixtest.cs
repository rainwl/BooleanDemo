using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrixtest : MonoBehaviour
{
    Transform originTransform;
    Transform TrepanTrans;
    // Start is called before the first frame update
    void Start()
    {
        originTransform = GameObject.Find("Cube").GetComponent<Transform>();
        TrepanTrans = GameObject.Find("default").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Matrix4x4 matrix = Matrix4x4.TRS(originTransform.position, originTransform.rotation, Vector3.one);
        TrepanTrans.position = matrix.MultiplyPoint(originTransform.position);
        TrepanTrans.rotation = matrix.rotation;
    }
}
