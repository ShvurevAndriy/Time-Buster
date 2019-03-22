using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccurateNextPositionFinder : MonoBehaviour {

    private const float epsilon = 0.01f;
    private const float startPredictionHeight = 0.5f;

    [SerializeField] float timeStep = 0.02f;
    [SerializeField] float accuracyDelta = 0.5f;
    [SerializeField] int maxIterationtonToFindAccuratePos = 10;
    [SerializeField] float distanceBetwinPoints = 1;
    [SerializeField] float maxPredictionTime = 5f;
    [SerializeField] float maxPredictionYDelta = 50f;

    private MarkersPool markersPool;
    private Rigidbody2D rigidBody;

    private void Start() {
        markersPool = FindObjectOfType<MarkersPool>();
    }

    public Vector3 FindNextPosition(float yVelocity, float angularSpeed, ref float angel, float radius, float gravity, float distance, ref float currentTimeSlice, Vector3 lastPos, Vector3 startPos) {
        Vector3 position;

        //Time where distance still to small
        float minTime = currentTimeSlice;
        //time where distance still to big
        float maxTime = 0;

        float currentDistance = 0;
        int left = 0, right = 0;
        int currentIterations = maxIterationtonToFindAccuratePos;
        float currentAngel;
        do {
            if (currentDistance > distance) {
                left++;
                maxTime = currentTimeSlice;
                currentTimeSlice = (maxTime + minTime) / 2;
            } else if (currentDistance < distance) {
                right++;
                minTime = currentTimeSlice;
                if (maxTime == 0) {
                    currentTimeSlice += timeStep;
                } else {
                    currentTimeSlice = (maxTime + minTime) / 2;
                }
            }
            currentAngel = angel;
            position = JumpPhysics.CalculatePositionAtTime(yVelocity, angularSpeed, ref currentAngel, radius, currentTimeSlice, gravity, startPos);
            currentDistance = Vector3.Distance(lastPos, position);
        } while (--currentIterations > 0 && (currentDistance < distance || currentDistance - distance > accuracyDelta));
        angel = currentAngel;
        if (currentIterations <= 0) {
            Debug.LogWarning("can't get accuracy left: " + left + "   right: " + right);
        }
        return position;
    }

    public bool PredictTuchPoint(float yVelocity, float angularSpeed, float startAngel, float radius, float gravity, int layerMask, Vector3 startPos, out Vector3 landingPoint, BoxCollider boxCollider) {
        float currentTimeSlice = 0;
        float angel = startAngel;
        Vector3 position = FindNextPosition(yVelocity, angularSpeed, ref angel, radius, gravity, startPredictionHeight, ref currentTimeSlice, startPos, startPos);
        do {
            Vector3 lastPos = position;
            angel = startAngel;
            position = FindNextPosition(yVelocity, angularSpeed, ref angel, radius, gravity, distanceBetwinPoints, ref currentTimeSlice, lastPos, startPos);

            RaycastHit raycastHit;
            Vector3 direction = (position - lastPos);

            if (Physics.BoxCast(position, boxCollider.bounds.extents, direction.normalized, out raycastHit, Quaternion.Euler(0, 90 - angel, 0), direction.magnitude, layerMask)) {
                landingPoint = raycastHit.point;
                return true;
            }
        } while (currentTimeSlice < maxPredictionTime && startPos.y - position.y < maxPredictionYDelta);
        landingPoint = Vector3.zero;
        return false;
    }
}
