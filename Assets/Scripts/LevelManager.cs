using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour {

    public static LevelManager instance;

    LevelGeneration levelGenerator;

    public LevelSettings levelSettings;

    public bool disableRoomDespawning;
    public bool usePlayerPref;

    public List<Room> rooms = new List<Room>();

    void Awake() {
        instance = this;
    }

    void Start () {
        if (usePlayerPref && PlayerPrefs.HasKey("Seed"))
            levelSettings.seed = PlayerPrefs.GetInt("Seed");

        levelGenerator = GetComponent<LevelGeneration>();
        levelGenerator.levelManager = this;
        levelGenerator.genStages = 100;
        levelGenerator.ClearLevel();

        levelGenerator.GenerateLevel(levelSettings, disableRoomDespawning);

    }

    void OnDisable() {
        GetComponent<NavMeshSurface>().RemoveData();
    }

    void Update () {
		
	}
}
