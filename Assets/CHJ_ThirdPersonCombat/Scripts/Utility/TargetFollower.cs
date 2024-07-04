using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollower : MonoBehaviour
{
    [SerializeField]private Transform _target;
    [SerializeField]private Vector3 _offset;

    public void SetTarget(Transform target)
    {
        _target = target;
        _offset = transform.position - _target.position;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        transform.position = _target.position + _offset;
    }
}
