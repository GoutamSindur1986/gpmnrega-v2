using Dapper;
using GpMnrega.DataLayer.Models;
using Microsoft.Data.SqlClient;

namespace GpMnrega.DataLayer.Repositories;

public interface IDeptUserRepository
{
    Task<int> CreateUserAsync(string userName, string email, string passwordHash,
        string blockCode, string agency, string phone);
    Task<DeptUser?> AuthenticateAsync(string email, string passwordHash);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UpdatePasswordAsync(string email, string passwordHash);
    Task<bool> UpdateSubscriptionAsync(string email, string subId, string taxId, string response, decimal amount);
    Task<DeptUser?> GetSubscriptionAsync(string email);
}

public class DeptUserRepository : IDeptUserRepository
{
    private readonly string _connString;

    public DeptUserRepository(string connectionString)
    {
        _connString = connectionString;
    }

    private SqlConnection Conn() => new(_connString);

    public async Task<int> CreateUserAsync(string userName, string email, string passwordHash,
        string blockCode, string agency, string phone)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync("CreateDeptUser", new
        {
            UserName = userName,
            Password = passwordHash,
            Email = email,
            Blockcode = blockCode,
            Agency = agency,
            phone
        }, commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<DeptUser?> AuthenticateAsync(string email, string passwordHash)
    {
        using var conn = Conn();
        return await conn.QueryFirstOrDefaultAsync<DeptUser>(
            "AuthenticateDeptUser",
            new { UserEmail = email, Password = passwordHash },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var conn = Conn();
        return await conn.ExecuteScalarAsync<bool>(
            "checkDeptEmailExists",
            new { UserEmail = email },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<bool> UpdatePasswordAsync(string email, string passwordHash)
    {
        using var conn = Conn();
        var rows = await conn.ExecuteAsync("UpdateDeptUserPassword",
            new { password = passwordHash, email },
            commandType: System.Data.CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<bool> UpdateSubscriptionAsync(string email, string subId, string taxId, string response, decimal amount)
    {
        using var conn = Conn();
        var rows = await conn.ExecuteAsync("UpdateDeptUserSubcription", new
        {
            Email = email,
            SubscriptionId = subId,
            taxid = taxId,
            SubscriptionResponse = response,
            Amount = amount
        }, commandType: System.Data.CommandType.StoredProcedure);
        return rows == 1;
    }

    public async Task<DeptUser?> GetSubscriptionAsync(string email)
    {
        using var conn = Conn();
        return await conn.QueryFirstOrDefaultAsync<DeptUser>(
            "CheckDeptSubscription",
            new { email },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
