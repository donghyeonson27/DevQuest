using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 체력을 관리해주는 주제 Subject
public class Hp_Subject : MonoBehaviour, Subject
{
    // 등록된 Observer들을 관리할 리스트
    private List<Observer> observers = new List<Observer>();

    private float myHp = 0f;
    private float enermyHp = 0f;

    public void RegisterObserver(Observer _observer)
    {
        // Observer 등록
        this.observers.Add(_observer);
    }
    public void RemoveObserver(Observer _observer)
    {
        // Observer 해제
        this.observers.Remove(_observer);
    }
    public void NotifyObservers()
    {
        // 모든 Observer 정보 업데이트
        for (int i = 0; i < this.observers.Count; i++)
        {
            // 각 Observer들이 보여줘야할 정보를 전부 매개변수로 가집니다. 
            // 매개변수에 모든 정보를 가지지않게끔 개선할 수도 있습니다. 
            // 중요한건 모든 옵저버가 갱신된다는 정의에서 벗어나지 않으면 됩니다.
            this.observers[i].ObserverUpdate(this.myHp, this.enermyHp);
        }
    }

    public void Changed(float _myHp, float _enemyHp)
    {
        // 정보가 변경되면 호출되어
        this.myHp = _myHp; this.enermyHp = _enemyHp;
        // 업데이트된 정보로 갱신해줍니다.
        this.NotifyObservers();
    }
}