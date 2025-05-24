using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class TransformBetweenPoints : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float duration = 2.0f;

    private bool isMoving = false;
    private float t = 0.0f;

    private bool hasMoved = false;
    
    public Animator _animator;
    public AnimatorController _idleState;
    public AnimatorController _walkingState;


    private void Start()
    {
        transform.position = pointA.position;
    }
    void Update()
    {
        if (isMoving)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(pointA.position, pointB.position, t);

            if (t >= 1.0f)
            {
                isMoving = false;
                t = 0.0f;
                _animator.runtimeAnimatorController = _idleState;
            }
        }
    }

    public void StartMoving()
    {
        Debug.Log("start moving called");
        if (!isMoving && !hasMoved)
        {
            transform.LookAt(pointB);
            isMoving = true;
            t = 0.0f;
            hasMoved = true;
            _animator.runtimeAnimatorController = _walkingState;
        
        }

      
    }
}
