
using ObjParser;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
/// <summary>
/// 改变场景中cube的transform(position和rotation),根据transform变换得到Matrix4x4矩阵
/// 将该矩阵作用到读取的做Bool操作的对象上,然后和读取的两个右手坐标系Obj做布尔运算
/// </summary>
public class DoubleBool : MonoBehaviour
{
    #region Fields
    Obj obj_A;
    Obj obj_B;
    Obj obj_BOOL;

    //A data
    float[] A_VerticesArray = null;
    uint[] A_TrianglesArray = null;
    uint A_VerticesSize;
    uint A_TrianglesSize;

    List<float> A_VerticesList;
    List<uint> A_TrianglesList;
    string A_ObjPath = Application.streamingAssetsPath + "/SM_YaoZhui_L4.obj";


    //B data
    float[] B_VerticesArray = null;
    uint[] B_TrianglesArray = null;
    uint B_VerticesSize;
    uint B_TrianglesSize;

    List<float> B_VerticesList;
    List<uint> B_TrianglesList;
    string B_ObjPath = Application.streamingAssetsPath + "/SM_YaoZhui_L5.obj";


    //BOOL data
    float[] BOOL_VerticesArray = null;
    uint[] BOOL_TrianglesArray = null;
    uint BOOL_VerticesSize;
    uint BOOL_TrianglesSize;

    List<float> BOOL_VerticesList;
    List<uint> BOOL_TrianglesList;
    string BOOL_ObjPath = Application.streamingAssetsPath + "/HJT_1.obj";

    //result_1
    int result_1_VerticesSize;
    int result_1_TrianglesSize;
    float[] result_1_VerticesOutArray;
    int[] result_1_TrianglesOutArray;
    string result_1_Path = Application.streamingAssetsPath + "/Result/result_1.obj";

    //result_2
    int result_2_VerticesSize;
    int result_2_TrianglesSize;
    float[] result_2_VerticesOutArray;
    int[] result_2_TrianglesOutArray;
    string result_2_Path = Application.streamingAssetsPath + "/Result/result_2.obj";

    //Trans
    Transform trepanTransform;

    #endregion

    #region C++ Dll Import

    [DllImport("boolean.dll", EntryPoint = "queryInfoNoUVs", CallingConvention = CallingConvention.Cdecl)]
    public extern static int queryInfoNoUVs(
        float[] pSrcMeshVertices, uint[] pSrcMeshFaceIndices, uint numSrcMeshVertices, uint numSrcMeshFaces,
        float[] pCutMeshVertices, uint[] pCutMeshFaceIndices, uint numCutMeshVertices, uint numCutMeshFaces,
        ref int resultVerticesSize, ref int resultFaceIndicesSize);

    [DllImport("boolean.dll", EntryPoint = "getBooleanResultNoUVs", CallingConvention = CallingConvention.Cdecl)]
    public extern static void getBooleanResultNoUVs(float[] resultVerticesOut, int[] resultFaceIndicesOut);

    #endregion

    void Start()
    {
        #region Init
        obj_A = new Obj();
        obj_A.LoadObj(A_ObjPath);
        A_VerticesList = new List<float>();
        A_TrianglesList = new List<uint>();

        obj_B = new Obj();
        obj_B.LoadObj(B_ObjPath);
        B_VerticesList = new List<float>();
        B_TrianglesList = new List<uint>();

        obj_BOOL = new Obj();
        obj_BOOL.LoadObj(BOOL_ObjPath);
        BOOL_VerticesList = new List<float>();
        BOOL_TrianglesList = new List<uint>();

        //或者直接做成Public的
        trepanTransform = GameObject.Find("Cube").GetComponent<Transform>();
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
        Common.GenerateMesh(A_VerticesArray, A_TrianglesArray, "A_obj", Color.red);
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
            uint[] uintArray = intArray.Select(j => (uint)j - 1).ToArray();
            B_TrianglesList.AddRange(uintArray);
        }
        B_TrianglesArray = B_TrianglesList.ToArray();
        B_VerticesSize = (uint)obj_B.VertexList.Count;
        B_TrianglesSize = (uint)obj_B.FaceList.Count;
        Common.GenerateMesh(B_VerticesArray, B_TrianglesArray, "B_obj", Color.red);
        #endregion

