using rwby;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RRoseMan : FighterManager
{
    public override async UniTask<bool> OnFighterLoaded()
    {
        var temp = new ModObjectReference((1, 1), 1);
        bool animationbankLoadResult = await ContentManager.singleton.LoadContentDefinition<IAnimationbankDefinition>(temp);
        if (animationbankLoadResult == false)
        {
            Debug.LogError("Error loading animationbank.");
            return false;
        }
        return true;
    }

    public override void Awake()
    {
        base.Awake();
        var temp = new ModObjectReference((1, 1), 1);
        fighterAnimator.RegisterBank(temp);
    }

    public override void Spawned()
    {
        base.Spawned();
        FStateManager.ChangeState((int)FighterCmnStates.IDLE);
    }
}