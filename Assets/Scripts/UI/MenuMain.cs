using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InMenus { Main, Create, None}

public class MenuMain : MonoBehaviour {

    public GameObject mainUI, createUI, continueUI;

    public Transform playerCharacter;
    Character character;

    InMenus inMenus = InMenus.Main;

    public Button big, small;

	void Start () {
        character = playerCharacter.GetComponent<Character>();

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
    }

    void OnstartGame() {
        if (HasSave()) {
            big.transform.GetChild(0).GetComponent<Text>().text = "Continue";
            small.transform.GetChild(0).GetComponent<Text>().text = "New";
            big.onClick.AddListener(() => Continue());
            small.onClick.AddListener(() => New());
        } else {
            small.gameObject.SetActive(false);
            big.transform.GetChild(0).GetComponent<Text>().text = "New";
            big.onClick.AddListener(() => New());
        }
    }

    void OnContinuedGame() {
        mainUI.SetActive(false);
        createUI.SetActive(false);
        continueUI.SetActive(true);

        inMenus = InMenus.None;
        Camera.main.transform.localPosition = Vector3.back * 60;
        Camera.main.fieldOfView = 20;
    }

    bool HasSave() {
        return true;
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
    }

    IEnumerator ZoomOut() {
        float timer = 0;

        Vector3 begin = Camera.main.transform.localPosition;

        while (timer < 1) {
            timer += Time.deltaTime * 0.5f;

            Camera.main.transform.localPosition = Vector3.Lerp(begin, begin + Vector3.back * 60, timer);
            Camera.main.fieldOfView = Mathf.Lerp(60, 20, timer);

            if(timer >= 1) {
                EnablePlayerWalking();
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
