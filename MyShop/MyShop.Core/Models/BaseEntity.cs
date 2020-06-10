using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Core.Models
{
    //This class is for the InMemoryRepository class. We are identifying the base a class needs. The other classes that want to use...
    //InMemoryRepository will need to enherit this base class.
    //Making this class an abstract class, means that it can not be called but only enherited.
    public abstract class BaseEntity
    {
        public string Id { get; set; }
        //this is for trouble shooting, so we can see when classes were created. (Auditing)
        public DateTimeOffset CreatedAt { get; set; }

        public BaseEntity()
        {
            //Generates a random ID
            this.Id = Guid.NewGuid().ToString();
            this.CreatedAt = DateTime.Now;
        }
    }
}
