// 내 hp ui
using UnityEngine.UI;
using UnityEngine;

public class MyHp_Observer : MonoBehaviour, Observer
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
        this.hpBar.fillAmount = _myHp;
    }
}