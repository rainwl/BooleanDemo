using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Boolean : MonoBehaviour
{
    #region Fields
    int resultVerticesSize;
    int resultFaceIndicesSize;
    #endregion

    #region C++ Dll Import
    //前面是mesh顶点数组,后面是数组的大小
    //最初我的写法是float*,现在直接用的[],看看不能用的话再改回去
    [DllImport("boolean.dll", EntryPoint = "queryInfoNoUVs", CallingConvention = CallingConvention.Cdecl)]
    public extern unsafe static int queryInfoNoUVs(
        float[] pSrcMeshVertices, uint[] pSrcMeshFaceIndices, uint numSrcMeshVertices, uint numSrcMeshFaces,
        float[] pCutMeshVertices, uint[] pCutMeshFaceIndices, uint numCutMeshVertices, uint numCutMeshFaces,
        ref int resultVerticesSize, ref int resultFaceIndicesSize);
    
    [DllImport("boolean.dll", EntryPoint = "getBooleanResultNoUVs", CallingConvention = CallingConvention.Cdecl)]
    public extern unsafe static void getBooleanResultNoUVs(float[] resultVerticesOut, int[] resultFaceIndicesOut);

    #endregion
    
    //  1.通过ObjParser(或者其他写法)将本地.obj的网格顶点等信息读取出来
    //  2.
    void Start()
    {

    }

}
