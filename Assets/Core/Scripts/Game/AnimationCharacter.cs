using System;
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public class AnimationCharacter : MonoBehaviour
    {
        private Animator Animator;
        public NavMeshAgent NavMeshAgent;

        private void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            Animator.SetFloat("Speed", NavMeshAgent.velocity.sqrMagnitude);
        }
    }
}