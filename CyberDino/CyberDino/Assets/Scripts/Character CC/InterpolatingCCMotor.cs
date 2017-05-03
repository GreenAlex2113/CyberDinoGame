using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CharacterController))]
public class InterpolatingCCMotor : CharacterMotor {
    private const float tolerance = 0.05f;

    public float updatesPerSecond = 20;
    public float slideSpeed = 8;

    [Tooltip("A surface with an incline equal to or above this number will be considered to be a wall. Wall detection caps out at Ceiling Angle.")]
    public float wallAngle = 80f;
    [Tooltip("A surface with an incline equal to or above this number will be considered to be a ceiling.")]
    public float ceilingAngle = 100f;

    private Transform tr;
    public override Vector3 Velocity { get { return velocity; } set { velocity.y = yVelocity = value.y; } }
    private Vector3 velocity;
    public Vector3 LastPosition { get; private set; }
    public Vector3 NextPosition { get; private set; }
    public Quaternion LastRotation { get; private set; }
    public Quaternion NextRotation { get; private set; }
    private float lastUpdateTime = 0;
    private float interpolationProgress;
    private float projectedDelta = 0;
    private CharacterController charController;
    private bool wasGrounded;
    public float yVelocity;
    private bool groundStickMove;
    private ControllerColliderHit groundStickyCollisionHit;


    //Awake() happens before Start()!
    void Awake() {
        tr = transform;
        charController = GetComponent<CharacterController>();
        LastPosition = NextPosition = tr.position;
        LastRotation = NextRotation = tr.rotation;
    }

