using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerImage;
    public Transform target;
    public GameObject projectilePrefab;
    public GameObject laserPrefab;
    public bool isEnemy = false;
    private Vector3 originalPosition;
    private SpriteRenderer characterSprite;
    public void setImage(string spriteurl)
    {
        characterSprite = playerImage.GetComponent<SpriteRenderer>();
        // Assuming you have a method to set the player's image
        // This is a placeholder for the actual implementation
        Debug.Log("Setting player image: " + spriteurl);
        APIRequester.Instance.GetSprite(spriteurl, (sprite) =>
        {
            characterSprite.sprite = sprite;
        });
        originalPosition = playerImage.transform.position;
    }
    /// <summary>
    /// 스킬 데이터에 따라 적절한 애니메이션 코루틴을 실행합니다.
    /// </summary>
    public void PlaySkillAnimation(Skill skill)
    {
        Debug.Log($"스킬 애니메이션 재생: {skill.skill_name}, 타입: {skill.visual_effect_type}");
        // 스킬의 시각 효과 타입에 따라 다른 코루틴을 호출합니다.
        switch (skill.visual_effect_type)
        {
            case "Shake":
                // Shake 효과에 필요한 데이터를 넘겨줍니다.
                StartCoroutine(ShakeCoroutine(skill.shake_effect));
                break;
            case "Projectile":
                // Projectile 효과에 필요한 데이터를 넘겨줍니다.
                StartCoroutine(ProjectileCoroutine(skill.projectile_effect));
                break;
            case "Laser":
                // Laser 효과에 필요한 데이터를 넘겨줍니다.
                StartCoroutine(LaserCoroutine(skill.laser_effect));
                break;
        }
    }

    /// <summary>
    /// 피격 애니메이션을 재생합니다.
    /// </summary>
    public void PlayHitAnimation()
    {
        StartCoroutine(BlinkRedCoroutine(3, 0.5f));
    }

    // --- 각 효과별 코루틴 ---

    private IEnumerator ShakeCoroutine(ShakeEffect effectData)
    {
        Debug.Log($"Shake 애니메이션 재생: 파티클 색상 {effectData.particle_color}");
        
        float duration = 0.5f;
        float magnitude = 0.1f;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            playerImage.transform.position = originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
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
            Vector3 spawnPosition = transform.position + spawnOffset;

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

    private IEnumerator BlinkRedCoroutine(int blinkCount, float totalDuration)
    {
        Color originalColor = characterSprite.color;
        Color hitColor = Color.red;
        float blinkDuration = totalDuration / (blinkCount * 2);

        for (int i = 0; i < blinkCount; i++)
        {
            characterSprite.color = hitColor;
            yield return new WaitForSeconds(blinkDuration);
            characterSprite.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }
}
