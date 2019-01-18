using Microsoft.AspNetCore.Mvc;
using Sample.Service.Interfaces;
using Uragano.Abstractions;
using Uragano.Remoting;

namespace Sample.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private IClientFactory ClientFactory { get; }
		private IServiceProxy ServiceProxy { get; }

		public ValuesController(IServiceProxy serviceProxy, IClientFactory clientFactory)
		{
			ServiceProxy = serviceProxy;
			ClientFactory = clientFactory;
		}

		// GET api/values
		[HttpGet]
		public IActionResult Get()
		{
			var proxy = ServiceProxy.GetService<IHelloService>("");
			return Ok(proxy.SayHello("owen"));
			//var c = ClientFactory.CreateClient("192.168.1.129", 5001);
			//c.SendAsync(new InvokeMessage { Message = Guid.NewGuid().ToString() });
			//return Ok("OK");
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
