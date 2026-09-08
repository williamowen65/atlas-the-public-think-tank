namespace Atlas.Participants.Participants;

/// <summary>Identifies a participant within the Participants boundary.</summary>
public sealed record ParticipantId
{
    public Guid Value { get; }

    /// <summary>Creates a validated participant id instance.</summary>
    public ParticipantId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "A participant ID cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new identifier.</summary>
    public static ParticipantId New()
    {
        return new ParticipantId(Guid.NewGuid());
    }

    /// <summary>Returns the identifier's display value.</summary>
    public override string ToString() => Value.ToString();
}
