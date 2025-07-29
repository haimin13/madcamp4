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
                UseEnemySkill();
        }
        else
        {
            UseEnemySkill();
            if (!model.isOver)
                UsePlayerSkill(skillIdx);
        }
    }
    public void UsePlayerSkill(int skillIdx)
    {
        Skill castedSkill = model.charaSkills[skillIdx];

        // if 공격스킬
        int atkStat = model.charaStatus[model.currentChara].tmpAtk;
        int spAtkStat = model.charaStatus[model.currentChara].tmpSpAtk;
        int defStat = model.enemyStatus.tmpDef;
        int spDefStat = model.enemyStatus.tmpSpDef;
        string defType = model.enemyStatus.charaType;

        int damage = model.CalculateDamage(castedSkill, atkStat, spAtkStat, defStat, spDefStat, defType);

        model.enemyStatus.currentHp -= damage;
        // view.ShowSkillAnimation

        view.UpdateStatusPanel(status: model.enemyStatus, isEnemy: true);
        if (model.isOver)
        {
            // 서버에 결과 전송
            // 보상?
            // 씬 전환
        }
    }

    public void UseEnemySkill()
    {
        // 랜덤하게 스킬 선택
        int idx = Random.Range(0, 4);
        Skill castedSkill = model.enemySkills[idx];

        // if 공격스킬
        int atkStat = model.enemyStatus.tmpAtk;
        int spAtkStat = model.enemyStatus.tmpSpAtk;
        int defStat = model.charaStatus[model.currentChara].tmpDef;
        int spDefStat = model.charaStatus[model.currentChara].tmpSpDef;
        string defType = model.charaStatus[model.currentChara].charaType;

        // 데미지 계산
        int damage = model.CalculateDamage(castedSkill, atkStat, spAtkStat, defStat, spDefStat, defType);

        // 데미지 적용
        model.charaStatus[model.currentChara].currentHp -= damage;
        // 애니메이션 출력
        view.UpdateStatusPanel(status: model.charaStatus[model.currentChara], isEnemy: false);
        // status 패널 업데이트
        if (model.isOver)
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
