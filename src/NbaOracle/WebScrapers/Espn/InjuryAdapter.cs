using System.Collections.Generic;
using System.Linq;
using NbaOracle.Data.Injuries;

namespace NbaOracle.WebScrapers.Espn;

public class InjuryAdapter
{
    public static List<PlayerInjury> Adapt(List<TeamInjuryReportData> teamReports)
    {
        return (from teamReport in teamReports from i in teamReport.Injuries select new PlayerInjury(teamReport.Team, i.PlayerName, i.EstimatedReturnDate, i.Status, i.Description)).ToList();
    }
}