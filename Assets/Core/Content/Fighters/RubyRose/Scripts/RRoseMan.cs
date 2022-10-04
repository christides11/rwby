using rwby;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RRoseMan : FighterManager
{
    public ModObjectSetContentReference[] animationbankReferences;
    public ModObjectSetContentReference[] effectbankReferences;
    public ModObjectSetContentReference[] soundbankReferences;
    public ModObjectSetContentReference[] projectilebankReferences;

    private ModGUIDContentReference[] animationbankRefs;
    private ModGUIDContentReference[] effectbankRefs;
    private ModGUIDContentReference[] soundbankRefs;
    private ModGUIDContentReference[] projectilebankRefs;
    
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
        
        for (int i = 0; i < soundbankReferences.Length; i++)
        {
            bool soundbankLoadResult = await ContentManager.singleton.LoadContentDefinition(ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference()
                {
                    contentGUID = soundbankReferences[i].contentGUID,
                    contentType = (int)ContentType.Soundbank,
                    modGUID = soundbankReferences[i].modGUID
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
            bool projectilebankLoadResult = await ContentManager.singleton.LoadContentDefinition(ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference()
                {
                    contentGUID = projectilebankReferences[i].contentGUID,
                    contentType = (int)ContentType.Projectilebank,
                    modGUID = projectilebankReferences[i].modGUID
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
        
        animationbankRefs = new ModGUIDContentReference[animationbankReferences.Length];
        effectbankRefs = new ModGUIDContentReference[effectbankReferences.Length];
        soundbankRefs = new ModGUIDContentReference[soundbankReferences.Length];
        projectilebankRefs = new ModGUIDContentReference[projectilebankReferences.Length];
        
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
            
            fighterEffector.RegisterBank(effectbankReferences[i]);
        }
        
        for (int i = 0; i < soundbankRefs.Length; i++)
        {
            soundbankRefs[i] = ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference()
                {
                    contentGUID = soundbankReferences[i].contentGUID,
                    contentType = (int)ContentType.Soundbank,
                    modGUID = soundbankReferences[i].modGUID
                }
            );
            
            fighterSounder.RegisterBank(soundbankReferences[i]);
        }
        
        for (int i = 0; i < projectilebankRefs.Length; i++)
        {
            projectilebankRefs[i] = ContentManager.singleton.ConvertModContentGUIDReference(new ModContentGUIDReference()
                {
                    contentGUID = projectilebankReferences[i].contentGUID,
                    contentType = (int)ContentType.Projectilebank,
                    modGUID = projectilebankReferences[i].modGUID
                }
            );
            
            projectileManager.RegisterBank(projectilebankReferences[i]);
        }
    }

    public override void Spawned()
    {
        base.Spawned();
        fighterWhiteboard.UpdateInt(0, WhiteboardModifyTypes.SET, 16); // Max bullets
        fighterWhiteboard.UpdateInt(1, WhiteboardModifyTypes.SET, fighterWhiteboard.Ints[0]); // Current Bullets
        fighterWhiteboard.UpdateInt(2, WhiteboardModifyTypes.SET,1); // Has Weapon
        fighterWhiteboard.UpdateInt(3, WhiteboardModifyTypes.SET,0); // Current Gundash
        FStateManager.ChangeState((int)FighterCmnStates.IDLE, 0);
    }
}