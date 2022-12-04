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
    [SerializeField] private GameObject worldObj;
    [SerializeField] private float chunkDetectionTime = 1f;
    [SerializeField] private CustomNoiseSettings customNoiseSettings;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Other")]
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject gameMenu;

    public static BlockAtlas BlockAtlas { get; private set; }
    public static float TextureOffset { get; private set; }
    public static CustomNoiseSettings CustomNoiseSettings { get; private set; }
    public static World World { get; private set; }
    public static Player Player { get; private set; }
    public static Action OnNewChunksGenerated { get; set; }
    public static GameObject WorldObj { get; private set; }
    public static ProgressBar ProgressBar { get; private set; }
    public static BiomeGenerator BiomeGenerator { get; private set; }
    public static bool GameMenuShown { get; private set; }

    
    private static GameObject title_s;
    private static GameObject gameMenu_s;
    private Vector3Int lastChunkPos;
    
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        title_s = title;
        gameMenu_s = gameMenu;
        OnNewChunksGenerated += startCheckingTheMap;
        BlockAtlas = blockAtlas;
        TextureOffset = textureOffset;
        CustomNoiseSettings = customNoiseSettings;
        ProgressBar = progressBar;
        BiomeGenerator = FindObjectOfType<BiomeGenerator>();
        WorldObj = worldObj;
        World = GameObject.FindWithTag("World").GetComponent<World>();
    }

    private void Start()
    {
        loadBlockDatas();
        World.WorldGenerator.OnWorldCreated += () =>
        {
            ProgressBar.gameObject.SetActive(false);
            title.SetActive(false);
            background.SetActive(false);
            spawnPlayer();
        };
        World.WorldGenerator.GenerateWorld();
    }

    public static void ShowGameMenu()
    {
        Player.BlockInput = true;
        GameMenuShown = true;
        title_s.gameObject.SetActive(true);
        gameMenu_s.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void HideGameMenu()
    {
        Player.BlockInput = false;
        GameMenuShown = false;
        title_s.gameObject.SetActive(false);
        gameMenu_s.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void loadBlockDatas()
    {
        ProgressBar.SetDescription("Loading block data");
        foreach (var blockData in BlockAtlas.BlockDatas)
            Block.AddBlockData(blockData);
        ProgressBar.SetProgress(0.05f);
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
            World.WorldGenerator.LoadAdditionalChunksRequest(Player);
            lastChunkPos = Player.ChunkPosition;
        }
        else
            StartCoroutine(checkIfShouldLoadNextPosition());
    }
}
