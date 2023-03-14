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

    Obj obj_A;
    Obj obj_B;

    //A data
    float[] A_VerticesArray = null;
    uint[] A_TrianglesArray = null;
    uint A_VerticesSize;
    uint A_TrianglesSize;

    //B data
    float[] B_VerticesArray = null;
    uint[] B_TrianglesArray = null;
    uint B_VerticesSize;
    uint B_TrianglesSize;

    //result
    int resultVerticesSize;
    int resultTrianglesSize;
    float[] resultVerticesOutArray;
    int[] resultTrianglesOutArray;

    //temp List
    List<float> A_VerticesList;
    List<uint> A_TrianglesList;
    List<float> B_VerticesList;
    List<uint> B_TrianglesList;

    //Path
    string objAPath = "D:\\Projects\\BooleanDemo\\Resource\\L5.obj";
    string ObjBPath = "D:\\Projects\\BooleanDemo\\Resource\\HJ.obj";
    string ObjCPath = "D:\\Projects\\BooleanDemo\\Resource\\C.obj";
    //string writeNewBPath = "D:\\Projects\\BooleanDemo\\Resource\\trepan.obj";
    Transform UnityBTransform;

    string BName = "B_";
    int BIndex = 0;
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

    /// <summary>
    /// Init the A and B Mesh data
    /// </summary>
    void Start()
    {
        #region Init

        obj_A = new Obj();
        obj_A.LoadObj(objAPath);
        A_VerticesList = new List<float>();
        A_TrianglesList = new List<uint>();

        obj_B = new Obj();
        obj_B.LoadObj(ObjBPath);
        B_VerticesList = new List<float>();
        B_TrianglesList = new List<uint>();

        UnityBTransform = GameObject.Find("B_Unity").GetComponent<Transform>();

        #endregion

        #region A Mesh data
        for (int i = 0; i < (uint)obj_A.VertexList.Count; i++)
        {
            A_VerticesList.Add((float)obj_A.VertexList[i].X);
            A_VerticesList.Add((float)obj_A.VertexList[i].Y);
            A_VerticesList.Add((float)obj_A.VertexList[i].Z);
        }
        A_VerticesArray = A_VerticesList.ToArray();
        for (int i = 0; i < (uint)obj_A.FaceList.Count; i++)
        {
            int[] intArray = obj_A.FaceList[i].VertexIndexList;
            uint[] uintArray = intArray.Select(j => (uint)j - 1).ToArray();
            A_TrianglesList.AddRange(uintArray);
        }
        A_TrianglesArray = A_TrianglesList.ToArray();
        A_VerticesSize = (uint)obj_A.VertexList.Count;
        A_TrianglesSize = (uint)obj_A.FaceList.Count;

        //Common.GenerateMesh(A_VerticesArray, A_TrianglesArray, "A_obj", Color.red);
        #endregion

        #region B Mesh data
        for (int i = 0; i < (uint)obj_B.VertexList.Count; i++)
        {
            B_VerticesList.Add((float)obj_B.VertexList[i].X);
            B_VerticesList.Add((float)obj_B.VertexList[i].Y);
            B_VerticesList.Add((float)obj_B.VertexList[i].Z);
        }
        B_VerticesArray = B_VerticesList.ToArray();
        for (int i = 0; i < (uint)obj_B.FaceList.Count; i++)
        {
            int[] intArray = obj_B.FaceList[i].VertexIndexList;
            uint[] uintArray = intArray.Select(jvalue => (uint)jvalue - 1).ToArray();
            B_TrianglesList.AddRange(uintArray);
        }
        B_TrianglesArray = B_TrianglesList.ToArray();
        B_VerticesSize = (uint)obj_B.VertexList.Count;
        B_TrianglesSize = (uint)obj_B.FaceList.Count;

        //Common.GenerateMesh(B_VerticesArray, B_TrianglesArray, "B_obj", Color.red);
        #endregion
    }

    void Update()
    {
        #region Matrix4x4
        Matrix4x4 matrix = Matrix4x4.TRS(UnityBTransform.position, UnityBTransform.rotation, Vector3.one);

        matrix.m01 = -matrix.m01;
        matrix.m02 = -matrix.m02;
        matrix.m03 = -matrix.m03;
        matrix.m10 = -matrix.m10;
        matrix.m20 = -matrix.m20;
        #endregion

        #region Obtain a new vertex array(float[]) for B
        ///The mesh vertices of the read object are transformed 
        ///in the same way as the object in the Unity scene 
        ///according to matrix4x4 to obtain a new vertex array(float[])
        Vector3[] vertices = new Vector3[obj_B.VertexList.Count];
        for (int i = 0; i < obj_B.VertexList.Count; i++)
        {
            vertices[i].x = (float)obj_B.VertexList[i].X;
            vertices[i].y = (float)obj_B.VertexList[i].Y;
            vertices[i].z = (float)obj_B.VertexList[i].Z;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = matrix.MultiplyPoint(vertices[i]);
        }
        //A new Mesh Vertex array of Vector3[] of the Trepan is obtained
        //Then convert to float[]
        for (int i = 0; i < vertices.Length; i++)
        {
            B_VerticesArray[i * 3] = vertices[i].x;
            B_VerticesArray[i * 3 + 1] = vertices[i].y;
            B_VerticesArray[i * 3 + 2] = vertices[i].z;
        }
        #endregion

        #region Continuous Boolean
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Generate new B Mesh and Write its Obj into the file
            Common.GenerateMesh(B_VerticesArray, B_TrianglesArray, BName + BIndex.ToString(), Color.green);
            //Common.WriteObj(writeNewBPath, B_VerticesArray, B_TrianglesArray);

            //Boolean and Write result obj into the file
            var error = queryInfoNoUVs(A_VerticesArray, A_TrianglesArray, A_VerticesSize, A_TrianglesSize,
                                       B_VerticesArray, B_TrianglesArray, B_VerticesSize, B_TrianglesSize,
                                       ref resultVerticesSize, ref resultTrianglesSize);
            if (error != 0) Debug.Log($"error:{error}");

            resultVerticesOutArray = new float[resultVerticesSize];
            resultTrianglesOutArray = new int[resultTrianglesSize];

            getBooleanResultNoUVs(resultVerticesOutArray, resultTrianglesOutArray);
            Common.GenerateMesh(resultVerticesOutArray, resultTrianglesOutArray, "C", Color.yellow);
            Common.WriteObj(ObjCPath, resultVerticesOutArray, resultTrianglesOutArray);
            BIndex++;

            A_VerticesArray = resultVerticesOutArray;
            A_TrianglesArray = resultTrianglesOutArray.Select(i => (uint)i).ToArray();
            A_VerticesSize = (uint)resultVerticesSize / 3;
            A_TrianglesSize = (uint)resultTrianglesSize / 3;
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
