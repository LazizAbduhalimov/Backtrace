using System.Collections.Generic;
using Client.Game;
using UnityEngine;

public class RewindBodyBase : MonoBehaviour
{
    private List<RewindBodyData> _frames = new();
    private int _maxFrames;
    private bool _isRewinding;

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
        if (_isRewinding)
            RewindStep();
        else
            RecordStep();
    }

    private void RecordStep()
    {
        if (_frames.Count >= _maxFrames)
            _frames.RemoveAt(0);

        _frames.Add(new RewindBodyData
        {
            Position = transform.position,
            Rotation = transform.rotation
        });
    }

    private void RewindStep()
    {
        if (_frames.Count > 0)
        {
            var frame = _frames[^1];
            transform.position = frame.Position;
            transform.rotation = frame.Rotation;
            _frames.RemoveAt(_frames.Count - 1);
        }
        else
        {
            StopRewind();
        }
    }

    public virtual void StartRewind() => _isRewinding = true;

    public virtual void StopRewind() => _isRewinding = false;
}