using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BattleSceneModel : MonoBehaviour
{
    public GameDataManager gameDataManager;
    public List<CurrentCharacterStatus> charaStatus;
    public CurrentCharacterStatus enemyStatus;
    public int currentChara;
    public int candidate = 99;
    public bool isOver = false;
    public bool isWin;
    public bool isDead = false;
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
        charaStatus = new List<CurrentCharacterStatus>();
        currentRound = GameDataManager.Instance.currentRound;
        currentChara = GameDataManager.Instance.selectedChara;
        for (int i = 0; i < GameDataManager.Instance.charaStatus.Count; i++)
        {
            charaStatus.Add(GameDataManager.Instance.charaStatus[i]);
        }
        LoadCharaSkills();

        // Enemy Initialize
        enemyStatus = new CurrentCharacterStatus();
        var enemy = CharacterSheet.Instance.enemies[currentRound - 1];
        enemyStatus.charaName = enemy.character_name;
        enemyStatus.charaType = enemy.character_type;
        enemyStatus.debuff = "정상";
        enemyStatus.duration = 0;
        enemyStatus.maxHp = enemy.stats.hp + 100;
        enemyStatus.currentHp = enemyStatus.maxHp;
        enemyStatus.tmpAtk = enemy.stats.atk;
        enemyStatus.tmpDef = enemy.stats.def;
        enemyStatus.tmpSpAtk = enemy.stats.sp_atk;
        enemyStatus.tmpSpDef = enemy.stats.sp_def;
        enemyStatus.tmpSpeed = enemy.stats.speed;
        enemyStatus.isAlive = true;
        enemySkills = enemy.skills;
    }

    public void LoadCharaSkills()
    {
        charaSkills = CharacterSheet.Instance.characters[currentChara].skills;
    }
    public void LoadTypeChart()
    {
        typeChart = GameDataManager.Instance.typeChart;
    }
    public int RankUpStat(int origStat, int step)
    {
        int k = 2;
        float weight = (k + Math.Max(0, step)) / (k + Math.Min(0, step));
        return (int)(origStat * weight);
    }
    public int CalculateDamage(Skill casted, int atkStat, int spAtkStat, int defStat, int spDefStat, string defType)
    {
        float atk = casted.base_power / 100f;
        float def = 0;
        if (casted.damage_type == "물리")
        {
            atk = atk * atkStat; // 임시
            def = defStat;
        }
        else
        {
            float multiplier = typeChart[casted.skill_type][defType];
            atk =  atk * spAtkStat * multiplier; // 임시
            def = spDefStat;
        }
        return (int)(atk * 100f / (def + 50f));

    }

    // Update is called once per frame
    void Update()
    {

    }
}
