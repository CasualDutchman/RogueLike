using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    public static LevelManager instance;

    LevelGeneration levelGenerator;

    public LevelSettings easyLevelSettings;
    public LevelSettings customLevelSettings;

    public bool disableRoomDespawning;
    public bool usePlayerPref;
    bool hasCustomSettings;

    public List<Room> rooms = new List<Room>();

    void Awake() {
        instance = this;
    }

    void Start () {
        if (usePlayerPref && PlayerPrefs.HasKey("CustomLevelSettings")) {
            hasCustomSettings = PlayerPrefs.GetInt("CustomLevelSettings") == 1;
            Debug.Log(hasCustomSettings);

            if (hasCustomSettings && PlayerPrefs.HasKey("LevelSettings")) {
                Debug.Log(PlayerPrefs.GetString("LevelSettings"));
                string[] data = PlayerPrefs.GetString("LevelSettings").Split('/');

                customLevelSettings.seed = PlayerPrefs.GetInt("Seed");

                customLevelSettings.mainRoadLengthMin = int.Parse(data[0]);
                customLevelSettings.mainRoadLengthMax = int.Parse(data[1]);
                customLevelSettings.branchWeight = int.Parse(data[2]);
                customLevelSettings.maxBranchOut = int.Parse(data[3]);
                customLevelSettings.offsetSpacingMin = int.Parse(data[4]);
                customLevelSettings.offsetSpacingMax = int.Parse(data[5]);
                customLevelSettings.offsetLocalMin = int.Parse(data[6]);
                customLevelSettings.offsetLocalMax = int.Parse(data[7]);
            }
        }else if(usePlayerPref) {
            easyLevelSettings.seed = PlayerPrefs.GetInt("Seed");
        }

        //Debug.Log(PlayerPrefs.GetInt("Seed"));

        levelGenerator = GetComponent<LevelGeneration>();
        levelGenerator.levelManager = this;
        levelGenerator.genStages = 100;
        levelGenerator.ClearLevel();

        levelGenerator.GenerateLevel(usePlayerPref && hasCustomSettings ? customLevelSettings : easyLevelSettings, disableRoomDespawning);

    }

    void OnDisable() {
        GetComponent<NavMeshSurface>().RemoveData();
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene(0);
        }
	}
}
