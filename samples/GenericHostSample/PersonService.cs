using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sample.Service.Interfaces;

namespace GenericHostSample
{

    public class PersonService : IPersonService
    {
        private IHelloService HelloService { get; }

        public PersonService(IHelloService helloService)
        {
            HelloService = helloService;
        }

        public async Task<object> GetName(int id)
        {
            await Task.Delay(TimeSpan.FromHours(1));
            return await Task.FromResult(new
            {
                name = $"[{id}]Owen",
                //message = await HelloService.SayHello("Owen")
            });
        }
    }
}
