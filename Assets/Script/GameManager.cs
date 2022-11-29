using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Block atlas settings")]
    [SerializeField] private BlockAtlas blockAtlas;
    [SerializeField] private float textureOffset = 0.001f;

    [Header("World settings")]
    [SerializeField] private float chunkDetectionTime = 1f;
    [SerializeField] private CustomNoiseSettings customNoiseSettings;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    public static BlockAtlas BlockAtlas { get; private set; }
    public static float TextureOffset { get; private set; }
    public static CustomNoiseSettings CustomNoiseSettings { get; private set; }
    public static World World { get; private set; }
    public static Player Player { get; private set; }
    public static Action OnNewChunksGenerated { get; set; }

    private Vector3Int lastChunkPos;
    
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnNewChunksGenerated += startCheckingTheMap;
        BlockAtlas = blockAtlas;
        TextureOffset = textureOffset;
        CustomNoiseSettings = customNoiseSettings;
        World = GameObject.FindWithTag("World").GetComponent<World>();
        Chunk.Biome = GameObject.FindWithTag("PlainsBiome").GetComponent<Biome>();

        loadBlockDatas();
        World.GenerateWorld(spawnPlayer);
    }

    private void loadBlockDatas()
    {
        foreach (var blockData in BlockAtlas.BlockDatas)
            Block.AddBlockData(blockData);
    }

    private void spawnPlayer()
    {
        if (Player != null)
            return;
        Vector3Int rayPos = new Vector3Int(World.ChunkSize / 2, World.ChunkHeight, World.ChunkSize / 2);
        if (Physics.Raycast(rayPos, Vector3.down, out RaycastHit hit, World.ChunkHeight))
        {
            Player = Instantiate(playerPrefab, hit.point + Vector3Int.up, Quaternion.identity).GetComponent<Player>();
            lastChunkPos = Player.ChunkPosition;
            startCheckingTheMap();
        }
    }
    
    private void startCheckingTheMap()
    {
        StopAllCoroutines();
        StartCoroutine(checkIfShouldLoadNextPosition());
    }

    private IEnumerator checkIfShouldLoadNextPosition()
    {
        yield return new WaitForSeconds(chunkDetectionTime);
        if (lastChunkPos != Player.ChunkPosition)
        {
            World.LoadAdditionalChunksRequest(Player);
            lastChunkPos = Player.ChunkPosition;
        }
        else
            StartCoroutine(checkIfShouldLoadNextPosition());
    }
}
