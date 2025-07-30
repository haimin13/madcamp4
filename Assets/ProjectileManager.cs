using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProjectileManager : MonoBehaviour
{
    private Transform target;
    private float speed;

    /// <summary>
    /// AnimationManager가 호출하여 투사체의 목표와 속도를 설정합니다.
    /// </summary>
    public void Initialize(Transform targetTransform, float moveSpeed)
    {
        this.target = targetTransform;
        this.speed = moveSpeed;
    }

    void Update()
    {
        // 목표가 없으면 (예: 이미 파괴됨) 스스로를 파괴합니다.
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 매 프레임 목표를 향해 이동합니다.
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 목표에 매우 가까워지면 스스로를 파괴합니다.
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            // 여기에 피격 이펙트(파티클 등)를 생성하는 코드를 추가할 수 있습니다.
            Destroy(gameObject);
        }
    }
}
