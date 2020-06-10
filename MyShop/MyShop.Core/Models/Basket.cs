using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Core.Models
{
    public class Basket : BaseEntity
    {
        //ICollection is for entity frame work. Known as lazy loading
        public virtual ICollection<BasketItem> BasketItems { get; set; }
        public Basket()
        {
            this.BasketItems = new List<BasketItem>();
        }
    }
}
