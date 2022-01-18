# LazyStackDynamoDBRepoTests

Before running the tests, publish the serverless.template with a stackname of "TestRepo-DB".

The LazyStacksDynamoDBRepo (DBRepo) maps CRUDL operations onto DynamoDBv2.Model namespace 
operations (low level access). 

Note that DynamoDB offers a variety of access libraries. This class uses the "Low Level" 
interfaces available in the DynamoDBv2.Model namespace.
https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/DynamoDBv2/NDynamoDBv2Model.html
