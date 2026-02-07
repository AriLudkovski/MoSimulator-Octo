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

        [SerializeField] private PidConstants armPid;
        [SerializeField] private PidConstants elevatorPid;

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


        [SerializeField] private float vertical;
        [SerializeField] private float horizontal;

        [SerializeField] private ReefscapeGamePieceIntake coralIntake;
        [SerializeField] private ReefscapeGamePieceIntake algaeIntake;

        [SerializeField] private GamePieceState coralStowState;
        [SerializeField] private GamePieceState coralL4State;
        [SerializeField] private GamePieceState algaeStowState;

        private RobotGamePieceController<ReefscapeGamePiece, ReefscapeGamePieceData>.GamePieceControllerNode _coralController;
        private RobotGamePieceController<ReefscapeGamePiece, ReefscapeGamePieceData>.GamePieceControllerNode _algaeController;




        private float _elevatorTargetHeight;
        private float _armTargetAngle;



        protected override void Start()
        {
            RobotGamePieceController.SetPreload(coralStowState);
            base.Start();

            arm.SetPid(armPid);
            _elevatorTargetHeight = 0;
            _armTargetAngle = 0;

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
        }

        private void UpdateSetpoints()
        {
            elevator.SetTarget(_elevatorTargetHeight);
            arm.SetTargetAngle(_armTargetAngle).withAxis(JointAxis.X);
        }




        private void LateUpdate()
        {
            arm.UpdatePid(armPid);
        }

        private void FixedUpdate()
        {
            // bool hasAlgae = _algaeController.HasPiece();
            // bool hasCoral = _coralController.HasPiece();
            switch (CurrentSetpoint)
            {
                case ReefscapeSetpoints.Stow:
                    SetSetpoint(stow);
                    break;
                case ReefscapeSetpoints.Intake:
                    // _algaeController.RequestIntake(algaeIntake, true);
                    _coralController.RequestIntake(coralIntake, true);
                    _coralController.SetTargetState(coralStowState);

                    break;
                case ReefscapeSetpoints.Place:
                    if (LastSetpoint == ReefscapeSetpoints.Barge)
                    {
                        SetSetpoint(bargePlace);
                    }
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
                    SetSetpoint(stow);
                    break;
                case ReefscapeSetpoints.Barge:
                    SetSetpoint(bargePrep);
                    break;
                case ReefscapeSetpoints.RobotSpecial:
                    SetState(ReefscapeSetpoints.Stow);
                    break;
                case ReefscapeSetpoints.Climb:
                    break;
                case ReefscapeSetpoints.Climbed:
                    break;
            }
            // SetSetpoint(intake);
            UpdateSetpoints();
        }

        private void PlacePiece()
        {
            if (_algaeController.HasPiece())
            {
                _algaeController.ReleaseGamePieceWithForce(new Vector3(0, horizontal, vertical));
            }
            else
            {
                if (LastSetpoint == ReefscapeSetpoints.L4)
                {
                    _coralController.ReleaseGamePieceWithForce(new Vector3(0, 0, -5));
                }
                else
                {
                    _coralController.ReleaseGamePieceWithForce(new Vector3(0, 0, 10));
                }
            }
        }
    }
}
