using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Common 
{
    /// <summary>
    /// Generate Mesh from VerticesArray and FaceIndicesArray
    /// </summary>
    /// <param name="VerticesArray"></param>
    /// <param name="FaceIndicesArray"></param>
    /// <param name="name"></param>
    /// <param name="meshcolor"></param>
    public static void GenerateMesh(float[] VerticesArray, uint[] FaceIndicesArray, string name, Color meshcolor)
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
    /// <summary>
    /// Write Float[] into Obj
    /// </summary>
    /// <param name="array"></param>
    /// <param name="sw"></param>
    public static void WriteFloatArrayToStream(float[] array, StreamWriter sw)
    {
        for (int i = 0; i < array.Length; i += 3)
        {
            if (i + 2 < array.Length)
            {
                sw.WriteLine("v {0} {1} {2}", array[i], array[i + 1], array[i + 2]);
            }
        }
    }
    /// <summary>
    /// Write int[] into Obj
    /// </summary>
    /// <param name="array"></param>
    /// <param name="sw"></param>
    public static void WriteUintArrayToStream(uint[] array, StreamWriter sw)
    {

        for (int i = 0; i < array.Length; i += 3)
        {
            if (i + 2 < array.Length)
            {
                sw.WriteLine("f {0} {1} {2} ", array[i] + 1, array[i + 1] + 1, array[i + 2] + 1);
            }

        }
    }

    public static void WriteObj(string writeobjpath, float[] VerticesArray, uint[] TrianlgesArray)
    {
        using (StreamWriter writer = new StreamWriter(writeobjpath))
        {
            WriteFloatArrayToStream(VerticesArray, writer);
            WriteUintArrayToStream(TrianlgesArray, writer);
        }       
    }
}
