using Dapper;
using GpMnrega.DataLayer.Models;
using Microsoft.Data.SqlClient;

namespace GpMnrega.DataLayer.Repositories;

public interface IGpUserRepository
{
    Task<int> CreateUserAsync(string userName, string email, string passwordHash,
        string panchayatCode, string vidhanSabha, string lokSabha, string phone, string activationCode);
    Task<GpUser?> AuthenticateAsync(string email, string passwordHash);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> VerifyEmailAsync(string email, string activationCode);
    Task UpdateEmailActivationAsync(string email);
    Task<bool> UpdatePasswordAsync(string email, string passwordHash);
    Task<bool> UpdateSubscriptionAsync(string email, string subId, string taxId, string response, decimal amount);
    Task<GpUser?> GetSubscriptionAsync(string email);
    Task UpdateSubEndStatusAsync(string email);

}

public class GpUserRepository : IGpUserRepository
{
    private readonly string _connString;

    public GpUserRepository(string connectionString)
    {
        _connString = connectionString;
    }

    private SqlConnection Conn() => new(_connString);

    public async Task<int> CreateUserAsync(string userName, string email, string passwordHash,
        string panchayatCode, string vidhanSabha, string lokSabha, string phone, string activationCode)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync("CreateUser", new
        {
            UserName = userName,
            Password = passwordHash,
            Email = email,
            PachayatCode = panchayatCode,
            VidhanSabha = vidhanSabha,
            LokSabha = lokSabha,
            phone,
            activationcode = activationCode
        }, commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<GpUser?> AuthenticateAsync(string email, string passwordHash)
    {
        using var conn = Conn();
        // SP returns same columns as before — map directly
        return await conn.QueryFirstOrDefaultAsync<GpUser>(
            "AuthenticateUser",
            new { UserEmail = email, Password = passwordHash },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var conn = Conn();
        var result = await conn.ExecuteScalarAsync<bool>(
            "checkEmailExists",
            new { UserEmail = email },
            commandType: System.Data.CommandType.StoredProcedure);
        return result;
    }

    public async Task<bool> VerifyEmailAsync(string email, string activationCode)
    {
        using var conn = Conn();
        return await conn.ExecuteScalarAsync<bool>(
            "EmailVerification",
            new { UserEmail = email, activationcode = activationCode },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task UpdateEmailActivationAsync(string email)
    {
        using var conn = Conn();
        await conn.ExecuteAsync("UpdateEmailActivation",
            new { email },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<bool> UpdatePasswordAsync(string email, string passwordHash)
    {
        using var conn = Conn();
        var rows = await conn.ExecuteAsync("UpdatePassword",
            new { password = passwordHash, email },
            commandType: System.Data.CommandType.StoredProcedure);
        return rows > 0;
    }

    public async Task<bool> UpdateSubscriptionAsync(string email, string subId, string taxId, string response, decimal amount)
    {
        using var conn = Conn();
        var rows = await conn.ExecuteAsync("UpdateUserSubcription", new
        {
            Email = email,
            SubscriptionId = subId,
            taxid = taxId,
            SubscriptionResponse = response,
            Amount = amount
        }, commandType: System.Data.CommandType.StoredProcedure);
        return rows == 1;
    }

    public async Task<GpUser?> GetSubscriptionAsync(string email)
    {
        using var conn = Conn();
        return await conn.QueryFirstOrDefaultAsync<GpUser>(
            "CheckSubscription",
            new { email },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task UpdateSubEndStatusAsync(string email)
    {
        using var conn = Conn();
        await conn.ExecuteAsync("UpdateSubEndStatus",
            new { Email = email },
            commandType: System.Data.CommandType.StoredProcedure);
    }

}
