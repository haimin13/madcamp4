// 파일 이름: ObjectPositioner.cs
// 이 스크립트를 위치를 조정하고 싶은 2D 오브젝트에 붙여주세요.

using UnityEngine;

public class ObjectPositioner : MonoBehaviour
{
    [Header("카메라 설정")]
    [Tooltip("기준이 될 메인 카메라를 여기에 연결하세요. 비워두면 MainCamera를 자동으로 찾습니다.")]
    public Camera mainCamera;

    [Header("위치 설정 (0.0 ~ 1.0)")]
    [Tooltip("화면의 가로 위치를 비율로 설정합니다. (0=왼쪽, 0.5=중앙, 1=오른쪽)")]
    [Range(0f, 1f)]
    public float viewportX = 0.5f;

    [Tooltip("화면의 세로 위치를 비율로 설정합니다. (0=아래, 0.5=중앙, 1=위)")]
    [Range(0f, 1f)]
    public float viewportY = 0.5f;

    void Start()
    {
        // mainCamera가 연결되지 않았다면, "MainCamera" 태그를 가진 카메라를 찾습니다.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 설정된 비율 위치에 오브젝트를 배치합니다.
        PositionObject();
    }

    /// <summary>
    /// 뷰포트 좌표를 기준으로 오브젝트의 위치를 설정하는 함수입니다.
    /// </summary>
    private void PositionObject()
    {
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다! 'MainCamera' 태그가 있는지 확인하세요.");
            return;
        }

        // 1. 뷰포트 좌표를 Vector3로 만듭니다.
        //    z값은 카메라로부터의 거리를 의미합니다. 2D에서는 카메라의 z위치와 오브젝트의 z위치의 차이를 사용합니다.
        Vector3 viewportPosition = new Vector3(viewportX, viewportY, Mathf.Abs(mainCamera.transform.position.z - transform.position.z));

        // 2. (핵심) Camera.ViewportToWorldPoint 함수를 사용하여 뷰포트 좌표를 월드 좌표로 변환합니다.
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(viewportPosition);

        // 3. 오브젝트의 실제 위치를 변환된 월드 좌표로 설정합니다.
        transform.position = worldPosition;
    }

    private void OnValidate()
    {
        // Start()가 호출되기 전이라도 mainCamera를 찾아줍니다.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        PositionObject();
    }
}
