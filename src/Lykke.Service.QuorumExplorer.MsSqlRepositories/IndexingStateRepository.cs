using System.Threading.Tasks;
using Lykke.Common.MsSql;
using Lykke.Service.QuorumExplorer.Domain.Repositories;
using Lykke.Service.QuorumExplorer.MsSqlRepositories.Constants;
using Lykke.Service.QuorumExplorer.MsSqlRepositories.Contexts;
using Lykke.Service.QuorumExplorer.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Service.QuorumExplorer.MsSqlRepositories
{
    public class IndexingStateRepository : IIndexingStateRepository
    {
        private readonly MsSqlContextFactory<QeContext> _contextFactory;

        public IndexingStateRepository(
            MsSqlContextFactory<QeContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<long?> GetLastIndexedBlockNumberAsync()
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var lastIndexedBlock = await context.BlocksData
                    .SingleOrDefaultAsync(b => b.Key == BlocksDataKeys.LastIndexedBlockNumberKey);

                if (lastIndexedBlock == null)
                    return null;

                return long.Parse(lastIndexedBlock.Value);
            }
        }

        public async Task SaveLastIndexedBlockNumber(long blockNumber)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var existingEntity = await context
                    .BlocksData
                    .SingleOrDefaultAsync(b => b.Key == BlocksDataKeys.LastIndexedBlockNumberKey);

                if (existingEntity != null)
                {
                    existingEntity.Value = blockNumber.ToString();
                    context.Update(existingEntity);
                }
                else
                {
                    var newEntity = new BlocksDataEntity
                    {
                        Key = BlocksDataKeys.LastIndexedBlockNumberKey,
                        Value = blockNumber.ToString()
                    };
                    context.Add(newEntity);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}