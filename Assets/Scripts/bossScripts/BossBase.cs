using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class BossBase
{
    public abstract void enterState(BossManager e);
    public abstract void updateState(BossManager e);
    public abstract void OnCollsionEnter(BossManager e);
}



