using System;
using System.Collections;
using Games.Reefscape.Enums;
using Games.Reefscape.GamePieceSystem;
using Games.Reefscape.Robots;
using JetBrains.Annotations;
using RobotFramework.Components;
using RobotFramework.Controllers.GamePieceSystem;
using RobotFramework.Controllers.PidSystems;
using RobotFramework.Enums;
using RobotFramework.GamePieceSystem;
using UnityEngine;

namespace Prefabs.Reefscape.Robots.Mods._4481.Remb
{
    public class Rembrant : ReefscapeRobotBase
    {

        [SerializeField] private GenericElevator elevator;
        [SerializeField] private GenericJoint arm;
        [SerializeField] private GenericJoint climbArm;
        [SerializeField] private GenericJoint climbWrist;
        [SerializeField] private GenericJoint droppyThing;

        [SerializeField] private PidConstants armPid;
        [SerializeField] private PidConstants elevatorPid;
        [SerializeField] private PidConstants climbArmPid;
        [SerializeField] private PidConstants climbWristPid;
        [SerializeField] private PidConstants droppyThingPid;

        [SerializeField] private Rembrantsetpoint stow;
        [SerializeField] private Rembrantsetpoint intake;
        [SerializeField] private Rembrantsetpoint l1;
        [SerializeField] private Rembrantsetpoint l2;
        [SerializeField] private Rembrantsetpoint l3;
        [SerializeField] private Rembrantsetpoint l4;
        [SerializeField] private Rembrantsetpoint lowAlgae;
        [SerializeField] private Rembrantsetpoint highAlgae;
        [SerializeField] private Rembrantsetpoint bargePrep;
        [SerializeField] private Rembrantsetpoint bargePlace;
        [SerializeField] private Rembrantsetpoint groundAlgae;
        [SerializeField] private Rembrantsetpoint lollipop;
        [SerializeField] private Rembrantsetpoint climb;
        [SerializeField] private Rembrantsetpoint climbed;


        [SerializeField] private float vertical;
        [SerializeField] private float horizontal;
        [SerializeField] private float bargeDelay;

        [SerializeField] private ReefscapeGamePieceIntake coralIntake;
        [SerializeField] private ReefscapeGamePieceIntake algaeIntake;

        [SerializeField] private GamePieceState coralStowState;
        [SerializeField] private GamePieceState coralL4State;
        [SerializeField] private GamePieceState algaeStowState;

        private bool algaePlaced = false;
        private RobotGamePieceController<ReefscapeGamePiece, ReefscapeGamePieceData>.GamePieceControllerNode _coralController;
        private RobotGamePieceController<ReefscapeGamePiece, ReefscapeGamePieceData>.GamePieceControllerNode _algaeController;




        private float _elevatorTargetHeight;
        private float _armTargetAngle;
        private float _climbArmTargetAngle;
        private float _climbWristTargetAngle;
        private float _droppyThingTargetAngle;

        protected override void Start()
        {
            RobotGamePieceController.SetPreload(coralStowState);
            base.Start();

            algaePlaced = false;
            arm.SetPid(armPid);
            climbArm.SetPid(climbArmPid);
            climbWrist.SetPid(climbWristPid);
            droppyThing.SetPid(droppyThingPid);
            _elevatorTargetHeight = 0;
            _armTargetAngle = 0;
            _climbArmTargetAngle = 0;
            _climbWristTargetAngle = 0;
            _droppyThingTargetAngle = 0;

            RobotGamePieceController.SetPreload(coralStowState);
            SetRobotMode(ReefscapeRobotMode.Coral);

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

        }

        private void SetSetpoint(Rembrantsetpoint setpoint)
        {
            _elevatorTargetHeight = setpoint.elevatorHeight;
            _armTargetAngle = setpoint.armAngle;
            _climbArmTargetAngle = setpoint.climbArmAngle;
            _climbWristTargetAngle = setpoint.climbWristAngle;
            _droppyThingTargetAngle = setpoint.droppyThingAngle;
        }

