using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GpMnrega.DataLayer.Repositories;

/// <summary>
/// Migrated from DataLayer/getgpcode.cs
/// Used for panchayat code lookup, regional name translations, official language.
/// </summary>
public interface IGpCodeRepository
{
    Task<string> GetPanchayatNameAsync(string panchayatCode);
    Task<IEnumerable<(string Name, string Code)>> GetDistrictsAsync(string stateCode = "15");
    Task<string?> GetBlockNameAsync(string talukCode);
    Task<GpOfficialLanguageData?> GetOfficialLanguageAsync(string email);
    Task UpdateVidLokRegionalNameAsync(string email, string lokSabha, string vidhanSabha);
    Task UpdatePanchayatRegionalNameAsync(string panchayatCode, string regionalName);
    Task UpdateBlockRegionalNameAsync(string blockCode, string regionalName);
    Task UpdateDistrictRegionalNameAsync(string districtCode, string regionalName);
}

public class GpOfficialLanguageData
{
    public string PanchyatName { get; set; } = "";
    public string PanchyatCode { get; set; } = "";
    public string PanchayatNameRegional { get; set; } = "";
    public string VidhanSabha { get; set; } = "";
    public string VidhanSabhaRegional { get; set; } = "";
    public string LokSabha { get; set; } = "";
    public string TalukName { get; set; } = "";
    public string TalukCode { get; set; } = "";
    public string TalukNameRegional { get; set; } = "";
    public string DistrictName { get; set; } = "";
    public string DistrictCode { get; set; } = "";
    public string DistrictNameRegional { get; set; } = "";
    public string LanguageCode { get; set; } = "kn"; // default Kannada
}

public class GpCodeRepository : IGpCodeRepository
{
    private readonly string _connString;

    public GpCodeRepository(string connectionString)
    {
        _connString = connectionString;
    }

    private SqlConnection Conn() => new(_connString);

    public async Task<string> GetPanchayatNameAsync(string panchayatCode)
    {
        using var conn = Conn();
        var result = await conn.ExecuteScalarAsync<string>(
            "getPanchayat",
            new { panchayatCode },
            commandType: CommandType.StoredProcedure);
        return result ?? "Error";
    }

    public async Task<IEnumerable<(string Name, string Code)>> GetDistrictsAsync(string stateCode = "15")
    {
        using var conn = Conn();
        // Original used raw SQL — keeping as-is, parameterised safely
        var rows = await conn.QueryAsync(
            "SELECT DistrictName, DistrictCode FROM District WHERE StateCode = @stateCode",
            new { stateCode });
        return rows.Select(r => ((string)r.DistrictName, (string)r.DistrictCode));
    }

    public async Task<string?> GetBlockNameAsync(string talukCode)
    {
        using var conn = Conn();
        return await conn.ExecuteScalarAsync<string>(
            "SELECT TalukName FROM Taluk WHERE TalukCode = @talukCode",
            new { talukCode });
    }

    public async Task<GpOfficialLanguageData?> GetOfficialLanguageAsync(string email)
    {
        using var conn = Conn();
        return await conn.QueryFirstOrDefaultAsync<GpOfficialLanguageData>(
            "GetOfficialLanguage",
            new { email },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateVidLokRegionalNameAsync(string email, string lokSabha, string vidhanSabha)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(
            "UpdateVidLokReginalName",
            new { email, lok = lokSabha, vid = vidhanSabha },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdatePanchayatRegionalNameAsync(string panchayatCode, string regionalName)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(
            "UpdatePanchayatReginalName",
            new { pcode = panchayatCode, prname = regionalName },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateBlockRegionalNameAsync(string blockCode, string regionalName)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(
            "UpdateBlockReginalName",
            new { bcode = blockCode, brname = regionalName },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateDistrictRegionalNameAsync(string districtCode, string regionalName)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(
            "UpdateDistReginalName",
            new { dcode = districtCode, drname = regionalName },
            commandType: CommandType.StoredProcedure);
    }
}
