using System.Collections.Generic;
using Client;
using Client.Game;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
    public static RewindManager Instance { get; private set; }

    private List<RewindBodyMb> trackedObjects = new ();
    private bool isRewinding = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Update()
    {
        // Включить/отключить перемотку на клавишу R (можно заменить)
        if (Input.GetKeyDown(KeyCode.R))
            StartRewind();
        else if (Input.GetKeyUp(KeyCode.R))
            StopRewind();
    }

    public void Register(RewindBodyMb obj)
    {
        if (!trackedObjects.Contains(obj))
            trackedObjects.Add(obj);
    }

    public void Unregister(RewindBodyMb obj)
    {
        if (trackedObjects.Contains(obj))
            trackedObjects.Remove(obj);
    }

    public void StartRewind()
    {
        if (isRewinding) return;
        isRewinding = true;

        foreach (var obj in trackedObjects)
            obj.StartRewind();
    }

    public void StopRewind()
    {
        if (!isRewinding) return;
        isRewinding = false;

        foreach (var obj in trackedObjects)
            obj.StopRewind();
    }
}