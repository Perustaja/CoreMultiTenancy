using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreMultiTenancy.Identity.Entities;
using CoreMultiTenancy.Identity.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Perustaja.Polyglot.Option;

namespace CoreMultiTenancy.Identity.Data.Repositories
{
    public class UserOrganizationRepository : IUserOrganizationRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<UserOrganizationRepository> _logger;
        private readonly ApplicationDbContext _applicationContext;
        public IUnitOfWork UnitOfWork { get => _applicationContext; }
        public UserOrganizationRepository(IConfiguration config,
            ILogger<UserOrganizationRepository> logger,
            ApplicationDbContext applicationContext)
        {
            _connectionString = config.GetConnectionString("IdentityDb");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _applicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
        }

        public async Task<List<UserOrganization>> GetAllByOrgId(Guid orgId)
        {
            return await _applicationContext.Set<UserOrganization>()
                .Where(uo => uo.OrgId == orgId && uo.AwaitingApproval == false)
                .Include(uo => uo.User)
                .ThenInclude(u => u.UserOrganizationRoles)
                .ThenInclude(uor => uor.Role)
                .ToListAsync();
        }

        public async Task<List<UserOrganization>> GetAwaitingAccessByOrgId(Guid orgId)
        {
            return await _applicationContext.Set<UserOrganization>()
                .Where(uo => uo.OrgId == orgId && uo.AwaitingApproval == true)
                .Include(uo => uo.User)
                .ToListAsync();
        }

        public async Task<Option<UserOrganization>> GetByIdsAsync(Guid orgId, Guid userId)
        {
            var res = await _applicationContext.Set<UserOrganization>()
                .Where(uo => uo.OrgId == orgId && uo.UserId == userId)
                .FirstOrDefaultAsync();
            return res != null
                ? Option<UserOrganization>.Some(res)
                : Option<UserOrganization>.None;
        }

        public UserOrganization Add(UserOrganization uo)
            => _applicationContext.Add(uo).Entity;

        public UserOrganization Update(UserOrganization uo)
            => _applicationContext.Set<UserOrganization>().Update(uo).Entity;

        public async Task<bool> ExistsAsync(Guid userId, Guid orgId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var res = await conn.QuerySingleAsync<int>(
                    @"SELECT COUNT(*) FROM UserOrganizations
                    WHERE UserId = @UserId
                    AND OrgId = @OrgId",
                    new { UserId = userId, OrgId = orgId }
                );
                return res > 0;
            }
        }
        public async Task<bool> ExistsWithAccessAsync(Guid userId, Guid orgId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var res = await conn.QuerySingleAsync<int>(
                    @"SELECT COUNT(*) FROM UserOrganizations
                    WHERE UserId = @UserId
                    AND OrgId = @OrgId
                    AND AwaitingApproval = false
                    AND Blacklisted = false",
                    new { UserId = userId, OrgId = orgId }
                );
                return res > 0;
            }
        }
    }
}