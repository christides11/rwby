using rwby;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RRoseMan : FighterManager
{
    public ModObjectGUIDReference[] animationbankReferences;
    public ModObjectSetContentReference[] effectbankReferences;
    public override async UniTask<bool> OnFighterLoaded()
    {
        for (int i = 0; i < animationbankReferences.Length; i++)
        {
            bool animationbankLoadResult = await ContentManager.singleton.LoadContentDefinition(animationbankReferences[i]);
            if (animationbankLoadResult == false)
            {
                Debug.LogError("Error loading animationbank.");
                return false;
            }
        }
        
        for (int i = 0; i < effectbankReferences.Length; i++)
        {
            bool animationbankLoadResult = await ContentManager.singleton.LoadContentDefinition(
                new ModObjectGUIDReference()
                {
                    contentGUID = effectbankReferences[i].contentGUID,
                    contentType = (int)ContentType.Effectbank,
                    modGUID =  effectbankReferences[i].modGUID
                });
            if (animationbankLoadResult == false)
            {
                Debug.LogError("Error loading effectbank.");
                return false;
            }
        }
        return true;
    }

    public override void Awake()
    {
        base.Awake();

        for (int i = 0; i < animationbankReferences.Length; i++)
        {
            fighterAnimator.RegisterBank(animationbankReferences[i]);
        }
        
        for (int i = 0; i < effectbankReferences.Length; i++)
        {
            fighterEffector.RegisterBank(new ModObjectGUIDReference()
            {
                contentGUID = effectbankReferences[i].contentGUID,
                contentType = (int)ContentType.Effectbank,
                modGUID =  effectbankReferences[i].modGUID
            });
        }
    }

    public override void Spawned()
    {
        base.Spawned();
        FStateManager.ChangeState((int)FighterCmnStates.IDLE);
    }
}