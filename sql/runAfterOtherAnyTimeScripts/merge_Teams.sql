merge nba.Team as target
using ( select 'ATL'
             , 'Atlanta Hawks'
        union
        select 'BOS'
             , 'Boston Celtics'
        union
        select 'BRK'
             , 'Brooklyn Nets'
        union
        select 'CHI'
             , 'Chicago Bulls'
        union
        select 'CHA'
             , 'Charlotte Bobcats'
        union
        select 'CHO'
             , 'Charlotte Hornets'
        union
        select 'CLE'
             , 'Cleveland Cavaliers'
        union
        select 'DAL'
             , 'Dallas Mavericks'
        union
        select 'DEN'
             , 'Denver Nuggets'
        union
        select 'DET'
             , 'Detroit Pistons'
        union
        select 'GSW'
             , 'Golden State Warriors'
        union
        select 'HOU'
             , 'Houston Rockets'
        union
        select 'IND'
             , 'Indiana Pacers'
        union
        select 'LAC'
             , 'Los Angeles Clippers'
        union
        select 'LAL'
             , 'Los Angeles Lakers'
        union
        select 'MEM'
             , 'Memphis Grizzlies'
        union
        select 'MIA'
             , 'Miami Heat'
        union
        select 'MIL'
             , 'Milwaukee Bucks'
        union
        select 'MIN'
             , 'Minnesota Timberwolves'
        union
        select 'NOH'
             , 'New Orleans Hornets'
        union
        select 'NJN'
             , 'New Jersey Nets'
        union
        select 'NOP'
             , 'New Orleans Pelicans'
        union
        select 'NYK'
             , 'New York Knicks'
        union
        select 'OKC'
             , 'Oklahoma City Thunder'
        union
        select 'ORL'
             , 'Orlando Magic'
        union
        select 'PHI'
             , 'Philadelphia 76ers'
        union
        select 'PHO'
             , 'Phoenix Suns'
        union
        select 'POR'
             , 'Portland Trail Blazers'
        union
        select 'SAC'
             , 'Sacramento Kings'
        union
        select 'SAS'
             , 'San Antonio Spurs'
        union
        select 'TOR'
             , 'Toronto Raptors'
        union
        select 'UTA'
             , 'Utah Jazz'
        union
        select 'WAS'
             , 'Washington Wizards' ) as source ( Identifier, Name )
on ( target.Identifier = source.Identifier )
when matched then update set Name = source.Name
when not matched then insert ( Identifier
                             , Name )
                      values
                           ( source.Identifier, source.Name );
go