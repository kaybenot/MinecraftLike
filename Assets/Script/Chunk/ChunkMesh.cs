using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ChunkMesh
{
    public List<Vector3> Vertices { get; }
    public List<int> Triangles { get; }
    public List<Vector2> UV { get; }
    public List<Vector3> ColliderVertices { get; }
    public List<int> ColliderTriangles { get; }
    public ChunkMesh WaterMesh { get; }

    public ChunkMesh(bool isMainMesh)
    {
        if(isMainMesh)
            WaterMesh = new ChunkMesh(false);
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        UV = new List<Vector2>();
        ColliderVertices = new List<Vector3>();
        ColliderTriangles = new List<int>();
    }

    public void TryAddBlock(Chunk chunk, BlockType blockType, Vector3Int globalPosition)
    {
        if (blockType == BlockType.Air)
            return;

        var dirs = DirectionExtensions.ListDirections();
        foreach (Direction dir in dirs)
        {
            var neighbourBlockCoords = globalPosition + dir.GetVector();
            var neighbourBlock = chunk.GetBlock(neighbourBlockCoords);

            if (neighbourBlock == null)
            {
                addBlockFace(dir, globalPosition, blockType);
                continue;
            }

            if (!neighbourBlock.BlockData.isSolid)
            {
                if (blockType == BlockType.Water)
                {
                    if (neighbourBlock.BlockType == BlockType.Air)
                        WaterMesh.addBlockFace(dir, globalPosition, blockType);
                }
                else
                    addBlockFace(dir, globalPosition, blockType);
            }
        }
    }

    private void addBlockFace(Direction direction, Vector3Int globalPosition, BlockType blockType)
    {
        var pos = globalPosition;
        addFaceVertices(direction, pos.x, pos.y, pos.z, blockType);
        addQuadTriangles(blockType);
        addFaceUV(direction, blockType);
    }
    
    private void addFaceVertices(Direction direction, int x, int y, int z, BlockType blockType)
    {
        var generatesCollider = Block.BlockDatas[blockType].generatesCollider;
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

    private void addFaceUV(Direction direction, BlockType blockType)
    {
        var tilePos = texturePosition(direction, blockType);
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

    private void addQuadTriangles(BlockType blockType)
    {
        Triangles.Add(Vertices.Count - 4);
        Triangles.Add(Vertices.Count - 3);
        Triangles.Add(Vertices.Count - 2);
        Triangles.Add(Vertices.Count - 4);
        Triangles.Add(Vertices.Count - 2);
        Triangles.Add(Vertices.Count - 1);
        if (Block.BlockDatas[blockType].generatesCollider)
        {
            ColliderTriangles.Add(Vertices.Count - 4);
            ColliderTriangles.Add(Vertices.Count - 3);
            ColliderTriangles.Add(Vertices.Count - 2);
            ColliderTriangles.Add(Vertices.Count - 4);
            ColliderTriangles.Add(Vertices.Count - 2);
            ColliderTriangles.Add(Vertices.Count - 1);
        }
    }
    
    private Vector2Int texturePosition(Direction direction, BlockType blockType)
    {
        var blockData = Block.BlockDatas[blockType];
        return direction switch
        {
            Direction.Up => blockData.up,
            Direction.Down => blockData.down,
            _ => blockData.side
        };
    }

    private void addVertex(Vector3 vertex, bool hasCollision)
    {
        Vertices.Add(vertex);
        if(hasCollision)
            ColliderVertices.Add(vertex);
    }
}
