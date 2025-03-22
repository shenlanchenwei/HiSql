using HiSql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        HiSqlClient hiSqlClient;

        public TestController(HiSqlClient _hiSqlClient)
        {
            this.hiSqlClient = _hiSqlClient;
        }

        [HttpGet]
        public async Task<object> Get()
        {
            var dataList = await this
                .hiSqlClient.Insert(
                    "test",
                    new
                    {
                        Id ="R"+new Random().Next().ToString(),
                        Name = "1111",
                        Desc = "Desc"
                    }
                )
                .ExecCommandAsync();
            return dataList;
        }
    }
}
