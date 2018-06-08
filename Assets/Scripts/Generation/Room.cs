using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Room : MonoBehaviour {

    LevelManager levelManager;

    public RoomData roomData;

    public bool endRoom = false;
    public bool spawnRoom = false;

    public bool visible = false;
    public bool closed = false;

    List<Enemy> enemies = new List<Enemy>();

    List<GameObject> doors = new List<GameObject>();

    float endTimer = 0;

    void Awake() {
        levelManager = LevelManager.instance;
    }
	
	void Update () {
        if (closed && enemies.Count > 0) {
            bool has = false;
            foreach (Enemy en in enemies) {
                if (en != null) {
                    has = true;
                }
            }
            if (!has) {
                if (endRoom) {
                    endTimer += Time.deltaTime;
                    if (endTimer >= 3f) {
                        SceneManager.LoadScene(0);
                    }
                }else {
                    OpenDoors();
                }
            }
        }
	}

    void FirstJoin() {
        if (roomData.roomType == RoomType.Room && !spawnRoom) {
            SpawnEnemies();
            if (endRoom) {
                SpawnBoss();
            }
            CloseDoors();
        }
    }

    void SpawnEnemies() {
        for (int i = 0; i < roomData.enemyPositions.Count; i++) {
            GameObject go = Instantiate(levelManager.theme.enemies[Random.Range(0, levelManager.theme.enemies.Length)], transform.GetChild(0));
            go.GetComponent<NavMeshAgent>().enabled = false;
            go.transform.position = roomData.enemyPositions[i];
            go.GetComponent<NavMeshAgent>().enabled = true;
            go.GetComponent<Enemy>().target = levelManager.player;
            enemies.Add(go.GetComponent<Enemy>());
        }
    }

    void SpawnBoss() {
        GameObject go = Instantiate(levelManager.theme.enemies[Random.Range(0, levelManager.theme.enemies.Length)], transform.GetChild(0));
        go.GetComponent<NavMeshAgent>().enabled = false;
        go.transform.position = roomData.node.worldPosition;
        go.GetComponent<NavMeshAgent>().enabled = true;

        go.transform.localScale = Vector3.one * 2;
        go.GetComponent<NavMeshAgent>().speed = 1;

        go.GetComponent<Enemy>().target = levelManager.player;
        go.GetComponent<Enemy>().health = 25;
        enemies.Add(go.GetComponent<Enemy>());
    }

    void CloseDoors() {
        foreach (KeyValuePair<Vector3, float> doorPos in roomData.doorLocations) {
            GameObject go = Instantiate(levelManager.theme.doorClosedObject, doorPos.Key, Quaternion.Euler(0, doorPos.Value, 0));
            go.transform.parent = transform.GetChild(0).transform;
            doors.Add(go);
        }

        levelManager.player.position = Vector3.Lerp(levelManager.player.position, roomData.node.worldPosition, 0.1f);
    }

    void OpenDoors() {
        while(doors.Count > 0) {
            Destroy(doors[0]);
            doors.RemoveAt(0);
        }
        closed = false;
    }

    void OnTriggerEnter(Collider other) {
        if (levelManager != null && !levelManager.disableRoomDespawning && !visible) {
            transform.GetChild(0).gameObject.SetActive(true);
            visible = true;
            closed = true;
            FirstJoin();
        }

        CameraRig.instance.SetNewPosition(roomData.node.worldPosition);

        if (endRoom)
            Debug.Log("End Game");
    }
}
