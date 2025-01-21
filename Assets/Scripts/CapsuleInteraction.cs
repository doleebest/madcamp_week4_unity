using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class CapsuleInteraction : MonoBehaviour
{
    private Animator capsuleAnimator;
    private string capsuleId; // 타임캡슐의 고유 ID

    void Start()
    {
        capsuleAnimator = GetComponent<Animator>();
    }

    void OnMouseDown()  // 터치/클릭 이벤트
    {
        OnTouchCapsule();
    }

    private void OnTouchCapsule()
    {
        // 1. 타임캡슐 열리는 애니메이션 재생
        if (capsuleAnimator != null)
        {
            capsuleAnimator.SetTrigger("Open");
        }
        
        // 2. 안드로이드 앱으로 신호 전달
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                // 타임캡슐 ID와 함께 열기 신호 전달
                activity.Call("openCapsuleContent", capsuleId);
            }
        }
    }
}