        #region BOOL Mesh data
        for (int i = 0; i < (uint)obj_BOOL.VertexList.Count; i++)
        {
            BOOL_VerticesList.Add((float)obj_BOOL.VertexList[i].X);
            BOOL_VerticesList.Add((float)obj_BOOL.VertexList[i].Y);
            BOOL_VerticesList.Add((float)obj_BOOL.VertexList[i].Z);
        }
        BOOL_VerticesArray = BOOL_VerticesList.ToArray();
        for (int i = 0; i < (uint)obj_BOOL.FaceList.Count; i++)
        {
            int[] intArray = obj_BOOL.FaceList[i].VertexIndexList;
            uint[] uintArray = intArray.Select(j => (uint)j - 1).ToArray();
            BOOL_TrianglesList.AddRange(uintArray);
        }
        BOOL_TrianglesArray = BOOL_TrianglesList.ToArray();
        BOOL_VerticesSize = (uint)obj_BOOL.VertexList.Count;
        BOOL_TrianglesSize = (uint)obj_BOOL.FaceList.Count;
        Common.GenerateMesh(BOOL_VerticesArray, BOOL_TrianglesArray, "BOOL_obj", Color.red);
        #endregion

    }

    void Update()
    {
        #region Matrix4x4
        Matrix4x4 matrix = Matrix4x4.TRS(trepanTransform.position, trepanTransform.rotation, trepanTransform.localScale);

        matrix.m01 = -matrix.m01;
        matrix.m02 = -matrix.m02;
        matrix.m03 = -matrix.m03;
        matrix.m10 = -matrix.m10;
        matrix.m20 = -matrix.m20;
        #endregion

        #region Obtain a new vertex array(float[]) for BOOL
        ///The mesh vertices of the read object are transformed 
        ///in the same way as the object in the Unity scene 
        ///according to matrix4x4 to obtain a new vertex array(float[])
        Vector3[] vertices = new Vector3[obj_BOOL.VertexList.Count];
        for (int i = 0; i < obj_BOOL.VertexList.Count; i++)
        {
            vertices[i].x = (float)obj_BOOL.VertexList[i].X;
            vertices[i].y = (float)obj_BOOL.VertexList[i].Y;
            vertices[i].z = (float)obj_BOOL.VertexList[i].Z;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = matrix.MultiplyPoint(vertices[i]);
        }
        //A new Mesh Vertex array of Vector3[] of the Trepan is obtained
        //Then convert to float[]
        for (int i = 0; i < vertices.Length; i++)
        {
            BOOL_VerticesArray[i * 3] = vertices[i].x;
            BOOL_VerticesArray[i * 3 + 1] = vertices[i].y;
            BOOL_VerticesArray[i * 3 + 2] = vertices[i].z;
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("GetKeyDown: Do Boolean");
            Common.GenerateMesh(BOOL_VerticesArray, BOOL_TrianglesArray, "NEW BOOL", Color.green);
            #region A bool
            var error = queryInfoNoUVs(A_VerticesArray, A_TrianglesArray, A_VerticesSize, A_TrianglesSize,
                BOOL_VerticesArray, BOOL_TrianglesArray, BOOL_VerticesSize, BOOL_TrianglesSize,
                ref result_1_VerticesSize, ref result_1_TrianglesSize);
            if (error != 0)
            {
                Debug.Log($"error:{error}");
            }
            else
            {
                result_1_VerticesOutArray = new float[result_1_VerticesSize];
                result_1_TrianglesOutArray = new int[result_1_TrianglesSize];
                getBooleanResultNoUVs(result_1_VerticesOutArray, result_1_TrianglesOutArray);
                Common.GenerateMesh(result_1_VerticesOutArray, result_1_TrianglesOutArray, "result_1", Color.white);
                Common.WriteObj(result_1_Path, result_1_VerticesOutArray, result_1_TrianglesOutArray);

                A_VerticesArray = result_1_VerticesOutArray;
                A_TrianglesArray = result_1_TrianglesOutArray.Select(i => (uint)i).ToArray(); ;
                A_VerticesSize = (uint)result_1_VerticesSize / 3;
                A_TrianglesSize = (uint)result_1_TrianglesSize / 3;
            }


            #endregion
            //B bool
            var error2 = queryInfoNoUVs(B_VerticesArray, B_TrianglesArray, B_VerticesSize, B_TrianglesSize,
                BOOL_VerticesArray, BOOL_TrianglesArray, BOOL_VerticesSize, BOOL_TrianglesSize,
                ref result_2_VerticesSize, ref result_2_TrianglesSize);
            if (error2 != 0)
            {
                Debug.Log($"error2:{error2}");
            }
            else
            {
                result_2_VerticesOutArray = new float[result_2_VerticesSize];
                result_2_TrianglesOutArray = new int[result_2_TrianglesSize];
                getBooleanResultNoUVs(result_2_VerticesOutArray, result_2_TrianglesOutArray);
                Common.GenerateMesh(result_2_VerticesOutArray, result_2_TrianglesOutArray, "result_2", Color.white);
                Common.WriteObj(result_2_Path, result_2_VerticesOutArray, result_2_TrianglesOutArray);

                B_VerticesArray = result_2_VerticesOutArray;
                B_TrianglesArray = result_2_TrianglesOutArray.Select(i => (uint)i).ToArray(); ;
                B_VerticesSize = (uint)result_2_VerticesSize / 3;
                B_TrianglesSize = (uint)result_2_TrianglesSize / 3;
            }


        }
    }
}