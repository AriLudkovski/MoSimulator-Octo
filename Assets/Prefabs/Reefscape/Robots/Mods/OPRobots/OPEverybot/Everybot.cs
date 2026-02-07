using System.Collections;
using Games.Reefscape.Enums;
using Games.Reefscape.GamePieceSystem;
using Games.Reefscape.Robots;
using Games.Reefscape.Scoring.Scorers;
using MoSimCore.BaseClasses.GameManagement;
using MoSimCore.Enums;
using MoSimLib;
using RobotFramework.Components;
using RobotFramework.Controllers.GamePieceSystem;
using RobotFramework.Controllers.PidSystems;
using RobotFramework.Enums;
using RobotFramework.GamePieceSystem;
using UnityEngine;

namespace Prefabs.Reefscape.Robots.Mods.EverybotPack.everybot
{
    public class Everybot : ReefscapeRobotBase
    {
        [Header("Everybot Components")]
        [SerializeField] private GenericJoint algaeArm;
        [SerializeField] private GenericJoint climber;
        [SerializeField] private GenericAnimationJoint rollerAnimation;
        [Header("PIDS")]
        [SerializeField] private PidConstants algaeArmPid;
        [SerializeField] private PidConstants climberPid;
        [Header("Setpoints")]
        [SerializeField] private EverybotSetpoint Stow;
        [SerializeField] private EverybotSetpoint Intake;
        [SerializeField] private EverybotSetpoint L1;
        [SerializeField] private EverybotSetpoint L1Place;
        [SerializeField] private EverybotSetpoint AlgaeIntake;

        private ClimbScorer _climbScorer;

        [Header("Game Piece States")]
        [SerializeField] private ReefscapeGamePieceIntake coralIntake;
        [SerializeField] private ReefscapeGamePieceIntake algaeIntake;

        [SerializeField] private GamePieceState coralStowState;
        [SerializeField] private GamePieceState algaeStowState;
        [SerializeField] private GamePieceState coralL4State;


        private RobotGamePieceController<ReefscapeGamePiece, ReefscapeGamePieceData>.GamePieceControllerNode _coralController;
        private RobotGamePieceController<ReefscapeGamePiece, ReefscapeGamePieceData>.GamePieceControllerNode _algaeController;

        [Header("Algae Stall Audio")]
        [SerializeField] private AudioSource algaeStallSource;
        [SerializeField] private AudioClip algaeStallAudio;

        [Header("Robot Audio")]
        [SerializeField] private AudioSource rollerSource;
        [SerializeField] private AudioClip intakeClip;


        private float _algaeArmTargetAngle;
        private float _climberTargetAngle;



