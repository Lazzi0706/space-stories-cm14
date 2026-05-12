using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Shared._Stories.LeadershipWhistle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LeadershipWhistleComponent : Component
{
    [DataField, AutoNetworkedField]
    public int OrderAreaBuff = 3;

    [DataField, AutoNetworkedField]
    public FixedPoint2 MultiplierOrderBuff = 1.25;

    [DataField, AutoNetworkedField]
    public TimeSpan ActionOrderCooldownDebuff = TimeSpan.FromSeconds(120);

    [DataField(customTypeSerializer: typeof(DictionarySerializer<STOrderTypes, SoundSpecifier>)), AutoNetworkedField]
    public Dictionary<STOrderTypes, SoundSpecifier> OrderWhistleSounds = new();
}

public enum STOrderTypes
{
    Move,
    Hold,
    Focus
}
