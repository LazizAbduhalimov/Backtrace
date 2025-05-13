using System.Collections.Generic;
using Client.Game;
using UnityEngine;
using UnityEngine.AI;

public class RewindBodyMb : MonoBehaviour
{
    [SerializeField] private float recordDuration = 5f;
    private List<RewindBodyData> frames = new();
    private int maxFrames;
    private bool isRewinding = false;

    private NavMeshAgent agent; // может быть null

    private void Start()
    {
        maxFrames = Mathf.RoundToInt(recordDuration / Time.fixedDeltaTime);
        RewindManager.Instance.Register(this);

        agent = GetComponent<NavMeshAgent>(); // если его нет — останется null
    }

    private void FixedUpdate()
    {
        if (isRewinding)
            RewindStep();
        else
            RecordStep();
    }

    private void RecordStep()
    {
        if (frames.Count >= maxFrames)
            frames.RemoveAt(0);

        frames.Add(new RewindBodyData
        {
            Position = transform.position,
            Rotation = transform.rotation
        });
    }

    private void RewindStep()
    {
        if (frames.Count > 0)
        {
            var frame = frames[^1];
            transform.position = frame.Position;
            transform.rotation = frame.Rotation;
            frames.RemoveAt(frames.Count - 1);
        }
        else
        {
            StopRewind();
        }
    }

    public void StartRewind()
    {
        isRewinding = true;

        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
            Debug.Log("Rewinding");
        }
    }

    public void StopRewind()
    {
        isRewinding = false;

        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = false;
            Debug.Log("Stopped rewinding");
        }
    }
}