using UnityEngine;

namespace Prefabs.Reefscape.Robots.Mods.OPJITB
{
    [CreateAssetMenu(fileName = "Setpoint", menuName = "Robot/OPJITB Setpoint", order = 0)]
    public class OPJITBSetpoint : ScriptableObject
    {
        [Tooltip("Deg")]
        public float armAngle;
        [Tooltip("Deg")]
        public float wristAngle;
        [Tooltip("Inch")]
        public float armDistance;
    }
}