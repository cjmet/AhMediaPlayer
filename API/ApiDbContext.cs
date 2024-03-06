using AhConfig;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ApiEndpoints
{
    class ApiDbContext(DbContextOptions<ApiDbContext> options) : IdentityDbContext<MyUser>(options) { }
    
}
