LazyStackAuthTestReact

This project was bootstrapped with [Create React App](https://github.com/facebookincubator/create-react-app).

You can find the react guide [here](https://github.com/facebookincubator/create-react-app/blob/master/packages/react-scripts/template/README.md).

Testing Notes:
The purpose of this react project is to test portions of the AWS-Amplify libraries used to make calls against 
the serverless stack created by the LazyStackAuthTests project.

Prerequsites:
Run the LazyStackAuthTests project in debug mode. See that projects readme.

Run this project using IIS Express.
- Note this copies the aws-exports.js file from the LazyStackAuthTests\bin\Debug\netcore3.1 to the local src folder.
- aws-exports.js contains the configuration information necessary for this project's' ClientApp to run against 
  the LazyStackAuthTests stack. 

Perform Test:
- Run this project  using IIS Express
- Use the webpage presented to create an account
- Select each button to test the functions.

Known Issue: CORS error for AWS::Serverless::Api Gateways
- Background: The LazyStackAuthTests stack creates two Api Gateways
	- HttpApiSecure of type  AWS::Serverless::HttpApi
	- ApiSecure of type AWS::Serverless::Api

- Sympton:
	Calls to the ApiSecure gateway report a CORS error. The calls are succesfully dispatched to the 
	gateway, processed by the proper Lambda, and return the expected value. However, a CORs error is reported:
	xhr.js:187 GET https://12345678901.execute-api.us-east-1.amazonaws.com/Dev/api/user net::ERR_FAILED 200

	After a few days of research we haven't been able to figure this one out so we are releasing this new 
	LazyStack repo with this error so we can deliver other more important features in a timely way. We will 
	come back to this error in a future release.

- Impact/Remediation:
	Use HttpApi (AWS::Serverless::HttpApi) gateways with browser based clients.



		