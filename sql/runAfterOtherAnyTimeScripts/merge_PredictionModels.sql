merge nba.PredictionModel as target
using ( select 'Random'
             , 'Randomly predict which team will win'
        union
        select 'HomeTeam'
             , 'Predict that the home team will win'
        union
        select 'AwayTeam'
             , 'Predict that the away team will win'
        union
        select 'MoreWinsLastSeason'
             , 'Predict team with more wins last season to win'
        union
        select 'HigherMarginOfVictoryLastSeason'
             , 'Predict team with higher margin of victory last season to win'
        union
        select 'HigherOffensiveRatingLastSeason'
             , 'Predict team with higher offensive rating last season to win'
        union
        select 'LowerDefensiveRatingLastSeason'
             , 'Predict team with lower defensive rating last season to win'
        union
        select 'MoreInternalWins'
             , 'Predict team with more internal wins to win' ) as source ( Name, Description )
on ( target.Name = source.Name )
when matched then update set Description = source.Description
when not matched then insert ( Name
                             , Description )
                      values
                           ( source.Name, source.Description );
go