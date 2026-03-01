using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorBackground : MonoBehaviour
{
    
    public Transform despawnBar, spawnBar;
    public List<BackgroundLayerData> backgroundLayers = new List<BackgroundLayerData>();

    private List<Transform> spawnedBackgroundLayers = new List<Transform>();
    private Vector3 depsawnBarStartPos, spawnBarStartPos;
    private PlayerCore ref_PlayerCore;


    void Start()
    {        
        if (despawnBar) { depsawnBarStartPos = despawnBar.position; despawnBar.gameObject.SetActive(false); }
        if (spawnBar) { spawnBarStartPos = spawnBar.position; spawnBar.gameObject.SetActive(false); }
        if (Manager_Platforms.Instance) ref_PlayerCore = Manager_Platforms.Instance.AssignBackground(this);
        // spawn the assets throughout the level
        //SpawnBackgroundLayers();
    }

    public void AssignBackgroundLayers(List<BackgroundLayerData> _bgLD)
    {
        backgroundLayers = _bgLD;
        SpawnBackgroundLayers();
    }
        

    public void SpawnBackgroundLayers()
    {
        if (backgroundLayers.Count == 0) { print("No Background Layers To Spawn"); return; }

        RemoveBackground();

        for ( int i = 0; i< backgroundLayers.Count; i++)
        {
            if (!backgroundLayers[i].layerParent) { print($"Missing Layer Background #{i}"); continue; }
            Transform bgLayer = Instantiate(backgroundLayers[i].layerParent);
            bgLayer.SetParent(transform);
            spawnedBackgroundLayers.Add(bgLayer);
        }
    }

    public void RemoveBackground()
    {
        if (transform.childCount > 2)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.ToLower().Contains("spawn_")) continue;
                //if (transform.GetChild(i).childCount == 0) continue;
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

    public void RunBackgroundMovement(Vector3 _direction)
    {
        if (spawnBar) spawnBar.position = new Vector3(spawnBarStartPos.x + ref_PlayerCore.transform.position.x, spawnBarStartPos.y, spawnBarStartPos.z);
        if(despawnBar) despawnBar.position = new Vector3(depsawnBarStartPos.x + ref_PlayerCore.transform.position.x, depsawnBarStartPos.y, depsawnBarStartPos.z);
        if (spawnedBackgroundLayers.Count == 0) { print("No Background Layers To Move"); return; }

        for(int i = 0; i < spawnedBackgroundLayers.Count; i++)
        {           
            if (!spawnedBackgroundLayers[i]) { print($"Missing Background Layer Parent To Review: {i}"); continue; }
            spawnedBackgroundLayers[i].position = new Vector3(0, 0 - (ref_PlayerCore.transform.position.y * backgroundLayers[i].layerSpeedMultiplier.y), spawnedBackgroundLayers[i].position.z);

            for (int j = 0; j < spawnedBackgroundLayers[i].childCount; j++)
            {
                Transform child = spawnedBackgroundLayers[i].GetChild(j);
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
    [Tooltip("X changes the speed an object moves opposite to the player (0 = no move, 1 = moves past fast) || Y = % height change when player moves up and down vertically")]
    public Vector2 layerSpeedMultiplier = new Vector2(1,-0.25f);

   

   
    //public BackgroundLayerData(string _newName,)
    //{
    //    //abilityNickname = _newName;
    //}

}//end of data for platforms