        private void UpdateSetpoints()
        {
            elevator.SetTarget(_elevatorTargetHeight);
            arm.SetTargetAngle(_armTargetAngle).withAxis(JointAxis.X);
            climbArm.SetTargetAngle(_climbArmTargetAngle).withAxis(JointAxis.X);
            climbWrist.SetTargetAngle(_climbWristTargetAngle).withAxis(JointAxis.X);
            droppyThing.SetTargetAngle(_droppyThingTargetAngle).withAxis(JointAxis.X);
        }




        private void LateUpdate()
        {
            arm.UpdatePid(armPid);
        }

        private void FixedUpdate()
        {
            if (!OuttakeAction.IsPressed())
            {
                algaePlaced = false;
            }
            bool hasAlgae = _algaeController.HasPiece();
            bool hasCoral = _coralController.HasPiece();
            switch (CurrentSetpoint)
            {
                case ReefscapeSetpoints.Stow:
                    SetSetpoint(stow);
                    break;
                case ReefscapeSetpoints.Intake:
                    // _algaeController.RequestIntake(algaeIntake, true);
                    _coralController.RequestIntake(coralIntake, true);
                    _coralController.SetTargetState(coralStowState);
                    if ((CurrentRobotMode == ReefscapeRobotMode.Coral && !hasCoral) ||
                    (LastSetpoint == ReefscapeSetpoints.Barge && !hasAlgae) ||
                    (CurrentRobotMode == ReefscapeRobotMode.Algae && hasAlgae))
                    {
                        SetSetpoint(stow);

                    }

                    break;
                case ReefscapeSetpoints.Place:
                    if (LastSetpoint == ReefscapeSetpoints.Barge)
                    {
                        StartCoroutine(ScoreBargeAlgae());

                        break;
                    }
                    algaePlaced = true;
                    PlacePiece();
                    break;
                case ReefscapeSetpoints.L1:
                    SetSetpoint(l1);
                    _coralController.SetTargetState(coralStowState);
                    break;
                case ReefscapeSetpoints.Stack:
                    SetSetpoint(lollipop);

                    break;
                case ReefscapeSetpoints.L2:
                    _coralController.SetTargetState(coralStowState);
                    SetSetpoint(l2);
                    break;
                case ReefscapeSetpoints.LowAlgae:
                    SetSetpoint(lowAlgae);
                    _algaeController.RequestIntake(algaeIntake, IntakeAction.IsPressed());
                    break;
                case ReefscapeSetpoints.L3:
                    _coralController.SetTargetState(coralStowState);
                    SetSetpoint(l3);
                    break;
                case ReefscapeSetpoints.HighAlgae:
                    SetSetpoint(highAlgae);
                    _algaeController.RequestIntake(algaeIntake, IntakeAction.IsPressed());
                    break;
                case ReefscapeSetpoints.L4:
                    SetSetpoint(l4);
                    _coralController.SetTargetState(coralL4State);
                    break;
                case ReefscapeSetpoints.Processor:
                    break;
                case ReefscapeSetpoints.Barge:
                    SetSetpoint(bargePrep);
                    break;
                case ReefscapeSetpoints.RobotSpecial:
                    SetState(ReefscapeSetpoints.Stow);
                    break;
                case ReefscapeSetpoints.Climb:
                    SetSetpoint(climb);
                    break;
                case ReefscapeSetpoints.Climbed:
                    SetSetpoint(climbed);
                    break;
            }
            // SetSetpoint(intake);
            UpdateSetpoints();
        }


        public IEnumerator ScoreBargeAlgae()
        {
            if (!algaePlaced)
            {
                SetSetpoint(bargePlace);

                yield return new WaitForSeconds(bargeDelay);

                PlacePiece();
            }
        }
        private void PlacePiece()
        {
            if (_algaeController.HasPiece() && ((CurrentRobotMode == ReefscapeRobotMode.Algae) || (!_coralController.HasPiece() && LastSetpoint == ReefscapeSetpoints.Barge)))
            {
                _algaeController.ReleaseGamePieceWithForce(new Vector3(0, horizontal, vertical));
            }
            else
            {
                if (LastSetpoint == ReefscapeSetpoints.L4)
                {
                    _coralController.ReleaseGamePieceWithForce(new Vector3(0, 0, -5));
                }
                else if (CurrentRobotMode == ReefscapeRobotMode.Coral)
                {
                    _coralController.ReleaseGamePieceWithForce(new Vector3(0, 0, 10));
                }
            }
        }
    }
}
