LazyStackAuthJs Experimental Lib

This experimental lib exists because Microsoft's .NET core lib used in Blazor does not implement
all the crypto functions necessary to support the AWSSDK Cognito authentication features. MS has 
stated they may address this issue in .NET 7.

This library uses Blazor Interop to call the AWS Amplify javascript libs to authenticate against 
AWS Cognito.

This is a sub-optimal approach for a number of reasons - a true and ugly hack.

On the other hand, it is a useful example of using JS from Blazor in a non-trivial way.

