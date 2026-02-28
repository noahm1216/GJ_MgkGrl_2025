using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorBackground : MonoBehaviour
{
    public PlayerCore ref_PlayerCore;
    public Transform despawnBar, spawnBar;
    public List<BackgroundLayerData> backgroundLayers = new List<BackgroundLayerData>();

    private Vector3 depsawnBarStartPos, spawnBarStartPos;

    // Start is called before the first frame update
    void Start()
    {
        if (despawnBar) { depsawnBarStartPos = despawnBar.position; despawnBar.gameObject.SetActive(false); }
        if (spawnBar) { spawnBarStartPos = spawnBar.position; spawnBar.gameObject.SetActive(false); }
        // spawn the assets throughout the level
    }

    // REMOVE THIS ONCE ITS TESTED... we will call it from platform manager
    void Update() 
    {
        // move the platforms
        // if an assets position is greater than X ... move to Y

        if (Manager_Platforms.Instance && ref_PlayerCore)
            RunBackgroundMovement(new Vector3(Manager_Platforms.Instance.CurrentSpeed(), 0, 0));
        else print("No Manager Platforms or PlayerCore Ref");
    }

    public void RunBackgroundMovement(Vector3 _direction)
    {
        if (spawnBar) spawnBar.position = new Vector3(spawnBarStartPos.x + ref_PlayerCore.transform.position.x, spawnBarStartPos.y, spawnBarStartPos.z);
        if(despawnBar) despawnBar.position = new Vector3(depsawnBarStartPos.x + ref_PlayerCore.transform.position.x, depsawnBarStartPos.y, depsawnBarStartPos.z);
        if (backgroundLayers.Count == 0) { print("No Background Layers To Move"); return; }

        for(int i = 0; i < backgroundLayers.Count; i++)
        {           
            if (!backgroundLayers[i].layerParent) { print($"No Background Layer Parent To Review: {i}"); continue; }
            backgroundLayers[i].layerParent.position = new Vector3(0, 0 - (ref_PlayerCore.transform.position.y * backgroundLayers[i].layerSpeedMultiplier.y), backgroundLayers[i].layerParent.position.z);

            for (int j = 0; j < backgroundLayers[i].layerParent.childCount; j++)
            {
                Transform child = backgroundLayers[i].layerParent.GetChild(j);
                // move each of the children by the layer multiplier
                child.position += (_direction * backgroundLayers[i].layerSpeedMultiplier.x);                
                if (child.position.x < despawnBar.position.x)
                    child.position = new Vector3(spawnBar.position.x, child.position.y, child.position.z);
                if (child.position.x > spawnBar.position.x)
                    child.position = new Vector3(despawnBar.position.x, child.position.y, child.position.z);
            }
        }
    }
}


// the custom data for platforms
[System.Serializable]
public class BackgroundLayerData
{
    public string layerNickname;
    public Transform layerParent;
    public List<Transform> objectsToPool = new List<Transform>();
    // maybe another custom class on the object we will instantiate
    //  - the ideal position
    //  - any randomness allowed to spawning
    //  - maybe minimum size / delay before something else can spawn after?
    [Tooltip("X changes the speed an object moves opposite to the player (0 = no move, 1 = moves past fast) || Y = % height change when player moves up and down vertically")]
    public Vector2 layerSpeedMultiplier = new Vector2(1,-0.25f);

   

   
    //public BackgroundLayerData(string _newName,)
    //{
    //    //abilityNickname = _newName;
    //}

}//end of data for platforms
