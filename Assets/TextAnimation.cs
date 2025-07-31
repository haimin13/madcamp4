using UnityEngine;
using System.Collections;

/// <summary>
/// UI 요소의 크기를 부드럽게 계속해서 변경하여 '숨쉬는' 효과를 만듭니다.
/// 이 스크립트를 크기를 변경하고 싶은 UI Text, Image, Panel 등에 붙여주세요.
/// </summary>
public class TextAnimation : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [Tooltip("가장 작아졌을 때의 크기 배율입니다.")]
    public float minScale = 0.6f;

    [Tooltip("가장 커졌을 때의 크기 배율입니다.")]
    public float maxScale = 1.4f;

    [Tooltip("한 번 커졌다가 작아지는 데 걸리는 시간(속도)입니다.")]
    public float speed = 0.4f;

    // UI 요소의 RectTransform 컴포넌트를 저장할 변수
    private RectTransform rectTransform;

    void Start()
    {
        // 이 스크립트가 붙어있는 게임 오브젝트의 RectTransform 컴포넌트를 가져옵니다.
        rectTransform = GetComponent<RectTransform>();

        // 애니메이션 코루틴을 시작합니다.
        StartCoroutine(PulseCoroutine());
    }

    private IEnumerator PulseCoroutine()
    {
        // 게임이 실행되는 동안 무한히 반복합니다.
        while (true)
        {
            // Mathf.Sin 함수를 사용하여 -1과 1 사이를 부드럽게 왕복하는 값을 만듭니다.
            // Time.time * speed 를 통해 시간에 따라 값이 계속 변하도록 합니다.
            float sinValue = Mathf.Sin(Time.time * speed);

            // sinValue의 범위(-1 ~ 1)를 0 ~ 1 사이의 비율(t)로 변환합니다.
            float t = (sinValue + 1f) / 2f;

            // Lerp 함수를 사용하여 minScale과 maxScale 사이를 t 비율에 맞춰 부드럽게 이동하는 값을 계산합니다.
            float currentScale = Mathf.Lerp(minScale, maxScale, t);

            // 계산된 크기 값을 UI 요소의 실제 크기(localScale)에 적용합니다.
            rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);

            // 다음 프레임까지 기다립니다.
            yield return null;
        }
    }
}
