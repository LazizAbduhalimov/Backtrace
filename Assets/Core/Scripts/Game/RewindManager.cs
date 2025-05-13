using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
    public float RecordDuration => _recordDuration; 
    public static RewindManager Instance { get; private set; }
    
    [SerializeField] private float _recordDuration;
    
    private readonly List<RewindBodyBase> _trackedObjects = new ();
    private bool _isRewinding;

    public int RewindLength => _trackedObjects.First().FramesLeft;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            StartRewind();
        else if (Input.GetKeyUp(KeyCode.R))
            StopRewind();
    }

    public void Register(RewindBodyBase obj)
    {
        if (!_trackedObjects.Contains(obj))
            _trackedObjects.Add(obj);
    }

    public void Unregister(RewindBodyBase obj)
    {
        if (_trackedObjects.Contains(obj))
            _trackedObjects.Remove(obj);
    }

    public void StartRewind()
    {
        if (_isRewinding) return;
        _isRewinding = true;

        foreach (var rewindBodyBase in _trackedObjects)
            rewindBodyBase.StartRewind();
    }

    public void StopRewind()
    {
        if (!_isRewinding) return;
        _isRewinding = false;

        foreach (var rewindBodyBase in _trackedObjects)
            rewindBodyBase.StopRewind();
    }
}