---
services: service-fabric
platforms: dotnet
author: msonecode
---

# Azure Service Fabric Key Functions Simple Project

## Introduction
Azure Service Fabric is an amazing framework. But it contains a lot of concepts need to understand.

In this C# solution project, you will understand all Azure Service Fabric key functions. I will present these functions by simplest and shortest codes.





## Prerequisites

*__Install Azure Service Fabric SDK__*

This example use Service Fabric SDK 2.1.163.9590.

Please follow below document to initialize your Service Fabric development environment.

https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/


*__Create test Service Fabric__*

Please follow below document to create an Azure Service Fabric.

https://azure.microsoft.com/en-us/documentation/articles/service-fabric-cluster-creation-via-portal/




## Project Structure

You can get detail explanation of below Service Fabric terminology from here.

https://azure.microsoft.com/en-us/documentation/articles/service-fabric-technical-overview/


*__Application1__*

It is the Service Fabric Application project. We need to use this project to publish and set all micro-services deployment configuration, such as instance count and partition count.


*__WebApi1__*

Example Stateless Web API service.

It listens 8972 port (you can change it from ServiceManifest.xml in this project) and acts as OWIN Web API service. You also can add MVC functions in this project.

We use this project as Service Fabric public HTTP access entry point.

This project has below public HTTP endpoints.

`http://[service fabric domain]:8972/api/values/getfromstatelessservice/1`

`http://[service fabric domain]:8972/api/values/getfromstatefulservice/[partition key]`

`http://[service fabric domain]:8972/api/values/settostatefulservice/[partition key]?value=[test value]`

`http://[service fabric domain]:8972/api/values/getfromactor/[actor id]`

`http://[service fabric domain]:8972/api/values/settoactor/[actor id]?value=[test value]`


*__Stateless1__*

Example Stateless service.

Web API service has communication with this service.

*__Stateful1__*

Example Stateful service.

Web API service has communication with this service.

*__Actor1 and Actor1.Interfaces__*

Example Stateful Actor and interface definition project.

Web API service has communication with this actor.




## Key Functions

Below are some very common scenarios and how to achieve them by codes.

*__1.	Startup Task__*

Please check WebApi1 project to find the solution source code.

Sometimes we need to install some dependency exe or run some bat file to preset VM environment before Service Fabric starts to run. Just as cloud service startup task. You can use below steps to achieve this target.

1\. Add startup.bat into project. Exe file is also acceptable.

2\. Set this bat file as "Copy to Output Directory".

3\. Add below XML configuration in ServiceManifest.xml

```xml
    <SetupEntryPoint>
      <ExeHost>
        <Program>startup.bat</Program>
      </ExeHost>
    </SetupEntryPoint>
```

4\. Notice you need to use `<Program>startup.bat</Program>` as a line.

*__2.	CORS__*

Please check WebApi1 project to find the solution source code.

CORS means return additional HTTP header in response. Actually it is not a Service Fabric topic issue. Service Fabric CORS means how to add CORS into OWIN Web API2 service.

1\. Install "Microsoft.Owin.Cors" assembly from nuget. In this example, this assembly has been installed.

2\. Add `appBuilder.UseCors(CorsOptions.AllowAll);` in Startup.cs

3\. Add below XML in app.config

```xml
  <system.webServer>
    <httpProtocol>
      <customHeaders>
        <add name="Access-Control-Allow-Origin" value="*" />
        <add name="Access-Control-Allow-Methods" value="GET, POST, OPTIONS, PUT, DELETE" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
```

4\. After publish, use this http://www.test-cors.org/ to test CORS.

*__3.	Communication between Web API service and Stateless Service__*

Web API service is the public entry service. But it always needs to invoke other backend services to finish business requests. We have 2 choices to achieve this target: create backend service public endpoint or use Service Fabric internal communication technology.

Please check Stateless1 and WebApi1 projects to find the solution source code.

1\. Open Stateless1/Stateless1.cs. There are interface `ITestStatelessService`, class `TestCount`, method `public Task<TestCount> GetCount()`. They defined Stateless1 service open API.

2\. Open WebApi1/Controllers/ValuesController.cs. Below codes indicate how to build communication with Stateless1 service.

