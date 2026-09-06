namespace Atlas.Participants.Participants;

public sealed record ParticipantId
{
    public Guid Value { get; }

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

    public static ParticipantId New()
    {
        return new ParticipantId(Guid.NewGuid());
    }

    public override string ToString() => Value.ToString();
}
