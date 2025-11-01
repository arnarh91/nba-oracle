merge nba.Season as target
using ( select 2009
             , 2010
             , '2009-10-27'
             , '2010-04-14'
             , '2010-04-17'
             , '2010-06-17'
             , 'LAL'
             , 'BOS'
        union
        select 2010
             , 2011
             , '2010-10-26'
             , '2011-04-13'
             , '2011-04-16'
             , '2011-06-12'
             , 'DAL'
             , 'MIA'
        union
        select 2011
             , 2012
             , '2011-12-25'
             , '2012-04-26'
             , '2012-04-28'
             , '2012-06-21'
             , 'MIA'
             , 'OKC'
        union
        select 2012
             , 2013
             , '2012-10-30'
             , '2013-04-17'
             , '2013-04-20'
             , '2013-06-20'
             , 'MIA'
             , 'SAS'
        union
        select 2013
             , 2014
             , '2013-10-29'
             , '2014-04-16'
             , '2014-04-19'
             , '2014-06-15'
             , 'SAS'
             , 'MIA'
        union
        select 2014
             , 2015
             , '2014-10-28'
             , '2015-04-15'
             , '2015-04-18'
             , '2015-06-16'
             , 'GSW'
             , 'CLE'
        union
        select 2015
             , 2016
             , '2015-10-27'
             , '2016-04-13'
             , '2016-04-16'
             , '2016-06-19'
             , 'CLE'
             , 'GSW'
        union
        select 2016
             , 2017
             , '2016-10-25'
             , '2017-04-12'
             , '2017-04-15'
             , '2017-06-12'
             , 'GSW'
             , 'CLE'
        union
        select 2017
             , 2018
             , '2017-10-17'
             , '2018-04-11'
             , '2018-04-14'
             , '2018-06-08'
             , 'GSW'
             , 'CLE'
        union
        select 2018
             , 2019
             , '2018-10-16'
             , '2019-04-10'
             , '2019-04-13'
             , '2019-06-13'
             , 'TOR'
             , 'GSW'
        union
        select 2019
             , 2020
             , '2019-10-22'
             , '2020-08-15'
             , '2020-08-17'
             , '2020-10-11'
             , 'LAL'
             , 'MIA'
        union
        select 2020
             , 2021
             , '2020-12-22'
             , '2021-05-21'
             , '2021-05-22'
             , '2021-07-20'
             , 'MIL'
             , 'PHO'
        union
        select 2021
             , 2022
             , '2021-10-19'
             , '2022-04-10'
             , null
             , null
             , 'GSW'
             , 'BOS'
        union
        select 2022
             , 2023
             , '2022-10-18'
             , '2023-04-09'
             , null
             , null
             , 'DEN'
             , 'MIA'
        union
        select 2023
             , 2024
             , '2023-10-24'
             , '2024-04-14'
             , null
             , null
             , 'BOS'
             , 'DAL'
        union
        select 2024
             , 2025
             , '2024-10-22'
             , '2025-04-13'
             , null
             , null
             , 'OKC'
             , 'IND' ) as source ( StartYear, EndYear, RegularSeasonStartDate, RegularSeasonEndDate, PlayoffStartDate, PlayoffEndDate, FinalsWinnerTeamIdentifier, FinalsLoserTeamIdentifier )
on ( target.StartYear = source.StartYear )
when matched then update set StartYear = source.StartYear
                           , EndYear = source.EndYear
                           , RegularSeasonStartDate = source.RegularSeasonStartDate
                           , RegularSeasonEndDate = source.RegularSeasonEndDate
                           , PlayoffStartDate = source.PlayoffStartDate
                           , PlayoffEndDate = source.PlayoffEndDate
                           , FinalsWinnerTeamIdentifier = source.FinalsWinnerTeamIdentifier
                           , FinalsLoserTeamIdentifier = source.FinalsLoserTeamIdentifier
when not matched then insert ( StartYear
                             , EndYear
                             , RegularSeasonStartDate
                             , RegularSeasonEndDate
                             , PlayoffStartDate
                             , PlayoffEndDate
                             , FinalsWinnerTeamIdentifier
                             , FinalsLoserTeamIdentifier )
                      values
                           ( source.StartYear, source.EndYear, source.RegularSeasonStartDate, source.RegularSeasonEndDate, source.PlayoffStartDate, source.PlayoffEndDate, source.FinalsWinnerTeamIdentifier, source.FinalsLoserTeamIdentifier );
go