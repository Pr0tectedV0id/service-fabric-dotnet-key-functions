---
services: service-fabric
platforms: dotnet
author: msonecode
---

# Azure Service Fabric Key Functions Simple Project

## Introduction

This C# solution project is trying to help you understand all the key functions in Azure Service Fabric with the simplest and shortest codes.

You can use this project as your first Azure Service Fabric example and sample.
<br/>
<br/>
<br/>

## Prerequisites

*__Install Azure Service Fabric SDK__*

This example uses Service Fabric SDK 2.1.163.9590.

The following document will enable you to initialize your Service Fabric development environment.

https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/
<br/>
<br/>

*__Create test Service Fabric__*

The following document will help you to create an Azure Service Fabric.

https://azure.microsoft.com/en-us/documentation/articles/service-fabric-cluster-creation-via-portal/
<br/>
<br/>
<br/>

## Project Structure

You can get more detailed explanation for Service Fabric terminology on this website.

https://azure.microsoft.com/en-us/documentation/articles/service-fabric-technical-overview/


*__Application1__*

It is the Service Fabric Application project. We need to use this project to publish and set all the micro-services deployment configurations, such as instance count and partition count.
<br/>
<br/>

*__WebApi1__*

Example Stateless Web API service.

It listens to 8972 port (you can change it from ServiceManifest.xml in this project) and acts as OWIN Web API service. You can also add MVC functions to this project.

We use this project as Service Fabric public HTTP access entry point.

This project includes the following public HTTP endpoints.

`http(s)://[service fabric domain]:[8972 or 8973]/api/values/getfromstatelessservice/1`

`http(s)://[service fabric domain]:[8972 or 8973]/api/values/getfromstatefulservice/[partition key]`

`http(s)://[service fabric domain]:[8972 or 8973]/api/values/settostatefulservice/[partition key]?value=[test value]`

`http(s)://[service fabric domain]:[8972 or 8973]/api/values/getfromactor/[actor id]`

`http(s)://[service fabric domain]:[8972 or 8973]/api/values/settoactor/[actor id]?value=[test value]`
<br/>
<br/>

*__Stateless1__*

Example Stateless service.

Web API service has communication with this service.
<br/>
<br/>

*__Stateful1__*

Example Stateful service.

Web API service has communication with this service.
<br/>
<br/>

*__Actor1 and Actor1.Interfaces__*

Example Stateful Actor and interface definition project.

Web API service has communication with this actor.
<br/>
<br/>
<br/>

## Key Functions

Here are some very common scenarios and the way to achieve them with codes.

*__Limit Service Running On Specific Node Type__*

According to MSDN, we can create more than 1 node types (virtual machine scale set) in one Azure Service Fabric. It usually means front end and back end since we can use smaller size and less VMs for front end, while bigger and more VMs for backend. In this example, I create 2 node types: nodetype1 and nodetype2. Please notice they have different domain names. You can get them from Azure Portal Public IP list.

In our solution, we need to configure WebApi1 service is running in front node type and other services running in backend node type.

In WebApi1/PackageRoot/ServiceManifest.xml, we use the below configuration to limit this service so that it can only run in "nodetype2". Please notice you need to add this configuration for not upgrade mode if your service fabric is ready for running.

```xml
<PlacementConstraints>(NodeType==nodetype2)</PlacementConstraints>
```

Besides, other services have been set to run in "nodetype1".

<br/>
<br/>



*__Add Startup Task__*

Please check WebApi1 project to find the solution source code.

Sometimes we need to install some dependency exe or to run some bat files to preset VM environment before Service Fabric starts to run, just like the cloud service startup task. You can use following steps to achieve this target.

1\. Add startup.bat to the project. Exe file is also acceptable.

2\. Set this bat file as "Copy to Output Directory".

3\. Add the following XML configuration in ServiceManifest.xml

```xml
    <SetupEntryPoint>
      <ExeHost>
        <Program>startup.bat</Program>
      </ExeHost>
    </SetupEntryPoint>
```

4\. Notice that `<Program>startup.bat</Program>` should be used in one line.

<br/>
<br/>




*__Add CORS__*

Please check WebApi1 project to find the solution source code.

CORS means to return additional HTTP header in response. Actually, it is not a Service Fabric topic issue. Service Fabric CORS means to add CORS into OWIN Web API2 service.

1\. Install "Microsoft.Owin.Cors" assembly from nuget, while it has been assembly installed in this example.

2\. Add `appBuilder.UseCors(CorsOptions.AllowAll);` in Startup.cs

3\. Add the following XML in app.config

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

4\. After publishing, use this http://www.test-cors.org/ to test CORS.

<br/>
<br/>





*__Enable HTTPS Web API Service__*

Please check WebApi1 and Application1 projects to find the solution source code.

