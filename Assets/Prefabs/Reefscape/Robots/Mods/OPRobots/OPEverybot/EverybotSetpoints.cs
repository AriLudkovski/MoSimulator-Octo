using UnityEngine;

namespace Prefabs.Reefscape.Robots.Mods.EverybotPack.everybot
{
   [CreateAssetMenu(fileName = "Setpoint", menuName = "Robot/Everybot Setpoint", order = 0)]
   public class EverybotSetpoint : ScriptableObject
   {
       [Tooltip("Degrees")] public float AlgaeArmAngle;
   }
}
