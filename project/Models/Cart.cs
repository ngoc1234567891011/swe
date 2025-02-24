using System.Collections.Generic;
using System.Linq;

namespace project.Models
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items
        {
            get { return items; }
        }

        public void Add(Product _sp, int sl, decimal discountedPrice)
        {
            var item = items.FirstOrDefault(s => s.shopping_sp.masp == _sp.masp);
            if (item == null)
            {
                items.Add(new CartItem
                {
                    shopping_sp = _sp,
                    shopping_sl = sl,
                    DiscountedPrice = discountedPrice // Lưu giá đã giảm
                });
            }
            else
            {
                item.shopping_sl += sl;
            }
        }

        public void Update(int id, int sl)
        {
            var item = items.Find(s => s.shopping_sp.masp == id);
            if (item != null)
            {
                item.shopping_sl = sl;
            }
        }

        public double Total()
        {
            decimal total = items.Sum(s => s.DiscountedPrice * s.shopping_sl);
            return (double)total;
        }

        public void Remove(int id)
        {
            items.RemoveAll(s => s.shopping_sp.masp == id);
        }

    }
}
