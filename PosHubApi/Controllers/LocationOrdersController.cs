using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PosHubApi.Data.Interfaces;
using PosHubApi.Dtos;

namespace PosHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationOrdersController : ControllerBase
    {
        private readonly ILocationOrdersRepository _locationOrdersRrepository;

        public LocationOrdersController(ILocationOrdersRepository locationOrdersRrepository)
        {
            _locationOrdersRrepository = locationOrdersRrepository;
        }

        [HttpGet("getOrderByOrderId/{applicationId}/{orderId}")]
        public async Task<ActionResult<OrderEventDto>> GetOrderByOrderId(string applicationId, string orderId)
        {
            try
            {
                string apiCall = $"LocationOrders/getOrderByOrderId";
                OrderEventDto order = await _locationOrdersRrepository.GetOrderByOrderId(applicationId, orderId, apiCall);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // [HttpPatch("updateOrderByOrderId/{applicationId}/{orderId}")]

        // db la edutu fill
        // public async Task<ActionResult<OrderEventDto>> UpdateOrderByOrderId(string applicationId, OrderUpdateRequestDto order, string orderId)
        // {
        //     try
        //     {
        //         string apiCall = $"Catalog/updateCatalogProductByProductId";
        //         OrderEventDto productRes = await _locationOrdersRrepository.UpdateOrderByOrderId(applicationId, order, orderId, apiCall);
        //         return Ok(productRes);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { error = ex.Message });
        //     }
        // }

    }
}