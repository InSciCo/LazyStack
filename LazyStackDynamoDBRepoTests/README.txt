LazyStackDynamoDBRepoTests

The LazyStacksDynamoDBRepo (DBRepo) maps CRUDL operations onto DynamoDBv2.Model namespace 
operations (low level access). 
Note that DynamoDB offers a variety of access libraries. This class uses the "Low Level" 
interfaces available in the DynamoDBv2.Model namespace.
https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/DynamoDBv2/NDynamoDBv2Model.html

We won't try and summerize the DynamoDB documentation here. Instead, we describe how we 
use DynamoDB in a prescriptive way to implement simple document storage and retrieval with 
the following features:
CRUDL
- Create 
- Read 
- Update - with optimistic locking
- Delete
- List 

Automatic Version Transforms
- Convert older versions of saved document content to newer version of that content on read
	- this eliminates downtime due to database schema changes 

Prescriptive Implementation of "Pseudo" Indexing
DynamoDB has a limited range of indexed record retrieval operations. There are no "indicies" in the 
SQL sense. We implement a pseudo index strategy using strict composite key construction rules. 
This approach abstracts much of the schema information out of DynamoDB into the application layer. 
The advantage of this approach is that we can follow the suggested guidelines from Amazon DynamoDB 
Architects for using a single table in an elegant and consistent way.

Essentially we create an "Envelope" of the data stored in each record. This envelope contains fields whose 
values are set based on the entity data being stored in a "Data" attribute. This prescriptive approach is 
partially implemented by the DataEnvelope virtual class. You complete the implementation by overriding these 
DataEnvelope methods:
	- SetDbRecordFromEntityInstance() // Updates Envelope from Entity Instance values
	- DataEnvelope.SetEntityInstanceFromDbRecord() // updates Entity Instance values from Envelope
	- DeserializeData(string data, string typeName) // Allow conversion of stored data entity version to most recent version

Presciptive Approach:
All table entries will have the following attributes:
string PK - holds partition key value 
string SK - holds sort key value 
string SK1 - holds sortkey1 value (may be null)
string SK2 - holds sortkey2 value (may be null)
string SK3 - holds sortkey3 value (may be null)
string SK4 - holds sortkey4 value (may be null)
string SK5 - holds sortkey5 value (may be null)
string GSI1PK - holds Global secondary index PK value (may be null) 
string GSI1SK - holds Glocal secondary index SK value (may be null)
string Status - Application defined status 
long UpdateUtcTick - Used in optimistic locking
long CreatedUtcTick - Used in optimistic locking
string General - Application defined value (optional and usually only a small amount of app data stored in index tables)
string Data - Contains a JSON representation of the data entity
string TypeName - Contains a type version we can use in a Automatic Version Transform process

We define the following indices:
KeySchema: PK + SK  (this is the primary key eg: Partition Key + Sort Key)

Local Secondary Indices: (Note: LSIs are sparse. Entries exist only if the associated sort key is not null)
PK-SK1-Index: PK + SK1 projection: SK, Status, UpdateUtcTick, CreateUtcTick, General
PK-SK2-Index: PK + SK2 projection: SK, Status, UpdateUtcTick, CreateUtcTick, General
PK-SK3-Index: PK + SK3 projection: SK, Status, UpdateUtcTick, CreateUtcTick, General
PK-SK4-Index: PK + SK4 projection: SK, Status, UpdateUtcTick, CreateUtcTick, General
PK-SK5-Index: PK + SK5 projection: SK, Status, UpdateUtcTick, CreateUtcTick, General

Global Secondary Indexs: (Note: GSIs are sparse. Entries exist only if the GSI1PK partitiona key is not null)
GSI1: GSI1PK + GSI1SK projection: PK, SK, Status, UpdateUtcTick, CreateUtcTick, General

Application Level Pattern for Composite Key construction 
Since AWS recommends an application have only one "Table", we need to use a composite key construction rule set to 
store various "data entities" in that table. Here is a simple ruleset:

PK - contains an identifier with the plural form of the data entity. Example Pets
SK - contains an identifier with the singular form of the data entity and an Id Example: Pet:1
SKn - 
