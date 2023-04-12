using Dapper;
using Microsoft.Extensions.Options;
using Route256.Week5.Homework.PriceCalculator.Dal.Entities;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;
using Route256.Week5.Homework.PriceCalculator.Dal.Repositories.Interfaces;
using Route256.Week5.Homework.PriceCalculator.Dal.Settings;

namespace Route256.Week5.Homework.PriceCalculator.Dal.Repositories;

public class CalculationRepository : BaseRepository, ICalculationRepository
{
    public CalculationRepository(
        IOptions<DalOptions> dalSettings) : base(dalSettings.Value)
    {
    }

    public async Task<long[]> Add(
        CalculationEntityV1[] entityV1,
        CancellationToken token)
    {
        const string sqlQuery = @"
insert into calculations (user_id, good_ids, total_volume, total_weight, price, at)
select user_id, good_ids, total_volume, total_weight, price, at
  from UNNEST(@Calculations)
returning id;
";

        var sqlQueryParams = new
        {
            Calculations = entityV1
        };

        await using var connection = await GetAndOpenConnection();
        var ids = await connection.QueryAsync<long>(
            new CommandDefinition(
                sqlQuery,
                sqlQueryParams,
                cancellationToken: token));

        return ids
            .ToArray();
    }

    public async Task Delete(long[] calculationIds, CancellationToken token)
    {
        const string sqlQuery = @"
delete
  from calculations
 where id in (select unnest(@CalculationIds))
";

        var sqlQueryParams = new
        {
            CalculationIds = calculationIds
        };

        await using var connection = await GetAndOpenConnection();
        var calculations = await connection.QueryAsync(
            new CommandDefinition(
                sqlQuery,
                sqlQueryParams,
                cancellationToken: token));
    }

    public async Task<CalculationEntityV1[]> Query(
        CalculationHistoryQueryModel query,
        CancellationToken token)
    {
        const string sqlQuery = @"
select id
     , user_id
     , good_ids
     , total_volume
     , total_weight
     , price
     , at
  from calculations
 where user_id = @UserId
 order by at desc
 limit @Limit offset @Offset
";

        var sqlQueryParams = new
        {
            UserId = query.UserId,
            Limit = query.Limit,
            Offset = query.Offset
        };

        await using var connection = await GetAndOpenConnection();
        var calculations = await connection.QueryAsync<CalculationEntityV1>(
            new CommandDefinition(
                sqlQuery,
                sqlQueryParams,
                cancellationToken: token));

        return calculations
            .Where(x => query.CalculationIds.Contains(x.Id) || query.CalculationIds.Length == 0)
            .ToArray();
    }

    public async Task<CalculationIdsModel[]> QueryIds(long[] calculationIds, CancellationToken token)
    {
        const string sqlQuery = @"
select id
     , user_id
     , good_ids
  from calculations
 where id in (select unnest(@CalculationIds))
";

        var sqlQueryParams = new
        {
            CalculationIds = calculationIds
        };

        await using var connection = await GetAndOpenConnection();
        var calculations = await connection.QueryAsync<CalculationIdsModel>(
            new CommandDefinition(
                sqlQuery,
                sqlQueryParams,
                cancellationToken: token));

        return calculations
            .ToArray();
    }

    public async Task<CalculationIdsModel[]> QueryIds(long userId, CancellationToken token)
    {
        const string sqlQuery = @"
select id
     , user_id
     , good_ids
  from calculations
 where user_id = @UserId
";

        var sqlQueryParams = new
        {
            UserId = userId
        };

        await using var connection = await GetAndOpenConnection();
        var calculations = await connection.QueryAsync<CalculationIdsModel>(
            new CommandDefinition(
                sqlQuery,
                sqlQueryParams,
                cancellationToken: token));

        return calculations
            .ToArray();
    }
}