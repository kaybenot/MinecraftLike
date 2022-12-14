using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    [Header("GUI")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject gameMenu;
    [SerializeField] private GameObject gameInterface;

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
    public static GameObject Interface { get; private set; }
    
    private static GameObject gameMenu_s;
    private Vector3Int lastChunkPos;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Interface = gameInterface;
        gameMenu_s = gameMenu;
        OnNewChunksGenerated += startCheckingTheMap;
        BlockAtlas = blockAtlas;
        TextureOffset = textureOffset;
        CustomNoiseSettings = customNoiseSettings;
        ProgressBar = loadingScreen.GetComponentInChildren<ProgressBar>();
        BiomeGenerator = FindObjectOfType<BiomeGenerator>();
        WorldObj = worldObj;
        World = GameObject.FindWithTag("World").GetComponent<World>();
    }

    private async void Start()
    {
        Save.LoadWorld(GameState.SaveSlot);
        loadBlockDatas();
        World.WorldGenerator.OnWorldCreated += () =>
        {
            loadingScreen.SetActive(false);
            Interface.SetActive(true);
            spawnPlayer();
        };
        World.WorldGenerator.GenerateWorld();
        await Task.Run(Save.RunSaveLoop);
    }

    private void OnDestroy()
    {
        Save.ClearSaveInfo();
    }

    public static void ShowGameMenu()
    {
        Player.BlockInput = true;
        GameMenuShown = true;
        gameMenu_s.SetActive(true);
        Interface.SetActive(false);
            
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void HideGameMenu()
    {
        Player.BlockInput = false;
        GameMenuShown = false;
        gameMenu_s.SetActive(false);
        Interface.SetActive(true);
        
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
