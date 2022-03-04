using rwby;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class RRoseMan : FighterManager
{
    public override async UniTask<bool> OnFighterLoaded()
    {
        //TODO
        /*
        bool globalSnbkLoadResult = await ContentManager.singleton.LoadContentDefinition<ISoundbankDefinition>(new ModObjectReference("core", "global"));
        if (globalSnbkLoadResult == false) return false;

        bool rubyRoseSnbkLoadResult = await ContentManager.singleton.LoadContentDefinition<ISoundbankDefinition>(new ModObjectReference("core", "rr"));
        if (rubyRoseSnbkLoadResult == false) return false;


        bool globalEffectbankLoadResult = await ContentManager.singleton.LoadContentDefinition<IEffectbankDefinition>(new ModObjectReference("core", "global"));
        if (globalEffectbankLoadResult == false)
        {
            return false;
        }

        bool animationbankLoadResult = await ContentManager.singleton.LoadContentDefinition<IAnimationbankDefinition>(new ModObjectReference("core", "rr"));
        if (animationbankLoadResult == false) return false;*/

        return true;
    }

    public override void Awake()
    {
        base.Awake();
        /*
        ISoundbankDefinition globalSnbk = ContentManager.singleton.GetContentDefinition<ISoundbankDefinition>(new ModObjectReference("core", "global"));
        if (globalSnbk == null)
        {
            Debug.LogError("Error loading global soundbank.");
        }
        _ = globalSnbk.Load();
        soundbankContainer.AddSoundbank(globalSnbk.Name, globalSnbk);

        ISoundbankDefinition rubyRoseSnbk = ContentManager.singleton.GetContentDefinition<ISoundbankDefinition>(new ModObjectReference("core", "rr"));
        if (rubyRoseSnbk == null)
        {
            Debug.LogError("Error loading global soundbank.");
        }
        _ = rubyRoseSnbk.Load();
        soundbankContainer.AddSoundbank(rubyRoseSnbk.Name, rubyRoseSnbk);


        IEffectbankDefinition globalEffectbank = ContentManager.singleton.GetContentDefinition<IEffectbankDefinition>(new ModObjectReference("core", "global"));
        if (globalEffectbank == null)
        {
            Debug.LogError("Error loading global effectbank.");
        }
        effectbankContainer.AddEffectbank(globalEffectbank.Name, globalEffectbank);

        IAnimationbankDefinition rrAnbk = ContentManager.singleton.GetContentDefinition<IAnimationbankDefinition>(new ModObjectReference("core", "rr"));
        if (rrAnbk == null)
        {
            Debug.LogError("Error loading ruby rose animationbank.");
        }
        animationbankContainer.AddAnimationbank(rrAnbk.Name, rrAnbk);*/
    }

    public override void Spawned()
    {
        base.Spawned();
        FStateManager.ChangeState((int)FighterCmnStates.IDLE);
    }
}