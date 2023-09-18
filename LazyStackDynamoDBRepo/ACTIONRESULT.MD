# ActionResult Cribsheet

The ActionResult class can get a little confusing to work with. This is a quick 
reference to help you remember what to use when.

ActionResult is an abstract class that represents the result of an action method. 
It is the base class for all action results.

`ActionResult<T>` is a generic class that represents the result of an action method. 
It is the base class for all action results that return a value.

`T? ActionReult<T>.Value` is the value returned by the action method.
`IActionResult? ActionResut<T>.Result` is the result of the action method.	

The, `ActionResult<T>` **does not** implement the `IActionResult` interface in ASP.NET Core. 
This can lead to confusion. 

1. `IActionResult`: This is an interface that represents an HTTP response from an action. 
The common ActionResults like `OkResult`, `NotFoundResult`, `ContentResult`, etc., all 
implement this interface.
2. `ActionResult<T>`: This is a special type introduced in ASP.NET Core for controller actions.

The `ActionResult<T>` struct that contains two properties:
```
T Value {get;}	
ActionResult Result {get; set;}	
```
When receiving an instance of ActionResult<T> you can interrogate 
the instance in the following ways:	
```
// Getting the IActionResult from the ActionResult<T>
ActionResult<T> result;
// ...
IActionResult actionResult = result.Result as IActionResult; // this will be null if the ActionResult<T> is not an IActionResult
int statusCode = actionResult is not null 
	? actionResult.StatusCode 
	: 500; // or whatever is appropriate for your application
```
This type of code is useful when you are perculating the results of an action 
up through a call chain and have to deal with type changes along the way.
```
ActionResult<T1> resultT1; 
// ...
int statusCode = resultT1.Result is not null 
	? resultT1.Result.StatusCode 
	: 500; // or wahtever is appropriate for your application
var list = new List<T2>();
foreach(var item in resultT1.Value)
{
	list.Add(new T2(item));
}
return new ObjectResult(list) { StatusCode = statusCode };
```


