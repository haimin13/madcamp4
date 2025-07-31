using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            Debug.Log(GameDataManager.Instance.currentRound);
            GameDataManager.Instance.selectedChara = 2;
            GameDataManager.Instance.runId = "run_a85f192c-c763-4f1c-a0ce-234c6a430f3d";

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

        AudioManager.Instance.PlayBGM(model.currentRound);

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
            yield return UseSkillCoroutine(true, skillIdx);
            if (model.isOver)
            {
                view.ShowGameOver(model.isWin);
                yield break;
            }

            yield return UseSkillCoroutine(false);
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
            yield return UseSkillCoroutine(false);
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
            yield return UseSkillCoroutine(true, skillIdx);
            if (model.isOver)
            {
                view.ShowGameOver(model.isWin);
                yield break;
            }
            else view.SetSkillButtonsInteractable(true);
        }
    }
    /*
    private IEnumerator UsePlayerSkillCoroutine(int skillIdx)
    {
        Skill castedSkill = model.charaSkills[skillIdx];
        view.SetLogtext($"{model.charaStatus[model.currentChara].charaName}은(는) {castedSkill.skill_name}을(를) 사용했다!");
        if (castedSkill.damage_type == "랭크" && castedSkill.base_power / 10 % 10 == 1) // 랭크 스킬
        {
            yield return enemy.PlaySkillAnimation(castedSkill);
        }
        else
        {
            yield return player.PlaySkillAnimation(castedSkill);
        }
        switch (castedSkill.damage_type)
        {
            case "랭크":
                yield return UseRankSkillCoruoutine(castedSkill);
                break;
            case "제어":
                // StartCoroutine(UseRankSkillCoruoutine(castedSkill));
                break;
            case "회복":
                yield return UseHealSkillCoroutine(castedSkill);
                break;
            case "방어":
                // StartCoroutine(UseRankSkillCoruoutine(castedSkill));
                break;
            default:
                yield return UseDamageSkillCoroutine(castedSkill);
                break;
        }
    }
    */

    private IEnumerator UseSkillCoroutine(bool isPlayer, int skillIdx = 0)
    {
        if (!isPlayer){skillIdx = Random.Range(0, model.enemySkills.Count);}
        Skill castedSkill = isPlayer ? model.charaSkills[skillIdx] : model.enemySkills[skillIdx];
        PlayerManager sourcePlayer = isPlayer ? player : enemy;
        PlayerManager destinationPlayer = isPlayer ? enemy : player;
        CurrentCharacterStatus source = isPlayer ? model.charaStatus[model.currentChara] : model.enemyStatus;
        CurrentCharacterStatus destination = isPlayer ? model.enemyStatus : model.charaStatus[model.currentChara];

        view.SetLogtext($"{source.charaName}은(는) {castedSkill.skill_name}을(를) 사용했다!");

        if (castedSkill.damage_type == "랭크" && castedSkill.base_power / 10 % 10 == 1) // 랭크 스킬
        {
            yield return destinationPlayer.PlaySkillAnimation(castedSkill);
        }
        else
        {
            yield return sourcePlayer.PlaySkillAnimation(castedSkill);
        }
        switch (castedSkill.damage_type)
        {
            case  "랭크":
                yield return UseRankSkillCoruoutine(castedSkill, source, destination);
                break;
            case "제어":
                // StartCoroutine(UseRankSkillCoruoutine(castedSkill));
                break;
            case "회복":
                yield return UseHealSkillCoroutine(castedSkill, source);
                break;
            case "방어":
                // StartCoroutine(UseRankSkillCoruoutine(castedSkill));
                break;
            default:
                yield return UseDamageSkillCoroutine(castedSkill, source, destination, destinationPlayer);
                break;
        }
    }
    private IEnumerator UseDamageSkillCoroutine(Skill castedSkill, CurrentCharacterStatus source, CurrentCharacterStatus destination, PlayerManager destinationPlayer)
    {
        int atkStat = source.tmpAtk;
        int spAtkStat = source.tmpSpAtk;
        int defStat = destination.tmpDef;
        int spDefStat = destination.tmpSpDef;
        string defType = destination.charaType;

        int damage = model.CalculateDamage(castedSkill, atkStat, spAtkStat, defStat, spDefStat, defType);

        destinationPlayer.PlayHitAnimation();

        destination.currentHp -= damage;
        view.UpdateStatusPanel(status: destination, isEnemy: destination == model.enemyStatus);
        yield return view.SetLogtextAndWait($"{destination.charaName}에게 {damage}의 데미지!");
        if (destination.currentHp <= 0)
        {
            destination.currentHp = 0;
            destination.isAlive = false;
            if (destination != model.enemyStatus) // destination이 enemy가 아니라면 player임
            {
                model.isDead = true;
                StartCoroutine(player.CharacterDeadAnimation()); // 죽는 애니메이션도 여기서 호출
            }
            yield return view.SetLogtextAndWait($"{destination.charaName}은(는) 쓰러졌다!");
            CheckGameState();
        }
    }
    private IEnumerator UseRankSkillCoruoutine(Skill castedSkill, CurrentCharacterStatus source, CurrentCharacterStatus destination)
    {
        // 랭크 스킬 사용 로직
        CurrentCharacterStatus target = castedSkill.base_power / 10 % 10 == 0 ? source : destination;
        int step = castedSkill.base_power / 100 * ((castedSkill.base_power / 10 % 10) == 0 ? 1 : -1);
        string targetStat = "오류";
        Debug.Log($"RankUpStat: {castedSkill.base_power}, step: {step}");
        switch (castedSkill.base_power % 10)
        {
            case 0: // 공격력 증가
                targetStat = "체력";
                target.maxHp = model.RankUpStat(target.maxHp, step);
                target.currentHp = model.RankUpStat(target.currentHp, step);
                break;
            case 1: // 방어력 증가
                targetStat = "공격력";
                target.tmpAtk = model.RankUpStat(target.tmpAtk, step);
                break;
            case 2: // 특수 공격력 증가
                targetStat = "방어력";
                target.tmpDef = model.RankUpStat(target.tmpDef, step);
                break;
            case 3: // 특수 방어력 증가
                targetStat = "특수 공격력";
                target.tmpSpAtk = model.RankUpStat(target.tmpSpAtk, step);
                break;
            case 4: // 속도 증가
                targetStat = "특수 방어력";
                target.tmpSpDef = model.RankUpStat(target.tmpSpDef, step);
                break;
            case 5: // 속도 증가
                targetStat = "스피드";
                target.tmpSpeed = model.RankUpStat(target.tmpSpeed, step);
                break;
            default:
                Debug.LogError("Invalid rank skill type");
                break;
        }
        string verb = castedSkill.base_power / 10 % 10 == 0 ? "증가" : "감소";
        string adjective = "";
        if (step == 2 || step == -2)
        {
            adjective = " 크게";
        }
        if (step == 3 || step == -3)
        {
            adjective = " 매우 크게";
        }
        yield return view.SetLogtextAndWait($"{target.charaName}의 {targetStat}이{adjective} {verb}했다!");
    }

    private IEnumerator UseHealSkillCoroutine(Skill castedSkill, CurrentCharacterStatus character)
    {
        int hp = character.currentHp;
        int maxHp = character.maxHp;
        int healAmount = (int)(castedSkill.base_power / 100f * 0.5f * maxHp / (character.healCount+1));
        character.healCount += 1; // 회복 횟수 증가
        Debug.Log($"HealAmount: {healAmount}, Current HP: {hp}, Max HP: {maxHp}, basepower {castedSkill.base_power}");

        character.currentHp += healAmount;
        if (character.currentHp > character.maxHp)
        {
            character.currentHp = character.maxHp;
        }
        Debug.Log($"HealAmount: {healAmount}, Current HP: {character.currentHp}, Max HP: {character.maxHp}");
        view.UpdateStatusPanel(status: character, isEnemy: character == model.enemyStatus);
        yield return view.SetLogtextAndWait($"{character.charaName}이(가) {healAmount}의 체력을 회복했다!");
    }

    private IEnumerator UseEnemySkillCoroutine()
    {
        // 랜덤하게 스킬 선택
        int idx = Random.Range(0, 4);
        Skill castedSkill = model.enemySkills[idx];
        view.SetLogtext($"{model.enemyStatus.charaName}은(는) {castedSkill.skill_name}을(를) 사용했다!");
        yield return enemy.PlaySkillAnimation(castedSkill);

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
            StartCoroutine(player.CharacterDeadAnimation());
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
        yield return StartCoroutine(player.CharacterChangeCoroutine(model.charaStatus[model.candidate].charaImageUrl));

        string prevChara = model.charaStatus[model.currentChara].charaName;
        string nextChara = model.charaStatus[model.candidate].charaName;
        model.currentChara = model.candidate;
        model.candidate = 99;
        model.LoadCharaSkills();
        view.ShowSkills(model.charaSkills);
        view.UpdateStatusPanel(model.charaStatus[model.currentChara], false);

        view.switchPanel.SetActive(false);
        view.SetSkillButtonsInteractable(false); // 스킬 버튼 비활성화

        yield return view.SetLogtextAndWait($"{prevChara} → {nextChara}로 교체했다!", 1.0f);

        if (!model.isDead) // 안죽었는데 교체로 턴 사용 -> 교체 후 상대 공격
        {
            yield return StartCoroutine(UseSkillCoroutine(false));
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

    public void GoNextRound()
    {
        model.StoreCurrentState();
        if (GameDataManager.Instance.currentRound != 9)
            SceneManager.LoadScene("StageListScene");
        else // 게임 클리어.
        {
            SceneManager.LoadScene("ClearScene");
        }
    }
}
