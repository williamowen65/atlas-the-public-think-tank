using Atlas.Contracts.Graph.V1;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Atlas.Graph.Tests;

/// <summary>Verifies node behavior and boundary rules.</summary>
[TestClass]
public class NodeTests
{
    /// <summary>Verifies that constructor sets title.</summary>
    [TestMethod]
    public void Constructor_SetsTitle()
    {
        var node = CreateNode("Climate adaptation");

        Assert.AreEqual(
            new NodeTitle("Climate adaptation"),
            node.Title);
    }

    /// <summary>Verifies that constructor sets description reference.</summary>
    [TestMethod]
    public void Constructor_SetsDescriptionReference()
    {
        var descriptionId = new NodeDescriptionId(Guid.NewGuid());

        var node = new Node(
            new NodeTitle("Climate adaptation"),
            descriptionId,
            NodeTypeId.New(),
            new NodeAuthorId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        Assert.AreEqual(descriptionId, node.DescriptionId);
    }


    /// <summary>Verifies that constructor sets author reference.</summary>
    [TestMethod]
    public void Constructor_SetsAuthorReference()
    {
        var authorId = new NodeAuthorId(Guid.NewGuid());

        var node = new Node(
            new NodeTitle("Climate adaptation"),
            new NodeDescriptionId(Guid.NewGuid()),
            NodeTypeId.New(),
            authorId,
            DateTimeOffset.UtcNow);

        Assert.AreEqual(authorId, node.AuthorId);
    }

    /// <summary>Verifies that rename with valid title changes title.</summary>
    [TestMethod]
    public void Rename_WithValidTitle_ChangesTitle()
    {
        var node = CreateNode("Climate adaptation");

        node.Rename(
            new NodeTitle("Updated title"),
            DateTimeOffset.UtcNow);

        Assert.AreEqual(
            new NodeTitle("Updated title"),
            node.Title);
    }

    /// <summary>Verifies that rename with blank title throws argument exception.</summary>
    [TestMethod]
    public void Rename_WithBlankTitle_ThrowsArgumentException()
    {
        var node = CreateNode("Climate adaptation");

        Assert.Throws<ArgumentException>(
            () => node.Rename(
                new NodeTitle("    "),
                DateTimeOffset.UtcNow));

        Assert.AreEqual(
            new NodeTitle("Climate adaptation"),
            node.Title);
    }

    /// <summary>Verifies that replace description reference changes reference.</summary>
    [TestMethod]
    public void ReplaceDescriptionReference_ChangesReference()
    {
        var node = CreateNode("Climate adaptation");
        var replacementId =
            new NodeDescriptionId(Guid.NewGuid());

        node.ReplaceDescriptionReference(
            replacementId,
            DateTimeOffset.UtcNow);

        Assert.AreEqual(replacementId, node.DescriptionId);
    }


    /// <summary>Verifies that archive records node archived event.</summary>
    [TestMethod]
    public void Archive_RecordsNodeArchivedEvent()
    {
        var node = CreateNode("Climate adaptation");
        node.ClearDomainEvents();
        var archivedAt = DateTimeOffset.UtcNow;

        node.Archive(archivedAt);

        var domainEvent = node.DomainEvents
            .OfType<NodeArchivedV1>()
            .Single();

        Assert.AreEqual(node.Id.Value, domainEvent.NodeId);
        Assert.AreEqual(
            node.DescriptionId.Value,
            domainEvent.DescriptionId);
        Assert.AreEqual(node.AuthorId.Value, domainEvent.AuthorId);
        Assert.AreEqual(archivedAt, domainEvent.OccurredAt);
    }

    /// <summary>Verifies that archive when already archived does not record another event.</summary>
    [TestMethod]
    public void Archive_WhenAlreadyArchived_DoesNotRecordAnotherEvent()
    {
        var node = CreateNode("Climate adaptation");

        node.Archive(DateTimeOffset.UtcNow);
        node.ClearDomainEvents();

        node.Archive(DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.IsEmpty(node.DomainEvents);
    }


    /// <summary>Verifies that constructor sets requested sub node types.</summary>
    [TestMethod]
    public void Constructor_SetsRequestedSubNodeTypes()
    {
        var commentTypeId = NodeTypeId.New();
        var evidenceTypeId = NodeTypeId.New();

        var node = new Node(
            new NodeTitle("Climate adaptation"),
            new NodeDescriptionId(Guid.NewGuid()),
            NodeTypeId.New(),
            new NodeAuthorId(Guid.NewGuid()),
            [commentTypeId, evidenceTypeId],
            DateTimeOffset.UtcNow);

        CollectionAssert.AreEquivalent(
            new[] { commentTypeId, evidenceTypeId },
            node.RequestedSubNodeTypes
                .Select(request => request.TypeId)
                .ToArray());
    }

    /// <summary>Verifies that request sub node type does not add duplicate.</summary>
    [TestMethod]
    public void RequestSubNodeType_DoesNotAddDuplicate()
    {
        var node = CreateNode("Climate adaptation");
        var commentTypeId = NodeTypeId.New();

        node.RequestSubNodeType(
            commentTypeId,
            DateTimeOffset.UtcNow);

        node.RequestSubNodeType(
            commentTypeId,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.HasCount(1, node.RequestedSubNodeTypes);
    }

    /// <summary>Verifies that stop requesting sub node type removes request.</summary>
    [TestMethod]
    public void StopRequestingSubNodeType_RemovesRequest()
    {
        var node = CreateNode("Climate adaptation");
        var commentTypeId = NodeTypeId.New();

        node.RequestSubNodeType(
            commentTypeId,
            DateTimeOffset.UtcNow);

        node.StopRequestingSubNodeType(
            commentTypeId,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.IsEmpty(node.RequestedSubNodeTypes);
    }

    /// <summary>Verifies that request sub node type with empty guid throws.</summary>
    [TestMethod]
    public void RequestSubNodeType_WithEmptyGuid_Throws()
    {
        var node = CreateNode("Climate adaptation");

        Assert.Throws<ArgumentException>(
            () => node.RequestSubNodeType(
                new NodeTypeId(Guid.Empty),
                DateTimeOffset.UtcNow));
    }


    /// <summary>Verifies that attach to parent adds parent and records event.</summary>
    [TestMethod]
    public void AttachToParent_AddsParentAndRecordsEvent()
    {
        var node = CreateNode("Climate adaptation");
        node.ClearDomainEvents();
        var parentId = NodeId.New();
        var attachedAt = DateTimeOffset.UtcNow;

        node.AttachToParent(parentId, attachedAt);

        CollectionAssert.Contains(
            node.ParentNodeIds.ToList(),
            parentId);

        var domainEvent = node.DomainEvents
            .OfType<NodeParentAttachedV1>()
            .Single();

        Assert.AreEqual(node.Id.Value, domainEvent.NodeId);
        Assert.AreEqual(parentId.Value, domainEvent.ParentNodeId);
        Assert.AreEqual(attachedAt, domainEvent.OccurredAt);
    }

    /// <summary>Verifies that attach to parent when already attached does not duplicate.</summary>
    [TestMethod]
    public void AttachToParent_WhenAlreadyAttached_DoesNotDuplicate()
    {
        var node = CreateNode("Climate adaptation");
        var parentId = NodeId.New();

        node.AttachToParent(parentId, DateTimeOffset.UtcNow);
        node.ClearDomainEvents();

        node.AttachToParent(
            parentId,
            DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.HasCount(1, node.ParentNodeIds);
        Assert.IsEmpty(node.DomainEvents);
    }

    /// <summary>Verifies that attach to parent when parent is self throws.</summary>
    [TestMethod]
    public void AttachToParent_WhenParentIsSelf_Throws()
    {
        var node = CreateNode("Climate adaptation");

        Assert.Throws<InvalidOperationException>(
            () => node.AttachToParent(
                node.Id,
                DateTimeOffset.UtcNow));
    }

    /// <summary>Verifies that attach to parent with empty guid throws.</summary>
    [TestMethod]
    public void AttachToParent_WithEmptyGuid_Throws()
    {
        var node = CreateNode("Climate adaptation");

        Assert.Throws<ArgumentException>(
            () => node.AttachToParent(
                new NodeId(Guid.Empty),
                DateTimeOffset.UtcNow));
    }

    /// <summary>Verifies that detach from parent removes parent and records event.</summary>
    [TestMethod]
    public void DetachFromParent_RemovesParentAndRecordsEvent()
    {
        var node = CreateNode("Climate adaptation");
        var parentId = NodeId.New();
        node.AttachToParent(parentId, DateTimeOffset.UtcNow);
        node.ClearDomainEvents();
        var detachedAt = DateTimeOffset.UtcNow.AddMinutes(1);

        node.DetachFromParent(parentId, detachedAt);

        Assert.IsEmpty(node.ParentNodeIds);

        var domainEvent = node.DomainEvents
            .OfType<NodeParentDetachedV1>()
            .Single();

        Assert.AreEqual(parentId.Value, domainEvent.ParentNodeId);
        Assert.AreEqual(detachedAt, domainEvent.OccurredAt);
    }

    /// <summary>Verifies that constructor allows multiple parents.</summary>
    [TestMethod]
    public void Constructor_AllowsMultipleParents()
    {
        var firstParentId = NodeId.New();
        var secondParentId = NodeId.New();

        var node = CreateNode("Relationship");

        node.AttachToParent(
            firstParentId,
            DateTimeOffset.UtcNow);
        node.AttachToParent(
            secondParentId,
            DateTimeOffset.UtcNow.AddMinutes(1));

        CollectionAssert.AreEquivalent(
            new[] { firstParentId, secondParentId },
            node.ParentNodeIds.ToArray());
    }

    /// <summary>Creates node during the current workflow.</summary>
    private static Node CreateNode(string title)
    {
        return new Node(
            new NodeTitle(title),
            new NodeDescriptionId(Guid.NewGuid()),
            NodeTypeId.New(),
            new NodeAuthorId(Guid.NewGuid()),
            DateTimeOffset.UtcNow);
    }
}
