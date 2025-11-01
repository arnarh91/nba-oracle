/*--------------------------------------------------
-- ROLE(S)
--------------------------------------------------*/
if database_principal_id( N'roleNba' ) is null
begin
  create role roleNba authorization dbo;
end;