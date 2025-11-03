using System;
using System.Collections.Generic;
using NbaOracle.ValueObjects;

namespace NbaOracle.WebScrapers.Espn;

public record TeamInjuryReportData(Team Team, List<PlayerInjuryData> Injuries);

public record PlayerInjuryData(string PlayerName, DateOnly EstimatedReturnDate, string Status, string Description);