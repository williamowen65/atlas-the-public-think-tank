using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Participants.Participants;
using Atlas.Participants.Profiles;

namespace Atlas.ConsoleApp.Participants;

public static class ParticipantCommands
{
    public static Participant Run(
        ParticipantId participantId,
        IParticipantRepository participants,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        Participant currentParticipant)
    {
        var viewing = true;

        while (viewing)
        {
            var participant = participants.GetById(participantId);

            if (participant is null)
            {
                ConsoleUi.Pause("That participant no longer exists.");
                return currentParticipant;
            }

            var authoredNodes = nodes
                .GetAll()
                .Where(node =>
                    node.AuthorId.Value == participant.Id.Value)
                .ToList();

            Console.Clear();
            ParticipantDisplay.WriteProfile(
                participant,
                nodes.GetAll(),
                currentParticipant);
            Console.WriteLine();
            Console.WriteLine("1. Edit profile");
            Console.WriteLine("2. View authored nodes");
            Console.WriteLine("3. Select as current participant");
            Console.WriteLine("4. Return");
            Console.WriteLine();
            Console.Write("Selection: ");

            switch (Console.ReadLine())
            {
                case "1":
                    currentParticipant = EditProfile(
                        participant,
                        participants,
                        currentParticipant);
                    break;

                case "2":
                    ViewAuthoredNodes(
                        participant,
                        authoredNodes,
                        nodes,
                        nodeTypes,
                        documents,
                        participants);
                    break;

                case "3":
                    if (!participant.IsActive)
                    {
                        ConsoleUi.Pause(
                            "An inactive participant cannot be selected.");
                        break;
                    }

                    currentParticipant = participant;
                    ConsoleUi.Pause(
                        $"Current participant: {participant.DisplayName}");
                    break;

                case "4":
                    viewing = false;
                    break;

                default:
                    ConsoleUi.Pause(
                        "Please select an option from 1 through 4.");
                    break;
            }
        }

        return currentParticipant;
    }

    private static Participant EditProfile(
        Participant participant,
        IParticipantRepository participants,
        Participant currentParticipant)
    {
        Console.Clear();
        Console.WriteLine("EDIT PARTICIPANT PROFILE");
        Console.WriteLine("------------------------");
        Console.WriteLine(
            $"Editing {participant.DisplayName} " +
            $"as {currentParticipant.DisplayName}.");
        Console.WriteLine();
        Console.Write(
            $"Display name ({participant.DisplayName}): ");
        var displayName = Console.ReadLine();

        Console.WriteLine(
            "Bio (blank keeps the current bio; /clear removes it):");
        Console.Write("> ");
        var bio = Console.ReadLine();

        var requestedDisplayName =
            string.IsNullOrWhiteSpace(displayName)
                ? participant.DisplayName
                : displayName;

        var requestedBio = bio switch
        {
            null or "" => participant.Bio,
            "/clear" => string.Empty,
            _ => bio
        };

        try
        {
            var workflow =
                new UpdateParticipantProfile(participants);

            var updatedParticipant = workflow.Execute(
                currentParticipant.Id,
                participant.Id,
                requestedDisplayName,
                requestedBio,
                DateTimeOffset.UtcNow);

            ConsoleUi.Pause("Profile updated.");

            return currentParticipant.Id == updatedParticipant.Id
                ? updatedParticipant
                : currentParticipant;
        }
        catch (UnauthorizedAccessException exception)
        {
            ConsoleUi.Pause($"Permission denied: {exception.Message}");
        }
        catch (ArgumentException exception)
        {
            ConsoleUi.Pause($"Unable to update profile: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            ConsoleUi.Pause($"Unable to update profile: {exception.Message}");
        }

        return currentParticipant;
    }

    private static void ViewAuthoredNodes(
        Participant participant,
        IReadOnlyCollection<Node> authoredNodes,
        INodeRepository nodes,
        INodeTypeRepository nodeTypes,
        IDocumentRepository documents,
        IParticipantRepository participants)
    {
        Console.Clear();
        Console.WriteLine(
            $"NODES AUTHORED BY {participant.DisplayName.ToUpperInvariant()}");
        Console.WriteLine(
            new string('-', 18 + participant.DisplayName.Length));

        if (authoredNodes.Count == 0)
        {
            Console.WriteLine("No authored nodes.");
            ConsoleUi.Pause();
            return;
        }

        NodeDisplay.WriteTableHeader();

        var index = 1;

        foreach (var node in authoredNodes)
        {
            NodeDisplay.WriteTableRow(
                node,
                nodes,
                nodeTypes,
                documents,
                participants,
                index);

            index++;
        }

        ConsoleUi.Pause();
    }
}
