using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ObjParser;
using UnityEngine.UIElements;
using System.Linq;
using System;
using System.IO;

public class Boolean : MonoBehaviour
{
    #region thinking

    //transformpoint,vertices trans,leftright,collider,同步环锯和Unity场景中的圆柱体,然后换掉,注意transform转换,然后再次写入到obj文件
    #endregion

    #region Fields

    //ObjParser dll
    Obj obj;
    Obj hj;
    Obj resultobj;

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
            uint[] uintArray = intArray.Select(j => (uint)j - 1).ToArray();
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
        for (int i = 0; i < (uint)hj.VertexList.Count; i++)
        {
            pCutMeshVerticesList.Add((float)hj.VertexList[i].X);
            pCutMeshVerticesList.Add((float)hj.VertexList[i].Y);
            pCutMeshVerticesList.Add((float)hj.VertexList[i].Z);
        }
        pCutMeshVertices = pCutMeshVerticesList.ToArray();
        //uint[] pCutMeshFaceIndices
        for (int i = 0; i < (uint)hj.FaceList.Count; i++)
        {
            int[] intArray = hj.FaceList[i].VertexIndexList;
            uint[] uintArray = intArray.Select(jvalue => (uint)jvalue - 1).ToArray();
            pCutMeshFaceIndicesList.AddRange(uintArray);
        }
        pCutMeshFaceIndices = pCutMeshFaceIndicesList.ToArray();
        //uint numCutMeshVertices
        numCutMeshVertices = (uint)hj.VertexList.Count;
        //uint numCutMeshFaces
        numCutMeshFaces = (uint)hj.FaceList.Count;

        #endregion

        #region MeshVisual
        GenerateMesh(pSrcMeshVertices, pSrcMeshFaceIndices, "src", Color.blue);
        GenerateMesh(pCutMeshVertices, pCutMeshFaceIndices, "cut", Color.green);
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

        #region Boolean
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
        if (error != 0)
        {
            Debug.Log($"error:{error}");
        }


        float[] resultVerticesOut = new float[resultVerticesSize];
        int[] resultFaceIndicesOut = new int[resultFaceIndicesSize];

        getBooleanResultNoUVs(resultVerticesOut, resultFaceIndicesOut);

        //Visual Result
        uint[] intArraySrc = resultFaceIndicesOut.Select(i => (uint)i).ToArray();
        GenerateMesh(resultVerticesOut, intArraySrc, "result", Color.yellow);

        #endregion

        #region WriteObj
        string writeobjpath = "D:\\Projects\\BooleanDemo\\Resource\\generate.obj";