    //And OnEnable also happans before Start()
    void OnEnable() {
        StartCoroutine(UpdateMovement());
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    public override void Move(float fixedDeltaTime) {
        //The actual movement code is run at a slow rate and the character's position is interpolated between those results.
        float t = CalucateInterpolationFactor();

        //Interpolate position.
        tr.position = Vector3.Lerp(LastPosition, NextPosition, t);

        //Interpolate rotation.
        tr.rotation = Quaternion.Slerp(LastRotation, NextRotation, t);
    }

    public float CalucateInterpolationFactor() {
        if (lastUpdateTime == 0f) {
            return 0;
        }

        float nextProjectedUpdate = lastUpdateTime + projectedDelta;
        float projectedTimeLeft = nextProjectedUpdate - Time.time;
        return 1 - (projectedTimeLeft / projectedDelta);
    }

    IEnumerator UpdateMovement() {
        lastUpdateTime = Time.time;
        float deltaTime;
        while (true) {
            yield return new WaitForSeconds(1f / updatesPerSecond);
            deltaTime = Time.time - lastUpdateTime;

            //Pre-Move

            //Move Input Clamping
            Vector3 adjustedMoveInput = gameCharacter.moveInput;
            //Don't allow the character to walk into steep slopes.
            if (SteepSlopeNormal != Vector3.zero && Vector3.Dot(gameCharacter.moveInput, SteepSlopeNormal) < 0) {
                adjustedMoveInput = Vector3.ProjectOnPlane(adjustedMoveInput, Vector3.ProjectOnPlane(SteepSlopeNormal, Vector3.up));
            }

            //And don't allow them to walk into walls or ceilings (ceilings especially because the character gets stuck very easily in them).
            //This also has the effect of allowing the character to more easily slide along a wall trying to walk into it.
            if (WallNormal != Vector3.zero && Vector3.Dot(gameCharacter.moveInput, WallNormal) < 0) {
                //Don't completely clamp the move input, leave just enough that the character will continue to collide with the wall.
                adjustedMoveInput = Vector3.Lerp(adjustedMoveInput, Vector3.ProjectOnPlane(adjustedMoveInput, Vector3.ProjectOnPlane(WallNormal, Vector3.up)), 0.99f);
            }

            //And don't allow them to walk into walls or ceilings (ceilings especially because the character gets stuck very easily in them).
            //This also has the effect of allowing the character to more easily slide along a wall trying to walk into it.
            if (CeilingNormal != Vector3.zero && Vector3.Dot(gameCharacter.moveInput, CeilingNormal) < 0) {
                //Don't completely clamp the move input, leave just enough that the character will continue to collide with the wall.
                adjustedMoveInput = Vector3.Lerp(adjustedMoveInput, Vector3.ProjectOnPlane(adjustedMoveInput, Vector3.ProjectOnPlane(CeilingNormal, Vector3.up)), 0.99f);
            }

            //Adjust it according to the slope we're on. (This stops the behaviour of "bumping" our way down slopes and helps us keep the proper speed when walking up slopes.)
            Vector3 normal = (GroundNormal != Vector3.zero) ? GroundNormal : SteepSlopeNormal;
            if (normal != Vector3.zero) {
                float magnitude = adjustedMoveInput.magnitude;
                adjustedMoveInput = Vector3.ProjectOnPlane(adjustedMoveInput, normal);
                //Projecting vectors can cut them short, let's make it not so.
                adjustedMoveInput = adjustedMoveInput.normalized * magnitude;
            }

            //Set horizontal velocity
            Vector3 moveVelocity = adjustedMoveInput * ((gameCharacter.IsSprinting) ? gameCharacter.sprintSpeed : gameCharacter.speed);

            //Jumping
            if (yVelocity <= 0) {
                IsJumping = false;
            }

            //Starting a jump
            if (jumpPressed && GroundNormal != Vector3.zero && !IsJumping) {
                yVelocity = CalculateJumpForce(gameCharacter.maxJumpHeight);
                GroundNormal = Vector3.zero;
                wasGrounded = false;
                IsJumping = true;
            }

            //Early jump termination
            if (!gameCharacter.jumpInput && IsJumping) {
                float minJumpForce = CalculateJumpForce(gameCharacter.minJumpHeight);
                if (yVelocity > minJumpForce) {
                    yVelocity = minJumpForce;
                }

                IsJumping = false;
            }

            //Gravity
            //Only apply gravity if we are not on the ground. This is so that gravity doesn't mess with the character walking up/down slopes.
            //The if statement can be removed to make going up slopes slower, and going down slopes faster.
            if (GroundNormal == Vector3.zero) {
                yVelocity -= gravity * deltaTime;
                yVelocity = Mathf.Max(yVelocity, -terminalVelocity);
            } else {
                //Apply a bit of force to make sure we keep registering ground collisions.
                yVelocity -= 0.005f;
            }

            //Sliding down steep slopes
            //We can't walk up them, so lets make sure we can't stand on them.
            //To do this, we need to slide down the steep slopes.
            if (GroundNormal == Vector3.zero && SteepSlopeNormal != Vector3.zero && yVelocity <= 0) {
                moveVelocity += Vector3.ProjectOnPlane(Vector3.down, SteepSlopeNormal).normalized * slideSpeed;
            }

            //Face the direction we're going!
            Quaternion rotationTarget = Quaternion.identity;
            LastRotation = NextRotation;
            if (gameCharacter.moveInput.magnitude >= tolerance) {
                rotationTarget = Quaternion.FromToRotation(Vector3.forward, gameCharacter.moveInput);
                rotationTarget.eulerAngles = new Vector3(0, rotationTarget.eulerAngles.y, 0);
                NextRotation = Quaternion.RotateTowards(NextRotation, rotationTarget, autoRotationSpeed * deltaTime);
            }

            //Reset hit tracking variabes
            wasGrounded = IsTouchingGround;

            IsTouchingGround = false;
            GroundNormal = Vector3.zero;
            GroundNormalAngle = -1;

            IsStandingOnSteepSlope = false;

            IsTouchingSteepSlope = false;
            SteepSlopeNormal = Vector3.zero;
            SteepSlopeNormalAngle = -1;

            IsTouchingWall = false;
            WallNormal = Vector3.zero;
            WallNormalAngle = -1;

            IsTouchingCeiling = false;
            CeilingNormal = Vector3.zero;
            CeilingNormalAngle = -1;

            IsAirborne = false;

            groundStickyCollisionHit = null;

            moveVelocity.y += yVelocity;

            //Move

            //Update our last position and set our true position to the next position.
            tr.position = LastPosition = NextPosition;
            //Actually Move!
            groundStickMove = false;
            charController.Move(moveVelocity * deltaTime);
            
            //Post-Move
            
            //Update our next position;
            NextPosition = tr.position;
            //Update interpolation values
            lastUpdateTime = Time.time;
            projectedDelta = (deltaTime + (1f / updatesPerSecond)) / 2f;
            projectedDelta -= Time.deltaTime;
            velocity = (LastPosition - NextPosition) / deltaTime;

            //Ground-stick effect
            //This triggers if and only if we just left the ground and it wasn't by jumping.
            if (GroundNormal == Vector3.zero && wasGrounded) {
                groundStickMove = true;
                charController.Move(Vector3.down * charController.stepOffset);
                Vector3 originalPosition = tr.position;

                // Counteract the movement from the ground sticky force if we didn't actually hit any ground or if the ground we hit was too steep.
                if (groundStickyCollisionHit == null) {
                    tr.position = originalPosition;
                } else {
                    float angle = Vector3.Angle(Vector3.up, groundStickyCollisionHit.normal);

                    if (angle > charController.slopeLimit - tolerance) {
                        tr.position = originalPosition;
                    } else {
                        //If we went over a peak then we want to kill our velocity, otherwise going down really fast should stay the same.
                        yVelocity = Mathf.Min(0, yVelocity);

                        //We DID just put ourselves back on the ground.
                        //So update the groundNormal information.
                        GroundNormal = groundStickyCollisionHit.normal;
                        //Only change vertical position.
                        tr.position = new Vector3(originalPosition.x, tr.position.y, originalPosition.z);
                    }
                }
            }

            //Post-Move
            if (((charController.collisionFlags & CollisionFlags.Below) != 0 && yVelocity < 0) || ((charController.collisionFlags & CollisionFlags.Above) != 0 && yVelocity > 0)) {
                yVelocity = 0;
            }

            if (!IsTouchingGround && IsTouchingSteepSlope) {
                IsStandingOnSteepSlope = true;
            }

            if (!IsTouchingGround && !IsTouchingSteepSlope) {
                IsAirborne = true;
            }

            // Jump button variables
            jumpPressed = false;

            //Put ourselves back at our LastPostion to avoid jittering when we get snapped back near there in Update()
            tr.position = LastPosition;
        }
	}

    void OnControllerColliderHit(ControllerColliderHit hit) {
        if (!groundStickMove) {
            //Normal movement collision
            float angle = Vector3.Angle(Vector3.up, hit.normal);

            if (angle < charController.slopeLimit + tolerance) {
                IsTouchingGround = true;
                GroundNormal = hit.normal;
                GroundNormalAngle = angle;
            } else if (angle < wallAngle - tolerance) {
                IsTouchingSteepSlope = true;
                SteepSlopeNormal = hit.normal;
                SteepSlopeNormalAngle = angle;
            } else if (angle < ceilingAngle - tolerance) {
                IsTouchingWall = true;
                WallNormal = hit.normal;
                WallNormalAngle = angle;
            } else {
                IsTouchingCeiling = true;
                CeilingNormal = hit.normal;
                CeilingNormalAngle = angle;
            }
        } else {
            // Ground sticky collision
            // Keep track if we hit anything going down.
            if (groundStickyCollisionHit == null) {
                groundStickyCollisionHit = hit;
            } else if (hit.point.y < groundStickyCollisionHit.point.y) {
                groundStickyCollisionHit = hit;
            }
        }
    }

    private float CalculateJumpForce(float height) {
        return Mathf.Sqrt(2 * height * gravity);
    }
}
