using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Uragano.Abstractions;
using Uragano.DynamicProxy;

namespace Sample.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private IServiceProxy ServiceProxy { get; }

		public ValuesController(IServiceProxy serviceProxy)
		{
			ServiceProxy = serviceProxy;
		}

		// GET api/values
		[HttpGet]
		public IActionResult Get()
		{
			var proxy = ServiceProxy.GetService<IHelloService>("");
			return Ok(proxy.SayHello("owen"));
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
