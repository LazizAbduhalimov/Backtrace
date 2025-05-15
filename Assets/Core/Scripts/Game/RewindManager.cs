using System;
using System.Collections.Generic;
using System.Linq;
using Client.Game;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class RewindManager : MonoBehaviour
{
    private AudioSource _audioSource;
    public float PitchSpeed = 1.5f;
    public bool GameOver;
    public int FramePerStepRewind => 2;
    public float RecordDuration => _recordDuration; 
    public static RewindManager Instance { get; private set; }
    
    [SerializeField] private float _recordDuration;
    
    private readonly List<RewindBodyBase> _trackedObjects = new ();
    public bool IsRewinding { get; private set; }

    public int RewindLength => _trackedObjects.First().FramesLeft;

    private Tween? _tween;
    private bool _afterRewind;

    private void Awake()
    {
        _audioSource = GameObject.Find("MainTheme").GetComponentInChildren<AudioSource>();
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Update()
    {
        if (GameOver) return;
        if (Input.GetKeyDown(KeyCode.R))
            StartRewind();
        else if (Input.GetKeyUp(KeyCode.R))
            StopRewind();
        else if (!IsRewinding && Input.anyKeyDown && !Player.IsDead)
        {
            SetTimeScale(1f);
            ChangePitch(1f);
        }
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
        ChangePitch(2.5f);
        SetTimeScale(1f);
        foreach (var rewindBodyBase in _trackedObjects)
        {
            rewindBodyBase.StartRewind();
        }
    }

    private void ChangePitch(float toPitch)
    {
        var fromPitch = _audioSource.pitch;
        var duration = Mathf.Abs(toPitch - fromPitch) / PitchSpeed;
        _tween?.Complete();
        _tween = Tween.Custom(_audioSource.pitch, toPitch, 0.25f,
            value => _audioSource.pitch = value, Ease.OutSine, useUnscaledTime: true);
    }

    public void StopRewind()
    {
        if (!IsRewinding) return;
        IsRewinding = false;
        ChangePitch(0.3f);
        foreach (var rewindBodyBase in _trackedObjects)
            rewindBodyBase.StopRewind();
        
        SetTimeScale(0f);
    }

    public void SetTimeScale(float timescale)
    {
        Time.timeScale = timescale;
    }

    public bool IsGamePaused()
    {
        return Time.timeScale == 0;
    }
}