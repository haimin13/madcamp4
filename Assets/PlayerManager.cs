using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerImage;
    public Transform EnemyTransform;
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
        // 이 곳에 투사체를 생성하고 발사하는 로직을 구현합니다.
        Debug.Log($"투사체 애니메이션 재생: {effectData.shape} 모양 {effectData.count}개, 색상 {effectData.color}");
        // 예시: 투사체 프리팹을 생성하고, ObjectMover 스크립트로 목표를 향해 이동시킴
        yield return new WaitForSeconds(1.0f); // 애니메이션 길이에 맞춰 대기
    }

    private IEnumerator LaserCoroutine(LaserEffect effectData)
    {
        // 이 곳에 레이저를 생성하는 로직을 구현합니다.
        Debug.Log($"레이저 애니메이션 재생: {effectData.origin}에서 발사, 굵기 {effectData.thickness}, 색상 {effectData.color}");
        // 예시: LineRenderer를 사용하여 레이저 효과 구현
        yield return new WaitForSeconds(1.5f); // 애니메이션 길이에 맞춰 대기
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
