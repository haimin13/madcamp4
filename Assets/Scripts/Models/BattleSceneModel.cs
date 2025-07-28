using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneModel : MonoBehaviour
{
    public GameDataManager gameDataManager;
    public List<CurrentCharacterStatus> charaStatus;
    public CurrentCharacterStatus enemyStatus;
    public int currentChara;
    public bool isWin;
    public int currentRound;
    public List<Skill> charaSkills;
    public List<Skill> enemySkills;
    public Dictionary<string, Dictionary<string, float>> typeChart;
    // Start is called before the first frame update
    void Start()
    {
        if (GameDataManager.Instance != null)
        {
            gameDataManager = GameDataManager.Instance;
        }
    }

    public void LoadCurrentStatus()
    {
        currentRound = gameDataManager.currentRound;
        currentChara = gameDataManager.selectedChara;
        for (int i = 0; i < gameDataManager.charaStatus.Count; i++)
        {
            charaStatus[i] = gameDataManager.charaStatus[i];
        }
        LoadCharaSkills();

        // Enemy Initialize
        var enemy = CharacterSheet.Instance.enemies[currentRound - 1];
        enemyStatus.charaName = enemy.character_name;
        enemyStatus.maxHp = enemy.stats.hp + 100;
        enemyStatus.currentHp = enemyStatus.maxHp;
        enemyStatus.isAlive = true;
        enemySkills = enemy.skills;
    }

    public void LoadCharaSkills()
    {
        charaSkills = CharacterSheet.Instance.characters[currentChara].skills;
    }
    public void LoadTypeChart()
    {
        typeChart = gameDataManager.typeChart;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
