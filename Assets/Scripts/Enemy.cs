using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{

    [Header("Preset Fields")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject splashFx;

    [Header("Settings")]
    [SerializeField] private float attackRange;

    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float wanderSpeed = 2.5f;
    [SerializeField] private float repathInterval = 2f;

    [SerializeField] private Collider targetCollider;
    private bool collidedWithTarget;

    private NavMeshAgent agent;
    private Vector3 wanderTarget;
    private float repathTimer;


    public enum State
    {
        None,
        Idle,
        Attack,
        Wander,
    }

    [Header("Debug")]
    public State state = State.None;
    public State nextState = State.None;

    private bool attackDone;

    private void Start()
    {

        // 상태 초기화
        state = State.None;
        nextState = State.Idle;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = wanderSpeed;
            agent.stoppingDistance = 0.1f;
            agent.updateRotation = true;
            agent.updateUpAxis = true;
        }

    }


    private void Update()
    {
        //1. 스테이트 전환 상황 판단
        if (nextState == State.None)
        {
            switch (state)
            {
                case State.Idle:
                    //1 << 6인 이유는 Player의 Layer가 6이기 때문
                    if (Physics.CheckSphere(transform.position, attackRange, 1 << 6, QueryTriggerInteraction.Ignore))
                    {
                        nextState = State.Attack;
                    }
                    break;
                case State.Attack:
                    if (attackDone)
                    {
                        nextState = State.Idle;
                        attackDone = false;
                    }
                    break;
                case State.Wander:
                    if (collidedWithTarget)
                    {
                        nextState = State.Attack;
                        collidedWithTarget = false;
                    }
                    break;

            }
        }

        //2. 스테이트 초기화
        if (nextState != State.None)
        {
            state = nextState;
            nextState = State.None;
            switch (state)
            {
                case State.Idle:
                    if (agent) agent.ResetPath();
                    break;
                case State.Attack:
                    if (agent) agent.ResetPath();
                    Attack();
                    break;
                case State.Wander:
                    BeginWander();
                    break;
                    //insert code here...
            }
        }

        //3. 글로벌 & 스테이트 업데이트
        //insert code here...
        switch (state)
        {
            case State.Wander:
                TickWander();
                break;
        }
    }

    private void Attack() //현재 공격은 애니메이션만 작동합니다.
    {
        animator.SetTrigger("attack");
    }

    public void InstantiateFx() //Unity Animation Event 에서 실행됩니다.
    {
        Instantiate(splashFx, transform.position, Quaternion.identity);
    }

    public void WhenAnimationDone() //Unity Animation Event 에서 실행됩니다.
    {
        attackDone = true;
    }

    private void BeginWander()
    {
        repathTimer = 0f;

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.speed = wanderSpeed;
            PickNewWanderPoint();
        }
        else
        {
            wanderTarget = PickPointSimple();
        }
    }

    private void TickWander()
    {
        repathTimer += Time.deltaTime;

        if (agent != null && agent.isActiveAndEnabled)
        {
            if (agent.destination != wanderTarget)
                agent.SetDestination(wanderTarget);

            bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f;
            bool stuck = repathTimer >= repathInterval && agent.velocity.sqrMagnitude < 0.01f;

            if (arrived || stuck)
                PickNewWanderPoint();
        }
        else
        {
            Vector3 to = (wanderTarget - transform.position);
            if (to.sqrMagnitude < 0.25f)
                wanderTarget = PickPointSimple();
            else
            {
                Vector3 dir = to.normalized;
                transform.position += dir * wanderSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
            }
        }
    }

    private void PickNewWanderPoint()
    {
        Vector3 random = Random.insideUnitSphere * wanderRadius + transform.position;
        if (NavMesh.SamplePosition(random, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            wanderTarget = hit.position;
            agent.SetDestination(wanderTarget);
        }
        else
        {
            wanderTarget = PickPointSimple();
            if (agent) agent.SetDestination(wanderTarget);
        }
    }

    private Vector3 PickPointSimple()
    {
        Vector3 p = Random.insideUnitSphere * wanderRadius + transform.position;
        p.y = transform.position.y;
        return p;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (targetCollider != null &&
            (collision.collider == targetCollider || collision.gameObject == targetCollider.gameObject))
        {
            collidedWithTarget = true;
            if (agent) agent.ResetPath();
            return;
        }

        nextState = State.Wander;
        if (agent) agent.ResetPath();
    }


    private void OnDrawGizmosSelected()
    {
        //Gizmos를 사용하여 공격 범위를 Scene View에서 확인할 수 있게 합니다. (인게임에서는 볼 수 없습니다.)
        //해당 함수는 없어도 기능 상의 문제는 없지만, 기능 체크 및 디버깅을 용이하게 합니다.
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, attackRange);
    }
}
