create table nba.GameBoxScore ( GameId int not null );

grant select, insert, update, delete on nba.GameBoxScore to roleNba;
alter table nba.GameBoxScore add constraint PK_GameBoxScore primary key clustered ( GameId );
alter table nba.GameBoxScore add constraint FK_GameBoxScore_GameId foreign key ( GameId ) references nba.Game ( Id );
create nonclustered index IDX_GameBoxScore_GameId on nba.GameBoxScore ( GameId );
go

create table nba.GameQuarterScore ( Id             int         identity (1, 1)
                                  , GameBoxScoreId int         not null
                                  , QuarterNumber  tinyint     not null
                                  , QuarterLabel   varchar (8) not null
                                  , HomeScore      int         not null
                                  , AwayScore      int         not null );
grant select, insert, update, delete on nba.GameQuarterScore to roleNba;
alter table nba.GameQuarterScore add constraint PK_GameQuarterScore primary key clustered ( Id );
alter table nba.GameQuarterScore
add constraint FK_GameQuarterScore_GameBoxScoreId foreign key ( GameBoxScoreId ) references nba.GameBoxScore ( GameId );
create nonclustered index IDX_GameQuarterScore_GameBoxScoreId on nba.GameQuarterScore ( GameBoxScoreId );
go

create table nba.GameDidNotPlay ( Id             int           identity (1, 1)
                                , GameBoxScoreId int           not null
                                , TeamIdentifier char (3)      not null
                                , PlayerId       int           not null
                                , Reason         varchar (128) not null );
grant select, insert, update, delete on nba.GameDidNotPlay to roleNba;
alter table nba.GameDidNotPlay add constraint PK_GameDidNotPlay primary key clustered ( Id );
alter table nba.GameDidNotPlay
add constraint FK_GameDidNotPlay_GameBoxScoreId foreign key ( GameBoxScoreId ) references nba.GameBoxScore ( GameId );
create nonclustered index IDX_GameDidNotPlay_GameBoxScoreId on nba.GameDidNotPlay ( GameBoxScoreId );
go

create table nba.GamePlayerBasicBoxScore ( Id                     int            identity (1, 1)
                                         , GameBoxScoreId         int            not null
                                         , PlayerId               int            not null
                                         , TeamIdentifier         char (3)       not null
                                         , Starter                bit            not null
                                         , SecondsPlayed          int            not null
                                         , FieldGoalsMade         int            not null
                                         , FieldGoalsAttempted    int            not null
                                         , ThreePointersMade      int            not null
                                         , ThreePointersAttempted int            not null
                                         , FreeThrowsMade         int            not null
                                         , FreeThrowsAttempted    int            not null
                                         , OffensiveRebounds      int            not null
                                         , DefensiveRebounds      int            not null
                                         , TotalRebounds          int            not null
                                         , Assists                int            not null
                                         , Steals                 int            not null
                                         , Blocks                 int            not null
                                         , Turnovers              int            not null
                                         , PersonalFouls          int            not null
                                         , Points                 int            not null
                                         , GameScore              decimal (6, 2) not null
                                         , PlusMinusScore         int            not null );
grant select, insert, update, delete on nba.GamePlayerBasicBoxScore to roleNba;
alter table nba.GamePlayerBasicBoxScore add constraint PK_GamePlayerBasicBoxScore primary key clustered ( Id );
alter table nba.GamePlayerBasicBoxScore
add constraint FK_GamePlayerBasicBoxScore_GameBoxScoreId foreign key ( GameBoxScoreId ) references nba.GameBoxScore ( GameId );
alter table nba.GamePlayerBasicBoxScore
add constraint FK_GamePlayerBasicBoxScore_PlayerId foreign key ( PlayerId ) references nba.Player ( Id );
create nonclustered index IDX_GamePlayerBasicBoxScore_GameBoxScoreId on nba.GamePlayerBasicBoxScore ( GameBoxScoreId );
create nonclustered index IDX_GamePlayerBasicBoxScore_PlayerId on nba.GamePlayerBasicBoxScore ( PlayerId );
go

create table nba.GamePlayerAdvancedBoxScore ( Id                           int            identity (1, 1)
                                            , GameBoxScoreId               int            not null
                                            , PlayerId                     int            not null
                                            , TeamIdentifier               char (3)       not null
                                            , TrueShootingPercentage       decimal (8, 6) not null
                                            , EffectiveFieldGoalPercentage decimal (8, 6) not null
                                            , ThreePointAttemptRate        decimal (8, 6) not null
                                            , FreeThrowsAttemptRate        decimal (8, 6) not null
                                            , OffensiveReboundPercentage   decimal (8, 6) not null
                                            , DefensiveReboundPercentage   decimal (8, 6) not null
                                            , TotalReboundPercentage       decimal (8, 6) not null
                                            , AssistPercentage             decimal (8, 6) not null
                                            , StealPercentage              decimal (8, 6) not null
                                            , BlockPercentage              decimal (8, 6) not null
                                            , TurnoverPercentage           decimal (8, 6) not null
                                            , UsagePercentage              decimal (8, 6) not null
                                            , OffensiveRating              int            not null
                                            , DefensiveRating              int            not null
                                            , BoxPlusMinus                 decimal (6, 2) not null );
