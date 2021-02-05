
namespace LazyStackDynamoDBRepo
{
    /// <summary>
    /// The Envelope is the record we store in the database.
    /// Because DynamoDB is a NoSQL database and does not provide 
    /// sophisticated indexing like a SQL database, we implement
    /// a ISAM style indexing strategy.
    /// See the LazyStack.io documentation for more information on
    /// how to map your ER into a DynamoDB table using this 
    /// Evenlope.
    /// </summary>
    public interface IDYDBEnvelope
    {
        /// <summary>
        /// Serialized Data (JSON)
        /// </summary>
        string Data { get; set; }

        /// <summary>
        /// name of class serialized into the Data string
        /// </summary>
        string TypeName { get; set; }

        /// <summary>
        /// Partition Key
        /// </summary>
        string PK { get; set; }

        /// <summary>
        /// Sort Key. Also called Range Key
        /// </summary>
        string SK { get; set; }

        /// <summary>
        /// for LSI PK-SK1-Index
        /// </summary>
        string SK1 { get; set; }

        /// <summary>
        /// for LSI PK-SK2-Index
        /// </summary>
        string SK2 { get; set; }

        /// <summary>
        /// for LSI PK-SK3-Index
        /// </summary>
        string SK3 { get; set; }

        /// <summary>
        /// for LSI PK-SK4-Index
        /// </summary>
        string SK4 { get; set; }

        /// <summary>
        /// for LSI PK-SK5-Index
        /// </summary>
        string SK5 { get; set; }

        // Global Secondary Indices

        /// <summary>
        /// partition key for GSI1
        /// </summary>
        string GSI1PK { get; set; }

        /// <summary>
        /// sort key for GSI1
        /// </summary>
        string GSI1SK { get; set; }

        /// <summary>
        /// Projection attribute
        /// </summary>
        string Status { get; set; }

        /// <summary>
        ///  Projection attribute
        /// </summary>
        long UpdateUtcTick { get; set; }

        /// <summary>
        /// Projection attribute
        /// </summary>
        long CreateUtcTick { get; set; }

        /// <summary>
        /// Projection attribute - kitchen sink
        /// </summary>
        string General { get; set; } 
    }
}
