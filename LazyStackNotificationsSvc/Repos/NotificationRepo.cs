using Amazon.DynamoDBv2;
using LazyStackDynamoDBRepo;
using LazyStackNotificationsSchema.Models;
using Microsoft.AspNetCore.Mvc;

namespace LazyStackNotificationSvc.Repos;

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
        PK = "Participant:"; // Partition key
        SK = $"{guid}:";
        SK1 = $"{EntityInstance.TopicId}:{EntityInstance.CreatedAt.ToString("X16")}:";
        base.SealEnvelope();
    }
}

public interface INotificationRepo : IDYDBRepository<NotificationEnvelope, Notification>
{
    Task<ActionResult<Notification>> Notification_Create_Async(string table, string lzUserId, Notification data, bool? useCache = null);
    Task<ActionResult<Notification>> Notification_Read_Id_Async(string table, string lzUserId, string id, bool? useCache = null);
    Task<ActionResult<ICollection<Notification>>> Notification_List_TopicId_DateTimeTicks_Async(string table, string lzUserId, string topicId, long dateTimeTicks, bool? useCache = null);
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

    public async Task<ActionResult<Notification>> Notification_Create_Async(string table, string lzUserId, Notification data, bool? useCache = null)
        => await CreateAsync(data, table, useCache: useCache);
    public async Task<ActionResult<Notification>> Notification_Read_Id_Async(string table, string lzUserId, string id, bool? useCache = null)
        => await ReadAsync(pK: PK, sK: $"{id}", table, useCache: useCache);

    public async Task<ActionResult<ICollection<Notification>>> Notification_List_TopicId_DateTimeTicks_Async(string table, string lzUserId, string topicId, long dateTimeTicks, bool? useCache = null)
    {
        var result = await ListAsync(QueryRange(PK, "SK1", $"{topicId}:{dateTimeTicks.ToString("X16")}:", $"{topicId}:{long.MaxValue.ToString("X16")}:", table: table), useCache: useCache);
        var value = result.Value ?? new List<Notification>();

        if (value.Count != 0)
            Console.WriteLine($"TopicId:{topicId} range:{topicId}:{dateTimeTicks.ToString("X16")} to {topicId}:{long.MaxValue.ToString("X16")} count:{value.Count}");

        return new ActionResult<ICollection<Notification>>(value);
    }

}