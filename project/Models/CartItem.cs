using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using project.Models;

public class CartItem
{
    public Product shopping_sp { get; set; }
    public int shopping_sl { get; set; }
    public decimal DiscountedPrice { get; set; } // Add this property
}

