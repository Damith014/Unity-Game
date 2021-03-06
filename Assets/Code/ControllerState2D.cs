﻿using UnityEngine;
using System.Collections;
using System; 
public class ControllerState2D  {

    public bool ISCollidingRight { get; set; }
    public bool IsCollidingLeft { get; set; }
    public bool IsCollidingAbove { get; set; }
    public bool IsCollidingBelow { get; set; }
    public bool IsMovingDownSlope { get; set; }
    public bool IsMovingUpSlope { get; set; }
    public bool IsGrounded { get { return IsCollidingBelow; } }
    public float SlopAngle { get; set; }
    public bool HasCollisions { get { return ISCollidingRight || IsCollidingLeft || IsCollidingAbove || IsCollidingBelow; } }

    public void Reset()
    {
        IsMovingUpSlope =
            IsMovingDownSlope =
            IsCollidingLeft =
            ISCollidingRight =
            IsCollidingAbove =
            IsCollidingBelow = false;
        SlopAngle = 0;
    }
    public override string ToString()
    {
        return string.Format("(controller:r:{0} l:{1} a:{2} b:{3} down-slope:{4} up-slope:{5} angle:{6}",ISCollidingRight,IsCollidingLeft,IsCollidingAbove,IsCollidingBelow,IsMovingDownSlope,IsMovingUpSlope,SlopAngle); 
    }

}