1\. Prepare a .pfx file. You can use makecert.exe to generate a test certificate. In this project, the name is vantest.pfx .

2\. Prepare installcert.cmd and installcert.ps1 file. You will use these files to install van.pfx certificate into VM localMachine\\MY store.

3\. RDP to every VM, copy the above 3 files to VM. Run installcert.cmd in command window. If you want to install certificate automatically when VMSS scale up or down, you need to use Azure Key-Vault and set certificate in deployment template.

4\. Open WebApi1/ServiceManifest.xml and add the below configuration.

```xml
<Endpoint Protocol="https" Name="ServiceHttpsEndpoint" Type="Input" Port="8973" />
```
5.\ Open Application1/ApplicationManifest.xml and add below configuration. Please notice X509FindValue is the thumbprint of vantest.pfx

```xml
    <Policies>
      <EndpointBindingPolicy EndpointRef="ServiceHttpsEndpoint" CertificateRef="TestCert1" />
    </Policies>
```

```xml
<Certificates>
    <EndpointCertificate X509FindValue="C82CD573B5CDE976B4799134F1618A80EBE57E9C" Name="TestCert1" />
</Certificates>
```

6\. Open WebApi1/OwinCommunicationListener.cs and modify it to listen https protocol. By default, it only listens http.

7\. Open WebApi1/WebApi1.cs and modify it to listen all endpoints in configuration. By default, it only listens endpoint "ServiceEndpoint".

8.\ Open Azure Service Fabric Portal, add 8973 port load balancer.

9.\ Publish Service Fabric, and test by https://[service fabric domain]:8973/api/values/getfromstatelessservice/1

<br/>
<br/>




*__Communication between Web API service and Stateless Service__*

Web API service is the public entry service which needs to invoke other backend services to finish business requests. We have 2 choices to achieve this target: creating backend service public endpoint or using Service Fabric internal communication technology.

Please check Stateless1 and WebApi1 projects to find the solution source code.

1\. Open Stateless1/Stateless1.cs. There are interface `ITestStatelessService`, class `TestCount` and method `public Task<TestCount> GetCount()`. They defined Stateless1 service open API.

2\. Open WebApi1/Controllers/ValuesController.cs. The codes at below will indicate how to build communication with Stateless1 service.

```C#

public async Task<string> GetFromStatelessService(int id)
{
    ServiceEventSource.Current.Message("Get invoked: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    var helloWorldClient = ServiceProxy.Create<ITestStatelessService>(new Uri("fabric:/Application1/Stateless1"));
    var message = await helloWorldClient.GetCount();
    return string.Format("[{0}] {1}: {2}", message.Time.ToString("yyyy-MM-dd HH:mm:ss"), message.Id, message.Count);
}

```

In this example, Stateless1 service returned an object so that WebApi1 service can receive the result object.

<br/>
<br/>




*__Communication between Web API service and Stateful Service__*

Please check Stateless1 and WebApi1 projects to find the solution source code.

1\. Open Stateful1/Stateful1.cs. There are interface `ITestService`, method `public async Task<string> GetCount()` and `public async Task SetCount(long count)`. They defined Stateful1 service open API.

Please notice that Stateful1 service uses ReliableDictionary to store value. This dictionary data will be synced among all the Stateful1 services in the same partition.

2\. Open WebApi1/Controllers/ValuesController.cs. The following codes indicate how to build communication with Stateful1 service.

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

Please notice that these API have a parameter called "partition id" and it only accepts 0-5.

This is because in Application1/ApplicationPackageRoot/ApplicationManifest.xml, there is XML configuration which limited Stateful1 partition key from 0-5. You can also use this configuration item to control Stateful Service partition key range.

```xml

    <Service Name="Stateful1">
      <StatefulService ServiceTypeName="Stateful1Type" TargetReplicaSetSize="[Stateful1_TargetReplicaSetSize]" MinReplicaSetSize="[Stateful1_MinReplicaSetSize]">
        <UniformInt64Partition PartitionCount="[Stateful1_PartitionCount]" LowKey="0" HighKey="5" />
      </StatefulService>
    </Service>

```

<br/>
<br/>




*__Communication between Web API service and Stateful Actor__*

Please check Actor1 and WebApi1 projects to find the solution source code.

1\. Open Actor1/Actor1.cs. There are methods `Task IActor1.SetCountAsync(int count)` and `Task<int> IActor1.GetCountAsync()`. They defined Stateful1 service open API.

Please notice that Actor1 uses StateManager to get and set value. StateManager cannot sync data among different actor objects.

Besides, we need to notice that actor is a single module. In other word, if there were any threads operating the same actor by the same actor id, these requests would be executed in one single thread.

2\. Open WebApi1/Controllers/ValuesController.cs. The following codes indicate how to build communication with Actor1.

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
