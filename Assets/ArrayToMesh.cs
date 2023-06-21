using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Stephanie Anderson
 *10/09/2021
 */

public class STEPH_ANDERSON_GSCR03 : MonoBehaviour
{
    #region Public Variables
    [Header("Dimensions and Size")]
    [SerializeField] int dimensions = 200;
    [Space]

    [Header("Wave Forms")]
    [SerializeField] float heightOne = 1f;
    [SerializeField] float heightTwo = 1f;
    [Space]
    [SerializeField] float speedOne = 1f;
    [SerializeField] float speedTwo = 3f;
    [Space]
    [SerializeField] float scaleOne = 5f;
    [SerializeField] float scaleTwo = 10f;
    [Space]

    [Header("Mesh Appearance")]
    [SerializeField] bool setCustomAppearance = false;
    [SerializeField] Color customColour;
    [SerializeField] Shader customShader;
    [Space]
    #endregion

    #region Private Variables
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    private int totalVerts;

    private Octave[] Octaves;
    #endregion

    #region Start
    void Start()
    {
        //FPS drops below 60FPS at 200 x 200
        if (dimensions == 0)
        {
            Debug.LogError("Dimension veraible must be above 0.");
        }

        totalVerts = CalculateTotalVerts();
        CreateOctaves();
        CreateMesh();
    }
    #endregion

    #region Update
    void Update()
    {
        Vector3[] vertices = mesh.vertices;
        for (int x = 0; x <= dimensions; x++)
        {
            for (int z = 0; z <= dimensions; z++)
            {
                float y = 0f;
                for (int o = 0; o < Octaves.Length; o++)
                {
                    //Perlin noise used in conjuction with Cos to create wave like structure
                    if (Octaves[o].alternate)
                    {
                        float perl = Mathf.PerlinNoise((x * Octaves[o].scale.x) / dimensions,
                                                       (z * Octaves[o].scale.y) / dimensions)
                                                        * Mathf.PI * 2f;
                        y += Mathf.Cos(perl + Octaves[o].speed.magnitude * Time.time)
                                       * Octaves[o].height;
                    }
                    else
                    {
                        float perl = Mathf.PerlinNoise((x * Octaves[o].scale.x + Time.time * Octaves[o].speed.x) / dimensions,
                                                       (z * Octaves[o].scale.y + Time.time * Octaves[o].speed.y) / dimensions)
                                                        - 0.5f;
                        y += perl * Octaves[o].height;
                    }
                }
                vertices[Index(x, z)] = new Vector3(x, y, z);
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
    #endregion

    #region Array Tools
    public int Index(int x, int z)
    {
        int index = x * (dimensions + 1) + z;
        return index;
    }
    #endregion

    #region Octave Creation
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }

    public void CreateOctaves()
    {
        Octaves = new Octave[2];
        Octaves[0].speed = new Vector2(speedOne, speedOne);
        Octaves[0].scale = new Vector3(scaleOne, scaleOne);
        Octaves[0].height = heightOne;
        Octaves[0].alternate = true;
        Octaves[1].speed = new Vector2(speedTwo, 0.0f);
        Octaves[1].scale = new Vector3(scaleTwo, scaleTwo);
        Octaves[1].height = heightTwo;
        Octaves[1].alternate = false;
    }
    #endregion

    #region Mesh Creation 
    public void CreateMesh()
    {
        mesh = new Mesh();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = GenerateVertices();
        mesh.triangles = GenerateTris();
        //mesh.normals = GenerateNormals();
        mesh.uv = GenerateUVs();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        if (setCustomAppearance)
        {
            if (customShader == null)
            {
                Debug.LogError("Custom Shader cannot be null. Please select a shader in the editor.", customShader);
            }
            SetMeshAppearance(customColour, customShader);
        }
        else
        {
            SetMeshAppearance(new Color(0.38f, 0.72f, 0.92f, 0.53f), Shader.Find("Parallax Diffuse"));
        }

        meshFilter.mesh = mesh;
    }

    public void SetMeshAppearance(Color colour, Shader shader)
    {
        Material mat = new Material(shader);
        mat.color = colour;
        meshRenderer.material = mat;
    }

    public int CalculateTotalVerts()
    {
        return (dimensions + 1) * (dimensions + 1);
    }
    #endregion

    #region Mesh Creation Methods
    public Vector3[] GenerateVertices()
    {
        Vector3[] vertices = new Vector3[totalVerts];
        for (int x = 0; x < dimensions; x++)
        {
            for (int z = 0; z < dimensions; z++)
            {
                vertices[Index(x, z)] = new Vector3(x, 0, z);
            }
        }
        return vertices;
    }


    public int[] GenerateTris()
    {
        int[] tris = new int[totalVerts * 6];
        for (int x = 0; x < dimensions; x++)
        {
            for (int z = 0; z < dimensions; z++)
            {
                tris[Index(x, z) * 6 + 0] = Index(x, z);
                tris[Index(x, z) * 6 + 1] = Index(x + 1, z + 1);
                tris[Index(x, z) * 6 + 2] = Index(x + 1, z);
                tris[Index(x, z) * 6 + 3] = Index(x, z);
                tris[Index(x, z) * 6 + 4] = Index(x, z + 1);
                tris[Index(x, z) * 6 + 5] = Index(x + 1, z + 1);
            }
        }
        return tris;
    }

    public Vector3[] GenerateNormals()
    {
        Vector3[] normals = new Vector3[totalVerts];
        for (int i = 0; i < dimensions; i++)
        {
            normals[i] = Vector3.forward;
        }
        return normals;
    }

    public Vector2[] GenerateUVs()
    {
        Vector2[] uv = new Vector2[totalVerts];
        for (int x = 0; x < dimensions; x++)
        {
            for (int z = 0; z < dimensions; z++)
            {
                uv[Index(x, z)] = new Vector2(x, z);
            }
        }
        return uv;
    }
    #endregion
}

