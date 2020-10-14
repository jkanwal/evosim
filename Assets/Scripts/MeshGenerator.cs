using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    public int xSize = 50;
    public int zSize = 50;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];


        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                var y = Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 30f + Random.Range(0f,1f);
                vertices[i] = new Vector3(x, y, z);
                i++;
            }            
        }
        int vert = 0;
        int tries = 0;
        triangles = new int[xSize*zSize*6];
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {

                triangles[tries] = vert;
                triangles[tries + 1] = vert + xSize + 1;
                triangles[tries + 2] = vert + 1;
                triangles[tries + 3] = vert + 1;
                triangles[tries + 4] = vert + xSize + 1;
                triangles[tries + 5] = vert + xSize + 2;

                vert++;
                tries += 6;
            }
            vert++;
        }
        

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
    }
        // Update is called once per frame
        void Update()
        {

        }
        /*
        private void OnDrawGizmos()
        {
        if(vertices== null)
        {
            return;
        }
            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.DrawSphere(vertices[i], 0.1f);
            }
        }
        */
}
