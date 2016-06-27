using SQLite.CodeFirst;
using System.Data.Entity;

namespace TaskRouter.Web.Models
{
    public class TaskRouterDbContext : DbContext
    {
        public TaskRouterDbContext()
            : base("TaskRouterConnection") { }

        public virtual DbSet<MissedCall> MissedCalls { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<TaskRouterDbContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}