using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject player;
    public Transform target;
    public GameObject projectilePrefab;
    public GameObject laserPrefab;
    public bool isEnemy = false;
    public Color statUpColor = Color.red;
    public Color statDownColor = Color.blue;
    public ParticleSystem healParticle;
    private Vector3 originalPosition;
    private SpriteRenderer characterSprite;
    private Material effectMaterial;
    private Color controlColor = Color.green;
    void Start()
    {
        originalPosition = player.transform.position;
    }
    public void setImage(string spriteurl)
    {
        characterSprite = player.GetComponent<SpriteRenderer>();
        effectMaterial = characterSprite.material;
        effectMaterial.SetFloat("_Alpha", 0f);
        // Assuming you have a method to set the player's image
        // This is a placeholder for the actual implementation
        Debug.Log("Setting player image: " + spriteurl);
        APIRequester.Instance.GetSprite(spriteurl, (sprite) =>
        {
            characterSprite.sprite = sprite;
        });
        player.transform.localScale = new Vector3(1f, 1f, 1f); // 초기 스케일로 설정
    }
    /// <summary>
    /// 스킬 데이터에 따라 적절한 애니메이션 코루틴을 실행합니다.
    /// </summary>
    public IEnumerator PlaySkillAnimation(Skill skill)
    {
        Debug.Log($"스킬 애니메이션 재생: {skill.skill_name}, 타입: {skill.visual_effect_type}, 공격종류: {skill.damage_type}");
        // 스킬의 시각 효과 타입에 따라 다른 코루틴을 호출합니다.
        switch (skill.damage_type)
        {
            case "랭크":
                yield return RankCoroutine(skill.base_power / 10 % 10 == 0);
                break;
            case "제어":
                yield return ControlCoroutine(skill.shake_effect);
                break;
            case "회복":
                yield return HealCoroutine(skill.shake_effect);
                break;
            case "방어":
                //StartCoroutine(DefenseCoroutine(skill.shake_effect));
                break;
            default:
                switch (skill.visual_effect_type)
                {
                    case "Shake":
                        // Shake 효과에 필요한 데이터를 넘겨줍니다.
                        yield return ShakeCoroutine(skill.shake_effect);
                        break;
                    case "Projectile":
                        // Projectile 효과에 필요한 데이터를 넘겨줍니다.
                        yield return ProjectileCoroutine(skill.projectile_effect);
                        break;
                    case "Laser":
                        // Laser 효과에 필요한 데이터를 넘겨줍니다.
                        yield return LaserCoroutine(skill.laser_effect);
                        break;
                }
                break;
        }
    }

    /// <summary>
    /// 피격 애니메이션을 재생합니다.
    /// </summary>
    public void PlayHitAnimation()
    {
        AudioManager.Instance.PlayHit();
        StartCoroutine(BlinkCoroutine(Color.red, 3, 0.5f));
    }

    // --- 각 효과별 코루틴 ---

    private IEnumerator ShakeCoroutine(ShakeEffect effectData)
    {
        Debug.Log($"Shake 애니메이션 재생: 파티클 색상 {effectData.particle_color}");

        AudioManager.Instance.PlayShake();
        float duration = 0.5f;
        float magnitude = 0.1f;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            player.transform.position = originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player.transform.position = originalPosition;
    }
    private IEnumerator ProjectileCoroutine(ProjectileEffect effectData)
    {
        if (projectilePrefab == null || target == null)
        {
            Debug.LogError("Projectile Prefab 또는 Target이 설정되지 않았습니다!");
            yield break;
        }

        Debug.Log($"투사체 애니메이션 재생: {effectData.shape} 모양 {effectData.count}개, 색상 {effectData.color}");
        yield return new WaitForSeconds(0.7f); // 애니메이션 시작 전 잠시 대기
        
        for (int i = 0; i < effectData.count; i++)
        {
            // 캐릭터 주변에 약간의 랜덤한 오프셋을 주어 투사체를 생성합니다.
            Vector3 spawnOffset = (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f;
            Vector3 spawnPosition = player.transform.position + spawnOffset;

            GameObject projectileGO = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // 투사체 색상 설정 (선택 사항)
            var projectileRenderer = projectileGO.GetComponent<SpriteRenderer>();
            if (projectileRenderer != null && ColorUtility.TryParseHtmlString(effectData.color, out Color projectileColor))
            {
                projectileRenderer.color = projectileColor;
            }

            // ProjectileManager에 목표와 속도를 알려줍니다.
            ProjectileManager projectileManager = projectileGO.GetComponent<ProjectileManager>();
            if (projectileManager != null)
            {   
                AudioManager.Instance.PlayProjectile();
                projectileManager.Initialize(target, 30f, effectData.shape); // 10f는 투사체 속도
            }

            // 여러 발을 쏠 경우, 약간의 시간 간격을 둡니다.
            if (effectData.count > 1)
            {
                yield return new WaitForSeconds(0.05f);
            }
        }

        // 모든 투사체가 발사된 후, 애니메이션이 끝났다고 간주하고 잠시 대기합니다.
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator LaserCoroutine(LaserEffect effectData)
    {
        if (laserPrefab == null || target == null)
        {
            Debug.LogError("Laser Prefab 또는 Target이 설정되지 않았습니다!");
            yield break;
        }

        Debug.Log($"레이저 애니메이션 재생: {effectData.origin}에서 발사, 굵기 {effectData.thickness}, 색상 {effectData.color}");
        AudioManager.Instance.PlayLaser();

        // 1. 레이저 프리팹을 생성합니다.
        GameObject laserGO = Instantiate(laserPrefab, Vector3.zero, Quaternion.identity);
        LineRenderer lineRenderer = laserGO.GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            Debug.LogError("Laser Prefab에 Line Renderer 컴포넌트가 없습니다!");
            Destroy(laserGO);
            yield break;
        }

        // 2. 레이저의 시작점과 끝점을 설정합니다.
        Vector3 startPoint = transform.position; // 시전자 위치
        Vector3 endPoint = target.position;   // 목표 위치

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        // 3. 레이저의 굵기와 색상을 설정합니다.
        float thickness = effectData.thickness / 5f; // 1~3 값을 0.1~0.3으로 변환

        float laserPositionOffset = 1.4f; // 레이저가 목표 위치에서 약간 떨어지도록 설정
        // origin 데이터에 따라 시작점을 변경할 수 있습니다. (선택 사항)
        if (effectData.origin == "TopToBottom")
        {
            startPoint = new Vector3(endPoint.x, endPoint.y + laserPositionOffset + 0.5f, 0); // 목표의 위쪽 하늘
            endPoint = new Vector3(endPoint.x, endPoint.y - laserPositionOffset, 0); // 목표의 아래쪽 땅
            thickness *= 5; // 레이저가 위에서 아래로 내려오므로 굵기를 두 배로 설정
            lineRenderer.sortingOrder = 3;
        }
        else if (effectData.origin == "BottomToTop")
        {
            startPoint = new Vector3(endPoint.x, endPoint.y - laserPositionOffset, 0); // 목표의 아래쪽 땅
            endPoint = new Vector3(endPoint.x, endPoint.y + laserPositionOffset + 0.5f, 0); // 목표의 위쪽 하늘
            thickness *= 5;
            lineRenderer.sortingOrder = 3;
        }

        lineRenderer.startWidth = thickness;
        lineRenderer.endWidth = thickness;

        if (ColorUtility.TryParseHtmlString(effectData.color, out Color laserColor))
        {
            lineRenderer.startColor = laserColor;
            lineRenderer.endColor = laserColor;
        }

        // --- 애니메이션 로직 ---
        float fireDuration = 0.15f; // 뻗어 나가는 데 걸리는 시간
        float holdDuration = 0.4f;  // 유지 시간
        float retractDuration = 0.2f; // 사라지는 데 걸리는 시간
        float elapsedTime = 0f;

        // 1. 발사 (뻗어 나가는 애니메이션)
        while (elapsedTime < fireDuration)
        {
            float t = elapsedTime / fireDuration;
            Vector3 currentEndPoint = Vector3.Lerp(startPoint, endPoint, t);
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, currentEndPoint);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 최종 위치 고정
        lineRenderer.SetPosition(1, endPoint);

        // 2. 유지
        yield return new WaitForSeconds(holdDuration);

        // 3. 소멸 (사라지는 애니메이션)
        elapsedTime = 0f;
        while (elapsedTime < retractDuration)
        {
            float t = elapsedTime / retractDuration;
            Vector3 currentStartPoint = Vector3.Lerp(startPoint, endPoint, t);
            lineRenderer.SetPosition(0, currentStartPoint); // 시작점이 끝점으로 이동
            lineRenderer.SetPosition(1, endPoint);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // --------------------

        // 4. 레이저 파괴
        Destroy(laserGO);
    }

    private IEnumerator RankCoroutine(bool isStatUp)
    {
        Debug.Log($"랭크 애니메이션 재생: {(isStatUp ? "상승" : "하락")}");

        if (isStatUp) AudioManager.Instance.PlayRankUp();
        else AudioManager.Instance.PlayRankDown();

        float animationDuration = 1f; // 애니메이션 지속 시간
        effectMaterial.SetColor("_EffectColor", isStatUp ? statUpColor : statDownColor);
        float scrollDirection = isStatUp ? 1.0f : -1.0f;

        float elapsedTime = 0f;

        // 2. 코루틴을 통해 시간에 따라 셰이더 변수 값을 변경합니다.
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            // 페이드 인/아웃: 포물선 형태로 알파값을 조절 (0 -> 1 -> 0)
            float alpha = 8.0f * progress * progress * (1.0f - progress);
            effectMaterial.SetFloat("_Alpha", alpha);

            // 스크롤: progress에 따라 물결이 위 또는 아래로 움직임
            effectMaterial.SetFloat("_ScrollY", -progress * scrollDirection);
            
            yield return null;
        }

        // 3. 애니메이션이 끝나면 효과를 완전히 끕니다.
        effectMaterial.SetFloat("_Alpha", 0f);
    }

    private IEnumerator HealCoroutine(ShakeEffect effectData)
    {
        Debug.Log($"회복 애니메이션 재생: 파티클 색상 {effectData.particle_color}");
        AudioManager.Instance.PlayHeal();
        float duration = 1f;
        Color color = Color.green;
        if (ColorUtility.TryParseHtmlString(effectData.particle_color, out Color tryColor))
        {
            color = tryColor;
        }
        var main = healParticle.main;
        main.startColor = color;
        healParticle.Play();
        yield return BlinkCoroutine(color, 1, duration);
    }

    public IEnumerator ControlCoroutine(ShakeEffect effectData = null)
    {
        float duration = 1f;
        if (effectData != null)
        {
            if (ColorUtility.TryParseHtmlString(effectData.particle_color, out Color tryColor))
            {
                controlColor = tryColor;
            }
        }
        AudioManager.Instance.PlayControl();
        Debug.Log($"제어 애니메이션 재생: 색상 {controlColor}");
        yield return BlinkCoroutine(controlColor, 1, duration);
    }

    /// <summary>
    /// 셰이더의 _FlashAmount 값을 조절하여 점멸 효과를 만드는 코루틴입니다.
    /// </summary>
    private IEnumerator BlinkCoroutine(Color flashColor, int blinkCount, float totalDuration)
    {
        if (effectMaterial == null)
        {
            Debug.LogError("효과를 적용할 머티리얼이 없습니다!");
            yield break; // 코루틴을 즉시 종료합니다.
        }

        // 1. C# 스크립트에서 셰이더의 _FlashColor 값을 원하는 색으로 설정합니다.
        effectMaterial.SetColor("_FlashColor", flashColor);

        float singleBlinkDuration = totalDuration / blinkCount;

        for (int i = 0; i < blinkCount; i++)
        {
            float elapsedTime = 0f;
            // 부드럽게 밝아지는 효과
            while (elapsedTime < singleBlinkDuration / 2)
            {
                float progress = elapsedTime / (singleBlinkDuration / 2);
                effectMaterial.SetFloat("_FlashAmount", Mathf.Lerp(0f, 0.8f, progress));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            elapsedTime = 0f;
            // 부드럽게 어두워지는 효과
            while (elapsedTime < singleBlinkDuration / 2)
            {
                float progress = elapsedTime / (singleBlinkDuration / 2);
                effectMaterial.SetFloat("_FlashAmount", Mathf.Lerp(0.8f, 0f, progress));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        // 애니메이션이 끝난 후, 확실하게 효과를 끕니다.
        effectMaterial.SetFloat("_FlashAmount", 0.0f);
    }
    public IEnumerator CharacterChangeCoroutine(string newSpriteUrl)
    {
        // --- 1. 왼쪽으로 사라지는 애니메이션 ---
        float moveDuration = 0.2f; // 이동에 걸리는 시간
        float changeDuration = 0.3f;
        float elapsedTime = 0f;

        // 화면 왼쪽 밖의 목표 지점을 계산합니다.
        Vector3 offScreenPosition = originalPosition + Vector3.left * 6f;
        if (player.transform.localScale.y == 0)
        { 
            elapsedTime = moveDuration; // 이미 사라진 상태라면 바로 이동
        }

        while (elapsedTime < moveDuration)
        {
            // Lerp를 사용해 부드럽게 이동
            player.transform.position = Vector3.Lerp(originalPosition, offScreenPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 정확한 위치에 고정
        player.transform.position = offScreenPosition;
        // --- 2. 캐릭터 이미지 교체 ---
        setImage(newSpriteUrl);
        yield return new WaitForSeconds(changeDuration); // 이미지 변경 후 잠시 대기
        // --- 3. 다시 제자리로 돌아오는 애니메이션 ---
        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            player.transform.position = Vector3.Lerp(offScreenPosition, originalPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 정확한 위치에 고정
        player.transform.position = originalPosition;
        yield return null;
    }
    public IEnumerator CharacterDeadAnimation()
    {
        // --- 1. 캐릭터가 사라지는 애니메이션 ---
        float squashDuration = 0.2f; // 페이드 아웃에 걸리는 시간
        float elapsedTime = 0f;

        while (elapsedTime < squashDuration)
        {
            float y = Mathf.Lerp(1f, 0, elapsedTime / squashDuration);
            player.transform.localScale = new Vector3(1f, y, 1f); // Y축을 줄여서 사라지는 효과
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        player.transform.localScale = new Vector3(1f, 0, 1f); // 최종적으로 Y축을 0으로 설정
    }
}
