MemcachedSharp
==============

A light-weight, async/non-blocking Memcached client for .NET

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
