using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;

        public ValuesController(IMemoryCache memoryCache, ApplicationDbContext context, IDistributedCache distributedCache)
        {
            _memoryCache = memoryCache;
            _context = context;
            _distributedCache = distributedCache;
        }

        [HttpGet("redis")]
        public async Task<IActionResult> GetAllCustomersUsingRedisCache()
        {
            var cacheKey = "studentList";
            string serializedCustomerList;
            var customerList = new List<Student>();
            var redisCustomerList = await _distributedCache.GetAsync(cacheKey);
            if (redisCustomerList != null)
            {
                serializedCustomerList = Encoding.UTF8.GetString(redisCustomerList);
                customerList = JsonConvert.DeserializeObject<List<Student>>(serializedCustomerList);
            }
            else
            {
                customerList = await _context.Students.ToListAsync();
                serializedCustomerList = JsonConvert.SerializeObject(customerList);
                redisCustomerList = Encoding.UTF8.GetBytes(serializedCustomerList);
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                await _distributedCache.SetAsync(cacheKey, redisCustomerList, options);
            }
            return Ok(customerList);
        }
        [HttpGet("redised")]
        public async Task<IActionResult> GetAllRedisCache()
        {
            var customerList = await _context.Students.ToListAsync();
            return Ok(customerList);
        }
    }
}
