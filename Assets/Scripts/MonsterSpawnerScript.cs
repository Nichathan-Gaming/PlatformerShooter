using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnerScript : MonoBehaviour
{
    [SerializeField] float secondsToNextSpawn;

    public void MonsterDeath(MonsterControllerScript deadMonster)
    {
        StartCoroutine(WaitToRespawn());

        IEnumerator WaitToRespawn()
        {
            yield return new WaitForSeconds(secondsToNextSpawn);

            deadMonster.Revive();
        }
    }
}
