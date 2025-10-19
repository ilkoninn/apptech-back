using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.TransactionDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Validators.OrderValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class OrderController : APIController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(string userId)
        {
            var dto = new GetAllOrderDTO() { userId = userId };

            return Ok(await _orderService.GetAllAsync(dto));
        }

        [HttpPost("purchase")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PurchaseOrder([FromBody] CreateTransactionDTO dto)
        {
            return StatusCode(201, await _orderService.PurchaseAsync(dto));
        }

        [HttpPost("increase")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IncreaseBalance([FromBody] IncreaseBalanceDTO dto)
        {
            var validations = await new IncreaseBalanceDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _orderService.IncreaseBalanceAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost("handle")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PaymentHandlerAsync([FromBody] OrderTokenResponseDTO dto)
        {
            return StatusCode(201, await _orderService.PaymentHandlerAsync(dto.token, dto.increaseOrBuy));
        }

        [HttpPost("buyexam")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuyExamAsync([FromBody] DecreaseBalanceForBuyExamDTO dto)
        {
            return StatusCode(201, await _orderService.DecreaseBalanceForBuyExamAsync(dto));
        }

        [HttpPost("buyexampackage")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuyExamPackageAsync([FromBody] DecreaseBalanceForForBuyExamPackageDTO dto)
        {
            return StatusCode(201, await _orderService.DecreaseBalanceForBuyExamPackageAsync(dto));
        }
    }
}
