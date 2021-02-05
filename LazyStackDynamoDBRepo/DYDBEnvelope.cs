namespace LazyStackDynamoDBRepo
{
    public abstract class DYDBEnvelope : IDYDBEnvelope
    {

        public string Data { get; set; } // serialized data (JSON)
        public string TypeName { get; set; } // name of class serialized into the Data string

        public string PK { get; set; }

        public string SK { get; set; }

        // Note: DO NOT add annotations to the optional attributes below
        // Doing so will cause an error in the DynamoDB library.
        // Instead, you must declare local secondary index range keys and Global secondary index keys
        // when you create the table using the SAM template.

        public string SK1 { get; set; } = null;

        public string SK2 { get; set; } = null;

        public string SK3 { get; set; } = null;

        public string SK4 { get; set; } = null;

        public string SK5 { get; set; } = null;

        public string GSI1PK { get; set; } = null;

        public string GSI1SK { get; set; } = null;

        public string Status { get; set; } = null; // Projection attribute

        public long CreateUtcTick { get; set; } = 0; // Projection attribute

        public long UpdateUtcTick { get; set; } = 0; // Projection attribute

        public string General { get; set; } = null; // Projection attribute

    }

    public class DefaultEnvelope : DYDBEnvelope
    {
        public DefaultEnvelope() { }
    }
}
