using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProjectileManager : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // 투사체의 스프라이트 렌더러
    public Sprite squareSprite; // 투사체 스프라이트
    private Transform target;
    private float speed;

    /// <summary>
    /// AnimationManager가 호출하여 투사체의 목표와 속도를 설정합니다.
    /// </summary>
    public void Initialize(Transform targetTransform, float moveSpeed, string type)
    {
        this.target = targetTransform;
        this.speed = moveSpeed;
        if (type == "막대기")
        {
            spriteRenderer.sprite = squareSprite; // 막대기 모양으로 설정
            transform.localScale = new Vector3(0.5f, 0.1f, 1f);
        }
    }

    void Update()
    {
        // 목표가 없으면 (예: 이미 파괴됨) 스스로를 파괴합니다.
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // --- (수정된 부분) 목표를 향해 회전하고 이동 ---
        // 1. 목표를 향하는 방향 벡터를 계산합니다.
        Vector3 direction = target.position - transform.position;
        direction.Normalize(); // 방향 벡터의 길이를 1로 만듭니다.

        // 2. 방향 벡터를 바탕으로 2D 각도를 계산합니다 (z축 회전).
        // Atan2는 x, y 컴포넌트로부터 올바른 각도를 라디안으로 반환합니다.
        // Rad2Deg를 곱해 우리가 아는 각도(degree)로 변환합니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 3. 계산된 각도로 회전합니다.
        // AngleAxis는 특정 축(Vector3.forward는 Z축)을 기준으로 회전하는 Quaternion을 만듭니다.
        // (참고: 투사체 스프라이트의 '오른쪽'이 앞을 향하도록 만들어야 합니다.)
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 4. 매 프레임 목표를 향해 이동합니다.
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        // ------------------------------------------------

        // 목표에 매우 가까워지면 스스로를 파괴합니다.
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            // 여기에 피격 이펙트(파티클 등)를 생성하는 코드를 추가할 수 있습니다.
            Destroy(gameObject);
        }
    }
}
