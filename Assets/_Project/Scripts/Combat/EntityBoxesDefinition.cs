using Fusion;

namespace rwby
{
    public struct EntityBoxesDefinition : INetworkStruct
    {
        [Networked, Capacity(10)] public NetworkArray<EntityBoxDefinition> hurtboxes => default;
        [Networked, Capacity(10)] public NetworkArray<EntityBoxDefinition> hitboxes => default;
        [Networked, Capacity(2)] public NetworkArray<EntityBoxDefinition> colboxes => default;
        [Networked, Capacity(1)] public NetworkArray<EntityBoxDefinition> throwableboxes => default;
        [Networked, Capacity(1)] public NetworkArray<EntityBoxDefinition> throwboxes => default;
    }
}