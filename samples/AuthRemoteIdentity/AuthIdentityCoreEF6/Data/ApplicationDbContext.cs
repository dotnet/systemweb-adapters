using Microsoft.AspNetCore.Identity.EntityFramework6;

namespace AuthIdentityCoreEF6.Data;

public class ApplicationDbContext(string connectionString) : IdentityEF6DbContext<ApplicationUser>(connectionString)
{
}
