using System;
using System.Collections.Generic;
using Client.Game;
using UnityEngine;

public class RewindBodyBase : MonoBehaviour
{
    public Animator Animator;
    public int FramesLeft => _frames.Count;
    private List<RewindBodyData> _frames = new();
    private int _maxFrames;
    public bool IsRewinding;

    private void OnValidate()
    {
        if (Animator == null)
            Animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        _maxFrames = Mathf.RoundToInt(RewindManager.Instance.RecordDuration / Time.fixedDeltaTime);
        RewindManager.Instance.Register(this);
    }

    private void FixedUpdate()
    {
        RewindCore();
    }

    protected virtual void RewindCore()
    {
        if (IsRewinding)
        {
            for (var r = 0; r < RewindManager.Instance.FramePerStepRewind; r++)
            {
                RewindStep();
            }
        }
        else
            RecordStep();
    }

    protected void RecordStep()
    {
        if (_frames.Count >= _maxFrames)
            _frames.RemoveAt(0);

        var rewindData = new RewindBodyData
        {
            Position = transform.position,
            Rotation = transform.rotation
        };

        if (Animator == null)
        {
            _frames.Add(rewindData);
            return;
        };
        
        var snapshot = new AnimatorSnapshot();
        if (Animator.IsInTransition(0))
        {
            var transition = Animator.GetNextAnimatorStateInfo(0);
            snapshot = new AnimatorSnapshot
            {
                stateHash = transition.fullPathHash,
                normalizedTime = transition.normalizedTime,
                isTransitioning = true
            };
        }
        else 
        {
            var state = Animator.GetCurrentAnimatorStateInfo(0);
            snapshot = new AnimatorSnapshot
            {
                stateHash = state.fullPathHash,
                normalizedTime = state.normalizedTime,
                isTransitioning = false
            };
        }

        rewindData.AnimatorSnapshot = snapshot;
        _frames.Add(rewindData);
    }

    protected void RewindStep()
    {
        if (_frames.Count > 0)
        {
            var frame = _frames[^1];
            transform.position = frame.Position;
            transform.rotation = frame.Rotation;
            if (frame.AnimatorSnapshot.HasValue)
            {
                var snapshot = frame.AnimatorSnapshot.Value;
                Animator.Play(snapshot.stateHash, 0, snapshot.normalizedTime);
                Animator.Update(0f); // Принудительно применяем новое состояние сразу   
            }
            _frames.RemoveAt(_frames.Count - 1);

        }
        else
        {
            StopRewind();
            RewindManager.Instance.StopRewind();
        }
    }

    public virtual void StartRewind() => IsRewinding = true;

    public virtual void StopRewind() => IsRewinding = false;
}

[System.Serializable]
public struct AnimatorSnapshot
{
    public int stateHash;
    public float normalizedTime;
    public bool isTransitioning;
}
