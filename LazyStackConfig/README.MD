# Software Configuration Management
Comprehensive "Software Configuration Management" (SCM) is not provided by the Visual Studio IDE or by version control systems like git. 

Comprehensive SCM addresses the cross-cutting concerns of Development, Testing and Production by decoupling an application's configuration from the application code base to attain sufficient flexibility to manage each environment with the least amount of DevOps and Development overhead.

The SCM strategy presented here is comprised of conventions for how repositories are checked
out to your workstation or build machine instance, and conventions for managing configurations 
in repositories separate from your code solution repositories. 

There are two types of configuration managed:
- **Build time** configuration files are embedded into a project assembly
- **Deploy time** configuration files are copied to a folder or API accessible by application at run time (example: wwwroot). We often seed the configuration files at build time for environments like dev and test.

Some of the cross-cutting concerns addressable by this configuration management strategy:
- Specific configuration by environment
- Management of any number of environments 
- Management of configuration(s) separately from application code development
  - developers can manage their own environments without affecting test or production 
  - devops engineers can easily audit/manage changes to configurations required by applications
- Avoid configuration bleed across environments - avoid including configuration information not actually used by an environment. Configuration information included in an assembly, especially in a client app, may be accidentally used, or worse, extracted by a bad actor.
- Seamless integrations with CICD pipelines. CI generally uses build time configuration while CD generally uses deploy time configurations. CICD pipelines may override or augment the configurations.

Note that these configurations do not affect MSBuild Debug or Release configurations. Developers
and DevOps engineers are responsible to ensuring that Debug, Release or other project level 
build configurations are performed and deployed as necessary. The SCM strategy presented here 
manages default configuration settings supplied to the application in each environment. Be 
aware that it is not uncommon for CI/CD pipelines to override these defaults either before 
embedding during the build (eg. integration), or by overwriting copied configurations after 
a build (eg. deployment).

## Embedding versus Copying 
Embedding config files in assembly manifests has two advantages:
- The files are never in your projects source tree and will never be commited to source control
- The files are a little more difficult to extract in client apps (Obscurity is not security but this helps a bit)

Copying config files to a project folder is helpful when you want the files to be easily accessed 
by the client at runtime. CD Pipelines can overwrite these files during deployment avoiding a build step.

## File System Folder Hierarchy Convention
We implement a simple Dev, Test, Prod development workflow by convention. You can have any 
number of environments, but for smaller teams, we highly recommend this simple workflow.
The convention is to have each environment under separate "env" folders in file system. 
In the example below, these env folders are "_Dev", "_Test", and "_Prod". We use these 
folder names to set some defaults in the configuration process but they are soft - meaning
you can easily override the defaults if you don't follow the env folder naming convention.

In the example below we are embedding config files in our service and embedding and copying config files 
into our client. You may do either embedding or copying or both in any project by referencing 
the config folders as described later in this README. 

- repos/_Dev/ -- solutions under this folder use a dev branch
  - AppName
  - AppName.Config
    - ClientConfigToEmbed
    - ClientConfigToCopy
    - ServiceConfigToEmbed
    - WebApiConfigToEmbed
  - AppName.Clients 
- repos/_Test/ -- solutions under this folder use the test branch
    - AppName
    - AppName.Config
        - ClientConfigToEmbed
        - ClientConfigToCopy
        - ServiceConfigToEmbed
        - WebApiConfigToEmbed
    - AppName.Clients 
- repos/_Prod/ -- solutions under this folder use the master/main branch
  - AppName
  - AppName.Config
    - ClientConfigToEmbed
    - ClientConfigToCopy
    - ServiceConfigToEmbed
    - WebApiCofigToEmbed
  -AppName.Clients

Note that the names "ClientConfigToEmbed", "ClientConfigToCopy", "ServiceConfigToEmbed", and "WebApiConfigToEmbed" may 
be anything you choose. If you have many projects in your solutions that are being independently configured, 
you would normally use config folder names that make it easy to understand which configuration folders are 
being used by which projects. Also, you can have multiple configuration folders used by a single project and have a 
configuration folder used by multiple projects. 

### ClientConfigToEmbed
This folder contains configuration files that are embeded into a project assembly used in the Client app. 

### ClientCofitToCopy
This folder contains configuration files that are copied into a the client's wwwroot/config folder.


### ServiceConfigToEmbed
This folder contains configuration files that are embedded into a a project assembly used in the service. 

### WebApiConfigToEmbed
This folder contains configuration files that are embedded into the WebApi project assembly for local debugging.

### Notes: 
- It is up to the developer to ensure the correct branches are used for each 
git repo. Only the environments the developer is working on need to be checked out. 
- A developer might have multiple dev branches on their machine in some 
complex workflows. Each of those should be under a different env folder. If a developer is just switching among dev branches, then only a single dev env folder may suffice. 
- You can use either or both embedded and copied configurations in your projects. It depends on your specific requirements as to when to use either approach.

