using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMesh
{
    public List<Vector3> Vertices { get; }
    public List<int> Triangles { get; }
    public List<Vector2> UV { get; }
    public List<Vector3> ColliderVertices { get; }
    public List<int> ColliderTriangles { get; }
    public ChunkMesh WaterMesh { get; }
    private bool IsMainMesh { get; }
    
    public ChunkMesh(bool isMainMesh)
    {
        if(isMainMesh)
            WaterMesh = new ChunkMesh(false);
        IsMainMesh = isMainMesh;
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        UV = new List<Vector2>();
        ColliderVertices = new List<Vector3>();
        ColliderTriangles = new List<int>();
    }


    public void TryAddBlock(Chunk chunk, Block block)
    {
        if (block.BlockType == BlockType.Air)
            return;

        foreach (Direction dir in DirectionExtensions.ListDirections())
        {
            var neighbourBlockCoords = block.GlobalPosition + dir.GetVector();
            var neighbourBlock = chunk.GetBlock(neighbourBlockCoords);
            
            if (neighbourBlock != null &&
                !neighbourBlock.BlockData.isSolid)
            {
                if (block.BlockType == BlockType.Water)
                {
                    if (neighbourBlock.BlockType == BlockType.Air)
                        WaterMesh.addBlockFace(dir, block);
                }
                else
                    addBlockFace(dir, block);
            }
        }
    }
    
    private void addBlockFace(Direction direction, Block block)
    {
        var pos = block.GlobalPosition;
        addFaceVertices(direction, pos.x, pos.y, pos.z, block);
        addQuadTriangles(block);
        addFaceUV(direction, block);
    }
    
    private void addFaceVertices(Direction direction, int x, int y, int z, Block block)
    {
        var generatesCollider = block.BlockData.generatesCollider;
        switch (direction)
        {
            case Direction.Back:
                addVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                addVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                break;
            case Direction.Forward:
                addVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                addVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                addVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                break;
            case Direction.Right:
                addVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                break;
            case Direction.Left:
                addVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                addVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                addVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                addVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                break;
            case Direction.Up:
                addVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                addVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                break;
            case Direction.Down:
                addVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                addVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                addVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    private void addFaceUV(Direction direction, Block block)
    {
        var tilePos = texturePosition(direction, block);
        float tileWidth = GameManager.BlockAtlas.TileWidth;
        float tileHeight = GameManager.BlockAtlas.TileHeight;
        float textureOffset = GameManager.TextureOffset;
        
        UV.Add(new Vector2(tileWidth * tilePos.x + tileWidth - textureOffset,
            tileHeight * tilePos.y + textureOffset));
        UV.Add(new Vector2(tileWidth * tilePos.x + tileWidth - textureOffset,
            tileHeight * tilePos.y + tileHeight - textureOffset));
        UV.Add(new Vector2(tileWidth * tilePos.x + textureOffset,
            tileHeight * tilePos.y + tileHeight - textureOffset));
        UV.Add(new Vector2(tileWidth * tilePos.x + textureOffset,
            tileHeight * tilePos.y + textureOffset));
    }

    private void addQuadTriangles(Block block)
    {
        Triangles.Add(Vertices.Count - 4);
        Triangles.Add(Vertices.Count - 3);
        Triangles.Add(Vertices.Count - 2);
        Triangles.Add(Vertices.Count - 4);
        Triangles.Add(Vertices.Count - 2);
        Triangles.Add(Vertices.Count - 1);
        if (block.BlockData.generatesCollider)
        {
            ColliderTriangles.Add(Vertices.Count - 4);
            ColliderTriangles.Add(Vertices.Count - 3);
            ColliderTriangles.Add(Vertices.Count - 2);
            ColliderTriangles.Add(Vertices.Count - 4);
            ColliderTriangles.Add(Vertices.Count - 2);
            ColliderTriangles.Add(Vertices.Count - 1);
        }
    }
    
    private Vector2Int texturePosition(Direction direction, Block block)
    {
        return direction switch
        {
            Direction.Up => block.BlockData.up,
            Direction.Down => block.BlockData.down,
            _ => block.BlockData.side
        };
    }

    private void addVertex(Vector3 vertex, bool hasCollision)
    {
        Vertices.Add(vertex);
        if(hasCollision)
            ColliderVertices.Add(vertex);
    }
}
