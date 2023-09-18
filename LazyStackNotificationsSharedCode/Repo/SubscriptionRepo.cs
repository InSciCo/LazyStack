using Amazon.DynamoDBv2;
using LazyStackDynamoDBRepo;
using Microsoft.AspNetCore.Mvc;
using LazyStackControllerBase;
namespace LazyStackNotificationsRepo.Models;

public class SubscriptionEnvelope : DataEnvelope<Subscription>
{
    public override void SealEnvelope()
    {
        // Set the Envelope Key fields from the EntityInstance data
        TypeName = CurrentTypeName;
        // Primary Key is PartitionKey + SortKey 
        PK = "Subscription:"; // Partition key
        SK = $"{EntityInstance.Id}:";
        SK1 = $"{EntityInstance.CreatedAt.ToString("X16")}:";
        base.SealEnvelope();
    }
    public override string CurrentTypeName { get; set; } = "Subscription.v1.0.0";
         
}
/// <summary>
/// Repo for CRUDL of Subscription records.
/// </summary>
public interface ISubscriptionRepo : IDYDBRepository<SubscriptionEnvelope, Subscription>
{
    Task<ActionResult<Subscription>> Subscription_Create_Async(ICallerInfo callerinfo, Subscription data, bool? useCache = null);
    Task<ActionResult<Subscription>> Subscription_Read_Id_Async(ICallerInfo callerInfo, string id, bool? useCache = null);
    Task<ActionResult<Subscription>> Subscription_Update_Async(ICallerInfo callerInfo, Subscription body);
    Task<StatusCodeResult> Subscription_Delete_Async(ICallerInfo callerInfo, string id);
    Task<ActionResult<ICollection<Subscription>>> Subscription_List_DateTimeTicks_Async(ICallerInfo callerInfo, long dateTimeTicks, bool? useCache = null);
}

public class SubscriptionRepo : DYDBRepository<SubscriptionEnvelope, Subscription>, ISubscriptionRepo
{
    public SubscriptionRepo(
        IAmazonDynamoDB client
        ) : base(client, envVarTableName: "TABLE_NAME")
    {
        UpdateReturnsOkResult = false; // just return value
        TTL = 48 * 60 * 60; // 48 hours 
    }

    const string PK = "Subscription:";

    public async Task<ActionResult<Subscription>> Subscription_Create_Async(ICallerInfo callerInfo, Subscription data, bool? useCache = null)
        => await CreateAsync(data, callerInfo.Table, useCache: useCache);
    public async Task<ActionResult<Subscription>> Subscription_Read_Id_Async(ICallerInfo callerInfo, string id, bool? useCache = null)
        => await ReadAsync(pK: PK, sK: id, callerInfo.Table, useCache: useCache);
    public async Task<ActionResult<Subscription>> Subscription_Update_Async(ICallerInfo callerInfo, Subscription body)
        => await UpdateAsync(body, callerInfo.Table);
    public async Task<StatusCodeResult> Subscription_Delete_Async(ICallerInfo callerInfo, string id)
        => await DeleteAsync(pK: PK, sK: id, callerInfo.Table);
    public async Task<ActionResult<ICollection<Subscription>>> Subscription_List_DateTimeTicks_Async(ICallerInfo callerInfo, long dateTimeTicks, bool? useCache = null)
    {
        var result = await ListAsync(QueryRange(PK, "SK1", $"{dateTimeTicks.ToString("X16")}:", $"{long.MaxValue.ToString("X16")}:", table: callerInfo.Table), useCache: useCache);
        var value = result.Value ?? new List<Subscription>();
        return new ActionResult<ICollection<Subscription>>(value);
    }
}