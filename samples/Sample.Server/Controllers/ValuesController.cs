using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sample.Common;
using Sample.Service.Interfaces;
using Uragano.Abstractions;
using Uragano.Core;

namespace Sample.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IHelloService HelloService { get; }

        private ICaching Caching { get; }

        public ValuesController(IHelloService helloService, ICaching caching)
        {
            HelloService = helloService;
            Caching = caching;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            object v = new object();
            await Caching.Set("test", v);
            await HelloService.SayHello();
            return Ok(new
            {
                name = await HelloService.SetMeta(("token", "bearer .....")).SayHello(Guid.NewGuid().ToString()),
                age = await HelloService.Age(),
                entity = await HelloService.SayHello(new TestModel { Id = 1, Name = "owen" })
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
