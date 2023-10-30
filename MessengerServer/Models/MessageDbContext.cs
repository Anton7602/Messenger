using MessengerService;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Models
{
    internal class MessageDbContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }

    }
}
