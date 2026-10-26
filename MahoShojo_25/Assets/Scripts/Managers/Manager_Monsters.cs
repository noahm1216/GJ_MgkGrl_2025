using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para> Handles holding and running monster behaviors during a level. Can ask it to spawn a monster from its list (which is provided by level data)</para>
/// </summary>
public class Manager_Monsters : MonoBehaviour
{
    public static Manager_Monsters Instance { get; private set; }
    private Manager_GameState _ManagerGameState;

    public CustomMonsterSpawner[] monstersInLevel { get; private set; } // reference passed during level load
    public List<MonsterOrigin> monstersInPlay = new List<MonsterOrigin>(); // the ones moving and running

    private float updateDelayStamp, updateDelayTime = 1; // to run update less

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        _ManagerGameState = Manager_GameState.Instance;
    }

    private bool RemoveAllMonstersFromMemory()
    {
        for (int i = 0; i < monstersInPlay.Count; i++)
        {
            if(monstersInPlay[i]) Destroy(monstersInPlay[i].gameObject);
        }
        monstersInPlay.Clear();
        monstersInLevel = null;
        return true;
    }

    public void UpdateMonstersInLevel(CustomMonsterSpawner[] _monsterList)
    {
        print($"Update Monsters In The Level with - {_monsterList.Length} monsters");
        if (!RemoveAllMonstersFromMemory()) return;
        if (_monsterList == null) return;

        monstersInLevel = new CustomMonsterSpawner[_monsterList.Length]; // allocate memory
        _monsterList.CopyTo(monstersInLevel, 0); // create a copy of the data to our local array
    }

    public void SpawnMonsterIntoPlay(MonsterOrigin _monster)
    {
        MonsterOrigin monsterToInitialize = null;
        monsterToInitialize = CheckMonstersInPlay(_monster);
        if (monsterToInitialize == null) monsterToInitialize = CreateMonsterToPlay(_monster);

        // next is to reset it and run it / play intro (which should happen on the monster)
        monsterToInitialize.BeginIntroduction();
    }

    private MonsterOrigin CheckMonstersInPlay(MonsterOrigin _monster)
    {
        MonsterOrigin monsterMatch = null;       
        for (int i = 0; i < monstersInPlay.Count; i++)  // see if its in play but turned off so we can pool data
        {
            if (!monstersInPlay[i]) continue;
            if (monstersInPlay[i].gameObject.activeSelf == true) continue;
            if (monstersInPlay[i] == _monster) monsterMatch = monstersInPlay[i];            
        }

        return monsterMatch;
    }

    private MonsterOrigin CreateMonsterToPlay(MonsterOrigin _monster)
    {
        MonsterOrigin monsterSpawned = null;
        for (int i = 0; i < monstersInLevel.Length; i++)
        {
            if (monstersInLevel[i] == null) continue;

            if (monstersInLevel[i].monsterBoss == _monster)
            {
                monsterSpawned = Instantiate(monstersInLevel[i].monsterBoss);
                monstersInPlay.Add(monsterSpawned);
                monsterSpawned.transform.SetParent(transform);
            }
        }        
        return monsterSpawned;
    }

    private void Update()
    {
        if (monstersInLevel == null || monstersInLevel.Length == 0) return;

        if (Time.time > updateDelayStamp + updateDelayTime) // running checks on a delay because its a lot and doesnt need to be so frequent
        {
            for (int i = 0; i < monstersInLevel.Length; i++) // Check conditions for spawning monsters
            {
                if (CheckSpawnConditions(monstersInLevel[i])) // if we havent spawned yet, we will and we can remove it from the list to no longer spawn
                {
                    monstersInLevel[i] = null;
                }
            }
            updateDelayStamp = Time.time;
        }

        if (monstersInPlay.Count == 0) return;

        for(int i = 0; i < monstersInPlay.Count; i++) // Run Any Monsters In Play
            if(monstersInPlay[i].gameObject.activeSelf == true) monstersInPlay[i].Tick();
        
    }


    private bool CheckSpawnConditions(CustomMonsterSpawner _monster)
    {
        if (_monster == null) return false;

        if (_ManagerGameState && _ManagerGameState.currentState == Manager_GameState.GAMESTATE.Playing)
        {
            if (_ManagerGameState.CheckMetConditions(_ManagerGameState.dataSinceLevelStarted, _monster.requiredConditions)) // Compare if we met enough win conditions in the level
            {
                print("Triggering MonsterSpawn");
                SpawnMonsterIntoPlay(_monster.monsterBoss); // spawn it                
                return true;
            }
        }
        return false;
    }
}