grant select, insert, update, delete on nba.GamePlayerAdvancedBoxScore to roleNba;
alter table nba.GamePlayerAdvancedBoxScore add constraint PK_GamePlayerAdvancedBoxScore primary key clustered ( Id );
alter table nba.GamePlayerAdvancedBoxScore
add constraint FK_GamePlayerAdvancedBoxScore_GameBoxScoreId foreign key ( GameBoxScoreId ) references nba.GameBoxScore ( GameId );
alter table nba.GamePlayerAdvancedBoxScore
add constraint FK_GamePlayerAdvancedBoxScore_PlayerId foreign key ( PlayerId ) references nba.Player ( Id );
create nonclustered index IDX_GamePlayerAdvancedBoxScore_GameBoxScoreId
  on nba.GamePlayerAdvancedBoxScore ( GameBoxScoreId );
create nonclustered index IDX_GamePlayerAdvancedBoxScore_PlayerId on nba.GamePlayerAdvancedBoxScore ( PlayerId );
go

create type nba.tt_Merge_GameBoxScore as table ( GameId int not null );

create type nba.tt_Merge_GameQuarterScore as table ( GameBoxScoreId int         not null
                                                   , QuarterNumber  tinyint     not null
                                                   , QuarterLabel   varchar (8) not null
                                                   , HomeScore      int         not null
                                                   , AwayScore      int         not null );

create type nba.tt_Merge_GameDidNotPlay as table ( GameBoxScoreId  int            not null
                                                 , TeamIdentifier  char (3)       not null
                                                 , SeasonStartYear int            not null
                                                 , PlayerName      nvarchar (128) not null
                                                 , Reason          varchar (128)  not null );

create type nba.tt_Merge_GamePlayerBasicBoxScore as table ( GameBoxScoreId         int            not null
                                                          , TeamIdentifier         char (3)       not null
                                                          , PlayerName             nvarchar (128) not null
                                                          , Starter                bit            not null
                                                          , SecondsPlayed          int            not null
                                                          , FieldGoalsMade         int            not null
                                                          , FieldGoalsAttempted    int            not null
                                                          , ThreePointersMade      int            not null
                                                          , ThreePointersAttempted int            not null
                                                          , FreeThrowsMade         int            not null
                                                          , FreeThrowsAttempted    int            not null
                                                          , OffensiveRebounds      int            not null
                                                          , DefensiveRebounds      int            not null
                                                          , TotalRebounds          int            not null
                                                          , Assists                int            not null
                                                          , Steals                 int            not null
                                                          , Blocks                 int            not null
                                                          , Turnovers              int            not null
                                                          , PersonalFouls          int            not null
                                                          , Points                 int            not null
                                                          , GameScore              decimal (6, 2) not null
                                                          , PlusMinusScore         int            not null );

create type nba.tt_Merge_GamePlayerAdvancedBoxScore as table ( GameBoxScoreId               int            not null
                                                             , TeamIdentifier               char (3)       not null
                                                             , PlayerName                   nvarchar (128) not null
                                                             , TrueShootingPercentage       decimal (8, 6) not null
                                                             , EffectiveFieldGoalPercentage decimal (8, 6) not null
                                                             , ThreePointAttemptRate        decimal (8, 6) not null
                                                             , FreeThrowsAttemptRate        decimal (8, 6) not null
                                                             , OffensiveReboundPercentage   decimal (8, 6) not null
                                                             , DefensiveReboundPercentage   decimal (8, 6) not null
                                                             , TotalReboundPercentage       decimal (8, 6) not null
                                                             , AssistPercentage             decimal (8, 6) not null
                                                             , StealPercentage              decimal (8, 6) not null
                                                             , BlockPercentage              decimal (8, 6) not null
                                                             , TurnoverPercentage           decimal (8, 6) not null
                                                             , UsagePercentage              decimal (8, 6) not null
                                                             , OffensiveRating              int            not null
                                                             , DefensiveRating              int            not null
                                                             , BoxPlusMinus                 decimal (6, 2) not null );
go

grant exec on type::nba.tt_Merge_GameBoxScore to roleNba;
grant exec on type::nba.tt_Merge_GameQuarterScore to roleNba;
grant exec on type::nba.tt_Merge_GameDidNotPlay to roleNba;
grant exec on type::nba.tt_Merge_GamePlayerBasicBoxScore to roleNba;
grant exec on type::nba.tt_Merge_GamePlayerAdvancedBoxScore to roleNba;
go