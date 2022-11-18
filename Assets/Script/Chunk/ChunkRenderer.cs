using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    public bool ShowGizmo = false;
    
    public Chunk Chunk { get; private set; }

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        mesh = meshFilter.mesh;
    }

    public void InitChunk(Chunk chunk)
    {
        Chunk = chunk;
    }

    private void renderMesh(ChunkMesh chunkMesh)
    {
        mesh.Clear();
        mesh.subMeshCount = 2;
        mesh.vertices = chunkMesh.Vertices.Concat(chunkMesh.WaterMesh.Vertices).ToArray();
        mesh.SetTriangles(chunkMesh.Triangles.ToArray(), 0);
        mesh.SetTriangles(chunkMesh.WaterMesh.Triangles.Select(val => val + chunkMesh.Vertices.Count).ToArray(), 1);
        mesh.uv = chunkMesh.UV.Concat(chunkMesh.WaterMesh.UV).ToArray();
        mesh.RecalculateNormals();

        Mesh collisionMesh = new Mesh
        {
            vertices = chunkMesh.ColliderVertices.ToArray(),
            triangles = chunkMesh.ColliderTriangles.ToArray()
        };
        collisionMesh.RecalculateNormals();
        meshCollider.sharedMesh = collisionMesh;
    }

    public void UpdateChunk()
    {
        renderMesh(Chunk.GetChunkMesh());
    }

    public void UpdateChunk(ChunkMesh data)
    {
        renderMesh(data);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(ShowGizmo)
            if (Application.isPlaying && Chunk != null)
            {
                Gizmos.color = Selection.activeObject == gameObject ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 1f, 0.4f);
                Gizmos.DrawCube(transform.position + new Vector3(Chunk.ChunkSize / 2f,
                                    Chunk.ChunkHeight / 2f, Chunk.ChunkSize / 2f), new Vector3(Chunk.ChunkSize,
                    Chunk.ChunkHeight, Chunk.ChunkSize));
            }
    }
#endif
}
