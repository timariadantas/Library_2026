using Library.Application.Services;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using NUlid;

namespace Library.Api.Controllers;

[ApiController]
[Route("loan")]
public class LoanController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoanController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    // POST api/loan
    [HttpPost]
    public IActionResult CreateLoan(
        Ulid portfolioId,
        Ulid userId,
        BookCondition condition,
        int period = 30)
    {
        _loanService.CreateLoan(portfolioId, userId, condition, period);

        return Ok("Loan created successfully");
    }

    // GET api/loan
    [HttpGet]
    public ActionResult<IEnumerable<Loan>> GetAll()
    {
        var loans = _loanService.GetAllLoans();
        return Ok(loans);
    }

    // GET api/loan/{id}
    [HttpGet("{id}")]
    public ActionResult<Loan> GetById(string id)
    {
        var loan = _loanService.GetLoanById(Ulid.Parse(id));

        if (loan == null)
            return NotFound();

        return Ok(loan);
    }

    // GET api/loan/user/{userId}
    [HttpGet("user/{userId}")]
    public ActionResult<IEnumerable<Loan>> GetByUser(string userId)
    {
        var loans = _loanService.GetLoansByUser(Ulid.Parse(userId));

        return Ok(loans);
    }

    // PUT api/loan/{id}/return
    [HttpPut("{id}/return")]
    public IActionResult ReturnLoan(string id, BookCondition condition)
    {
        _loanService.ReturnLoan(Ulid.Parse(id), condition);

        return Ok("Book returned successfully");
    }

    // DELETE api/loan/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteLoan(string id)
    {
        _loanService.DeleteLoan(Ulid.Parse(id));

        return Ok("Loan deleted");
    }
}