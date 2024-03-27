using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Red_Mango_API.Data;
using Red_Mango_API.Models.Dto;
using Red_Mango_API.Models;
using Red_Mango_API.Utility;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Red_Mango_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ApiResponse _response;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
            _response = new ApiResponse();
        }
        //Begin  endpoint for Getting ALL Orders 


        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId,
            string searchString, string status, int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                IQueryable<OrderHeader> orderHeaders =
                    _db.OrderHeaders.Include(u => u.OrderDetails)
                    .ThenInclude(u => u.MenuItem)
                    .OrderByDescending(u => u.OrderHeaderId);



                if (!string.IsNullOrEmpty(userId))
                {
                    orderHeaders = orderHeaders.Where(u => u.ApplicationUserId == userId);
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    orderHeaders = orderHeaders
                        .Where(u => u.PickupPhoneNumber.ToLower().Contains(searchString.ToLower()) ||
                    u.PickupEmail.ToLower().Contains(searchString.ToLower())
                    || u.PickupName.ToLower().Contains(searchString.ToLower()));
                }
                if (!string.IsNullOrEmpty(status))
                {
                    orderHeaders = orderHeaders.Where(u => u.Status.ToLower() == status.ToLower());
                }

                Pagination pagination = new()
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = await orderHeaders.CountAsync(), // Await the asynchronous count operation
                };

                Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));

                _response.Result = await orderHeaders.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(); // Await the asynchronous query execution
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }

        //End  endpoint for Getting Orders 


        //Begin  endpoint for Getting INDIVIDUAL Orders 

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrders(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var orderHeaders = await _db.OrderHeaders
                    .Include(u => u.OrderDetails)
                    .ThenInclude(u => u.MenuItem)
                    .Where(u => u.OrderHeaderId == id)
                    .ToListAsync(); // Execute the query asynchronously and await it

                if (orderHeaders == null || !orderHeaders.Any())
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = orderHeaders;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }

        //End  endpoint for Getting INDIVIDUAL Orders 


        //Begin  endpoint for Creating (POST) Orders 

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDTO orderHeaderDTO)
        {
            try
            {
                OrderHeader order = new()
                {
                    ApplicationUserId = orderHeaderDTO.ApplicationUserId,
                    PickupEmail = orderHeaderDTO.PickupEmail,
                    PickupName = orderHeaderDTO.PickupName,
                    PickupPhoneNumber = orderHeaderDTO.PickupPhoneNumber,
                    OrderTotal = orderHeaderDTO.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentID = orderHeaderDTO.StripePaymentIntentID,
                    TotalItems = orderHeaderDTO.TotalItems,
                    Status = String.IsNullOrEmpty(orderHeaderDTO.Status) ? SD.status_pending : orderHeaderDTO.Status,
                };

                if (ModelState.IsValid)
                {
                    _db.OrderHeaders.Add(order);
                    await _db.SaveChangesAsync(); // Await the asynchronous SaveChangesAsync method

                    foreach (var orderDetailDTO in orderHeaderDTO.OrderDetailsDTO)
                    {
                        OrderDetails orderDetails = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDTO.ItemName,
                            MenuItemId = orderDetailDTO.MenuItemId,
                            Price = orderDetailDTO.Price,
                            Quantity = orderDetailDTO.Quantity,
                        };
                        _db.OrderDetails.Add(orderDetails);
                    }

                    await _db.SaveChangesAsync(); // Await the asynchronous SaveChangesAsync method

                    _response.Result = order;
                    order.OrderDetails = null;
                    _response.StatusCode = HttpStatusCode.Created;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }

            return _response;
        }

        //End endpoint for Creating (POST) Orders

        //Begin  endpoint for Putting Orders 

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDTO orderHeaderUpdateDTO)
        {
            try
            {
                if (orderHeaderUpdateDTO == null || id != orderHeaderUpdateDTO.OrderHeaderId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }

                OrderHeader orderFromDb = await _db.OrderHeaders.FirstOrDefaultAsync(u => u.OrderHeaderId == id); // Use FirstOrDefaultAsync and await it

                if (orderFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }

                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupName))
                {
                    orderFromDb.PickupName = orderHeaderUpdateDTO.PickupName;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupPhoneNumber))
                {
                    orderFromDb.PickupPhoneNumber = orderHeaderUpdateDTO.PickupPhoneNumber;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupEmail))
                {
                    orderFromDb.PickupEmail = orderHeaderUpdateDTO.PickupEmail;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.Status))
                {
                    orderFromDb.Status = orderHeaderUpdateDTO.Status;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.StripePaymentIntentID))
                {
                    orderFromDb.StripePaymentIntentID = orderHeaderUpdateDTO.StripePaymentIntentID;
                }

                await _db.SaveChangesAsync(); // Use SaveChangesAsync and await it

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }

        //End  endpoint for Putting Orders 
    }
}
