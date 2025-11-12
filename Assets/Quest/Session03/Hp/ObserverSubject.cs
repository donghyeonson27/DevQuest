// Subject 인터페이스
public interface Subject
{
    // Observer 등록
    void RegisterObserver(Observer _observer);
    // Observer 해제
    void RemoveObserver(Observer _observer);
    // 모든 Observer 업데이트
    void NotifyObservers();
}

// Observer 인터페이스
public interface Observer
{
    // 정보 갱신 및 초기화
    void ObserverUpdate(float _myHp, float _enemyHp);
}