using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class RewindManager : MonoBehaviour
{
    public int FramePerStepRewind => 2;
    public float RecordDuration => _recordDuration; 
    public static RewindManager Instance { get; private set; }
    
    [SerializeField] private float _recordDuration;
    
    private readonly List<RewindBodyBase> _trackedObjects = new ();
    public bool IsRewinding { get; private set; }

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
        Debug.Log(Time.timeScale);
        if (Input.GetKeyDown(KeyCode.R))
            StartRewind();
        else if (Input.GetKeyUp(KeyCode.R))
            StopRewind();
        else if (!IsRewinding &&  Input.anyKeyDown)
            SetTimeScale(1f);
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
        if (IsRewinding) return;
        IsRewinding = true;
        foreach (var rewindBodyBase in _trackedObjects)
            rewindBodyBase.StartRewind();
    }

    public void StopRewind()
    {
        if (!IsRewinding) return;
        IsRewinding = false;

        foreach (var rewindBodyBase in _trackedObjects)
            rewindBodyBase.StopRewind();
        
        SetTimeScale(0f);
    }

    private void SetTimeScale(float timescale)
    {
        Time.timeScale = timescale;
    }

    public bool IsGamePaused()
    {
        return Time.timeScale == 0;
    }
}