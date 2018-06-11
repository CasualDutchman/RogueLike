using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//contains information about a level
//can change based on difficulty
[CreateAssetMenu(fileName = "LevelSettings", menuName = "Settings/LevelSettings", order = 1)]
public class LevelSettings : ScriptableObject {

    [Header("Level Gen")]
    public int seed;

    public int mainRoadLengthMin;
    public int mainRoadLengthMax;
    public int branchWeight;
    public int maxBranchOut;

    public float offsetSpacingMin;
    public float offsetSpacingMax;
    public float offsetLocalMin;
    public float offsetLocalMax;
}
