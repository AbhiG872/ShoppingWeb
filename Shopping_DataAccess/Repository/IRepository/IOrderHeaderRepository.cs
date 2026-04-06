using Shopping_DataAccess.Repository.IRepository;
using Shopping_Models;
using ShoppingWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopping_DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int id, string orderstatus,string? PaymentStatus=null);

        void UpdateStripPaymentId(int id,string sessionId, string paymentIntentId);
    }

}