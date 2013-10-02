MemcachedSharp
==============

A light-weight, async/non-blocking Memcached client for .NET

#Install on NuGet

> Install-Package MemcachedSharp

#Setup and Options
```c#
var client = new MemcachedClient("somehost:11211", new MemcachedOptions
{
	// setup my options here
	ConnectTimeout = TimeSpan.FromSeconds(2),
	ReceiveTimeout = TimeSpan.FromSeconds(2),
	EnablePipelining = true,
	MaxConnections = 2,
	MaxConcurrentRequestPerConnection = 15
});
```
####ConnectTimeout
The amount of time to wait before abandoning an attempt to connect to a Memcached host.
* Default: 2 seconds, ```TimeSpan.FromSeconds(2)```.
* Infinite timeout: -1 millisecond, ```Timeout.InfiniteTimeSpan``` or ```TimeSpan.FromMilliseconds(-1)```.

####ReceiveTimeout
The amount of time to wait before abandoning an attempt to receive more data from a Memcached host.
Please note that requests may take longer than the specified receive timeout. This timeout only limits the amount of time that a connection may remain idle when data is expected.
* Default: 2 seconds, ```TimeSpan.FromSeconds(2)```.
* Infinite timeout: -1 millisecond, ```Timeout.InfiniteTimeSpan``` or ```TimeSpan.FromMilliseconds(-1)```.

####EnablePipelining
Enables the client to send multiple requests on the same connection before receiving any of the responses. This works a lot like [http pipelining](http://en.wikipedia.org/wiki/HTTP_pipelining).
* Default: true

####MaxConnections
The maximum number of connections that may be opened at one time to the target host. Note that this number doesn't need to be very high to achieve high throughput if pipelining is enabled.
* Default: 2

####MaxConcurrentRequestPerConnection
The maximum number of requests that may be sent at one time on the same connection.
* Default: 15
* Only applicable if EnablePipelining is true.

#Example

```c#
using System;
using MemcachedSharp;
using System.Threading.Tasks;

using(var client = new MemcachedClient("localhost:11211"))
{
	Console.Write("get foo...");
	var foo = await client.Get("foo");
	Console.WriteLine(foo == null ? "not found." : "found.");
	
	Console.Write("set foo...");
	await client.Set("foo", Encoding.UTF8.GetBytes("Hello, World!"));
	Console.WriteLine("done.");
	
	Console.Write("get foo...");
	foo = await client.Get("foo");
	Console.WriteLine(foo == null ? "not found." : "found.");
	foo.Dump("Foo");
	
	Console.Write("delete foo...");
	await client.Delete("foo");
	Console.WriteLine(foo == null ? "not found." : "deleted.");
	
	Console.Write("get foo...");
	foo = await client.Get("foo");
	Console.WriteLine(foo == null ? "not found." : "found.");
}
```

#The MemcachedClient class

* Instances are stateful. Connections are created the first time they are needed and persist as long as the MemcachedClient instance exists.
* Instances should be disposed when they are no longer needed. Doing so will close pooled connections to Memcached.
* For long-lived applications with periodic requests to Memcached I recommend keeping a single instance alive for the life-time of the application.
