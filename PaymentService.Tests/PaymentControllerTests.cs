using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PaymentService.Controllers;
using PaymentService.Data;
using PaymentService.Models.DTO.PaymentDTOs;

namespace PaymentService.Tests
{
    public class PaymentControllerTests
    {
        private const string UserId = "66666666-6666-6666-6666-666666666666";

        //repozitorijum je podmetnut, kontroler se testira bez baze
        private static PaymentController BuildController(Mock<IPaymentRepository> repository, bool withUserHeader = true)
        {
            var controller = new PaymentController(repository.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            if (withUserHeader)
            {
                controller.ControllerContext.HttpContext.Request.Headers["X-User-Id"] = UserId;
            }

            return controller;
        }

        [Fact]
        public async Task CreatePayment_WithoutUserHeader_ReturnsBadRequest()
        {
            var repository = new Mock<IPaymentRepository>();
            var controller = BuildController(repository, withUserHeader: false);

            var response = await controller.CreatePayment(new PaymentCreationDTO());

            Assert.IsType<BadRequestObjectResult>(response.Result);
            repository.Verify(r => r.CreatePaymentAsync(It.IsAny<PaymentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreatePayment_WhenInvoiceMissing_ReturnsNotFound()
        {
            var repository = new Mock<IPaymentRepository>();
            repository
                .Setup(r => r.CreatePaymentAsync(It.IsAny<PaymentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(OperationResult<PaymentConfirmationDTO>.Fail(OperationOutcome.NotFound));

            var controller = BuildController(repository);

            var response = await controller.CreatePayment(new PaymentCreationDTO());

            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task CreatePayment_WhenInvoiceAlreadyPaid_ReturnsConflict()
        {
            var repository = new Mock<IPaymentRepository>();
            repository
                .Setup(r => r.CreatePaymentAsync(It.IsAny<PaymentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(OperationResult<PaymentConfirmationDTO>.Fail(OperationOutcome.InvoiceIsPaid));

            var controller = BuildController(repository);

            var response = await controller.CreatePayment(new PaymentCreationDTO());

            Assert.IsType<ConflictObjectResult>(response.Result);
        }

        [Fact]
        public async Task CreatePayment_WhenSuccessful_ReturnsCreated()
        {
            var confirmation = new PaymentConfirmationDTO { PaymentId = Guid.NewGuid(), Amount = 500m };

            var repository = new Mock<IPaymentRepository>();
            repository
                .Setup(r => r.CreatePaymentAsync(It.IsAny<PaymentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(OperationResult<PaymentConfirmationDTO>.Ok(confirmation));

            var controller = BuildController(repository);

            var response = await controller.CreatePayment(new PaymentCreationDTO());

            var created = Assert.IsType<CreatedAtRouteResult>(response.Result);
            Assert.Equal(confirmation, created.Value);
        }

        [Fact]
        public async Task CreatePayment_PassesUserIdFromHeaderToRepository()
        {
            var repository = new Mock<IPaymentRepository>();
            repository
                .Setup(r => r.CreatePaymentAsync(It.IsAny<PaymentCreationDTO>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(OperationResult<PaymentConfirmationDTO>.Ok(new PaymentConfirmationDTO()));

            var controller = BuildController(repository);

            await controller.CreatePayment(new PaymentCreationDTO());

            repository.Verify(
                r => r.CreatePaymentAsync(It.IsAny<PaymentCreationDTO>(), Guid.Parse(UserId), It.IsAny<bool>()),
                Times.Once);
        }

        [Fact]
        public void GetPaymentById_WhenMissing_ReturnsNotFound()
        {
            var repository = new Mock<IPaymentRepository>();
            repository.Setup(r => r.GetPaymentById(It.IsAny<Guid>())).Returns((PaymentDTO?)null);

            var controller = BuildController(repository);

            var response = controller.GetPaymentById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public void DeletePayment_WhenSuccessful_ReturnsNoContent()
        {
            var repository = new Mock<IPaymentRepository>();
            repository.Setup(r => r.DeletePayment(It.IsAny<Guid>())).Returns(OperationResult<bool>.Ok(true));

            var controller = BuildController(repository);

            var response = controller.DeletePayment(Guid.NewGuid());

            Assert.IsType<NoContentResult>(response);
        }
    }
}
