using ObjParser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

public class UnityObjBool : MonoBehaviour
{
    #region Fields
    Obj obj;
    Obj trepan;
    //Src data
    float[] pSrcMeshVertices = null;
    uint[] pSrcMeshFaceIndices = null;
    uint numSrcMeshVertices;
    uint numSrcMeshFaces;
    //Cut data
    float[] pCutMeshVertices = null;
    uint[] pCutMeshFaceIndices = null;
    uint numCutMeshVertices;
    uint numCutMeshFaces;

    //result
    int resultVerticesSize;
    int resultFaceIndicesSize;
    float[] resultVerticesOut;
    int[] resultFaceIndicesOut;
    uint[] resultTrianglesOut;
    uint numVerticesOut;
    uint numTrianglesOut;

    List<float> pSrcVerticesList;
    List<uint> pSrcMeshFaceIndicesList;
    List<float> pCutMeshVerticesList;
    List<uint> pCutMeshFaceIndicesList;
    string objPath = "D:\\Projects\\BooleanDemo\\Resource\\L5.obj";
    string trepanPath = "D:\\Projects\\BooleanDemo\\Resource\\HJ.obj";
    string writetrepanpath = "D:\\Projects\\BooleanDemo\\Resource\\trepan.obj";
    Transform trepanTransform;

    string trepanName = "Trepan_";
    int trepanIndex = 0;
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

    void Start()
    {
        #region init

        obj = new Obj();
        trepan = new Obj();
        obj.LoadObj(objPath);
        trepan.LoadObj(trepanPath);

        pSrcVerticesList = new List<float>();
        pSrcMeshFaceIndicesList = new List<uint>();
        pCutMeshVerticesList = new List<float>();
        pCutMeshFaceIndicesList = new List<uint>();

        trepanTransform = GameObject.Find("Trepan(Unity)").GetComponent<Transform>();

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
            uint[] uintArray = intArray.Select(j => (uint)j - 1).ToArray();
            pSrcMeshFaceIndicesList.AddRange(uintArray);
        }
        pSrcMeshFaceIndices = pSrcMeshFaceIndicesList.ToArray();

        //uint numCutMeshVertices
        numSrcMeshVertices = (uint)obj.VertexList.Count;
        //uint numSrcMeshFaces
        numSrcMeshFaces = (uint)obj.FaceList.Count;

        Common.GenerateMesh(pSrcMeshVertices, pSrcMeshFaceIndices, "L5(objfromfile)", Color.red);


        #endregion

        #region CutMesh data
        //float[] pCutMeshVertices
        for (int i = 0; i < (uint)trepan.VertexList.Count; i++)
        {
            pCutMeshVerticesList.Add((float)trepan.VertexList[i].X);
            pCutMeshVerticesList.Add((float)trepan.VertexList[i].Y);
            pCutMeshVerticesList.Add((float)trepan.VertexList[i].Z);
        }
        pCutMeshVertices = pCutMeshVerticesList.ToArray();
        //uint[] pCutMeshFaceIndices
        for (int i = 0; i < (uint)trepan.FaceList.Count; i++)
        {
            int[] intArray = trepan.FaceList[i].VertexIndexList;
            uint[] uintArray = intArray.Select(jvalue => (uint)jvalue - 1).ToArray();
            pCutMeshFaceIndicesList.AddRange(uintArray);
        }
        pCutMeshFaceIndices = pCutMeshFaceIndicesList.ToArray();
        //uint numCutMeshVertices
        numCutMeshVertices = (uint)trepan.VertexList.Count;
        //uint numCutMeshFaces
        numCutMeshFaces = (uint)trepan.FaceList.Count;

        Common.GenerateMesh(pCutMeshVertices, pCutMeshFaceIndices, "originTrepan", Color.red);
        #endregion

    }

    void Update()
    {
        #region Matrix4x4
        Matrix4x4 matrix = Matrix4x4.TRS(trepanTransform.position, trepanTransform.rotation, Vector3.one);

        matrix.m01 = -matrix.m01;
        matrix.m02 = -matrix.m02;
        matrix.m03 = -matrix.m03;
        matrix.m10 = -matrix.m10;
        matrix.m20 = -matrix.m20;

        #endregion

        #region Obtain a new vertex array(float[])
        ///The mesh vertices of the read object are transformed 
        ///in the same way as the object in the Unity scene 
        ///according to matrix4x4 to obtain a new vertex array(float[])
        Vector3[] vertices = new Vector3[trepan.VertexList.Count];
        for (int i = 0; i < trepan.VertexList.Count; i++)
        {
            vertices[i].x = (float)trepan.VertexList[i].X;
            vertices[i].y = (float)trepan.VertexList[i].Y;
            vertices[i].z = (float)trepan.VertexList[i].Z;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = matrix.MultiplyPoint(vertices[i]);
        }
        //A new Mesh Vertex array of Vector3[] of the Trepan is obtained
        //Then convert to float[]
        for (int i = 0; i < vertices.Length; i++)
        {
            pCutMeshVertices[i * 3] = vertices[i].x;
            pCutMeshVertices[i * 3 + 1] = vertices[i].y;
            pCutMeshVertices[i * 3 + 2] = vertices[i].z;
        }
        #endregion

        #region Generate new trepan Mesh and Write its Obj into the file
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Common.GenerateMesh(pCutMeshVertices, pCutMeshFaceIndices, trepanName+trepanIndex.ToString(), Color.green);
            Common.WriteObj(writetrepanpath, pCutMeshVertices,pCutMeshFaceIndices);
            trepanIndex++;
        }
        #endregion

    }
}

