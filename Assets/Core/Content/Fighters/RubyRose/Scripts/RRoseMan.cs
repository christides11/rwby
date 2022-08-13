using rwby;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RRoseMan : FighterManager
{
    public ModObjectSetContentReference[] animationbankReferences;
    public ModObjectSetContentReference[] effectbankReferences;

    private ModGUIDContentReference[] animationbankRefs;
    private ModGUIDContentReference[] effectbankRefs;
    
    public override async UniTask<bool> OnFighterLoaded()
    {
        for (int i = 0; i < animationbankReferences.Length; i++)
        {
            bool animationbankLoadResult = await ContentManager.singleton.LoadContentDefinition(ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference()
                {
                    contentGUID = animationbankReferences[i].contentGUID,
                    contentType = (int)ContentType.Animationbank,
                    modGUID = animationbankReferences[i].modGUID
                }
            ));
            if (animationbankLoadResult == false)
            {
                Debug.LogError("Error loading animationbank.");
                return false;
            }
        }
        
        for (int i = 0; i < effectbankReferences.Length; i++)
        {
            bool animationbankLoadResult = await ContentManager.singleton.LoadContentDefinition(ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference()
                {
                    contentGUID = effectbankReferences[i].contentGUID,
                    contentType = (int)ContentType.Effectbank,
                    modGUID = effectbankReferences[i].modGUID
                }
            ));
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
        
        animationbankRefs = new ModGUIDContentReference[animationbankReferences.Length];
        effectbankRefs = new ModGUIDContentReference[effectbankReferences.Length];
        
        for (int i = 0; i < animationbankRefs.Length; i++)
        {
            animationbankRefs[i] = ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference()
                {
                    contentGUID = animationbankReferences[i].contentGUID,
                    contentType = (int)ContentType.Animationbank,
                    modGUID = animationbankReferences[i].modGUID
                }
            );
            
            fighterAnimator.RegisterBank(animationbankReferences[i]);
        }
        
        for (int i = 0; i < effectbankRefs.Length; i++)
        {
            effectbankRefs[i] = ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference()
                {
                    contentGUID = effectbankReferences[i].contentGUID,
                    contentType = (int)ContentType.Effectbank,
                    modGUID = effectbankReferences[i].modGUID
                }
            );
            
            fighterEffector.RegisterBank(effectbankRefs[i]);
        }
    }

    public override void Spawned()
    {
        base.Spawned();
        FStateManager.ChangeState((int)FighterCmnStates.IDLE, 0);
    }
}