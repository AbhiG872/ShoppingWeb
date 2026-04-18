using Microsoft.AspNetCore.Mvc;
using Shopping_DataAccess.Repository.IRepository;

namespace ShoppingWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> Invokeasync()
        {

        }
    }
}
