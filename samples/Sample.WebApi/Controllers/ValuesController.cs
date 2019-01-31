using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sample.Service.Interfaces;
using Uragano.Abstractions;
using Uragano.Core;


namespace Sample.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IHelloService HelloService { get; }

        private IServiceProvider ServiceProvider { get; }


        public ValuesController(IHelloService helloService, IServiceProvider serviceProvider)
        {
            HelloService = helloService;
            ServiceProvider = serviceProvider;

        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var r = ServiceProvider.GetServices<IRemotingInvoke>();
            return Ok(new
            {
                count = r.Count(),
                result = await HelloService.SetMeta(("token", "bearer .....")).SayHello(Guid.NewGuid().ToString()),
            });
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
