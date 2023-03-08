using System.Collections.Generic;
using rwby;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

public class RRoseMan : FighterManager
{
    public SharedModSetContentReference[] animationbankReferences;
    public SharedModSetContentReference[] effectbankReferences;
    public SharedModSetContentReference[] soundbankReferences;
    public SharedModSetContentReference[] projectilebankReferences;

    private ModIDContentReference[] animationbankRefs;
    private ModIDContentReference[] effectbankRefs;
    private ModIDContentReference[] soundbankRefs;
    private ModIDContentReference[] projectilebankRefs;

    [Networked] public NetworkObject currentScythe { get; set; }

    public float scytheCatchDistance = 3.0f;
    
    public override async UniTask<bool> OnFighterLoaded()
    {
        for (int i = 0; i < animationbankReferences.Length; i++)
        {
            bool animationbankLoadResult = await ContentManager.singleton.LoadContentDefinition(ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference()
                {
                    contentGUID = animationbankReferences[i].reference.contentGUID,
                    contentType = ContentType.Animationbank,
                    modGUID = animationbankReferences[i].reference.modGUID
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
            bool animationbankLoadResult = await ContentManager.singleton.LoadContentDefinition(ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference()
                {
                    contentGUID = effectbankReferences[i].reference.contentGUID,
                    contentType = ContentType.Effectbank,
                    modGUID = effectbankReferences[i].reference.modGUID
                }
            ));
            if (animationbankLoadResult == false)
            {
                Debug.LogError("Error loading effectbank.");
                return false;
            }
        }
        
        for (int i = 0; i < soundbankReferences.Length; i++)
        {
            bool soundbankLoadResult = await ContentManager.singleton.LoadContentDefinition(ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference()
                {
                    contentGUID = soundbankReferences[i].reference.contentGUID,
                    contentType = ContentType.Soundbank,
                    modGUID = soundbankReferences[i].reference.modGUID
                }
            ));
            if (soundbankLoadResult == false)
            {
                Debug.LogError("Error loading soundbanks.");
                return false;
            }
        }
        
        for (int i = 0; i < projectilebankReferences.Length; i++)
        {
            bool projectilebankLoadResult = await ContentManager.singleton.LoadContentDefinition(ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference()
                {
                    contentGUID = projectilebankReferences[i].reference.contentGUID,
                    contentType = ContentType.Projectilebank,
                    modGUID = projectilebankReferences[i].reference.modGUID
                }
            ));
            if (projectilebankLoadResult == false)
            {
                Debug.LogError("Error loading projectilebanks.");
                return false;
            }
        }

        return true;
    }

    public override void Awake()
    {
        base.Awake();
        
        animationbankRefs = new ModIDContentReference[animationbankReferences.Length];
        effectbankRefs = new ModIDContentReference[effectbankReferences.Length];
        soundbankRefs = new ModIDContentReference[soundbankReferences.Length];
        projectilebankRefs = new ModIDContentReference[projectilebankReferences.Length];
        
        for (int i = 0; i < animationbankRefs.Length; i++)
        {
            animationbankRefs[i] = ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference()
                {
                    contentGUID = animationbankReferences[i].reference.contentGUID,
                    contentType = ContentType.Animationbank,
                    modGUID = animationbankReferences[i].reference.modGUID
                }
            );
            
            fighterAnimator.RegisterBank(animationbankReferences[i].reference);
        }
        
        for (int i = 0; i < effectbankRefs.Length; i++)
        {
            effectbankRefs[i] = ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference()
                {
                    contentGUID = effectbankReferences[i].reference.contentGUID,
                    contentType = ContentType.Effectbank,
                    modGUID = effectbankReferences[i].reference.modGUID
                }
            );
            
            fighterEffector.RegisterBank(effectbankReferences[i].reference);
        }
        
        for (int i = 0; i < soundbankRefs.Length; i++)
        {
            soundbankRefs[i] = ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference()
                {
                    contentGUID = soundbankReferences[i].reference.contentGUID,
                    contentType = ContentType.Soundbank,
                    modGUID = soundbankReferences[i].reference.modGUID
                }
            );
            
            fighterSounder.RegisterBank(soundbankReferences[i].reference);
        }
        
        for (int i = 0; i < projectilebankRefs.Length; i++)
        {
            projectilebankRefs[i] = ContentManager.singleton.ConvertStringToGUIDReference(new ModContentStringReference()
                {
                    contentGUID = projectilebankReferences[i].reference.contentGUID,
                    contentType = ContentType.Projectilebank,
                    modGUID = projectilebankReferences[i].reference.modGUID
                }
            );
            
            projectileManager.RegisterBank(projectilebankReferences[i].reference);
        }
    }

    int boff;
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (DisableUpdate) return;
        if (stateManager.CurrentMoveset == 2 && fighterWhiteboard.Ints[5] == 1 && inputManager.GetC(out boff, 0, 3).firstPress)
        {
            if (!currentScythe) return;
            if (Vector3.Distance(transform.position, currentScythe.transform.position) < scytheCatchDistance)
            {
                inputManager.ClearBuffer();
                stateManager.SetMoveset(0);
                stateManager.MarkForStateChange(physicsManager.IsGroundedNetworked ? (int)FighterCmnStates.IDLE : (int)FighterCmnStates.FALL);
                Runner.Despawn(currentScythe);
                currentScythe = null;
            }
        }
    }

    public override void Spawned()
    {
        base.Spawned();
    }

    public override void HandleDeath()
    {
        base.HandleDeath();
    }

    public override void HandleRespawn()
    {
        base.HandleRespawn();
        fighterWhiteboard.UpdateInt(0, WhiteboardModifyTypes.SET, 16); // Max bullets
        fighterWhiteboard.UpdateInt(1, WhiteboardModifyTypes.SET, fighterWhiteboard.Ints[0]); // Current Bullets
        fighterWhiteboard.UpdateInt(2, WhiteboardModifyTypes.SET,1); // Has Weapon
        fighterWhiteboard.UpdateInt(3, WhiteboardModifyTypes.SET,0); // Current Gundash
        fighterWhiteboard.UpdateInt(4, WhiteboardModifyTypes.SET,3); // Gundash Max
        fighterWhiteboard.UpdateInt(5, WhiteboardModifyTypes.SET,0); // Can Grab Scythe
        FStateManager.ChangeState((int)FighterCmnStates.IDLE, 0);
    }

    public override List<ModIDContentReference> GetLoadedContentList()
    {
        return base.GetLoadedContentList();
    }
}