```C#

public async Task<string> GetFromStatelessService(int id)
{
    ServiceEventSource.Current.Message("Get invoked: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    var helloWorldClient = ServiceProxy.Create<ITestStatelessService>(new Uri("fabric:/Application1/Stateless1"));
    var message = await helloWorldClient.GetCount();
    return string.Format("[{0}] {1}: {2}", message.Time.ToString("yyyy-MM-dd HH:mm:ss"), message.Id, message.Count);
}

```

In this example, Stateless1 service returned an object and WebApi1 service can get the result object.

*__4.	Communication between Web API service and Stateful Service__*

Please check Stateless1 and WebApi1 projects to find the solution source code.

1\. Open Stateful1/Stateful1.cs. There are interface `ITestService`, method `public async Task<string> GetCount()` and `public async Task SetCount(long count)`. They defined Stateful1 service open API.

Please notice Stateful1 service uses ReliableDictionary to store value. This dictionary data will be synced between all Stateful1 services in the same partition.

2\. Open WebApi1/Controllers/ValuesController.cs. Below codes indicate how to build communication with Stateful1 service.

```C#

// GET api/values/getfromstatefulservice/[0-5]
public async Task<string> GetFromStatefulService(int id)
{
    var helloWorldClient = ServiceProxy.Create<ITestService>(new Uri("fabric:/Application1/Stateful1"), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(id));
    var message = await helloWorldClient.GetCount();
    return message;
}

// GET api/values/settostatefulservice/[0-5]?value=123
[HttpGet]
public async Task<string> SetToStatefulService(int id, long value)
{
    /* Explnation of partition key
     * if there are 2 partitions with key from 0-5.
     * 0-2 partition key will map to partition #1
     * 3-5 partition key will map to partition #2.
     * different partitions have different state.
     * which means if you set value 10 to any one of partition key 0-2,
     * then partition #1 will have value 10 but partition #2 still have value 0
    */
    var helloWorldClient = ServiceProxy.Create<ITestService>(new Uri("fabric:/Application1/Stateful1"), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(id));
    await helloWorldClient.SetCount(value);
    return String.Format("id: {0} has been set to {1}", id, value);
}

```

Please notice these API have a parameter called "partition id" and it only accepts 0-5.

This is because in Application1/ApplicationPackageRoot/ApplicationManifest.xml, there is XML configuration limited Stateful1 partition key from 0-5. You can also use this configuration item to control Stateful Service partition key range.

```xml

    <Service Name="Stateful1">
      <StatefulService ServiceTypeName="Stateful1Type" TargetReplicaSetSize="[Stateful1_TargetReplicaSetSize]" MinReplicaSetSize="[Stateful1_MinReplicaSetSize]">
        <UniformInt64Partition PartitionCount="[Stateful1_PartitionCount]" LowKey="0" HighKey="5" />
      </StatefulService>
    </Service>

```

*__5.	Communication between Web API service and Stateful Actor__*

Please check Actor1 and WebApi1 projects to find the solution source code.

1\. Open Actor1/Actor1.cs. There are methods `Task IActor1.SetCountAsync(int count)` and `Task<int> IActor1.GetCountAsync()`. They defined Stateful1 service open API.

Please notice Actor1 uses StateManager to get and set value. StateManager cannot sync data between different actor object.

Also we need to notice actor is a single module. Which means if there are some threads operating the same actor by the same actor id, these requests will be executed in a single thread.

2\. Open WebApi1/Controllers/ValuesController.cs. Below codes indicate how to build communication with Actor1.

```C#

// GET api/values/settoactor/1?value=123
[HttpGet]
public async Task<string> SetToActor(long actorid, int value)
{
    var start = DateTime.Now;
    var actor = ActorProxy.Create<IActor1>(new ActorId(actorid), new Uri("fabric:/Application1/Actor1ActorService "));
    await actor.SetCountAsync(value);
    return String.Format(
        "{0}\n\nActor id: {1} has been set to {2}\n\n{3}",
        start.ToString("yyyy-MM-dd HH:mm:ss"), actorid, value, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
}

// GET api/values/getfromactor/1
[HttpGet]
public async Task<string> GetFromActor(long actorid)
{
    var actor = ActorProxy.Create<IActor1>(new ActorId(actorid), new Uri("fabric:/Application1/Actor1ActorService "));
    var value = await actor.GetCountAsync();
    return value.ToString();
}

```

These codes indicate how to invoke actor objects by different actor id.
