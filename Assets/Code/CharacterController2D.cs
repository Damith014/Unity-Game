using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterController2D : MonoBehaviour
{
    private const float skinWidht = .02f;
    private const int TotalHorizontalRays = 5;
    private const int TotalVerticalRays = 4; 

    public  static  readonly float SlopeLimiteTangant = Mathf.Tan(75f * Mathf.Deg2Rad);
	
    public LayerMask PlatFormMask;
    public ControlllerParameters2D DefaultParameters;

    public ControllerState2D State { get; private set; }
	public Vector2 Velocity { get  {return _velocity;}  }
	public bool CanJump
   {
        get { 
             if (Parameters.JumpRestrictions == ControlllerParameters2D.JumpBehavior.CanJumpAnyWhere)
                 return _jumpIn <= 0;

             if (Parameters.JumpRestrictions == ControlllerParameters2D.JumpBehavior.CanJumpOnGround)
                return State.IsGrounded;

             return false;
             
         } 
     }

	public ControlllerParameters2D Parameters { get { return _overrideParameters ?? DefaultParameters; } }
  	public GameObject StandingOn { get; private set; }

	 public Vector3 PlatformVelocity { get; private set; }


	public bool HandleCollisions { get; set; }
    private Vector2 _velocity;
    private Transform _transform;
    private Vector3 _localScale;
    private BoxCollider2D _boxCollider;
    private ControlllerParameters2D _overrideParameters;
    private float _jumpIn;
    private GameObject _lastStangingOn;

    private Vector3 
        _raycastTopLeft,
        _raycastBottomRight,
        _raycastBottomLeft;

    private Vector3
       _activeLocalPlatformPoint,
      _activeGlobalPlatformPoint;

   private float 
        _verticalDistanceBetweenRays,
        _horizontalDistanceBetweenRays;

    
    
    public void Awake()
	{ 
		HandleCollisions = true;
		State = new ControllerState2D();


		_transform = transform;
		_localScale = transform.localScale;
		_boxCollider = GetComponent<BoxCollider2D>();
	

        var colliderWidth = _boxCollider.size.x * Mathf.Abs(transform.localScale.x-(2*skinWidht));
        _horizontalDistanceBetweenRays = colliderWidth / (TotalHorizontalRays - 1);

        var colliderHeight = _boxCollider.size.y - Mathf.Abs(transform.localScale.y- (2 * skinWidht));
        _verticalDistanceBetweenRays = colliderHeight / (TotalHorizontalRays - 1);
    }
    public void AddForce(Vector2 force)
    {
        _velocity = force;
    }
    public void SetForce(Vector2 force)
    {
       _velocity += force;
    }
    public void SetHorizontalForce(float x)
    {
      	_velocity.x = x;
        
    }
    public void SetVerticalForce(float y)
    {
       _velocity.y=y;
    }
    public void Jump()
    {
        //TODO Moving platform support
        AddForce(new Vector2(0,Parameters.JumpMagnitude));
        _jumpIn = Parameters.JumpFrequency;

    }
    public void LateUpdate()
    {
		_jumpIn -= Time.deltaTime;
		Move(Velocity * Time.deltaTime);
		_velocity.y += Parameters.Gravity * Time.deltaTime;
       

        
    }
    private void Move(Vector2 deltaMovement)
    {
       var wasGronded = State.IsCollidingBelow;
        State.Reset();



        if (HandleCollisions)
        {
            HandlePlatforms();
            CalculateRayOrigins();

            if (deltaMovement.y < 0 && wasGronded)
            {
                HandelVerticalSlope(ref deltaMovement);
            }

            if (Mathf.Abs(deltaMovement.x) > .001f)
                MoveHorizontally(ref deltaMovement);

            MoveVertically(ref deltaMovement);

           CorrectHorizontalPlacement(ref deltaMovement, true);
          CorrectHorizontalPlacement(ref deltaMovement, false);
        }

        _transform.Translate(deltaMovement, Space.World);

        // TODO: Additional moing platform code 

       if(Time.deltaTime > 0)
            _velocity = deltaMovement / Time.deltaTime;

        _velocity.x = Mathf.Min(_velocity.x, Parameters.MaxVelocity.x);
        _velocity.y = Mathf.Min(_velocity.y, Parameters.MaxVelocity.y);

        if (State.IsMovingUpSlope)
            _velocity.y = 0;
            

        if (StandingOn != null)
        {
            _activeGlobalPlatformPoint = transform.position;
            _activeLocalPlatformPoint = StandingOn.transform.InverseTransformPoint(transform.position);
            
            if (_lastStangingOn != StandingOn)
            {
                if (_lastStangingOn != null)
                    _lastStangingOn.SendMessage("ControllerExit2D", this, SendMessageOptions.DontRequireReceiver);

                StandingOn.SendMessage("ControllerEnter2D", this, SendMessageOptions.DontRequireReceiver);
                _lastStangingOn = StandingOn;
            }
            else if (StandingOn != null)
                StandingOn.SendMessage("ControllerStay2D", this, SendMessageOptions.DontRequireReceiver);
           
        }
        else if (_lastStangingOn != null)
        {
           _lastStangingOn.SendMessage("ControllerExit2D", this, SendMessageOptions.DontRequireReceiver);
           _lastStangingOn=null;
       }
    }
    public void HandlePlatforms()
    {
        if (StandingOn != null)
        {
            var newGlobalPlatformPoint = StandingOn.transform.TransformPoint(_activeLocalPlatformPoint);
            var moveDistancee = newGlobalPlatformPoint - _activeGlobalPlatformPoint;

            if (moveDistancee != Vector3.zero)
                transform.Translate(moveDistancee, Space.World);

            PlatformVelocity = (newGlobalPlatformPoint - _activeGlobalPlatformPoint) / Time.deltaTime;

        }else
            PlatformVelocity = Vector3.zero;
        
        StandingOn = null;
    }

    private void CorrectHorizontalPlacement(ref Vector2 deltaMovement ,bool isRight)
    {
        var halfWidth = (_boxCollider.size.x * _localScale.x) / 2f;
        var rayOrigin = isRight ? _raycastBottomRight : _raycastBottomLeft;


        if (isRight)
            rayOrigin.x -= (halfWidth - skinWidht);
        else
            rayOrigin.x += (halfWidth - skinWidht);

        var rayDirection = isRight ? Vector2.right : -Vector2.right;
        var offset = 0f;

        for (var i = 1; i < TotalHorizontalRays -2; i++)
        {
			var rayVector = new Vector2(deltaMovement.x+rayOrigin.x, deltaMovement.y + rayOrigin.y + (i * _verticalDistanceBetweenRays));
            //Debug.DrawRay(rayVector,rayDirection*halfWidth,isRight?Color.cyan:Color.magenta);

            var raycastHit = Physics2D.Raycast(rayVector, rayDirection, halfWidth, PlatFormMask);
            if (!raycastHit)
                continue;
            offset = isRight ? ((raycastHit.point.x - _transform.position.x) - halfWidth) : (halfWidth - (_transform.position.x - raycastHit.point.x));

            
        }
        deltaMovement.x += offset;
    }
    public void CalculateRayOrigins()
    {
       var size = new Vector2(_boxCollider.size.x * Mathf.Abs(_localScale.x), _boxCollider.size.y * Mathf.Abs(_localScale.y)) / 2;
       var center = new Vector2(_boxCollider.center.x * _localScale.x, _boxCollider.center.y * _localScale.x);
        
        
        _raycastTopLeft = _transform.position + new Vector3(center.x - size.x + skinWidht, center.y + size.y - skinWidht);
        _raycastBottomRight = _transform.position + new Vector3(center.x + size.x - skinWidht, center.y - size.y + skinWidht);
        _raycastBottomLeft = _transform.position + new Vector3(center.x - size.x + skinWidht, center.y - size.y + skinWidht);

    }
    private void MoveHorizontally(ref Vector2 deltaMovement)
    {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + skinWidht;
        var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
        var rayOrigin = isGoingRight ? _raycastBottomRight : _raycastBottomLeft;

        for (var i = 0; i < TotalHorizontalRays-2; i++)
        {
            var rayVector =new Vector2(rayOrigin.x,rayOrigin.y+(i*_verticalDistanceBetweenRays));
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);

            var rayCastHit = Physics2D.Raycast(rayVector,rayDirection,rayDistance,PlatFormMask);
            if (!rayCastHit)
                continue;
            
            if (i == 0 && HandelHorizontalSlope(ref deltaMovement, Vector2.Angle(rayCastHit.normal, Vector2.up), isGoingRight))
                break;

            deltaMovement.x = rayCastHit.point.x - rayVector.x;
            rayDistance = Mathf.Abs(deltaMovement.x);

            if (isGoingRight)
            {
                deltaMovement.x -= skinWidht;
                State.ISCollidingRight = true;

            }
            else
            {
                deltaMovement.x += skinWidht;
                State.IsCollidingLeft = true;
            }

            if (rayDistance < skinWidht + .0001f)
                break;
        }
    }
    private void MoveVertically(ref Vector2 deltaMovement)
    { 
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y)+skinWidht;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        var rayOrigin = isGoingUp ? _raycastTopLeft : _raycastBottomLeft;

        rayOrigin.x += deltaMovement.x;

        var standingOnDistance = float.MaxValue;
        for (var i = 0; i < TotalVerticalRays; i++)
        {
            var rayVector = new Vector2(rayOrigin.x + (i * _horizontalDistanceBetweenRays), rayOrigin.y);
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);

            var raycastHit=Physics2D.Raycast(rayVector,rayDirection,rayDistance,PlatFormMask);
            
            if (!raycastHit)
                continue;
            if (!isGoingUp)
            {
                var verticalDistanceToHit = _transform.position.y - raycastHit.point.y;

                if (verticalDistanceToHit < standingOnDistance)
                {
                    standingOnDistance = verticalDistanceToHit;
                    StandingOn = raycastHit.collider.gameObject;
                }
            } 

            deltaMovement.y = raycastHit.point.y - rayVector.y;
            rayDistance = Mathf.Abs(deltaMovement.y);
            if (isGoingUp)
            {
                deltaMovement.y -= skinWidht;
                State.IsCollidingAbove = true;
            }
            else
            {
                deltaMovement.y += skinWidht;
                State.IsCollidingBelow = true;
            }

            if (!isGoingUp && deltaMovement.y > .0001f)
                State.IsMovingUpSlope = true;
            if(rayDistance<skinWidht+.0001f)
                break;
        }
    }
    private void HandelVerticalSlope(ref Vector2 deltaMovement)
    {
        var center = (_raycastBottomLeft.x + _raycastBottomRight.x) / 2;
        var direction = -Vector2.up;
        
        var slopeDistance=SlopeLimiteTangant*(_raycastBottomRight.x-center);
        var slopeRayVector = new Vector2(center,_raycastBottomLeft.y);

        Debug.DrawRay(slopeRayVector,direction*slopeDistance,Color.yellow);

        var raycastHit = Physics2D.Raycast(slopeRayVector, direction, slopeDistance, PlatFormMask);
        if (!raycastHit)
            return;
        //Resharper disable compareOffloatsByEqualltyOperator
        var isMovingDownSlope = Mathf.Sign(raycastHit.normal.x)==Math.Sign(deltaMovement.x);
        if (!isMovingDownSlope)
            return;


        var angle = Vector2.Angle(raycastHit.normal, Vector2.up);
        if (Mathf.Abs(angle) < .0001f)
            return;

        State.IsMovingDownSlope = true;
        State.SlopAngle = angle;
        deltaMovement.y = raycastHit.point.y - slopeRayVector.y;
     }
   private bool HandelHorizontalSlope(ref Vector2 delaMovement,float angle,bool isGoingRight)
    {
      if (Mathf.RoundToInt(angle) == 90)
            return false;
        if (angle > Parameters.SlopeLimit)
        {
            delaMovement.x = 0f;
            return true;
        }
        if (delaMovement.y > .07f)
            return true;
        delaMovement.x += isGoingRight ? -skinWidht : skinWidht;
        delaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * delaMovement.x);
        State.IsMovingUpSlope = true;
        State.IsCollidingBelow = true;
        return true;
      

    }
    public void OnTriggerEnter2D(Collider2D other)
    {
     	var parameters = other.gameObject.GetComponent<ControllerPhsyicsVolume2D>();
       		if (parameters == null)
            	return;
        _overrideParameters = parameters.Parameters;
       
    }
    public void OnTriggerExit2D(Collider2D other)
    {
      var parameters = other.gameObject.GetComponent<ControllerPhsyicsVolume2D>();
        if (parameters == null)
            return;
        _overrideParameters = null;
    }
}
