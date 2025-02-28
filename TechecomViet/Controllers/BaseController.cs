using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechecomViet.Reponsitory;

public class BaseController : Controller
{
    protected readonly DataContext _dataContext;


    public BaseController(DataContext context)
    {
        _dataContext = context;
    }

    protected async Task<int> GetCartItemCountAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return 0;
        }

        var cart = await _dataContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        return cart?.Items.Count ?? 0;
    }

    protected async Task SetCartItemCountAsync()
    {
        var cartItemCount = await GetCartItemCountAsync();
        ViewBag.CartItemCount = cartItemCount;
    }
}