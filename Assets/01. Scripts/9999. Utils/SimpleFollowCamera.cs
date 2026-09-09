using System;
using HelloServer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    // 타겟을 부드럽게(Lerp) 따라가는 범용 추적 카메라

    public class SimpleFollowCamera : MonoBehaviour
    {
        public enum States
        {
            Stopped,
            Holding,
            Following
        }

        [field: SerializeField] public Transform Target { get; set; }
        [SerializeField] private float lerpSpeed; // 따라가는 속도, Damping 값
        [SerializeField] private Vector3 offset;

        private Camera Cam { get; set; }
        private States State { get; set; }

        private static readonly Key[] FollowKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };
        private Key followKey;
        private string followedPlayerId;

        private int currentPoint;
        public float ChangeWaitTime { get; private set; }

        private void Start()
        {
            Cam = Camera.main;

            // [보완]: 시작할 때 인스펙터에 타겟을 넣어두었다면, 바로 Following 상태로 시작하게 만듭니다.
            if (Target != null)
            {
                State = States.Following;
            }
            else
            {
                State = States.Stopped;
            }
        }


        private void Update()
        {
            SetFollowKey();
        }

        private void SetFollowKey()
        {
            if (IntegratedDebugUI.IsTeleportModeActive)
            {
                followedPlayerId = null;
                return;
            }

            if (RoomClient.IsInitialized == false 
                || RoomClient.Instance.Me == null 
                || RoomClient.Instance.SnapshotMessage == null)
            {
                followedPlayerId = null;
                return;
            }
            
            Keyboard keyboard = Keyboard.current;
            PlayerState[] players = RoomClient.Instance.SnapshotMessage?.Players;

            for (int i = 0; i < players.Length; i++)
            {
                if (keyboard[FollowKeys[i]].wasPressedThisFrame)
                {
                    followKey = FollowKeys[i];
                    followedPlayerId = players[i].Id;
                }
            }
        }

        private void LateUpdate()
        {
            switch(State)
            {
                case States.Stopped:
                    // 멈춤 상태가 되면 타겟을 비워버립니다.
                    Target = null;
                    break;
                case States.Holding:
                    // Holdin은 Target은 존재하지만 카메라를 이동하지 않는 상태
                    break;
                case States.Following:
                    // Target의 위치를 적절히 보간하여 적용합니다.
                    // ps : lerpSpeed는 댐핑값이라고 해서 속도에 대한 보정값입니다.
                    // =======================
                    // 아래의 보간내용은 (transform.position)을 (Target.position + offset)으로 초당 (lerpSpeed) 속도로
                    // 계속 가깝게 만드는 코드 입니다. Target.position이 멀다면 더 빨리, 가깝다면 느리게
                    Vector3 targetPos = GetFollowPosition();
                    Vector3 lerpPos = Vector3.Lerp(Cam.transform.position, targetPos + offset, lerpSpeed * Time.deltaTime);
                    lerpPos.z = Cam.transform.position.z; // 2D의 경우 z는 바뀌지 않습니다.
                    Cam.transform.position = lerpPos;
                    
                    if(currentPoint >= GameManager.Instance.CheckPoints.Length) return;
                    
                    if (GameManager.Instance.IsCamTargetChange && Mathf.Abs(Cam.transform.position.x -
                                                                            GameManager.Instance.CheckPoints[currentPoint].transform.position.x) <= 0.1f)
                    {
                        GameManager.Instance.IsCamTargetChange = false;
                        ChangeWaitTime = 0.0f;
                        GameManager.Instance.spawnPointIndex = currentPoint;
                        GameManager.Instance.UpdateSpawnPoint();
                        currentPoint++;
                    }
                    break;
            }
        }

        private Vector3 GetFollowPosition()
        {
            Keyboard keyboard = Keyboard.current;
            PlayerState[] players = RoomClient.Instance.SnapshotMessage?.Players;

            if (GameManager.Instance.IsCamTargetChange)
            {
                ChangeWaitTime += Time.deltaTime;
                if (ChangeWaitTime >= 1.0f)
                {
                    lerpSpeed = 1f;
                    if (currentPoint <= GameManager.Instance.CheckPoints.Length)
                    {
                        return GameManager.Instance.CheckPoints[currentPoint].transform.position;    
                    }

                    return Target.position;
                }
            }
            
            if (followedPlayerId != null && keyboard[followKey].isPressed)
            {
                // 배열 순서가 바뀌어도 누를 때 선택한 플레이어를 계속 따라갑니다.
                foreach (PlayerState player in players)
                {
                    if (player.Id == followedPlayerId)
                    {
                        return new Vector3(player.Body.X, player.Body.Y, Target.position.z);
                    }
                }
            }

            // 키 해제 또는 대상 퇴장 시 기존 자신의 Target으로 복귀합니다.
            followedPlayerId = null;
            return Target.position;
        }

        private void OnDisable()
        {
            followedPlayerId = null;
        }

        public void ChangeState(States state)
        {
            if(state == States.Following && Target == null)
            {
                Debug.LogWarning("SimpleFollowCamera :: Target이 없어 Following 상태로 전환할 수 없습니다");
                return;
            }

            State = state;
            followedPlayerId = null;
        }

    }

}


