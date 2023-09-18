using Amazon.DynamoDBv2;
using LazyStackDynamoDBRepo;
using LazyStackNotificationsSchema.Models;
using Microsoft.AspNetCore.Mvc;
using LazyStackControllerBase;
namespace LazyStackNotificationsRepo.Models;

public class NotificationEnvelope : DataEnvelope<Notification>
{
    public override void SealEnvelope()
    {
        // Set the Envelope Key fields from the EntityInstance data
        TypeName = CurrentTypeName;
        PayloadId = EntityInstance.Id ??= Guid.NewGuid().ToString();
        var guid = Guid.NewGuid().ToString();
        EntityInstance.Id = guid;
        // Primary Key is PartitionKey + SortKey 
        PK = "Notification:"; // Partition key
        SK = $"{guid}:";
        SK1 = $"{EntityInstance.TopicId}:{EntityInstance.CreatedAt.ToString("X16")}:";
        base.SealEnvelope();
    }
    public override string CurrentTypeName { get; set; } = "Notification.v1.0.0";
         
}

public interface INotificationRepo : IDYDBRepository<NotificationEnvelope, Notification>
{
    Task<ActionResult<Notification>> Notification_Create_Async(ICallerInfo callerinfo, Notification data, bool? useCache = null);
    Task<ActionResult<Notification>> Notification_Read_Id_Async(ICallerInfo callerInfo, string id, bool? useCache = null);
    Task<ActionResult<ICollection<Notification>>> Notification_List_TopicId_DateTimeTicks_Async(ICallerInfo callerInfo, string topicId, long dateTimeTicks, bool? useCache = null);
}

public class NotificationRepo : DYDBRepository<NotificationEnvelope, Notification>, INotificationRepo
{
    public NotificationRepo(
        IAmazonDynamoDB client
        ) : base(client, envVarTableName: "TABLE_NAME")
    {
        UpdateReturnsOkResult = false; // just return value
        TTL = 48 * 60 * 60; // 48 hours 
    }

    const string PK = "Notification:";

    public async Task<ActionResult<Notification>> Notification_Create_Async(ICallerInfo callerInfo, Notification data, bool? useCache = null)
        => await CreateAsync(data, callerInfo.Table, useCache: useCache);
    public async Task<ActionResult<Notification>> Notification_Read_Id_Async(ICallerInfo callerInfo, string id, bool? useCache = null)
        => await ReadAsync(pK: PK, sK: $"{id}", callerInfo.Table, useCache: useCache);

    public async Task<ActionResult<ICollection<Notification>>> Notification_List_TopicId_DateTimeTicks_Async(ICallerInfo callerInfo, string topicId, long dateTimeTicks, bool? useCache = null)
    {
        var result = await ListAsync(QueryRange(PK, "SK1", $"{topicId}:{dateTimeTicks.ToString("X16")}:", $"{topicId}:{long.MaxValue.ToString("X16")}:", table: callerInfo.Table), useCache: useCache);
        var value = result.Value ?? new List<Notification>();

        if (value.Count != 0)
            Console.WriteLine($"TopicId:{topicId} range:{topicId}:{dateTimeTicks.ToString("X16")} to {topicId}:{long.MaxValue.ToString("X16")} count:{value.Count}");

        return new ActionResult<ICollection<Notification>>(value);
    }

}