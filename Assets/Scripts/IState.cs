using UnityEngine;

public interface IState
{
    void OnEnter();

    void Execute(float timeStep);

    void OnExit();
}