#region Discard

//transform.position = matrix.GetColumn(3);
//transform.rotation = Quaternion.LookRotation(matrix.GetColumn(2),matrix.GetColumn(1));
//transform.localScale = new Vector3(matrix.GetColumn(0).magnitude,matrix.GetColumn(1).magnitude,matrix.GetColumn(2).magnitude);

//var pos = trepanTransform.position;
//var rot = trepanTransform.rotation.eulerAngles;
//t.position = new Vector3(-pos.x, pos.y, pos.z);
//t.rotation = Quaternion.Euler(rot[0], -rot[1], -rot[2]);



//for (int i = 0; i < (uint)trepan.FaceList.Count; i++)
//{
//    int[] intarray = trepan.FaceList[i].VertexIndexList;
//    uint[] uintArray = intarray.Select(jvalue => (uint)jvalue - 1).ToArray();
//    pCutMeshFaceIndicesList.AddRange(uintArray);
//}
//pCutMeshFaceIndices = pCutMeshFaceIndices.ToArray();

//int[] triangles = objMesh.triangles;//读取面片数组

//for (uint i = 0; i < pCutMeshFaceIndices.Length; i += 3)
//{
//    // 更新面片的索引
//    pCutMeshFaceIndices[i] = i;
//    pCutMeshFaceIndices[i + 1] = i + 1;
//    pCutMeshFaceIndices[i + 2] = i + 2;
//}
//int[] cutmeshfaceintarray = pCutMeshFaceIndices.Select(i => (int)i).ToArray();
//objMesh.triangles = triangles;




//Vector3[] vertices = mesh.vertices;
//for(int i = 0; i < (vertices.Length - 1); i++)
//{

//}
//for (int i = 0; i < (vertices.Length); i++)
//{
//    vertices[i] = mytransform.TransformPoint(new Vector3(vertices[i].x, vertices[i].y, vertices[i].z));
//}
//float[] verticesArray = new float[vertices.Length * 3];
//for (int i = 0; i < vertices.Length; i++)
//{
//    verticesArray[i * 3] = vertices[i].x;
//    verticesArray[i * 3 + 1] = vertices[i].y;
//    verticesArray[i * 3 + 2] = vertices[i].z;
//}
//int[] triangles = mesh.triangles;
////uint[] uinttriArray = triangles.Select(i => (uint)i).ToArray();
//uint vertexCount = (uint)mesh.vertexCount;
//uint triangleCount = (uint)mesh.triangles.Length / 3;



//读取环锯obj的顶点数组
//Vector3[] vertices = new Vector3[trepan.VertexList.Count];
//for (int i = 0; i < trepan.VertexList.Count; i++)
//{
//    vertices[i].x = (float)trepan.VertexList[i].X;
//    vertices[i].y = (float)trepan.VertexList[i].Y;
//    vertices[i].z = (float)trepan.VertexList[i].Z;
//}
//Vector3[] transformedVertices = new Vector3[vertices.Length];//创建新的顶点数组
//for (int i = 0; i < transformedVertices.Length; i++)
//{
//    transformedVertices[i] = matrix.MultiplyPoint3x4(vertices[i]);
//}
////得到了转换为右手坐标系下的V3矩阵后,把读进来的外部obj的顶点值进行更新
//for (int i = 0; i < transformedVertices.Length; i++)
//{
//    pCutMeshVertices[i] = transformedVertices[i].x;
//    pCutMeshVertices[i + 1] = transformedVertices[i].y;
//    pCutMeshVertices[i + 2] = transformedVertices[i].z;
//}




//Vector3[] vertexarray = new Vector3[numCutMeshVertices];

//for (int i = 0; i < numCutMeshVertices; i++)
//{
//    vertexarray[i].x = pCutMeshVertices[i];
//    vertexarray[i].y = pCutMeshVertices[i + 1];
//    vertexarray[i].z = pCutMeshVertices[i + 2];
//}
//Vector3[] transformedVertices = new Vector3[numCutMeshVertices];
//for (int i = 0; i < vertexarray.Length; i++)
//{
//    transformedVertices[i] = matrix.MultiplyPoint3x4(vertexarray[i]);
//}

//for (int i = 0; i < transformedVertices.Length; i++)
//{
//    pCutMeshVertices[i] = transformedVertices[i].x;
//    pCutMeshVertices[i + 1] = transformedVertices[i].y;
//    pCutMeshVertices[i + 2] = transformedVertices[i].z;
//}

#endregion