## Configuration use in .NET Projects
Use the configuration files in your .csproj file like this:
```
<PropertyGroup>
    <SourceEmbedConfigFolder>..\..\AppName.Config\ClientConfigToEmbed</SourceEmbedConfigFolder>
</PropertyGroup>
<Import Project="..\..\LazyStackConfig\EmbedConfig.xml" /> 
```
or
```
<PropertyGroup>
    <SourceCopyConfigFolder>..\..\AppName.Config\ClientConfigToCopy</SourceCopyConfigFolder>
</PropertyGroup>
<Import Project="..\..\LazyStackConfig\CopyConfig.xml" /> 
```

Set the property relative folder reference based on the depth of your project in the 
folder hierarchy.

### EmbedConfig.xml 
The EmbedConfig.xml project script embeds json config files to your project assembly where you can read and process 
them. See sample client and service projects for examples of how to load json config files from an assembly. 

The LazyStackConfig project provides the static class LzConfig with a static method 
ReadEmbeddedConfig(Assembly assembly) that loads and merges each json config file into a single 
JObject. Of course, you can use your own configuration loader. 

### CopyConfig.xml
The CopyConfig.xml project script copies json config files to a specified project's $(DestCopyConfigFolder) folder,
which defaults to "wwwroot\config". If you want to change the default, do something like this:
```
<PropertyGroup>
    <SourceCopyConfigFolder>..\..\AppName.Config\ClientConfigToCopy</SourceCopyConfigFolder>
    <DestCopyConfigFolder>myfolder</DestCopyConfigFolder>
</PropertyGroup>
<Import Project="..\..\LazyStackConfig\CopyConfig.xml" /> 
```

## AppName.Config repository folders contain:

### currentenv.xml
**currentenv.xml** -- optional top level config specifies env file to use. 

**Note that currentenv.xml is not a tracked file.** You must create it in your local folder. This allows each directory hierarchy to use different configurations. 

If **currentenv.xml** doesn't exist then the env file default is set based on this logic:
- env.prod.xml -- used when no other default is found
- env.dev.xml -- used when config folder path contains the string "_Dev"
- env.test.xml -- used when config folder path contains the string "_Test"
- env.prod.xml -- used when config folder path contains the string "_Prod"
 
#### Env files
- env.dev.xml -- list of config files to load for dev env
- env.test.xml -- list of config files to load for test env
- end.prod.xml -- list of config files to load for prod env

If no env file is found in the folder, then all JSON config files are loaded.

#### JSON Config files examples:
- config.dev.json -- usually generated by LazyStackMDD for client configuration
- config.test.json -- usually generated by LazyStackMDD for client configuration
- config.env.prod.json  -- usually generated by LazyStackMDD for client configuration
- config.webapi.json -- usually static, defines local web service ports etc. for client configuration

## What do config.*.json files contain?
Anything you need. For LazyStack applications we generate ClientConfigToCopy/config.*.json 
files containing connectivity information to AWS resources and implement the 
project logic necessary to use those configurations. 

Since you are responsible for implementing the json config files in your project, 
you can do anything necessary to address your specific requirements. 

## Don't put secrets in configuration files 
Do not, under any circumstance, place secrets in configuration files. Even embedded config files.

But, hang on, what is a secret?

Generally, API keys and client ids are not secrets. One way or another, they can be 
extracted from a running application - especially clients. 

Passwords are a great example of a secret. Another example is an AWS Secret Access Key provided 
to allow the AWS CLI to make calls to your AWS resources. It's ok to have a AWS profile name 
in a configuration file, but not a AWS Secret Access Key. 

If you need to retrieve a secret in a service consider using AWS Secrets service and giving your 
service permissions to use the service. This way the secrets can be updated (rotated on 
a schedule for instance) without having to rebuild your application service. And, since the
permissions are managed by the deployment, there is never any secret to exploit in your source.

## CI Pipelines 
When build applications using CI Pipelines, a pipeline process checks out the repos and builds the solutions. 
It is best practice to write currentenv.xml as part of your pipeline process instead of relying on the default env file selection.

## CD Pipelines
When an application is deployed with a CD pipeline, the pipeline may overwrite configuration files in the projects 
output directories. Note that this is not possible with embedded config files. Often, the config files copied 
into project folders during the build process represent "default" values with the expectation that a CD 
pipeline will overwrite the value when needed. Another common scenario is the app may read, at runtime, configuration 
values from a service or local store; again the values copied in at build time service as defaults.

Note that Microsoft.Extensions.Configuration Configuration object is immutable after it 
is constructed, so it can be painful to use with the override at runtime strategy. One way to handle this is 
to treat the values in in the Configuration instance only as defaults.

### Special Use Cases 
There is very little "hard-coded" in this configuration processing strategy. You can manage any number of 
environments and config files. Only the default env.prod.xml files is specifically referenced by the 
configbuildscript.xml and configdeployscript.xml. If you always write the client.xml or service.xml file
in your pipelines then even the use of this default file name is avoided.

## Complexity versus Capability
When implementing configuration management, we always walk a fine line between complexity and capability. The configuration
strategy used here is a little more complex than most simple .NET solutions. However,
it provides the ability to handle typical configurations easily while providing for custom and more
advanced requirements in a standard way. 