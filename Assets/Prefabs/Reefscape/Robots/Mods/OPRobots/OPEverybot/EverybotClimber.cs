using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Prefabs.Reefscape.Robots.Mods.EverybotPack.everybot
{
    public class EverybotClimber : MonoBehaviour
    {

        [Header("Trigger Colliders")]
        public EverybotClimberHelper Trigger1;
        public EverybotClimberHelper Trigger2;

        [Header("Colliders to Enable")]
        public Collider[] hitBoxesToEnable;
        [Header("Colliders to Disable")]
        public Collider[] hitBoxesToDisable;

        private bool _firstTriggered;
        private bool _secondTriggered;

        
        private void Start()
        {
            foreach (Collider elem in hitBoxesToEnable)
            {
                elem.enabled = false;
            }
            foreach (Collider elem in hitBoxesToDisable)
            {
                elem.enabled = true;
            }
        }

        public void NotifyTriggered(EverybotClimberHelper helper, bool isTriggered)
        {
            if (helper == Trigger1)
                _firstTriggered = isTriggered;

            if (helper == Trigger2)
                _secondTriggered = isTriggered;

            TryEnablingColliders();
            TryDisablingColliders();
        }

        private void TryEnablingColliders()
        {
            if (_firstTriggered && _secondTriggered) {
                foreach (Collider elem in hitBoxesToEnable)
                {
                    elem.enabled = true;
                }
            } else
            {
                foreach (Collider elem in hitBoxesToEnable)
                {
                    elem.enabled = false;
                }
            }
        }
        private void TryDisablingColliders()
        {
            if (_firstTriggered && _secondTriggered)
            {
                foreach (Collider elem in hitBoxesToDisable)
                {
                    elem.enabled = false;
                }
            }
            else
            {
                foreach (Collider elem in hitBoxesToDisable)
                {
                    elem.enabled = true;
                }
            }
        }
    }
}