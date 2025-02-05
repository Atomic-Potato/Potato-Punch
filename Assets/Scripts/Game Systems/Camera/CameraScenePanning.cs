﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScenePanning : MonoBehaviour, ICameraStrategy
{
    public CameraPanningPoint firstPanningPoint;

    CameraPanningPoint nextPanningPoint;
    CameraPanningPoint previousPanningPoint;

    Transform target;

    public static int CurrentPanningType;
    public static int VerticalPanning{ get{ return 1; } }
    public static int HorizontalPanning{ get { return 2; } }

    public static Vector2 FlowDirection;
    

    float refcamSpeed;

    void Awake() 
    {
        Initialize();
    }

    void Start()
    {
        target = Player.Instance.transform;
    }

    void ICameraStrategy.ExecuteFixedUpdate() {
        if(CurrentPanningType == VerticalPanning){
            FollowVertically();
        }
        else if(CurrentPanningType == HorizontalPanning){
            FollowHorizontally();
        }
        else{
            transform.position = target.position; // Fail safe so in case it would happen the type will be fixed at the next point
            throw new System.Exception("Unknown camera panning direction value: " + CurrentPanningType);
        }
    }

    void ICameraStrategy.ExecuteUpdate() {
        if(CurrentPanningType == VerticalPanning){
            LockCameraBetweenVerticalPoints();
        }
        else if(CurrentPanningType == HorizontalPanning){
            LockCameraBetweenHorizontalPoints();
        }
    }

    public void Initialize(){
        previousPanningPoint = firstPanningPoint;
        nextPanningPoint = firstPanningPoint.getNextPoint;

        transform.position = new Vector3 (previousPanningPoint.x, previousPanningPoint.y, transform.position.z);

        CurrentPanningType = GetPanningType();
        FlowDirection = GetFlowDirection(CurrentPanningType);
    }

    private void FollowVertically(){
        
        Transform panningPoint = nextPanningPoint != null ? nextPanningPoint.transform : previousPanningPoint.transform;

        transform.position = new Vector3(
            panningPoint.position.x, 
            Mathf.SmoothDamp(transform.position.y, target.transform.position.y, ref refcamSpeed, 0.1f), 
            transform.position.z);
    }

    private void FollowHorizontally(){
        Transform panningPoint = nextPanningPoint != null ? nextPanningPoint.transform : previousPanningPoint.transform;

        transform.position = new Vector3(
            Mathf.SmoothDamp(transform.position.x, target.transform.position.x, ref refcamSpeed, 0.1f), 
            panningPoint.position.y, 
            transform.position.z);
    }

    void LockCameraBetweenVerticalPoints(){
        if(FlowDirection == Vector2.up){
            LockBetweenPreviousAndNext();
        }
        else{
            LockBetweenNextAndPrevious();
        }

        void LockBetweenPreviousAndNext(){
            // Previous point is to the bottom of Next
            // Next point is to the top of Previous

            if(transform.position.y >= nextPanningPoint.y){
                // Set camera position to the reached point
                transform.position = new Vector3 (nextPanningPoint.x, nextPanningPoint.y, transform.position.z);

                if(nextPanningPoint.getNextPoint != null){
                    int nextPanningType = GetPanningType(nextPanningPoint, nextPanningPoint.getNextPoint);
                    if(nextPanningType == HorizontalPanning)
                        ShiftPointsWhenInNextPanningRegion(Vector2.right, nextPanningType, nextPanningPoint, nextPanningPoint.getNextPoint);
                    else
                        ShiftToNextPanningPoints();
                }
            }
            else if(transform.position.y <= previousPanningPoint.y){
                // Set camera position to the reached point
                transform.position = new Vector3 (previousPanningPoint.x, previousPanningPoint.y, transform.position.z);

                if(previousPanningPoint.getPreviousPoint != null){
                    int nextPanningType = GetPanningType(previousPanningPoint, previousPanningPoint.getPreviousPoint);
                    if(nextPanningType == HorizontalPanning)
                        ShiftPointsWhenInNextPanningRegion(Vector2.left, nextPanningType, previousPanningPoint, previousPanningPoint.getPreviousPoint);
                    else
                        ShiftToPreviousPanningPoints();
                }
            }
        }

        void LockBetweenNextAndPrevious(){
            // Previous point is to the top of Next
            // Next point is to the bottom of Previous

            if(transform.position.y <= nextPanningPoint.y){
                // Set camera position to the reached point
                transform.position = new Vector3 (nextPanningPoint.x, nextPanningPoint.y, transform.position.z);

                if(nextPanningPoint.getNextPoint != null){
                    int nextPanningType = GetPanningType(nextPanningPoint, nextPanningPoint.getNextPoint);
                    if(nextPanningType == HorizontalPanning)
                        ShiftPointsWhenInNextPanningRegion(Vector2.right, nextPanningType, nextPanningPoint, nextPanningPoint.getNextPoint);
                    else
                        ShiftToNextPanningPoints();
                }
            }
            else if(transform.position.y >= previousPanningPoint.y){
                transform.position = new Vector3 (previousPanningPoint.x, previousPanningPoint.y, transform.position.z);

                if(previousPanningPoint.getPreviousPoint != null){
                    int nextPanningType = GetPanningType(previousPanningPoint, previousPanningPoint.getPreviousPoint);
                    if(nextPanningType == HorizontalPanning)
                        ShiftPointsWhenInNextPanningRegion(Vector2.left, nextPanningType, previousPanningPoint, previousPanningPoint.getPreviousPoint);
                    else
                        ShiftToPreviousPanningPoints();
                }
            }
        }
    }

    void LockCameraBetweenHorizontalPoints(){
        if(FlowDirection == Vector2.right){
            LockBetweenPreviousAndNext();
        }
        else{
            LockBetweenNextAndPrevious();
        }

        void LockBetweenPreviousAndNext(){
            // Previous point is to the left of Next
            // Next point is to the right of Previous

            if(transform.position.x >= nextPanningPoint.x){
                // Normal case: Scene edge
                // Set camera position to the reached point
                transform.position = new Vector3 (nextPanningPoint.x, nextPanningPoint.y, transform.position.z);

                // Unusual case: Switch point
                if(nextPanningPoint.getNextPoint != null){
                    int nextPanningType = GetPanningType(nextPanningPoint, nextPanningPoint.getNextPoint);
                    if(nextPanningType == VerticalPanning)
                        ShiftPointsWhenInNextPanningRegion(Vector2.right, nextPanningType, nextPanningPoint, nextPanningPoint.getNextPoint);
                    else
                        ShiftToNextPanningPoints();
                }
            }
            else if(transform.position.x <= previousPanningPoint.x){
                // Normal case: Scene edge
                // Set camera position to the reached point
                transform.position = new Vector3 (previousPanningPoint.x, previousPanningPoint.y, transform.position.z);

                // Unusual case: Switch point
                if(previousPanningPoint.getPreviousPoint  != null){
                    int nextPanningType = GetPanningType(previousPanningPoint, previousPanningPoint.getPreviousPoint);
                    if(nextPanningType == VerticalPanning)
                        ShiftPointsWhenInNextPanningRegion(Vector2.left, nextPanningType, previousPanningPoint, previousPanningPoint.getPreviousPoint);
                    else
                        ShiftToPreviousPanningPoints();
                }
            }
        }

        void LockBetweenNextAndPrevious(){
            // Previous point is to the right of Next
            // Next point is to the left of Previous

            if(transform.position.x <= nextPanningPoint.x){
                // Set camera position to the reached point
                transform.position = new Vector3 (nextPanningPoint.x, nextPanningPoint.y, transform.position.z);

                if(nextPanningPoint.getNextPoint != null){
                    int nextPanningType = GetPanningType(nextPanningPoint, nextPanningPoint.getNextPoint);
                    if(nextPanningType == VerticalPanning)
                        ShiftPointsWhenInNextPanningRegion(Vector2.right, nextPanningType, nextPanningPoint, nextPanningPoint.getNextPoint);
                    else
                        ShiftToNextPanningPoints();
                }
            }
            else if(transform.position.x >= previousPanningPoint.x){
                // Set camera position to the reached point
                transform.position = new Vector3 (previousPanningPoint.x, previousPanningPoint.y, transform.position.z);

                if(previousPanningPoint.getPreviousPoint != null){
                    int nextPanningType = GetPanningType(previousPanningPoint, previousPanningPoint.getPreviousPoint);
                    if(nextPanningType == VerticalPanning)
                        ShiftPointsWhenInNextPanningRegion(Vector2.left, nextPanningType, previousPanningPoint, previousPanningPoint.getPreviousPoint);
                    else
                        ShiftToPreviousPanningPoints();
                }
            }
        }
    }

    void ShiftToNextPanningPoints(){
        previousPanningPoint = nextPanningPoint;
        nextPanningPoint = nextPanningPoint.getNextPoint;

        CurrentPanningType = GetPanningType();
        FlowDirection = GetFlowDirection(CurrentPanningType);
    }

    void ShiftToPreviousPanningPoints(){
        nextPanningPoint = previousPanningPoint;
        previousPanningPoint = nextPanningPoint.getPreviousPoint;

        CurrentPanningType = GetPanningType();
        FlowDirection = GetFlowDirection(CurrentPanningType);
    }

    void ShiftPointsWhenInNextPanningRegion(Vector2 shiftingDirection, int panningType, CameraPanningPoint startPoint, CameraPanningPoint endPoint){
            Vector2 nextFlowDirection = GetFlowDirection(panningType, startPoint, endPoint);
            
            if(nextFlowDirection == Vector2.up){
                if(target.transform.position.y > startPoint.y){
                    if(shiftingDirection == Vector2.right)
                        ShiftToNextPanningPoints();
                    else
                        ShiftToPreviousPanningPoints();
                }
            }
            else if(nextFlowDirection == Vector2.down){
                if(target.transform.position.y < startPoint.y){
                    if(shiftingDirection == Vector2.right)
                        ShiftToNextPanningPoints();
                    else
                        ShiftToPreviousPanningPoints();
                }
            }
            else if(nextFlowDirection == Vector2.left){
                if(target.transform.position.x < startPoint.x){
                    if(shiftingDirection == Vector2.right)
                        ShiftToNextPanningPoints();
                    else
                        ShiftToPreviousPanningPoints();
                }
            }
            else if(nextFlowDirection == Vector2.right){
                if(target.transform.position.x > startPoint.x){
                    if(shiftingDirection == Vector2.right)
                        ShiftToNextPanningPoints();
                    else
                        ShiftToPreviousPanningPoints();
                }
            }
        }

    /// <summary>
    /// Checks the difference in height and width between the panning points
    /// and returns the camera pannind direction based on which is greater
    /// </summary>
    /// <returns>Vertical if height is greater, otherwise Horizontal</returns>
    int GetPanningType(){
        float differenceInHeight = Mathf.Abs(previousPanningPoint.y - nextPanningPoint.y); 
        float differenceInWidth = Mathf.Abs(previousPanningPoint.x - nextPanningPoint.x);

        if(differenceInHeight > differenceInWidth)
            return VerticalPanning;
        else
            return HorizontalPanning; 
    }

    int GetPanningType(CameraPanningPoint previous, CameraPanningPoint next){
        float differenceInHeight = Mathf.Abs(previous.y - next.y); 
        float differenceInWidth = Mathf.Abs(previous.x - next.x);

        if(differenceInHeight > differenceInWidth)
            return VerticalPanning;
        else
            return HorizontalPanning; 
    }

    Vector2 GetFlowDirection(int panningType){
        if(panningType == HorizontalPanning){
            if(previousPanningPoint.x < nextPanningPoint.x)
                return Vector2.right;
            else
                return Vector2.left;
        }
        else{
             if(previousPanningPoint.y < nextPanningPoint.y)
                return Vector2.up;
            else
                return Vector2.down;   
        }
    }

    Vector2 GetFlowDirection(int panningType, CameraPanningPoint previous, CameraPanningPoint next){
        if(panningType == HorizontalPanning){
            if(previous.x < next.x)
                return Vector2.right;
            else
                return Vector2.left;
        }
        else{
             if(previous.y < next.y)
                return Vector2.up;
            else
                return Vector2.down;   
        }
    }
}
