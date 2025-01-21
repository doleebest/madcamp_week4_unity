using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleLocator : MonoBehaviour
{
    [SerializeField] private GameObject capsulePrefab;
    private bool isCapsuleVisible = false;
    private Vector3 savedLocation;
    private float proximityThreshold = 10f; // 미터 단위

    private void Start()
    {
        // 안드로이드에서 저장된 위치 정보 받아오기
        StartCoroutine(CheckProximity());
    }

    IEnumerator CheckProximity()
    {
        while (true)
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                // 현재 위치와 저장된 위치 사이의 거리 계산
                float distance = Vector3.Distance(new Vector3(
                    Input.location.lastData.latitude,
                    0,
                    Input.location.lastData.longitude
                ), savedLocation);

                // 근처에 오면 타임캡슐 표시
                if (distance < proximityThreshold && !isCapsuleVisible)
                {
                    ShowCapsule();
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void ShowCapsule()
    {
        // AR 공간에 타임캡슐 생성
        Instantiate(capsulePrefab, savedLocation, Quaternion.identity);
        isCapsuleVisible = true;
    }
}
