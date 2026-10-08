using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para> Handles holding and running monster behaviors during a level. Can ask it to spawn a monster from its list (which is provided by level data)</para>
/// </summary>
public class Manager_Monsters : MonoBehaviour
{
    public static Manager_Monsters Instance { get; private set; }

    public CustomMonsterSpawner[] monstersInLevel { get; private set; } // may need to change to transform if prefabbing doesnt work
    public List<MonsterOrigin> monstersInPlay = new List<MonsterOrigin>();


    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
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
        if (!RemoveAllMonstersFromMemory()) return;
        if (_monsterList == null) return;

        _monsterList.CopyTo(monstersInLevel, _monsterList.Length); // create a copy of the data to our local array
    }

    public void SpawnMonsterIntoPlay(MonsterOrigin _monster)
    {
        MonsterOrigin monsterToInitialize = null;
        monsterToInitialize = CheckMonstersInPlay(_monster);
        if (monsterToInitialize == null) monsterToInitialize = CreateMonsterToPlay(_monster);

        // next is to reset it and run it / play intro (which should happen on the monster)

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
        if (monstersInPlay.Count == 0) return;

        for(int i = 0; i < monstersInPlay.Count; i++)
        {
            if(monstersInPlay[i].gameObject.activeSelf == true) monstersInPlay[i].Tick();
        }
    }
}
