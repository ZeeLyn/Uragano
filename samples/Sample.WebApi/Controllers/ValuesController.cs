using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sample.Service.Interfaces;
using Uragano.Abstractions;
using Uragano.DynamicProxy;
using Uragano.Remoting;

namespace Sample.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private IClientFactory ClientFactory { get; }
		private IServiceProxy ServiceProxy { get; }

		private IProxyGenerator ProxyGenerator { get; }

		public ValuesController(IServiceProxy serviceProxy, IClientFactory clientFactory, IProxyGenerator proxyGenerator)
		{
			ServiceProxy = serviceProxy;
			ClientFactory = clientFactory;
			ProxyGenerator = proxyGenerator;
		}

		// GET api/values
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			//var proxy = ServiceProxy.GetService<IHelloService>();
			//var id = Guid.NewGuid().ToString();
			//var r = await proxy.SayHello(id);
			//return Ok(new
			//{
			//	Send = id,
			//	Rece = r.Message
			//});
			ProxyGenerator.GenerateProxy(new[] { typeof(IHelloService) });
			return Ok();
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult<string> Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
