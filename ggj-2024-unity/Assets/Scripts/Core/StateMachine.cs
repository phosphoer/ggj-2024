using System.Collections.Generic;

[System.Serializable]
public class StateMachine<T>
{
  public event System.Action StateUpdate;
  public event System.Action StateEnter;
  public event System.Action StateExit;

  public T CurrentState => _currentState;
  public T LastState => _lastState;
  public float CurrentStateTime => _stateTimer;

  private T _currentState;
  private T _lastState;
  private T _nextState;
  private float _stateTimer;
  private bool _pendingChange;

  public void Update(float dt)
  {
    if (_pendingChange)
    {
      StateExit?.Invoke();
      _lastState = _currentState;
      _currentState = _nextState;
      _stateTimer = 0;
      _pendingChange = false;
      StateEnter?.Invoke();
    }

    _stateTimer += dt;
    StateUpdate?.Invoke();
  }

  public void GoToState(T nextState)
  {
    _nextState = nextState;
    _pendingChange = true;
  }
}