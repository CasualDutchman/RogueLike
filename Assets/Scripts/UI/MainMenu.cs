using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public InputField inputField;
    public Text actualSeed;

    int seed;

    public void OnChangeInput(string str) {
        str = str.ToLower();
        if (str.Length > 0) {
            bool b = int.TryParse(str, out seed);
            if (!b) {
                seed = str.GetHashCode();
            }
            actualSeed.text = seed.ToString();
        } else {
            actualSeed.text = "Random";
        }
    }

	public void Dungeon() {
        if (inputField.text.Length <= 0) {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        PlayerPrefs.SetInt("Seed", seed);

        SceneManager.LoadScene(1);
    }
}
