using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneController : MonoBehaviour
{
    public BattleSceneModel model;
    public BattleSceneView view;
    public APIRequester apiRequester;
    // Start is called before the first frame update
    void Start()
    {
        if (APIRequester.Instance != null)
            apiRequester = APIRequester.Instance;
        model.LoadCurrentStatus();
        model.LoadTypeChart();
        view.ShowSkills(model.charaSkills);
        view.SetStatusPanel(model.charaStatus[model.currentChara], model.enemyStatus);
    }

    public void SelectSkill(int skillIdx)
    {
        if (model.charaStatus[model.currentChara].tmpSpeed >= model.enemyStatus.tmpSpeed)
        {
            UsePlayerSkill(skillIdx);
            if (!model.isOver)
            {
                UseEnemySkill();
                view.SetSkillButtonsInteractable(true);
            }
        }
        else
        {
            UseEnemySkill();
            if (!model.isOver)
            {
                UsePlayerSkill(skillIdx);
                view.SetSkillButtonsInteractable(true);
            }
        }
        // 내 캐릭 죽고 남은 캐릭 남아있는 경우 추가
    }
    public void UsePlayerSkill(int skillIdx)
    {
        Skill castedSkill = model.charaSkills[skillIdx];
        view.SetLogtext($"{model.charaStatus[model.currentChara].charaName}은(는) {castedSkill.skill_name}을(를) 사용했다!");

        // if 공격스킬
        int atkStat = model.charaStatus[model.currentChara].tmpAtk;
        int spAtkStat = model.charaStatus[model.currentChara].tmpSpAtk;
        int defStat = model.enemyStatus.tmpDef;
        int spDefStat = model.enemyStatus.tmpSpDef;
        string defType = model.enemyStatus.charaType;

        int damage = model.CalculateDamage(castedSkill, atkStat, spAtkStat, defStat, spDefStat, defType);

        // view.ShowSkillAnimation

        model.enemyStatus.currentHp -= damage;
        view.UpdateStatusPanel(status: model.enemyStatus, isEnemy: true);
        view.SetLogtext($"{model.enemyStatus.charaName}에게 {damage}의 데미지!");
        if (model.enemyStatus.currentHp <= 0)
        {
            model.enemyStatus.currentHp = 0;
            model.enemyStatus.isAlive = false;
            view.SetLogtext($"{model.enemyStatus.charaName}은(는) 쓰러졌다!");
            CheckGameState();
        }
    }

    public void UseEnemySkill()
    {
        // 랜덤하게 스킬 선택
        int idx = Random.Range(0, 4);
        Skill castedSkill = model.enemySkills[idx];
        view.SetLogtext($"{model.enemyStatus.charaName}은(는) {castedSkill.skill_name}을(를) 사용했다!");

        // if 공격스킬
        int atkStat = model.enemyStatus.tmpAtk;
        int spAtkStat = model.enemyStatus.tmpSpAtk;
        int defStat = model.charaStatus[model.currentChara].tmpDef;
        int spDefStat = model.charaStatus[model.currentChara].tmpSpDef;
        string defType = model.charaStatus[model.currentChara].charaType;

        int damage = model.CalculateDamage(castedSkill, atkStat, spAtkStat, defStat, spDefStat, defType);

        // 애니메이션 출력
        // view.ShowSkillAnimation();

        model.charaStatus[model.currentChara].currentHp -= damage;
        view.UpdateStatusPanel(status: model.charaStatus[model.currentChara], isEnemy: false);
        view.SetLogtext($"{model.charaStatus[model.currentChara].charaName}에게 {damage}의 데미지!");

        if (model.charaStatus[model.currentChara].currentHp <= 0)
        {
            model.charaStatus[model.currentChara].currentHp = 0;
            model.charaStatus[model.currentChara].isAlive = false;
            view.SetLogtext($"{model.charaStatus[model.currentChara].charaName}은(는) 쓰러졌다!");
            CheckGameState();
        }
    }

    public void CheckGameState()
    {
        if (!model.enemyStatus.isAlive)
        {
            model.isOver = true;
            model.isWin = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
