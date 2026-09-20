namespace Atlas.Communities.Communities;

public interface ICommunityRepository
{
    IReadOnlyCollection<Community> GetAll();
    Community? GetById(CommunityId id);
    void Save(Community community);
}
