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
        static object Photo(string id, string image, string alt, string caption, string ratio) => new
        {
            id, src = $"https://images.unsplash.com/{image}?w=900&fit=max", alt, caption, frame = new { ratio }
        };

        var number = int.Parse(row.Id.ToString()[^6..], System.Globalization.CultureInfo.InvariantCulture);
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
            case 18:
                layouts.Add(new { id = "community-network", name = "Network proposal and evidence",
                    slots = new[] { new { id = "left", weight = 65 }, new { id = "right", weight = 35 } } });
                AddSection("community-network", Text(text), Text("## Compare local approaches\n\nLook for a community network with published coverage, operating costs, and feedback from residents."));
                break;
            default:
                AddSection("full", Text(text));
                break;
        }

        var composition = JsonSerializer.Serialize(new { version = 1, layouts, sections, blocks, assignments }, SeedJson);
        var validated = SectionedBlock.Reconstitute(new BlockId(row.Id), composition, row.CreatedAt, row.UpdatedAt);
        row.Kind = validated.Kind;
        row.Composition = validated.Composition;
        row.Markdown = validated.SearchableText;
    }
}
