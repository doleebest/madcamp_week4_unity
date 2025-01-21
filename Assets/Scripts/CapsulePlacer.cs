using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class CapsulePlacer : MonoBehaviour
{
    [SerializeField] private GameObject capsulePrefab;  // Inspector에서 설정할 타임캡슐 프리팹
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    
    private void Awake()
    {
        // AR 컴포넌트 참조 가져오기
        raycastManager = FindObjectOfType<ARRaycastManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();
    }

    private void Update()
    {
        // 화면 터치 감지
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                // 터치한 위치에 타임캡슐 배치 시도
                PlaceCapsule(touch.position);
            }
        }
    }

    private void PlaceCapsule(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        
        if (raycastManager.Raycast(touchPosition, hits))
        {
            // 터치한 위치의 AR 평면에 타임캡슐 생성
            Pose pose = hits[0].pose;
            GameObject capsule = Instantiate(capsulePrefab, pose.position, pose.rotation);
            
            // 생성된 위치 정보 저장 (위도, 경도로 변환 필요)
            SaveCapsuleLocation(pose.position);
        }
    }

    private void SaveCapsuleLocation(Vector3 position)
    {
        // 위치 데이터를 JSON으로 변환
        string locationJson = JsonUtility.ToJson(new CapsuleLocationData
        {
            x = position.x,
            y = position.y,
            z = position.z
        });
        
        // 안드로이드로 위치 데이터 전달
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                activity.Call("receiveCapsuleLocation", locationJson);
            }
        }
    }
}

[System.Serializable]
public class CapsuleLocationData
{
    public float x;
    public float y;
    public float z;
}
