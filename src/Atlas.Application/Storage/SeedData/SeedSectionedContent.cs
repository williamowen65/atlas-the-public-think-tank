using System.Text.Json;
using Atlas.Content.Blocks;
using Atlas.Persistence;

namespace Atlas.Application.Storage;

internal static partial class DemoData
{
    private static readonly JsonSerializerOptions SeedJson = new(JsonSerializerDefaults.Web);

    private static void SetSectionedContent(BlockRow row, string text)
    {
        var layouts = new List<object>
        {
            new { id = "full", name = "Full width", slots = new[] { new { id = "left", weight = 1 } } },
            new { id = "equal", name = "Two equal columns", slots = new[] { new { id = "left", weight = 1 }, new { id = "right", weight = 1 } } },
            new { id = "wide-left", name = "Wide left", slots = new[] { new { id = "left", weight = 2 }, new { id = "right", weight = 1 } } },
            new { id = "wide-right", name = "Wide right", slots = new[] { new { id = "left", weight = 1 }, new { id = "right", weight = 2 } } }
        };
        var sections = new List<object>();
        var blocks = new List<object>();
        var assignments = new List<object>();

        void AddSection(string layout, object left, object? right = null)
        {
            var sectionId = $"section-{sections.Count + 1}";
            sections.Add(new { id = sectionId, layoutId = layout, breakBefore = sections.Count > 0 });
            AddBlock("left", left);
            if (right is not null) AddBlock("right", right);

            void AddBlock(string slot, object body)
            {
                var blockId = $"block-{blocks.Count + 1}";
                var fields = JsonSerializer.SerializeToElement(body, SeedJson)
                    .EnumerateObject().ToDictionary(property => property.Name, property => (object)property.Value);
                fields.Add("id", blockId);
                blocks.Add(fields);
                assignments.Add(new { sectionId, slotId = slot, blockId });
            }
        }

        static object Text(string value) => new { kind = "text", text = value };
        static object Link(string url) => new { kind = "link", url };
        var photoSequence = 0;
        object Photo(string id, string image, string alt, string caption, string ratio) => new
        {
            id = $"{id}-{++photoSequence}", src = $"https://images.unsplash.com/{image}?w=900&fit=max", alt, caption, frame = new { ratio }
        };

        var number = int.Parse(row.Id.ToString()[^6..], System.Globalization.CultureInfo.InvariantCulture);
        object Street(string caption, string ratio = "4 / 5") =>
            Photo("street", "photo-1519501025264-65ba15a82390", "Buildings lining a city street", caption, ratio);
        object Buildings(string caption) =>
            Photo("buildings", "photo-1518005020951-eccb494ad742", "Curving buildings viewed from below", caption, "1 / 1");
        object Woods(string caption) =>
            Photo("woods", "photo-1441974231531-c6227db76b6e", "Sunlight filtering through tall trees", caption, "4 / 5");
        object Hills(string caption) =>
            Photo("hills", "photo-1470071459604-3b5ec3a7fe05", "Mist over green hills", caption, "16 / 9");
        static object Image(object photo) => new { kind = "image", photos = new[] { photo } };
        static object Gallery(string style, params object[] photos) => new { kind = "gallery", galleryStyle = style, photos };

        switch (number)
        {
            case 1:
                AddSection("wide-left", Text(text), new { kind = "image", photos = new[]
                {
                    Photo("street", "photo-1519501025264-65ba15a82390", "Buildings lining a city street", "Start with a place you know.", "4 / 5")
                } });
                break;
            case 6:
            case 12:
            case 22:
                AddSection("full", new { kind = "gallery", galleryStyle = number == 6 ? "slideshow" : number == 12 ? "masonry" : "grid",
                    photos = new[]
                    {
                        Photo("woods", "photo-1441974231531-c6227db76b6e", "Light coming through a forest", "A view from the forest floor.", "4 / 5"),
                        Photo("hills", "photo-1470071459604-3b5ec3a7fe05", "Mist over green hills", "A wider view of the landscape.", "16 / 9")
                    } });
                AddSection("full", Text(text));
                break;
            case 9:
            case 11:
                AddSection("wide-left", Text(text), Link("https://www.fisheries.noaa.gov/west-coast/endangered-species-conservation/saving-southern-resident-killer-whales"));
                break;
            case 222:
                AddSection("full", Text("AI-assisted education may collect sensitive patterns about children and families. " + text));
                AddSection("equal",
                    Text("## What should schools collect?\n\nStart by listing the information a learning tool needs to help a student."),
                    Text("## Who can use it?\n\nDistinguish access for students, families, teachers, and vendors. Decide when the data should be deleted."));
                AddSection("full", Text("## Questions to bring to a school board meeting\n\n- Which records does the tool create, and who can export them?\n- Can families review and correct those records?\n- What happens to the records when the school stops using the tool?"));
                break;
            case 18:
                layouts.Add(new { id = "community-network", name = "Network proposal and evidence",
                    slots = new[] { new { id = "left", weight = 65 }, new { id = "right", weight = 35 } } });
                AddSection("community-network", Text(text), Text("## Compare local approaches\n\nLook for a community network with published coverage, operating costs, and feedback from residents."));
                break;
            case >= 201 and <= 206:
            case >= 401 and <= 415:
                AddSection("wide-left", Text(text), Image(number % 2 == 0
                    ? Woods("Upstream habitat is one part of a connected watershed.")
                    : Hills("Consider the whole watershed when measuring recovery.")));
                if (number is 201 or 206 or 415)
                    AddSection("equal", Text("## What to measure\n\nTrack changes in habitat, prey availability, and seasonal conditions."),
                        Link("https://www.fisheries.noaa.gov/west-coast/endangered-species-conservation/saving-southern-resident-killer-whales"));
                break;
            case >= 207 and <= 213:
                AddSection(number % 2 == 0 ? "equal" : "wide-left", Text(text),
                    Image(number % 2 == 0 ? Buildings("Existing buildings can constrain or expand housing choices.") : Street("Housing choices depend partly on access to nearby services.")));
                break;
            case >= 219 and <= 221:
                AddSection("full", Text(text));
                AddSection("equal", Text("## Try it in a lesson\n\nCompare a task students do independently with one where a tool offers hints."),
                    Text("## Discuss the result\n\nAsk students to explain their reasoning and identify where the tool helped."));
                break;
            case >= 223 and <= 227:
                AddSection("full", Text(text));
                AddSection("full", Gallery(number == 224 ? "mosaic" : number == 226 ? "slideshow" : "grid",
                    Street("Observe who can use the space comfortably.", "1 / 1"),
                    Buildings("Consider how the surroundings affect the experience."),
                    Woods("Look for places that offer shade and a quieter route.")));
                break;
            case >= 232 and <= 241:
                AddSection("wide-left", Text(text), Image(number is 234 or 235 or 236
                    ? Buildings("Existing public and commercial buildings offer space to experiment.")
                    : Woods("The conditions along a route can matter as much as the destination.")));
                if (number is 232 or 238 or 240)
                    AddSection("full", Gallery("masonry", Hills("Consider connections across the wider landscape."),
                        Street("Check what people encounter on the ground."), Woods("Note where vegetation or shade already exists.")));
                break;
            case 245:
            case 246:
                AddSection("wide-right", Image(Street("Map which buildings already have reliable service.")), Text(text));
                AddSection("full", Text("## Compare the options\n\nRecord current coverage, installation costs, and who would maintain the network."));
                break;
            default:
                AddSection("full", Text(text));
                break;
        }

        var composition = JsonSerializer.Serialize(new { version = 1, layouts, sections, blocks, assignments }, SeedJson);
        var validated = SectionedBlock.Reconstitute(new BlockId(row.Id), composition, row.CreatedAt, row.UpdatedAt);
        row.Kind = validated.Kind;
        row.Composition = validated.Composition;
        row.CompositionSearchText = validated.SearchableText;
        row.Markdown = null;
    }
}