        protected override void Start()
        {
            base.Start();

            _climbScorer = gameObject.GetComponent<ClimbScorer>();
            algaeArm.SetPid(algaeArmPid);
            climber.SetPid(climberPid);
            _algaeArmTargetAngle = 0;
            _climberTargetAngle = 37.133755f;

            RobotGamePieceController.SetPreload(coralStowState);
            _coralController = RobotGamePieceController.GetPieceByName(ReefscapeGamePieceType.Coral.ToString());
            _algaeController = RobotGamePieceController.GetPieceByName(ReefscapeGamePieceType.Algae.ToString());

            _coralController.gamePieceStates = new[]
            {
                coralStowState,
                coralL4State
            };
            _coralController.intakes.Add(coralIntake);

            _algaeController.gamePieceStates = new[] { algaeStowState };
            _algaeController.intakes.Add(algaeIntake);

            algaeStallSource.clip = algaeStallAudio;
            algaeStallSource.loop = true;
            algaeStallSource.Stop();

            rollerSource.clip = intakeClip;
            rollerSource.loop = true;
            rollerSource.Stop();

        }
        private void SetSetpoint(EverybotSetpoint setpoint)
        {
            _algaeArmTargetAngle = setpoint.AlgaeArmAngle;
        }
        private void UpdateSetpoints()
        {
            algaeArm.SetTargetAngle(_algaeArmTargetAngle).withAxis(JointAxis.X);
            climber.SetTargetAngle(_climberTargetAngle).withAxis(JointAxis.X);
        }
        private void LateUpdate()
        {
            algaeArm.UpdatePid(algaeArmPid);
            climber.UpdatePid(climberPid);
        }
        private void FixedUpdate()
        {
            bool hasAlgae = _algaeController.HasPiece();
            bool hasCoral = _coralController.HasPiece();

            _algaeController.SetTargetState(algaeStowState);
            _coralController.SetTargetState(coralStowState);


            switch (CurrentSetpoint)
            {
                case ReefscapeSetpoints.Stow:
                    SetSetpoint(Stow);
                    _algaeController.RequestIntake(algaeIntake, false);
                    _coralController.RequestIntake(coralIntake, false);
                    rollerAnimation.VelocityRoller(0).useAxis(JointAxis.X);
                    break;
                case ReefscapeSetpoints.Intake:
                    if (CurrentRobotMode == ReefscapeRobotMode.Algae)
                    {
                        SetSetpoint(AlgaeIntake);
                        rollerAnimation.VelocityRoller(IntakeAction.IsPressed() ? 250 : 0).useAxis(JointAxis.X);
                    }
                    else
                    {
                        SetSetpoint(Intake);
                        rollerAnimation.VelocityRoller(IntakeAction.IsPressed() ? -250 : 0).useAxis(JointAxis.X);
                    }
                    _algaeController.RequestIntake(algaeIntake, CurrentRobotMode == ReefscapeRobotMode.Algae && !hasAlgae);
                    bool shouldIntake = CurrentRobotMode == ReefscapeRobotMode.Coral && !hasCoral && !hasAlgae;
                    // Debug.Log($"Intake - Mode: {CurrentRobotMode}, HasCoral: {hasCoral}, ShouldIntake: {shouldIntake}, Controller Current State: {_coralController.currentStateNum}");
                    _coralController.RequestIntake(coralIntake, shouldIntake);
                    break;
                case ReefscapeSetpoints.Place:
                    PlacePiece();
                    // if (LastSetpoint == ReefscapeSetpoints.L1)
                    // {
                    // SetSetpoint(L1Place);
                    // }
                    break;
                case ReefscapeSetpoints.L1:
                    SetSetpoint(L1);
                    break;
                case ReefscapeSetpoints.Stack:
                    SetSetpoint(Intake);
                    _algaeController.RequestIntake(algaeIntake, IntakeAction.IsPressed() && !hasAlgae);
                    _coralController.RequestIntake(coralIntake, false);

                    break;
                case ReefscapeSetpoints.L2:
                    break;
                case ReefscapeSetpoints.LowAlgae:
                    break;
                case ReefscapeSetpoints.L3:
                    break;
                case ReefscapeSetpoints.HighAlgae:
                    break;
                case ReefscapeSetpoints.L4:
                    SetSetpoint(L1);
                    _coralController.SetTargetState(coralL4State);
                    break;
                case ReefscapeSetpoints.Processor:
                    SetSetpoint(Stow);
                    break;
                case ReefscapeSetpoints.Barge:
                    break;
                case ReefscapeSetpoints.RobotSpecial:
                    SetState(ReefscapeSetpoints.Stow);
                    break;
                case ReefscapeSetpoints.Climb:
                    SetSetpoint(Stow);
                    _climberTargetAngle = 0;
                    break;
                case ReefscapeSetpoints.Climbed:
                    _climberTargetAngle = 75;
                    break;
            }
            UpdateSetpoints();
            UpdateAudio();
            if (_climbScorer.AutoClimbTriggered && CurrentSetpoint == ReefscapeSetpoints.Climb)
                SetState(ReefscapeSetpoints.Climbed);
            if (CurrentSetpoint is ReefscapeSetpoints.Climb or ReefscapeSetpoints.Climbed) DriveController.SetDriveMp(0.5f);
            else DriveController.SetDriveMp(1);
        }
        private void PlacePiece()
        {
            if (CurrentRobotMode == ReefscapeRobotMode.Algae)
            {
                rollerAnimation.VelocityRoller(-250).useAxis(JointAxis.X);
                _algaeController.ReleaseGamePieceWithForce(new Vector3(0, 0, 2));
                if (OuttakeAction.IsPressed() == false)
                {
                    SetState(ReefscapeSetpoints.Stow);
                    UpdateSetpoints();
                }
            }
            else if (LastSetpoint == ReefscapeSetpoints.L4)
            {
                bool released = _coralController.ReleaseGamePieceWithForce(new Vector3(0, 0, -2));

            }
            else
            {

                // Debug.Log($"Placing coral piece - HasPiece before: {_coralController.HasPiece()}");
                rollerAnimation.VelocityRoller(250).useAxis(JointAxis.X);
                bool released = _coralController.ReleaseGamePieceWithForce(new Vector3(2, 0, 0));
                StartCoroutine(BarSlap());
            }
        }
        IEnumerator BarSlap()
        {
            yield return new WaitForSeconds(0.15f);
            rollerAnimation.VelocityRoller(0).useAxis(JointAxis.X);
            rollerSource.Stop();
            SetSetpoint(L1Place);
            UpdateSetpoints();
            yield return new WaitForSeconds(0.4f);
            SetState(ReefscapeSetpoints.Stow);
            UpdateSetpoints();
            yield break;
        }

        private void UpdateAudio()
        {
            if (BaseGameManager.Instance.RobotState == RobotState.Disabled)
            {
                if (rollerSource.isPlaying || algaeStallSource.isPlaying)
                {
                    rollerSource.Stop();
                    algaeStallSource.Stop();
                }

                return;
            }

            if (((IntakeAction.IsPressed() && !_coralController.HasPiece() && !_coralController.HasPiece()) ||
                 OuttakeAction.IsPressed()) &&
                !rollerSource.isPlaying)
            {
                rollerSource.Play();
            }
            else if (!IntakeAction.IsPressed() && !OuttakeAction.IsPressed() && rollerSource.isPlaying)
            {
                rollerSource.Stop();
            }
            else if (IntakeAction.IsPressed() && (_coralController.HasPiece() || _algaeController.HasPiece()))
            {
                rollerSource.Stop();
            }

            if (_algaeController.HasPiece() && !algaeStallSource.isPlaying)
            {
                algaeStallSource.Play();
            }
            else if (!_algaeController.HasPiece() && algaeStallSource.isPlaying)
            {
                algaeStallSource.Stop();
            }
        }
    }
}