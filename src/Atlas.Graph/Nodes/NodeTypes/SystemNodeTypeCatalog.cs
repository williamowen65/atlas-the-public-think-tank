namespace Atlas.Graph.Nodes.NodeTypes;

/// <summary>Built-in Graph type policy, invoked explicitly by host startup.</summary>
public static class SystemNodeTypeCatalog
{
    public static void Ensure(
        INodeTypeRepository nodeTypes)
    {
        var existingTypes = nodeTypes.GetAll();
        var createdAt = AtlasTime.UtcNow;

        var systemTypes = new[]
        {
            ("Issue", "A problem or concern to investigate.", true),
            ("Question", "A question that invites answers.", true),
            ("Idea", "A proposed concept or possibility.", true),
            ("Solution", "A proposed response to a problem.", true),
            ("Evidence", "Information supporting or challenging a claim.", false),
            ("Relationship", "A connection involving multiple nodes.", true),
            ("Location", "A place associated with another node.", true)
        };

        foreach (var (name, description, autoPluralize) in systemTypes)
        {
            var existingType = existingTypes.SingleOrDefault(type =>
                string.Equals(
                    type.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase
                ));

            if (existingType is not null)
            {
                existingType.ChangeAutoPluralize(
                    autoPluralize,
                    actorId: "system",
                    actorIsModerator: true,
                    changedAt: createdAt
                );

                nodeTypes.Save(existingType);
                continue;
            }

            nodeTypes.Save(
                NodeTypeDefinition.CreateSystemDefined(
                    name,
                    description,
                    createdAt,
                    autoPluralize
                ));
        }
    }

}
