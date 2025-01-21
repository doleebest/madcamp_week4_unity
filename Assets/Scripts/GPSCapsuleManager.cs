using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSCapsuleManager : MonoBehaviour
{
    [SerializeField] private GameObject capsulePrefab;
    private float updateInterval = 1f;
    private float proximityThreshold = 10f; // 10미터 내에 있으면 표시

    // 위치 정보를 저장할 구조체
    [System.Serializable]
    public struct CapsuleGPS
    {
        public float latitude;
        public float longitude;
        public string capsuleId;
    }

    private void Start()
    {
        StartCoroutine(StartGPSService());
    }

    private IEnumerator StartGPSService()
    {
        // GPS 권한 체크
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS not enabled");
            yield break;
        }

        // GPS 서비스 시작
        Input.location.Start();
        
        // GPS 초기화 대기
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // GPS 서비스 실행 중인지 확인
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.Log("GPS not running");
            yield break;
        }

        StartCoroutine(CheckNearbyCapsules());
    }

    private IEnumerator CheckNearbyCapsules()
    {
        while (true)
        {
            // 현재 GPS 위치
            float currentLat = Input.location.lastData.latitude;
            float currentLong = Input.location.lastData.longitude;

            // 안드로이드로부터 저장된 캡슐 위치들 요청
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                string capsuleData = activity.Call<string>("getNearbyCapsules", currentLat, currentLong);
                CapsuleGPS[] capsules = JsonUtility.FromJson<CapsuleGPS[]>(capsuleData);

                foreach (var capsule in capsules)
                {
                    float distance = CalculateDistance(
                        currentLat, currentLong,
                        capsule.latitude, capsule.longitude
                    );

                    if (distance < proximityThreshold)
                    {
                        ShowCapsule(capsule);
                    }
                }
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
{
    // Haversine 공식을 사용한 거리 계산
    float R = 6371e3f; // 지구 반경 (미터), float 타입으로 명시
    float φ1 = lat1 * Mathf.Deg2Rad;
    float φ2 = lat2 * Mathf.Deg2Rad;
    float Δφ = (lat2-lat1) * Mathf.Deg2Rad;
    float Δλ = (lon2-lon1) * Mathf.Deg2Rad;

    float a = Mathf.Sin(Δφ/2) * Mathf.Sin(Δφ/2) +
              Mathf.Cos(φ1) * Mathf.Cos(φ2) *
              Mathf.Sin(Δλ/2) * Mathf.Sin(Δλ/2);
    float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1-a));

    return R * c;
}

    private void ShowCapsule(CapsuleGPS capsule)
    {
        // AR 공간에 캡슐 표시
        Vector3 capsulePosition = GetARPosition(capsule.latitude, capsule.longitude);
        GameObject capsuleObj = Instantiate(capsulePrefab, capsulePosition, Quaternion.identity);
        capsuleObj.name = $"Capsule_{capsule.capsuleId}";
    }

    private Vector3 GetARPosition(float latitude, float longitude)
    {
        // GPS 좌표를 AR 공간 좌표로 변환
        // 현재 카메라 위치를 기준으로 상대적 위치 계산
        Vector3 cameraPosition = Camera.main.transform.position;
        return new Vector3(cameraPosition.x + 2f, cameraPosition.y, cameraPosition.z + 2f);
    }
}
