using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum InMenus { Main, Create, None}

public class MenuMain : MonoBehaviour {

    public GameObject mainUI, createUI, continueUI;

    public Transform playerCharacter;
    Character character;

    public CameraRig cameraRig;

    InMenus inMenus = InMenus.Main;

    public Button big, small;

    public GameObject[] options;
    int[] indexes;

	void Start () {
        character = playerCharacter.GetComponent<Character>();

        indexes = new int[options.Length];

        if (Time.time < 10) {
            OnstartGame();
        }else {
            OnContinuedGame();
        }
	}
	
    void Update() {
        if(inMenus == InMenus.Create) {
            Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            mouse = new Vector3(mouse.x, 0, mouse.y);
            Vector3 player = Camera.main.WorldToViewportPoint(playerCharacter.position);
            player = new Vector3(player.x, 0, player.y);
            Vector3 v = mouse - player;
            character.UpdateCharacter(v.x, v.z);
            character.AimGun(player, mouse);
        }

        if (playerCharacter.position.y < -1f) {
            int seed = Random.Range(0, int.MaxValue);
            PlayerPrefs.SetInt("Seed", seed);
            PlayerPrefs.Save();
            SceneManager.LoadScene(1);
        }
    }

    void OnstartGame() {
        if (HasSave()) {
            string[] data = PlayerPrefs.GetString("CharacterSkin").Split('/');
            character.SetNewCharacter(character.custom.GetCharacter(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4])));

            indexes[0] = int.Parse(data[0]);
            indexes[1] = int.Parse(data[1]);
            indexes[2] = int.Parse(data[2]);
            indexes[3] = int.Parse(data[3]);
            indexes[4] = int.Parse(data[4]);

            big.transform.GetChild(0).GetComponent<Text>().text = "Continue";
            small.transform.GetChild(0).GetComponent<Text>().text = "New";
            big.onClick.AddListener(() => Continue());
            small.onClick.AddListener(() => New());
        } else {
            indexes[0] = Random.Range(0, character.custom.hairs.Count);
            indexes[1] = Random.Range(0, character.custom.heads.Count);
            indexes[2] = Random.Range(0, character.custom.faces.Count);
            indexes[3] = Random.Range(0, character.custom.bodies.Count);
            indexes[4] = Random.Range(0, character.custom.hands.Count);

            character.SetNewCharacter(character.custom.GetCharacter(indexes[0], indexes[1], indexes[2], indexes[3], indexes[4]));

            small.gameObject.SetActive(false);
            big.transform.GetChild(0).GetComponent<Text>().text = "New";
            big.onClick.AddListener(() => New());
        }
    }

    void OnContinuedGame() {
        string[] data = PlayerPrefs.GetString("CharacterSkin").Split('/');
        character.SetNewCharacter(character.custom.GetCharacter(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4])));

        mainUI.SetActive(false);
        createUI.SetActive(false);
        continueUI.SetActive(true);
        character.aimOverride = false;

        inMenus = InMenus.None;
        Camera.main.transform.localPosition = Vector3.back * 35;
        Camera.main.fieldOfView = 20;

        EnablePlayerWalking();
        cameraRig.enabled = true;
    }

    bool HasSave() {
        return PlayerPrefs.HasKey("CharacterSkin");
    }

    public void Continue() {
        mainUI.SetActive(false);
        createUI.SetActive(false);
        continueUI.SetActive(true);
        StartCoroutine(ZoomOut());
    }

    void EnablePlayerWalking() {
        playerCharacter.GetComponent<Player>().enabled = true;
        playerCharacter.GetComponent<PlayerMovement>().enabled = true;
    }

    public void New() {
        mainUI.SetActive(false);
        createUI.SetActive(true);
        continueUI.SetActive(false);
        inMenus = InMenus.Create;

        for (int i = 0; i < options.Length; i++) {
            int k = i;
            options[i].transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => MinusItem(k));
            options[i].transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => PlusItem(k));
        }
    }

    public void PlusItem(int i) {
        print(i);
        indexes[i]++;
        switch (i) {
            case 0: if (indexes[i] >= character.custom.hairs.Count) indexes[i] = 0; break;
            case 1: if (indexes[i] >= character.custom.heads.Count) indexes[i] = 0; break;
            case 2: if (indexes[i] >= character.custom.faces.Count) indexes[i] = 0; break;
            case 3: if (indexes[i] >= character.custom.bodies.Count) indexes[i] = 0; break;
            case 4: if (indexes[i] >= character.custom.hands.Count) indexes[i] = 0; break;
        }
        character.SetNewCharacter(character.custom.GetCharacter(indexes[0], indexes[1], indexes[2], indexes[3], indexes[4]));
    }

    public void MinusItem(int i) {
        indexes[i]--;
        if (indexes[i] < 0) {

            switch (i) {
                case 0: indexes[i] = character.custom.hairs.Count - 1; break;
                case 1: indexes[i] = character.custom.heads.Count - 1; break;
                case 2: indexes[i] = character.custom.faces.Count - 1; break;
                case 3: indexes[i] = character.custom.bodies.Count - 1; break;
                case 4: indexes[i] = character.custom.hands.Count - 1; break;
            }
        }
        character.SetNewCharacter(character.custom.GetCharacter(indexes[0], indexes[1], indexes[2], indexes[3], indexes[4]));
    }

    public void Save() {
        string str = indexes[0].ToString() + "/" + indexes[1].ToString() + "/" + indexes[2].ToString() + "/" + indexes[3].ToString() + "/" + indexes[4].ToString();
        PlayerPrefs.SetString("CharacterSkin", str);
        PlayerPrefs.Save();

        Continue();
    }

    IEnumerator ZoomOut() {
        float timer = 0;

        Vector3 begin = Camera.main.transform.localPosition;

        while (timer < 1) {
            timer += Time.deltaTime * 0.5f;

            Camera.main.transform.localPosition = Vector3.Lerp(begin, begin + Vector3.back * 35, timer);
            Camera.main.fieldOfView = Mathf.Lerp(60, 20, timer);

            if(timer >= 1) {
                EnablePlayerWalking();
                cameraRig.enabled = true;
                character.aimOverride = false;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
