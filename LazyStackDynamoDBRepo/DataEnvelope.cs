using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LazyStackDynamoDBRepo
{
    public abstract class DataEnvelope<T> : IDataEnvelope<T>
        where T : class, new()
    {

        public DataEnvelope()
        {
            var now = DateTime.UtcNow.Ticks;
            CreateUtcTick = now;
            UpdateUtcTick = now;
        }

        private Dictionary<string, AttributeValue> _dbRecord;
        public Dictionary<string, AttributeValue> DbRecord 
        {
            get { return _dbRecord; }
            set
            {
                _dbRecord = value;
                SetEntityInstanceFromDbRecord();
            }
        } 

        private T _entityInstance;
        public T EntityInstance 
        { 
            get { return _entityInstance;  }
            set
            {
                _entityInstance = value;
                SetDbRecordFromEntityInstance();
            }
        } // Data entity in latest version form

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

        /// <summary>
        /// You must implement this method
        /// The EntityInstance Set method calls this method.
        /// </summary>
        protected virtual void SetDbRecordFromEntityInstance() 
        {
            _dbRecord = new Dictionary<string, AttributeValue>
            {
                { "TypeName", new AttributeValue() { S = TypeName } },
                { "PK", new AttributeValue() { S = PK } },
                { "SK", new AttributeValue() { S = SK } },
                { "CreateUtcTick", new AttributeValue { N = CreateUtcTick.ToString() } },
                { "UpdateUtcTick", new AttributeValue { N = UpdateUtcTick.ToString() } }
            };

            if (!string.IsNullOrEmpty(SK1))
                _dbRecord.Add("SK1", new AttributeValue() { S = SK1 });

            if (!string.IsNullOrEmpty(SK2))
                _dbRecord.Add("SK2", new AttributeValue() { S = SK2 });

            if (!string.IsNullOrEmpty(SK3))
                _dbRecord.Add("SK3", new AttributeValue() { S = SK3 });

            if (!string.IsNullOrEmpty(SK4))
                _dbRecord.Add("SK4", new AttributeValue() { S = SK4 });

            if (!string.IsNullOrEmpty(SK5))
                _dbRecord.Add("SK5", new AttributeValue() { S = SK5 });

            if (!string.IsNullOrEmpty(GSI1PK))
                _dbRecord.Add("GSI1PK", new AttributeValue() { S = GSI1PK });

            if (!string.IsNullOrEmpty(GSI1SK))
                _dbRecord.Add("GSI1SK", new AttributeValue() { S = GSI1SK });

            if (!string.IsNullOrEmpty(Status))
                _dbRecord.Add("Status", new AttributeValue() { S = Status });

            if (!string.IsNullOrEmpty(General))
                _dbRecord.Add("General", new AttributeValue() { S = General });

            // Serialize the entity data
            // We always serialize to the latest entity version
            if (EntityInstance != null)
            {
                _dbRecord.Add("Data", new AttributeValue() { S = JsonConvert.SerializeObject(EntityInstance) });
            }
        }

        /// <summary>
        /// You must implement this method
        /// The DbRecord Set method calls this method.
        /// </summary>
        protected virtual void SetEntityInstanceFromDbRecord() 
        {
            if (_dbRecord.TryGetValue("PK", out AttributeValue pk))
                PK = pk.S;

            if (_dbRecord.TryGetValue("TypeName", out AttributeValue typeName))
                TypeName = typeName.S;

            if (_dbRecord.TryGetValue("SK", out AttributeValue sk))
                SK = sk.S;

            if (_dbRecord.TryGetValue("CreateUtcTick", out AttributeValue createUtcTick))
                CreateUtcTick = long.Parse(createUtcTick.N);

            if (_dbRecord.TryGetValue("UpdateUtcTick", out AttributeValue updateUtcTick))
                UpdateUtcTick = long.Parse(updateUtcTick.N);

            if (_dbRecord.TryGetValue("SK1", out AttributeValue sk1))
                SK1 = sk1.S;

            if (_dbRecord.TryGetValue("SK2", out AttributeValue sk2))
                SK2 = sk2.S;

            if (_dbRecord.TryGetValue("SK3", out AttributeValue sk3))
                SK3 = sk3.S;

            if (_dbRecord.TryGetValue("SK4", out AttributeValue sk4))
                SK4 = sk4.S;

            if (_dbRecord.TryGetValue("SK5", out AttributeValue sk5))
                SK5 = sk5.S;

            if (_dbRecord.TryGetValue("GSI1PK", out AttributeValue gsi1pk))
                GSI1PK = gsi1pk.S;

            if (_dbRecord.TryGetValue("GSI1SK", out AttributeValue gsi1sk))
                GSI1SK = gsi1sk.S;

            if (_dbRecord.TryGetValue("Status", out AttributeValue status))
                Status = status.S;

            if (_dbRecord.TryGetValue("General", out AttributeValue general))
                General = general.S;

            // serialize the json data to the EntityInstance
            // TODO: we have to figure out our strategy on deserializing old versions

            if (_dbRecord.TryGetValue("Data", out AttributeValue data))
            {
                DeserializeData(data.S, typeName.S);
            }
        }

        /// <summary>
        /// Override this method if you want to customize how conversion among types
        /// This method is called from the SetEntityInstanceFromDbRecord method
        /// </summary>
        /// <param name="data"></param>
        /// <param name="typeName"></param>
        protected virtual void DeserializeData(string data, string typeName)
        {
            _entityInstance = JsonConvert.DeserializeObject<T>(data);
        }
    }
}
