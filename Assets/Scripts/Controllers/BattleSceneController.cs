using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleSceneController : MonoBehaviour
{
    public BattleSceneModel model;
    public BattleSceneView view;
    public APIRequester apiRequester;
    public PlayerManager player;
    public PlayerManager enemy;
    
    // Start is called before the first frame update
    void Start()
    {
        bool debug = true;
        if (debug)
        {
            GameDataManager.Instance.currentRound = 1;
            GameDataManager.Instance.selectedChara = 0;
            // save
            // GameDataManager.Instance.SaveCharacters();
            // GameDataManager.Instance.SaveTypeChart();

            // load
            GameDataManager.Instance.LoadCharacters();
            GameDataManager.Instance.LoadTypeChart();
            GameDataManager.Instance.UpdateCharacterStatus();
        }
        if (APIRequester.Instance != null)
            apiRequester = APIRequester.Instance;

        model.LoadCurrentStatus();
        model.LoadTypeChart();

        view.ShowSkills(model.charaSkills);
        view.SetStatusPanel(model.charaStatus[model.currentChara], model.enemyStatus);

        player.setImage(model.charaStatus[model.currentChara].charaImageUrl);
        enemy.setImage(model.enemyStatus.charaImageUrl);
    }
    public void SelectSkill(int skillIdx)
    {
        StartCoroutine(SelectSkillCoroutine(skillIdx));
    }

    private IEnumerator SelectSkillCoroutine(int skillIdx)
    {
        if (model.charaStatus[model.currentChara].tmpSpeed >= model.enemyStatus.tmpSpeed)
        {
            yield return StartCoroutine(UsePlayerSkillCoroutine(skillIdx));
            if (model.isOver)
            {
                view.ShowGameOver(model.isWin);
                yield break;
            }

            yield return StartCoroutine(UseEnemySkillCoroutine());
            if (model.isOver)
            {
                view.ShowGameOver(model.isWin);
                yield break;
            }
            if (!model.charaStatus[model.currentChara].isAlive)
                view.ShowSwitchPanel();
            else view.SetSkillButtonsInteractable(true);
        }
        else
        {
            yield return StartCoroutine(UseEnemySkillCoroutine());
            if (model.isOver)
            {
                view.ShowGameOver(model.isWin);
                yield break;
            }
            if (!model.charaStatus[model.currentChara].isAlive)
            {
                view.ShowSwitchPanel();
                yield break;
            }
            yield return StartCoroutine(UsePlayerSkillCoroutine(skillIdx));
            if (model.isOver)
            {
                view.ShowGameOver(model.isWin);
                yield break;
            }
            else view.SetSkillButtonsInteractable(true);
        }
    }
    public void UsePlayerSkill(int skillIdx)
    {
        StartCoroutine(UsePlayerSkillCoroutine(skillIdx));
    }
    private IEnumerator UsePlayerSkillCoroutine(int skillIdx)
    {
        Skill castedSkill = model.charaSkills[skillIdx];
        player.PlaySkillAnimation(castedSkill);
        yield return view.SetLogtextAndWait($"{model.charaStatus[model.currentChara].charaName}은(는) {castedSkill.skill_name}을(를) 사용했다!");

        // if 공격스킬
        int atkStat = model.charaStatus[model.currentChara].tmpAtk;
        int spAtkStat = model.charaStatus[model.currentChara].tmpSpAtk;
        int defStat = model.enemyStatus.tmpDef;
        int spDefStat = model.enemyStatus.tmpSpDef;
        string defType = model.enemyStatus.charaType;

        int damage = model.CalculateDamage(castedSkill, atkStat, spAtkStat, defStat, spDefStat, defType);

        enemy.PlayHitAnimation();

        model.enemyStatus.currentHp -= damage;
        view.UpdateStatusPanel(status: model.enemyStatus, isEnemy: true);
        yield return view.SetLogtextAndWait($"{model.enemyStatus.charaName}에게 {damage}의 데미지!");
        if (model.enemyStatus.currentHp <= 0)
        {
            model.enemyStatus.currentHp = 0;
            model.enemyStatus.isAlive = false;
            yield return view.SetLogtextAndWait($"{model.enemyStatus.charaName}은(는) 쓰러졌다!");
            CheckGameState();
        }
    }
    public void UseEnemySkill()
    {
        StartCoroutine(UseEnemySkillCoroutine());
    }

    private IEnumerator UseEnemySkillCoroutine()
    {
        // 랜덤하게 스킬 선택
        int idx = Random.Range(0, 4);
        Skill castedSkill = model.enemySkills[idx];
        enemy.PlaySkillAnimation(castedSkill);
        yield return view.SetLogtextAndWait($"{model.enemyStatus.charaName}은(는) {castedSkill.skill_name}을(를) 사용했다!");

        // if 공격스킬
        int atkStat = model.enemyStatus.tmpAtk;
        int spAtkStat = model.enemyStatus.tmpSpAtk;
        int defStat = model.charaStatus[model.currentChara].tmpDef;
        int spDefStat = model.charaStatus[model.currentChara].tmpSpDef;
        string defType = model.charaStatus[model.currentChara].charaType;

        int damage = model.CalculateDamage(castedSkill, atkStat, spAtkStat, defStat, spDefStat, defType);

        // 애니메이션 출력
        player.PlayHitAnimation();

        model.charaStatus[model.currentChara].currentHp -= damage;
        view.UpdateStatusPanel(status: model.charaStatus[model.currentChara], isEnemy: false);
        yield return view.SetLogtextAndWait($"{model.charaStatus[model.currentChara].charaName}에게 {damage}의 데미지!");

        if (model.charaStatus[model.currentChara].currentHp <= 0)
        {
            model.charaStatus[model.currentChara].currentHp = 0;
            model.charaStatus[model.currentChara].isAlive = false;
            yield return view.SetLogtextAndWait($"{model.charaStatus[model.currentChara].charaName}은(는) 쓰러졌다!");
            model.isDead = true;
            CheckGameState();
        }
    }

    public void CheckGameState()
    {
        if (!model.enemyStatus.isAlive)
        {
            model.isOver = true;
            model.isWin = true;
            return;
        }
        if (!model.charaStatus[model.currentChara].isAlive)
        {
            bool exists = model.charaStatus.Any(c => c.isAlive);
            if (!exists)
            {
                model.isOver = true;
                model.isWin = false;
            }
            return;
        }
    }

    public void ChangeCandidate(int idx)
    {
        Debug.Log($"ChangeCandidate idx: {idx}");
        model.candidate = idx;
        view.ShowCandidate(CharacterSheet.Instance.characters[idx].skills);
    }

    public void SwitchCharacter()
    {
        StartCoroutine(SwitchCharacterCoroutine());
    }
    private IEnumerator SwitchCharacterCoroutine()
    {
        if (model.candidate == 99)
        {
            Debug.Log("no candidate selected!");
            yield break;
        }
        if (!model.charaStatus[model.candidate].isAlive)
        {
            Debug.Log("It is DEAD!");
            yield break;
        }
        if (model.currentChara == model.candidate)
        {
            Debug.Log("It's same!");
            yield break;
        }

        string prevChara = model.charaStatus[model.currentChara].charaName;
        string nextChara = model.charaStatus[model.candidate].charaName;
        model.currentChara = model.candidate;
        model.candidate = 99;
        model.LoadCharaSkills();
        player.setImage(model.charaStatus[model.currentChara].charaImageUrl);
        view.ShowSkills(model.charaSkills);
        view.UpdateStatusPanel(model.charaStatus[model.currentChara], false);

        view.switchPanel.SetActive(false);

        yield return view.SetLogtextAndWait($"{prevChara} → {nextChara}로 교체했다!", 1.0f);

        if (!model.isDead) // 안죽었는데 교체로 턴 사용 -> 교체 후 상대 공격
        {
            yield return StartCoroutine(UseEnemySkillCoroutine());
            if (model.isOver)
            {
                view.ShowGameOver(model.isWin);
                yield break;
            }
            if (!model.charaStatus[model.currentChara].isAlive)
            {
                view.ShowSwitchPanel();
                yield break;
            }
            view.OnCancelButtonClicked();
            view.SetSkillButtonsInteractable(true);   // 교체+적턴 끝나고 버튼 살리기
            yield break;
        }
        else
        {
            model.isDead = false;
            view.OnCancelButtonClicked();
            view.SetSkillButtonsInteractable(true);
            yield break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
