using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour 
{
    public static Monster main;

    //HideInInspector를 통해 playercontrol, cam, camc는 초기화 과정 중 
    //자연스럽게 설정되며, 인스펙터를 통해 설정할 필요가 없다는 점을 
    //(동료 개발자나 미래의 나 자신에게) 알려줍니다.
    [HideInInspector] public player player;
    [HideInInspector] public Camera cam;
    [HideInInspector] public CameraControl camc;

    public Transform playerSpawn;
    public GameObject[] dialogs = { };

    private void Awake()
    {
        //싱글톤 설정
        main = this;
        //싱글톤 초기화
        player = GameObject.FindObjectOfType<player>();
        cam = Camera.main;
        camc = GameObject.FindObjectOfType<CameraControl>();
    }

    //싱글톤의 메소드
    public bool DialogOpen()
    {
        foreach (GameObject dialog in dialogs)
        {
            if (dialog.activeInHierarchy) return true;
        }
        return false;
    }

    public Transform target;

    NavMeshAgent nmAgent;

    private void Start()
    {
        nmAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //타겟이 없으면 추적 안 함 = 늘 따라오도록
        if (!target) return;

        //타겟과 병아리 사이의 방향 벡터 생성
        Vector3 to = target.position - transform.position;

        // 앞에 10M 정면 40도 이내에 있을 때에만 오도록
        if (to.sqrMagnitude <= 10f * 5f && Vector3.Angle(transform.forward, to) <= 40f)
        {
            nmAgent.isStopped = false;                 // 혹시 멈춰 있었으면 해제
            nmAgent.SetDestination(target.position);   // 목적지 갱신
        }
    }
}
