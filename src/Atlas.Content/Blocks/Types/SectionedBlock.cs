using System.Text.Json;

namespace Atlas.Content.Blocks;

/// <summary>A versioned composition of sections, layouts and their assigned content.</summary>
public sealed class SectionedBlock : ContentBlock
{
    public override string Kind => "sectioned";
    public string Composition { get; private set; }
    public string SearchableText { get; private set; }

    public SectionedBlock(string composition, DateTimeOffset at) : this(BlockId.New(), composition, at, at) { }

    private SectionedBlock(BlockId id, string composition, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        : base(id, createdAt, updatedAt)
    {
        (Composition, SearchableText) = Validate(composition);
    }

    public void Update(string composition, DateTimeOffset at)
    {
        var validated = Validate(composition);
        ChangedAt(at);
        (Composition, SearchableText) = validated;
    }

    public static SectionedBlock Reconstitute(BlockId id, string composition, DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new(id, composition, createdAt, updatedAt);

    private static (string, string) Validate(string composition)
    {
        composition = Required(composition, nameof(composition), 512_000);
        try
        {
            using var json = JsonDocument.Parse(composition, new JsonDocumentOptions { MaxDepth = 24 });
            var root = json.RootElement;
            if (root.GetProperty("version").GetInt32() != 1) throw new ArgumentException("Unknown composition version.");
            Choice(root, "feedPresentation", "opening-content", "title-only");
            var layouts = root.GetProperty("layouts").EnumerateArray().ToDictionary(
                layout => ReadRequiredJsonString(layout, "id"),
                layout => layout.GetProperty("slots").EnumerateArray().ToDictionary(slot => ReadRequiredJsonString(slot, "id"), slot =>
                {
                    var weight = slot.GetProperty("weight").GetDouble();
                    if (!double.IsFinite(weight) || weight <= 0 || weight > 100) throw new ArgumentException("Invalid column width.");
                    return weight;
                }));
            if (layouts.Count is < 1 or > 100 || layouts.Values.Any(slots => slots.Count is < 1 or > 2))
                throw new ArgumentException("Invalid layout count or column count.");

            var sections = root.GetProperty("sections").EnumerateArray().ToDictionary(section => ReadRequiredJsonString(section, "id"));
            if (sections.Count is < 1 or > 100) throw new ArgumentException("Invalid section count.");
            var slotsBySection = new Dictionary<string, HashSet<string>>();
            foreach (var (id, section) in sections)
            {
                var baseSlots = layouts[ReadRequiredJsonString(section, "layoutId")].Keys;
                var slots = new HashSet<string>();
                if (section.TryGetProperty("rows", out var rows))
                {
                    if (rows.GetArrayLength() is < 1 or > 100) throw new ArgumentException("Invalid row count.");
                    foreach (var row in rows.EnumerateArray())
                    {
                        IEnumerable<string> rowSlots = row.ValueKind == JsonValueKind.Null ? baseSlots.AsEnumerable() : [Required(row.GetString(), "row", 500)];
                        foreach (var slot in rowSlots)
                            if (!slots.Add(slot)) throw new ArgumentException("Duplicate row slot.");
                    }
                }
                else slots.UnionWith(baseSlots);
                slotsBySection.Add(id, slots);
            }

            var blocks = root.GetProperty("blocks").EnumerateArray().ToDictionary(block => ReadRequiredJsonString(block, "id"));
            if (blocks.Count is < 1 or > 100) throw new ArgumentException("Invalid block count.");
            var positions = new HashSet<(string, string)>();
            var assigned = new HashSet<string>();
            foreach (var assignment in root.GetProperty("assignments").EnumerateArray())
            {
                var section = ReadRequiredJsonString(assignment, "sectionId");
                var slot = ReadRequiredJsonString(assignment, "slotId");
                var block = ReadRequiredJsonString(assignment, "blockId");
                if (!slotsBySection[section].Contains(slot))
                    throw new ArgumentException("Unknown section slot.");
                Choice(assignment, "vertical", "start", "center", "end");
                if (!blocks.ContainsKey(block) || !assigned.Add(block) || !positions.Add((section, slot)))
                    throw new ArgumentException("Invalid block assignment.");
            }
            if (assigned.Count != blocks.Count) throw new ArgumentException("Every block must be placed in a section.");

            var text = new List<string>();
            var photoIds = new HashSet<string>();
            foreach (var block in blocks.Values)
            {
                switch (ReadRequiredJsonString(block, "kind"))
                {
                    case "text":
                        Choice(block, "alignment", "left", "center", "right");
                        text.Add(Optional(block.GetProperty("text").GetString(), 100_000, "text"));
                        break;
                    case "link":
                        text.Add(HttpUrl(ReadRequiredJsonString(block, "url"), "url").AbsoluteUri);
                        break;
                    case "image":
                    case "gallery":
                        Choice(block, "galleryStyle", "grid", "mosaic", "masonry", "slideshow");
                        Choice(block, "galleryWidth", "full", "centered");
                        Choice(block, "masonryAlignment", "top", "center");
                        Range(block, "gridColumns", 1, 4);
                        Range(block, "masonryColumns", 1, 4);
                        if (block.TryGetProperty("slideshow", out var slideshow))
                        {
                            Choice(slideshow, "height", "compact", "standard", "tall");
                            Choice(slideshow, "fit", "contain", "cover");
                        }
                        var photos = block.GetProperty("photos");
                        if (photos.GetArrayLength() is < 1 or > 100) throw new ArgumentException("Invalid photo count.");
                        foreach (var photo in photos.EnumerateArray())
                        {
                            if (!photoIds.Add(ReadRequiredJsonString(photo, "id"))) throw new ArgumentException("Duplicate photo ID.");
                            HttpUrl(ReadRequiredJsonString(photo, "src"), "src");
                            Choice(photo, "captionPlacement", "below", "overlay");
                            if (photo.TryGetProperty("frame", out var frame))
                            {
                                Range(frame, "width", 25, 100);
                                Range(frame, "x", 0, 100);
                                Range(frame, "y", 0, 100);
                                Range(frame, "zoom", 1, 3);
                                Choice(frame, "ratio", "natural", "1 / 1", "4 / 5", "2 / 3", "4 / 3", "16 / 9", "circle", "pill", "blob");
                                Choice(frame, "corners", "square", "rounded", "extra-rounded");
                                Choice(frame, "gridVertical", "start", "center", "end");
                                if (frame.TryGetProperty("blobTangents", out var tangents) && tangents.GetArrayLength() > 12)
                                    throw new ArgumentException("Too many crop anchors.");
                            }
                            text.Add(Required(photo.GetProperty("alt").GetString(), "alt", 2000));
                            text.Add(Optional(photo.GetProperty("caption").GetString(), 2000, "caption"));
                        }
                        break;
                    default:
                        throw new ArgumentException("Unknown section block kind.");
                }
            }

            return (composition, string.Join("\n", text));
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        {
            throw new ArgumentException("Invalid sectioned composition.", nameof(composition), error);
        }
    }

    private static void Choice(JsonElement item, string property, params string[] options)
    {
        if (item.TryGetProperty(property, out var value) && !options.Contains(value.GetString()))
            throw new ArgumentException($"Invalid {property}.");
    }

    private static void Range(JsonElement item, string property, double min, double max)
    {
        if (item.TryGetProperty(property, out var value) &&
            (!value.TryGetDouble(out var number) || !double.IsFinite(number) || number < min || number > max))
            throw new ArgumentException($"Invalid {property}.");
    }

    private static string ReadRequiredJsonString(JsonElement item, string name)
    {
        var value = item.GetProperty(name).GetString();
        var normalized = Required(value, name, 500);
        if (value != normalized) throw new ArgumentException($"Invalid {name}.");
        return normalized;
    }
}
