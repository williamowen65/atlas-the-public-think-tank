using Atlas.ConsoleApp.Storage;
using Atlas.Content.Documents;

namespace Atlas.ConsoleApp.Tests;

[TestClass]
public sealed class DocumentPersistenceTests
{
    [TestMethod]
    public void UpdateContentPreservesDocumentIdentity()
    {
        var createdAt = new DateTimeOffset(2026, 9, 19, 12, 0, 0, TimeSpan.Zero);
        var document = new Document("Original description", createdAt);
        var originalId = document.Id;

        document.UpdateContent("Updated description");

        Assert.AreEqual(originalId, document.Id);
        Assert.AreEqual("Updated description", document.Content);
        Assert.AreEqual(createdAt, document.CreatedAt);
    }

    [TestMethod]
    public void SaveUpdatedDocumentReplacesPersistedBodyWithoutChangingId()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            $"atlas-content-tests-{Guid.NewGuid():N}");
        var filePath = Path.Combine(directory, "documents.json");

        try
        {
            var repository = new JsonDocumentRepository(filePath);
            var document = new Document(
                "Original description",
                new DateTimeOffset(2026, 9, 19, 12, 0, 0, TimeSpan.Zero));
            var originalId = document.Id;

            repository.Save(document);
            document.UpdateContent("Updated description");
            repository.Save(document);

            var reloadedRepository = new JsonDocumentRepository(filePath);
            var reloaded = reloadedRepository.GetById(originalId);

            Assert.IsNotNull(reloaded);
            Assert.AreEqual(originalId, reloaded.Id);
            Assert.AreEqual("Updated description", reloaded.Content);
            Assert.AreEqual(1, reloadedRepository.GetAll().Count);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
