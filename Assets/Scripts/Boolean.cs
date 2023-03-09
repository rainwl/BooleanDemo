using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ObjParser;
using UnityEngine.UIElements;
using System.Linq;
using System;

public class Boolean : MonoBehaviour
{
    #region thinking

    //transformpoint,vertices trans,leftright,collider,同步环锯和Unity场景中的圆柱体,然后换掉,注意transform转换,然后再次写入到obj文件

    //首先,通过ObjParser将本地obj的顶点数组读出来,面数组读出来,以及size
    #endregion

    #region Fields

    //ObjParser dll
    Obj obj;
    Obj hj;

    //C++ dll
    float[] pSrcMeshVertices = null;
    uint[] pSrcMeshFaceIndices = null;
    uint numSrcMeshVertices;
    uint numSrcMeshFaces;

    float[] pCutMeshVertices = null;
    uint[] pCutMeshFaceIndices = null;
    uint numCutMeshVertices;
    uint numCutMeshFaces;

    int resultVerticesSize;
    int resultFaceIndicesSize;

    List<float> pSrcVerticesList;
    List<uint> pSrcMeshFaceIndicesList;
    List<float> pCutMeshVerticesList;
    List<uint> pCutMeshFaceIndicesList;

    //ObjParser Write Path
    string objPath = "D:\\Projects\\BooleanDemo\\Resource\\srcout2.obj";
    string hjPath = "D:\\Projects\\BooleanDemo\\Resource\\cutout2.obj";
    #endregion

    #region C++ Dll Import

    [DllImport("boolean.dll", EntryPoint = "queryInfoNoUVs", CallingConvention = CallingConvention.Cdecl)]
    public extern unsafe static int queryInfoNoUVs(
        float[] pSrcMeshVertices, uint[] pSrcMeshFaceIndices, uint numSrcMeshVertices, uint numSrcMeshFaces,
        float[] pCutMeshVertices, uint[] pCutMeshFaceIndices, uint numCutMeshVertices, uint numCutMeshFaces,
        ref int resultVerticesSize, ref int resultFaceIndicesSize);

    [DllImport("boolean.dll", EntryPoint = "getBooleanResultNoUVs", CallingConvention = CallingConvention.Cdecl)]
    public extern unsafe static void getBooleanResultNoUVs(float[] resultVerticesOut, int[] resultFaceIndicesOut);

    #endregion

    #region Implement
    void Start()
    {
        #region Init
        obj = new Obj();
        hj = new Obj();
        obj.LoadObj(objPath);
        hj.LoadObj(hjPath);
        pSrcVerticesList = new List<float>();
        pSrcMeshFaceIndicesList = new List<uint>();
        pCutMeshVerticesList = new List<float>();
        pCutMeshFaceIndicesList = new List<uint>();
        #endregion

        #region SrcMesh data
        //float[] pSrcMeshVertices
        for (int i = 0; i < (uint)obj.VertexList.Count; i++)
        {
            pSrcVerticesList.Add((float)obj.VertexList[i].X);
            pSrcVerticesList.Add((float)obj.VertexList[i].Y);
            pSrcVerticesList.Add((float)obj.VertexList[i].Z);
        }
        pSrcMeshVertices = pSrcVerticesList.ToArray();

        //uint[] pSrcMeshFaceIndices      
        for (int i = 0; i < (uint)obj.FaceList.Count; i++)
        {
            int[] intArray = obj.FaceList[i].VertexIndexList;
            uint[] uintArray = intArray.Select(j => (uint)j-1).ToArray();
            pSrcMeshFaceIndicesList.AddRange(uintArray);
        }
        pSrcMeshFaceIndices = pSrcMeshFaceIndicesList.ToArray();

        //uint numCutMeshVertices
        numSrcMeshVertices = (uint)obj.VertexList.Count;
        //uint numSrcMeshFaces
        numSrcMeshFaces = (uint)obj.FaceList.Count;

        #endregion

        #region CutMesh data
        //float[] pCutMeshVertices
        for(int i = 0; i < (uint)hj.VertexList.Count; i++)
        {
            pCutMeshVerticesList.Add((float)hj.VertexList[i].X);
            pCutMeshVerticesList.Add((float)hj.VertexList[i].Y);
            pCutMeshVerticesList.Add((float)hj.VertexList[i].Z);
        }
        pCutMeshVertices = pCutMeshVerticesList.ToArray();
        //uint[] pCutMeshFaceIndices
        for(int i = 0;i < (uint)hj.FaceList.Count; i++)
        {
            int[] intArray = hj.FaceList[i].VertexIndexList;
            uint[] uintArray = intArray.Select(jvalue => (uint)jvalue-1).ToArray();
            pCutMeshFaceIndicesList.AddRange(uintArray);
        }
        pCutMeshFaceIndices = pCutMeshFaceIndicesList.ToArray();
        //uint numCutMeshVertices
        numCutMeshVertices = (uint)hj.VertexList.Count;
        //uint numCutMeshFaces
        numCutMeshFaces = (uint)hj.FaceList.Count;


        #endregion

        #region output
        //for (int i = 0; i < pSrcMeshVertices.Length; i++)
        //{
        //    Debug.Log(pSrcMeshVertices[i]);
        //}
        //for (int i = 0; i < pSrcMeshFaceIndices.Length; i++)
        //{
        //    Debug.Log(pSrcMeshFaceIndices[i]);
        //}
        //Debug.Log("numSrcMeshVertices: " + numSrcMeshVertices);
        //Debug.Log("numSrcMeshFaces: " + numSrcMeshFaces);
        //Debug.Log("numCutMeshVertices: " + numCutMeshVertices);
        //Debug.Log("numCutMeshFaces: " + numCutMeshFaces);
        //for (int i = 0; i < pCutMeshVertices.Length; i++)
        //{
        //    Debug.Log(pCutMeshVertices[i]);
        //}
        //for (int i = 0; i < pCutMeshFaceIndices.Length; i++)
        //{
        //    Debug.Log(pCutMeshFaceIndices[i]);
        //}

        #endregion

        #region IMP
        var error = queryInfoNoUVs(pSrcMeshVertices, 
            pSrcMeshFaceIndices,
            numSrcMeshVertices,
            numSrcMeshFaces,
            pCutMeshVertices,
            pCutMeshFaceIndices,
            numCutMeshVertices,
            numCutMeshFaces,
            ref resultVerticesSize,
            ref resultFaceIndicesSize
            );
        if ( error != 0 )
        {
            Debug.Log($"error:{error}");
        }

        float[] resultVerticesOut = new float[resultVerticesSize];
        int[] resultFaceIndicesOut = new int[resultFaceIndicesSize];
       
        getBooleanResultNoUVs(resultVerticesOut, resultFaceIndicesOut);
        #endregion
    }
    #endregion
}