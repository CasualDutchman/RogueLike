using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public Toggle advanced;
    public InputField inputFieldSeed;
    public InputField inputFieldRoadMin, inputFieldRoadMax;
    public InputField inputFieldBranchWeights;
    public InputField inputFieldBranchout;
    public InputField inputFieldWOffsetMin, inputFieldWOffsetMax;
    public InputField inputFieldLOffsetMin, inputFieldLOffsetMax;

    int GetSeed() {
        string str = inputFieldSeed.text.ToLower();
        if (str.Length <= 0)
            return Random.Range(0, int.MaxValue);

        int seed = 0;
        bool b = int.TryParse(str, out seed);
        if (!b) {
            seed = str.GetHashCode();
        }
        return seed;
    }

	public void Dungeon() {
        PlayerPrefs.SetInt("CustomLevelSettings", advanced.isOn ? 1 : 0);
        int seed = GetSeed();
        PlayerPrefs.SetInt("Seed", seed);
        Debug.Log(advanced.isOn);
        Debug.Log(seed);
        if (advanced.isOn) {
            string str = "";
            str += inputFieldRoadMin.text.ToString() + "/";
            str += inputFieldRoadMax.text.ToString() + "/";
            str += inputFieldBranchWeights.text.ToString() + "/";
            str += inputFieldBranchout.text.ToString() + "/";
            str += inputFieldWOffsetMin.text.ToString() + "/";
            str += inputFieldWOffsetMax.text.ToString() + "/";
            str += inputFieldLOffsetMin.text.ToString() + "/";
            str += inputFieldLOffsetMax.text.ToString();
            Debug.Log(str);

            PlayerPrefs.SetString("LevelSettings", str);
        }

        SceneManager.LoadScene(1);
    }

    public void OnChangeMainRoadMin(string s) {
        if (int.Parse(s) < 3) {
            inputFieldRoadMin.text = 3.ToString();
        }else if (int.Parse(s) > 20) {
            inputFieldRoadMin.text = 20.ToString();
        }
    }
}
