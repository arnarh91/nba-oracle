using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using NbaOracle.ValueObjects;

namespace NbaOracle.Data.Injuries;

public record PlayerInjury(Team Team, string PlayerName, DateOnly EstimatedReturnDate, string InjuryStatus, string InjuryDescription);

public class InjuryReportRepository
{
    private readonly IDbConnection _dbConnection;

    public InjuryReportRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }

    public async Task Merge(List<PlayerInjury> injuries)
    {
        var dt = new DataTable();
        dt.Columns.Add("PlayerName", typeof(string));
        dt.Columns.Add("TeamIdentifier", typeof(string));
        dt.Columns.Add("InjuryStatus", typeof(string));
        dt.Columns.Add("EstimatedReturnDate", typeof(DateTime));
        dt.Columns.Add("InjuryDescription", typeof(string));
        dt.Columns.Add("EffectiveFrom", typeof(DateTime));

        foreach (var i in injuries)
        {
            dt.Rows.Add(i.PlayerName, i.Team.Identifier, i.InjuryStatus, i.EstimatedReturnDate.ToDateTime(TimeOnly.MinValue), i.InjuryDescription, DateTime.Today);
        }
            
        var parameters = new DynamicParameters();
        parameters.Add("@Injuries", dt.AsTableValuedParameter("nba.tt_Merge_PlayerInjuryReport"));
        
        await _dbConnection.ExecuteAsync("nba.sp_MergeInjuryReportStaging", parameters, commandType:CommandType.StoredProcedure);
    }
}