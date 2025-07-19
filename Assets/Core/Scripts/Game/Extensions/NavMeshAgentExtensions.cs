using UnityEngine.AI;

namespace Game
{
    public static class NavMeshAgentExtensions
    {
        public static bool IsMoving(this NavMeshAgent agent)
        {
            return agent.velocity.sqrMagnitude > 0.01f &&
                   agent.remainingDistance > agent.stoppingDistance &&
                   !agent.pathPending;
        }

    }
}