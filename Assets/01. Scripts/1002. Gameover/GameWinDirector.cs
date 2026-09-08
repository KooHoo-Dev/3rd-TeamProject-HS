using UnityEngine;
using DG.Tweening;

public class GameWinDirector : MonoBehaviour
{
    [Header("Hierarchy Targets")]
    [SerializeField] private CanvasGroup gameWinPanelGroup; // GameWinPanel (Canvas Group 추가 권장)
    [SerializeField] private RectTransform titleRect;           // GameClear ("We made it!" 이미지)
    [SerializeField] private RectTransform menuButton;          // Button (메인 메뉴 버튼)

    [Header("Drop Settings")]
    [Tooltip("화면 위쪽으로 띄워놓을 낙하 거리")]
    [SerializeField] private float dropDistance = 1000f;
    [SerializeField] private float titleDropDuration = 0.4f;   // 타이틀 낙하 시간
    [SerializeField] private float delayBeforeButton = 0.2f;   // 타이틀 착지 후 버튼 등장까지의 텀

    [Header("Camera & Audio")]
    [SerializeField] private Transform cameraTransform;         // Main Camera (비워둘 시 자동 탐색)
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winSfx;                 // 클리어 팡파레 또는 돌 착지음

    private Vector2 titleTargetPos;
    private Vector3 titleTargetScale;
    private Vector3 buttonTargetScale;
    private bool isInitialized = false;

    private void Awake()
    {
        InitPositions();
    }

    private void InitPositions()
    {
        if (isInitialized) return;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // 씬 뷰에 배치된 기본 좌표 및 스케일 캐싱 (Scale: 0.7 등 자동 보존)
        if (titleRect != null)
        {
            titleTargetPos = titleRect.anchoredPosition;
            titleTargetScale = titleRect.localScale;
        }

        if (menuButton != null)
        {
            buttonTargetScale = menuButton.localScale;
        }

        isInitialized = true;
    }

    private void OnEnable()
    {
        PlayWinSequence();
    }

    public void PlayWinSequence()
    {
        InitPositions();

        // 1. 기존 트윈 초기화
        DOTween.Kill(transform);
        if (titleRect != null) titleRect.DOKill();
        if (menuButton != null) menuButton.DOKill();

        // 2. 초기 상태 세팅 (타이틀은 화면 위로, 버튼은 크기 0)
        if (gameWinPanelGroup != null) gameWinPanelGroup.alpha = 0f;

        if (titleRect != null)
        {
            titleRect.anchoredPosition = titleTargetPos + new Vector2(0f, dropDistance);
            titleRect.localScale = titleTargetScale;
        }

        if (menuButton != null)
        {
            menuButton.localScale = Vector3.zero;
        }

        // 3. DOTween 시퀀스 연출
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); // Time.timeScale = 0 환경 대응

        // 배경 패널 부드럽게 페이드인 (0.25초)
        if (gameWinPanelGroup != null)
        {
            seq.Append(gameWinPanelGroup.DOFade(1f, 0.25f));
        }

        // [1단계] 육중한 돌 타이틀 급강하 & 착지 (바운스)
        if (titleRect != null)
        {
            // 배경 페이드와 동시에 위에서 떨어짐
            seq.Join(titleRect.DOAnchorPos(titleTargetPos, titleDropDuration).SetEase(Ease.OutBounce));

            // 착지 순간 카메라 진동 & 승리 효과음
            seq.AppendCallback(() =>
            {
                if (audioSource && winSfx) audioSource.PlayOneShot(winSfx);

                if (cameraTransform != null)
                    cameraTransform.DOShakePosition(0.2f, strength: 0.25f, vibrato: 15, randomness: 90f);
            });
        }

        // [2단계] 여운 텀 (인스펙터에서 시간 조절 가능)
        seq.AppendInterval(delayBeforeButton);

        // [3단계] 메인 메뉴 버튼 팝업
        if (menuButton != null)
        {
            seq.Append(menuButton.DOScale(buttonTargetScale, 0.25f).SetEase(Ease.OutBack));
        }
    }

    private void OnDisable()
    {
        DOTween.Kill(transform);
        if (titleRect != null) titleRect.DOKill();
        if (menuButton != null) menuButton.DOKill();
    }
}