using System;
using System.Data;
using Dapper;

namespace NbaOracle.Infrastructure.Persistence;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object? value)
    {
        if (value is null or DBNull)
            return null;
        
        return DateOnly.FromDateTime((DateTime)value);
    } 

    public override void SetValue(IDbDataParameter parameter, DateOnly? value)
    {
        if (value is null)
            return;
        
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }
}