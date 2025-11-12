// 적 hp ui
using UnityEngine.UI;
using UnityEngine;

public class EnemyHp_Observer : MonoBehaviour, Observer
{
    [SerializeField]
    private Image hpBar = null;

    // 옵저버는 멤버변수로 Subject를 가집니다.
    private Hp_Subject subject = null;

    public void Init(Hp_Subject _subject)
    {
        // Subject를 초기화해줍니다.
        this.subject = _subject;
    }
    public void ObserverUpdate(float _myHp, float _enemyHp)
    {
        // 새로 받은 정보를 갱신해줍니다.
        this.hpBar.fillAmount = _enemyHp;
    }
}

// 간단한 구조이기에 느슨한 결합에도 똑같은 코드가 작성되었지만 
// 좀만 형태가 복잡해지면 느슨한 결합의 가장큰 장점인 내부적으로는 달라도 
// 결과적인 역할은 동일한 코드를 볼 수 있게 됩니다.