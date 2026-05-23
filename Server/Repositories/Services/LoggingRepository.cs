using Server.Database;
using Server.Models.Logging;
using Server.Repositories.Interfaces;

namespace Server.Repositories.Services
{
    public class LoggingRepository : GenericRepository<ErrorLogging>, ILoggingRepository
    {
        public LoggingRepository(DatabaseContext context) : base(context)
        {
        }
    }
}
