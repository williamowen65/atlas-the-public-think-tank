using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Atlas.Persistence.Migrations;

[DbContext(typeof(AtlasDataContext))]
public sealed class AtlasDataContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder) => AtlasDataContext.ConfigureModel(modelBuilder);
}