        using (StreamWriter writer = new StreamWriter(writeobjpath))
        {
            WriteFloatArrayToStream(resultVerticesOut, writer);
            WriteIntArrayToStream(resultFaceIndicesOut, writer);
        }
        #endregion

    }
    void Update()
    {
        #region MoveCut
        //if Get Space Down,boolean according to the new position
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject obj = GameObject.Find("cut");
            MeshFilter meshfilterobj = obj.GetComponent<MeshFilter>();
            Mesh meshobj = new Mesh();
            meshobj = meshfilterobj.mesh;
            Vector3[] vector3verticesobj = meshobj.vertices;
            int[] intfaceobj = meshobj.triangles;
            uint[] uintArray = intfaceobj.Select(j => (uint)j).ToArray();

            float[] objvertices = vector3verticesobj.SelectMany(v => new float[] { v.x, v.y, v.z }).ToArray();
            uint facesize = (uint)meshobj.triangles.Count();
            uint vertexsize = (uint)meshobj.vertexCount;

            int newsultverticesSize = 0;
            int newsultfacesSize = 0;
            var error = queryInfoNoUVs(pSrcMeshVertices,
                                       pSrcMeshFaceIndices,
                                       numSrcMeshVertices,
                                       numSrcMeshFaces,
                                       objvertices,
                                       uintArray,
                                       vertexsize,
                                       facesize,
                                       ref newsultverticesSize,
                                       ref newsultfacesSize
                                       );
            if (error != 0)
            {
                Debug.Log($"error:{error}");
            }

            float[] newresultVerticesOut = new float[newsultverticesSize];
            int[] newresultFaceIndicesOut = new int[newsultfacesSize];
            getBooleanResultNoUVs(newresultVerticesOut, newresultFaceIndicesOut);


            Vector3[] vectorArrayresult = new Vector3[newresultVerticesOut.Length / 3];
            for (int i = 0; i < vectorArrayresult.Length; i++)
            {
                int j = i * 3;
                vectorArrayresult[i] = new Vector3(newresultVerticesOut[j], newresultVerticesOut[j + 1], newresultVerticesOut[j + 2]);
            }
            //int[] intArrayresult = resultFaceIndicesOut.Select(i => (int)i).ToArray();
            Mesh meshresult = new Mesh();
            meshresult.vertices = vectorArrayresult;
            meshresult.triangles = newresultFaceIndicesOut;
            meshresult.RecalculateNormals();
            GameObject result = new GameObject();
            result.name = "newresult";
            MeshFilter meshFilterresult = result.AddComponent<MeshFilter>();
            MeshRenderer meshrendererresult = result.AddComponent<MeshRenderer>();
            Material materialresult = new Material(Shader.Find("Standard"));
            materialresult.color = Color.white;
            meshFilterresult.mesh = meshresult;
            meshrendererresult.material = materialresult;
        }
        //if Get Key.H down,boolean accoring to the new pos.
        if (Input.GetKeyDown(KeyCode.H))
        {
            //手动拖动Hierarchy中的cut到一个新的位置,
            //然后获取此时cut的新位置,
            //此时是否需要转换成右手坐标系
            //得到此时的cut的网格的顶点和面片索引信息
            //然后依次新的信息和src进行bool操作,生成新的mesh并可视化

            #region NewPos
            GameObject go = GameObject.Find("cut");
            Mesh mesh = go.GetComponent<MeshFilter>().mesh;
            Transform transform = go.GetComponent<Transform>();
            Vector3[] vertices = mesh.vertices;
            //Convert to right-hand coordinate system
            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    // 将X坐标乘以-1并进行旋转
            //    vertices[i] = transform.TransformPoint(new Vector3(-vertices[i].x, vertices[i].y, vertices[i].z));              
            //}
            for (int i = 0; i < vertices.Length; i++)
            {               
                vertices[i] = transform.TransformPoint(new Vector3(vertices[i].x, vertices[i].y, vertices[i].z));
            }

            float[] verticesArray = new float[vertices.Length * 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                verticesArray[i * 3] = vertices[i].x;
                verticesArray[i * 3 + 1] = vertices[i].y;
                verticesArray[i * 3 + 2] = vertices[i].z;
            }

            int[] triangles = mesh.triangles;
            //Convert to right-hand coordinate system
            //for (int i = 0; i < triangles.Length; i += 3)
            //{
            //    // 交换第二个和第三个索引
            //    int temp = triangles[i + 1];
            //    triangles[i + 1] = triangles[i + 2];
            //    triangles[i + 2] = temp;
            //}
            uint[] uinttriArray = triangles.Select(i => (uint)i).ToArray();

            uint vertexCount = (uint)mesh.vertexCount;
            uint triangleCount = (uint)mesh.triangles.Length / 3;
            GenerateMesh(verticesArray, uinttriArray, "new cut", Color.white);
            #endregion


            #region Bool and Visual
            int newVerticesSize = 0;
            int newFaceIndicesSize = 0;

            var error = queryInfoNoUVs(pSrcMeshVertices,
            pSrcMeshFaceIndices,
            numSrcMeshVertices,
            numSrcMeshFaces,
            verticesArray,
            uinttriArray,
            vertexCount,
            triangleCount,
            ref newVerticesSize,
            ref newFaceIndicesSize
            );
            if (error != 0)
            {
                Debug.Log($"error:{error}");
            }

            float[] newVerticesSizeOut = new float[newVerticesSize];
            int[] newFaceIndicesSizeOut = new int[newFaceIndicesSize];

            getBooleanResultNoUVs(newVerticesSizeOut, newFaceIndicesSizeOut);

            //Visual Result
            uint[] intArraySrc = newFaceIndicesSizeOut.Select(i => (uint)i).ToArray();
            GenerateMesh(newVerticesSizeOut, intArraySrc, "new result", Color.red);

            #endregion


        }

        #endregion
    }

    /// <summary>
    /// Write Float[] into Obj
    /// </summary>
    /// <param name="array"></param>
    /// <param name="sw"></param>
    void WriteFloatArrayToStream(float[] array, StreamWriter sw)
    {
        for (int i = 0; i < array.Length; i += 3)
        {
            if (i + 2 < array.Length)
            {
                sw.WriteLine("v {0} {1} {2}", array[i], array[i + 1], array[i + 2]);
            }
            //else if (i + 1 < array.Length)
            //{
            //    sw.WriteLine("v {0} {1}", array[i], array[i + 1]);
            //}
            //else
            //{
            //    sw.WriteLine("v {0}", array[i]);
            //}
        }
    }
    /// <summary>
    /// Write int[] into Obj
    /// </summary>
    /// <param name="array"></param>
    /// <param name="sw"></param>
    void WriteIntArrayToStream(int[] array, StreamWriter sw)
    {
        for (int i = 0; i < array.Length; i += 3)
        {
            if (i + 2 < array.Length)
            {
                sw.WriteLine("f {0} {1} {2} ", array[i], array[i + 1], array[i + 2]);
                //if (array[i] == array[i + 1] || array[i] == array[i + 2] || array[i + 1] == array[i + 2])
                //{
                //    Debug.Log($"{array[i]} {array[i+1]} {array[i+2]}");
                //}
            }
            //else if (i + 1 < array.Length)
            //{
            //    sw.WriteLine("f {0} {1} ", array[i], array[i + 1]);
            //}
            //else
            //{
            //    sw.WriteLine("f {0} ", array[i]);
            //}
        }
    }
    /// <summary>
    /// Generate Mesh from VerticesArray and FaceIndicesArray
    /// </summary>
    /// <param name="VerticesArray"></param>
    /// <param name="FaceIndicesArray"></param>
    /// <param name="name"></param>
    /// <param name="meshcolor"></param>
    void GenerateMesh(float[] VerticesArray, uint[] FaceIndicesArray, string name, Color meshcolor)
    {
        Vector3[] vectorArrayresult = new Vector3[VerticesArray.Length / 3];
        for (int i = 0; i < vectorArrayresult.Length; i++)
        {
            int j = i * 3;
            vectorArrayresult[i] = new Vector3(VerticesArray[j], VerticesArray[j + 1], VerticesArray[j + 2]);
        }
        //int[] intArrayresult = resultFaceIndicesOut.Select(i => (int)i).ToArray();
        int[] intArraySrc = FaceIndicesArray.Select(i => (int)i).ToArray();

        Mesh meshresult = new Mesh();
        meshresult.vertices = vectorArrayresult;
        meshresult.triangles = intArraySrc;
        meshresult.RecalculateNormals();

        GameObject result = new GameObject();
        result.name = name;
        MeshFilter meshFilterresult = result.AddComponent<MeshFilter>();
        MeshRenderer meshrendererresult = result.AddComponent<MeshRenderer>();
        Material materialresult = new Material(Shader.Find("Standard"));
        materialresult.color = meshcolor;
        meshFilterresult.mesh = meshresult;
        meshrendererresult.material = materialresult;
    }


    #endregion
}