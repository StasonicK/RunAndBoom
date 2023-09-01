using System;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class AgentMoveToHero : Follow
    {
        [SerializeField] private NavMeshAgent _agent;

        private Transform _heroTransform;
        private bool _move;
        private bool _isMovable;

        protected override void OnEnabled() =>
            _agent.enabled = true;

        protected override void OnDisabled() =>
            _agent.enabled = false;

        protected override void Run() =>
            SetDestinationForAgent();

        public void Construct(Transform heroTransform, float speed, bool isMovable)
        {
            _isMovable = isMovable;
            _heroTransform = heroTransform;
            _agent.speed = speed;
            _agent.enabled = false;
        }

        private void SetDestinationForAgent()
        {
            if (_heroTransform && _agent != null && _isMovable)
            {
                if (_move && _agent.enabled)
                {
                    try
                    {
                        _agent.destination = _heroTransform.position;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Debug.Log($"SetDestinationForAgent error: {e}");
                        throw;
                    }
                }
            }
        }

        public override void Move()
        {
            _move = true;
            _agent.enabled = true;
        }

        public override void Stop()
        {
            _move = false;
            _agent.enabled = false;
        }
    }
}