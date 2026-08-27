using System;
using UnityEngine;

namespace WeGotOne
{
    public class DayTrigger : MonoBehaviour
    {
        [SerializeField] private DayLoop.DayType targetDayType;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerRoot>() == null)
                return;

            DayLoop.Instance.SetDayType(targetDayType);

            Destroy(gameObject);
        }
    }
}