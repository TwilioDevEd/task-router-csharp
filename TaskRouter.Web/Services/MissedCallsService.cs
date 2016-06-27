using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using TaskRouter.Web.Models;

namespace TaskRouter.Web.Services
{
    public interface IMissedCallsService
    {
        Task<IEnumerable<MissedCall>> FindAllAsync();
        Task<int> CreateAsync(MissedCall missedCall);
    }

    public class MissedCallsService : IMissedCallsService
    {
        private readonly TaskRouterDbContext _context;

        public MissedCallsService(TaskRouterDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MissedCall>> FindAllAsync()
        {
            return await _context.MissedCalls
                .ToListAsync();
        }

        public async Task<int> CreateAsync(MissedCall missedCall)
        {
            _context.MissedCalls.Add(missedCall);
            return await _context.SaveChangesAsync();
        }
    }
}