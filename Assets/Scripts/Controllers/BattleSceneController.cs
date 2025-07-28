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

    public void UsePlayerSkill(int skillIdx)
    {
        Skill castedSkill = model.charaSkills[skillIdx];
        int damage = castedSkill.base_power;

        // 데미지 계산식

        model.enemyStatus.currentHp -= damage;
        // view.ShowSkillAnimation
        view.UpdateStatusPanel(status: model.enemyStatus, isEnemy: true);
        if (IsBattleOver())
        {
            // 서버에 결과 전송
            // 보상?
            // 씬 전환
        }
        else
        {
            useEnemySkill();
        }
    }

    public void useEnemySkill()
    {
        // 랜덤하게 스킬 선택
        // int damage = castedSkill.base_power;
        // 데미지 계산
        // 데미지 적용
        // 애니메이션 출력
        // status 패널 업데이트
        // 게임종료 확인
        // 턴전환

    }
    public bool IsBattleOver()
    {
        // 한쪽이라도 끝났는지
        // model.isWin = false;
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
