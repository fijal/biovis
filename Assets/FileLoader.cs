using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class FileLoader : MonoBehaviour
{
    const int MAX_X = 58;
    const int MAX_Y = 58;
    const double MIN = -104.00;
    const double MAX = -52.56;
    const int TIMESPAN = 30;
    const float CELL_SIZE = 0.1f;
    const float VERT_CELL_SIZE = 0.5f;
    const float X_COEFF = 0.1f;
    const float Y_COEFF = 0.1f;
    const float Z_COEFF = 0.01f;
    const float Z_DIFF = 0.5f;
    int counter = 0;
    double[] values, coords_raw;
    int [,] coords;
    Matrix4x4[] boxes;
    Mesh box;

    public Material cubeMat;
    public TextAsset coordsInfo;
    public TextAsset[] simulationAssets;

    // Start is called before the first frame update
    double[] ReadArray(TextAsset input)
    {
        var s = new MemoryStream(input.bytes);
        var br = new BinaryReader(s);
        Debug.Assert(br.ReadChar() =='a');
        var shape_depth = (int)br.ReadChar();
        //Debug.Log(String.Format("Shape depth: {0}", shape_depth));
        var shape = new long[shape_depth];
        var total_count = 1L;
        for (int i = 0; i < shape_depth; i++)
        {
            shape[i] = (long)br.ReadUInt64();
            //Debug.Log(String.Format("Shape[{0}] = {1}", i, shape[i]));
            total_count *= shape[i];
        }
        //Debug.Log(String.Format("Total count: {0}", total_count));
        var res = new double[total_count];
        var count = 0L;
        while (count < total_count)
        {
            res[count] = br.ReadDouble();
            count += 1;
        }
        return res;
        //Debug.Log(String.Format("{0} {1}", shape[0], shape[1]));
    }

    /*int find_next_row(double[] arr, int index)
    {
    }*/

    int[] GenerateTris(Vector3[] vertices)
    {
        var res = new int[vertices.Length * 6];
        var i = 0;
        for (int ix = 0; ix < MAX_X - 1; ix++)
        {
            for (int iy = 0; iy < MAX_Y - 1; iy++)
            {
                res[i] = ix + iy * MAX_X;
                res[i + 2] = ix + 1 + iy * MAX_X;
                res[i + 1] = ix + (iy + 1) * MAX_X;
                res[i + 3] = ix + 1 + iy * MAX_X;
                res[i + 4] = ix + (iy + 1) * MAX_X;
                res[i + 5] = ix + 1 + (iy + 1) * MAX_X;
                i += 6;
                /*res[i] = ix + iy * MAX_X;
                res[i + 1] = ix + 1 + iy * MAX_X;
                res[i + 2] = ix + (iy + 1) * MAX_X;
                res[i + 3] = ix + 1 + iy * MAX_X;
                res[i + 5] = ix + (iy + 1) * MAX_X;
                res[i + 4] = ix + 1 + (iy + 1) * MAX_X;
                i += 6;*/
            }
        }
        return res;
    }

    public Mesh GenerateMesh(int[,] coords, double[] values)
    {
        var mesh = new Mesh();
        
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        var vertices = new Vector3[MAX_X * MAX_Y];
        for (int ix = 0; ix < MAX_X; ix++)
        {
            for (int iy = 0; iy < MAX_Y; iy++)
            {
                var i = coords[ix, iy];
                var j = ix + iy * MAX_X;
                if (i != -1)
                {
                    var y = (float)((values[i] - MIN) / (MAX - MIN)) * VERT_CELL_SIZE;
                    vertices[j] = new Vector3(ix * CELL_SIZE, y, iy * CELL_SIZE);
                } else
                {
                    vertices[j] = new Vector3(ix * CELL_SIZE, 0, iy * CELL_SIZE);
                }
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = GenerateTris(mesh.vertices);
        //mesh.normals = GenerateNormals();
        //mesh.uv = GenerateUVs();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    void Start()
    {
        counter = 0;
        values = ReadArray(simulationAssets[0]);
        coords_raw = ReadArray(coordsInfo);
        coords = new int[MAX_X, MAX_Y];
        for (int x = 0; x < MAX_X; x++)
            for (int y = 0; y < MAX_Y; y++)
                coords[x, y] = -1;
        for (int i = 0; i < coords_raw.Length / 2; i++)
        {
            var x = (int)(coords_raw[i * 2]);
            var y = (int)(coords_raw[i * 2 + 1]);
            coords[x, y] = i;
        }
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box = go.GetComponent<MeshFilter>().mesh;
        Destroy(go);
        boxes = new Matrix4x4[coords_raw.Length / 2];
        /*for (int i = 0; i < coords_raw.Length / 2; i++)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.position = new Vector3((float)coords_raw[i *2] * X_COEFF, (float)values[i] * Z_COEFF + Z_DIFF,
                (float)coords_raw[i * 2 + 1] * Y_COEFF);
            box.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            boxes[i] = box;
        }*/
        /*var mesh = GenerateMesh(coords, values);
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        var shader = Shader.Find("Parallax Diffuse");
        Material mat = new Material(shader);
        mat.color = new Color(0.38f, 0.72f, 0.92f, 0.53f);
        meshRenderer.material = mat;
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;*/
    }

    Matrix4x4[] slice = new Matrix4x4[1000];

    void DrawMeshInstanced(Mesh mesh, Material mat, Matrix4x4[] matrices)
    {
        int start = 0;
        int stop = matrices.Length;
        while (start < stop)
        {
            int len = stop - start;
            if (len > 1000)
                len = 1000;
            Array.Copy(matrices, start, slice, 0, len);
            Graphics.DrawMeshInstanced(mesh, 0, mat, slice, len);
            start += len;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 2781; i++)
        {
            float x = (float)coords_raw[i * 2] * X_COEFF;
            var y = (float)coords_raw[i * 2 + 1] * X_COEFF;
            var v = (float)values[i * 249 + counter / TIMESPAN];
            var v_next = (float)values[i * 249 + counter / TIMESPAN + 1];
            double step = counter % TIMESPAN;
            var z = (v + (v_next - v) * (step / TIMESPAN)) * Z_COEFF + Z_DIFF;
            var position = new Vector3(x, (float)z, y);
            var rotation = Quaternion.Euler(0, 0, 0);
            var scale = new Vector3(0.08f, 0.08f, 0.08f);
            boxes[i] = Matrix4x4.TRS(position, rotation, scale);
        }
        DrawMeshInstanced(box, cubeMat, boxes);
        counter += 1;
    }
}
