import React from "react";
import Amplify, { Auth, API } from "aws-amplify";
import { AmplifyAuthenticator, AmplifySignOut } from "@aws-amplify/ui-react";
import awsconfig from './aws-exports';
Amplify.configure(awsconfig);


const App = () => (
    <AmplifyAuthenticator>
        <div>
            <button onClick={getUserHttpApi}>GetUserHttpApi</button>
            <button onClick={getUserApi}>GetUserApi</button>
            <button onClick={getTestJsonApi}>GetTestJsonApi</button>
            <AmplifySignOut />
        </div>
    </AmplifyAuthenticator>
);

async function getUserHttpApi() {
    var apiName = 'HttpApiSecure';
    var path = '/httpapi/user';
    //var myInit = {
    //    headers: {
    //        Authorization: `${(await Auth.currentSession()).getAccessToken().getJwtToken()}`,
    //    },
    //};
    console.log("JWT", (await Auth.currentSession()).getAccessToken().getJwtToken());

    API
        .get(apiName, path)
        .then(response => {
            alert(response);
        })
        .catch(error => {
            console.log("the error", error);
        });
}

// Warning
// This function throws a CORS error.
// xhr.js:187 GET https://12345678901.execute-api.us-east-1.amazonaws.com/Dev/api/user net::ERR_FAILED 200
// The preflight call works correctly.
// The lambda fires and returns a value.
// The browser throws the error after receiving what seems to be a valid response.
// This is being investigated.
// Note: The same call from C# against the Api Gateway works fine. Of course, there isn't any CORS involved 
// in that call. 
async function getUserApi() {
    var apiName = 'ApiSecure';
    var path = '/api/user';
    console.log("SecretKey",)

    // Print keys for side-by-side tests in postman
    console.log("JWT", (await Auth.currentSession()).getAccessToken().getJwtToken());
    console.log("AccessKey", (await Auth.currentCredentials()).accessKeyId);
    console.log("SecretKey", (await Auth.currentCredentials()).secretAccessKey);
    console.log("SessionToken", (await Auth.currentCredentials()).sessionToken);

    API
        .get(apiName, path)
        .then(response => {
            alert(response.name);
        })
        .catch(error => {
            console.log("the error", error);
        });
}

// Warning
// This function throws a CORS error.
// xhr.js:187 GET https://12345678901.execute-api.us-east-1.amazonaws.com/Dev/api/testjson net::ERR_FAILED 200
// The preflight call works correctly.
// The lambda fires and returns a value.
// The browser throws the error after receiving what seems to be a valid response.
// This is being investigated.
// Note: The same call from C# against the Api Gateway works fine. Of course, there isn't any CORS involved 
// in that call. 
async function getTestJsonApi() {
    var apiName = 'ApiSecure';
    var path = '/api/testjson';
    console.log("SecretKey",)

    // Print keys for side-by-side tests in postman
    console.log("JWT", (await Auth.currentSession()).getAccessToken().getJwtToken());
    console.log("AccessKey", (await Auth.currentCredentials()).accessKeyId);
    console.log("SecretKey", (await Auth.currentCredentials()).secretAccessKey);
    console.log("SessionToken", (await Auth.currentCredentials()).sessionToken);

    API
        .get(apiName, path)
        .then(response => {
            alert(response.name);
        })
        .catch(error => {
            console.log("the error", error);
        });

}


export default App;
