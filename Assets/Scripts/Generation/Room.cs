using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    LevelManager levelManager;

    public RoomData roomData;

    public bool activeRoom = false;
    public bool endRoom = false;

    void Awake() {
        levelManager = LevelManager.instance;
    }
	
	void Update () {
        if (activeRoom)
            OnUpdate();
	}

    void OnUpdate() {
        
    }

    void OnTriggerEnter(Collider other) {
        if (!levelManager.disableRoomDespawning) {
            transform.GetChild(0).gameObject.SetActive(true);
            activeRoom = true;
        }

        if (endRoom)
            Debug.Log("End Game");
    }

    void OnTriggerExit(Collider other) {
        if (!levelManager.disableRoomDespawning) {
            transform.GetChild(0).gameObject.SetActive(false);
            activeRoom = false;
        }
    }
}
