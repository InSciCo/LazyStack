using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetStoreApi.Models;

namespace PetStoreRepo
{
    public class PetStoreRepo : IPetStoreRepo
    {
        public PetStoreRepo()
        {
            Tags = new Dictionary<long, Tag>
            {
                {1, new Tag() {Id=1, Name="HasShots"} },
                {2, new Tag() {Id=1, Name="HouseTrained"} },
                {3, new Tag() {Id=1, Name="Crazy"} }
            };

            Categories = new Dictionary<long, Category>
            {
                {1, new Category(){Id = 1,Name = "Dog",}}, 
                {2, new Category(){Id = 1,Name = "Cat",}}
            };

            Pets = new Dictionary<long, Pet>
            {
                {1,new Pet() { Id = 1, Category = Categories[1], Name = "Alf", Tags = new List<Tag>() { Tags[1],Tags[2] }, PetStatus = Pet.PetStatusEnum.AvailableEnum, PhotoUrls= new List<string>{"https://www.example.com/yada.jpg" } }},
                {2,new Pet() { Id = 2, Category = Categories[2], Name = "Jose", Tags = new List<Tag>() { Tags[1],Tags[2] }, PetStatus = Pet.PetStatusEnum.PendingEnum, PhotoUrls=new List<string>{"https://www.example.com/yada.jpg" } }},
                {3,new Pet() { Id = 3, Category = Categories[1], Name = "Buddy", Tags = new List<Tag>() { Tags[1],Tags[2] }, PetStatus = Pet.PetStatusEnum.SoldEnum, PhotoUrls=new List<string>{"https://www.example.com/yada.jpg" } }},
                {4,new Pet() { Id = 4, Category = Categories[2], Name = "Gypsy", Tags = new List<Tag>() { Tags[1],Tags[2] }, PetStatus = Pet.PetStatusEnum.AvailableEnum, PhotoUrls=new List<string>{"https://www.example.com/yada.jpg" } }},
                {5,new Pet() { Id = 4, Category = Categories[2], Name = "Sweetie", Tags = new List<Tag>() { Tags[1],Tags[2], Tags[3], }, PetStatus = Pet.PetStatusEnum.AvailableEnum, PhotoUrls=new List<string>{"https://www.example.com/yada.jpg"} }}
            };

            Orders = new Dictionary<long, Order>
            {
                {1,new Order() { Id = 1, OrderStatus = Order.OrderStatusEnum.DeliveredEnum, Complete = true, PetId = 1, Quantity = 1, ShipDate = DateTime.Now, UserId = "tony" }}
            };


        }

        public Dictionary<long, Pet> Pets { get; set; }
        public Dictionary<long, Tag> Tags { get; set; }
        public Dictionary<long, Category> Categories { get; set; }
        public Dictionary<long, Order> Orders { get; set; }

        #region pet
        // Returns Pet in ObjectResult
        public async Task<IActionResult> AddPet(Pet pet)
        {
            long newKey = 0;
            foreach (long key in Pets.Keys)
                newKey = Math.Max(newKey, key);
            newKey++;
            pet.Id = newKey;

            // replace references to tags to Tags dictionary
            // this ensures we pick up Updates to Tags
            for (int i = 0; i < pet.Tags.Count; i++)
            {
                pet.Tags[i] = Tags[pet.Tags[i].Id];
            }

            // replace reference to cateogry to Catetory dictionary
            // this ensures we pick up Update to Category
            if (Categories.ContainsKey(pet.Category.Id))
                pet.Category = Categories[pet.Category.Id];
            else
                return new ObjectResult(null) { StatusCode = 405 };

            Pets.Add(newKey, pet);

            return new ObjectResult(pet);
        }

        // Returns Pet in ObjectResult
        public async Task<IActionResult> GetPetById(long id)
        {

            if (Pets.ContainsKey(id))
                return new ObjectResult(Pets[id]);
            
            return new ObjectResult(null) { StatusCode = 404 };
        }

        // Returns only status code
        public async Task<IActionResult> UpdatePet(Pet pet)
        {
            var id = pet.Id;
            if (Pets.ContainsKey(id))
            {
                Pets[id] = pet;
                return new ObjectResult(null);
            }
            else
                return new ObjectResult(null) { StatusCode = 400 };
        }

        // Returns only status code
        public async Task<IActionResult> UpdatePetWithForm(long petId, string name, string status)
        {
            try
            {
                if (Pets.ContainsKey(petId))
                {
                    Pets[petId].Name = name;
                    Pets[petId].PetStatus = Enum.Parse<Pet.PetStatusEnum>(status, ignoreCase: true);
                    return new ObjectResult(null);
                }
                else
                    return new ObjectResult(null) { StatusCode = 405 };
            }
            catch
            {
                return new ObjectResult(null) { StatusCode = 405 };
            }
        }

        // Returns only status code
        public async Task<IActionResult> DeletePet(long id)
        {
            foreach (Order order in Orders.Values)
            {
                if (order.PetId == id)
                    return new ObjectResult(null) { StatusCode = 400 };
            }

            if (Pets.ContainsKey(id))
            {
                Pets.Remove(id);
                return new ObjectResult(null);
            }
            return new ObjectResult(null) { StatusCode = 404 };
        }

        // returns List<Pet> in ObjectResult
        public async Task<IActionResult>  GetInventory()
        {
            var pets = Pets.Values.ToList<Pet>();
            return new ObjectResult(pets);
        }

        // returns List<Pet> in ObjectResult
        public async Task<IActionResult> FindPetsByStatus(List<string> statuses)
        {
            var statusesEnum = new List<Pet.PetStatusEnum>();
            foreach (string status in statuses)
            {
                Pet.PetStatusEnum statusEnum;
                // Check Status Enum
                try
                {
                    statusEnum = JsonConvert.DeserializeObject<Pet.PetStatusEnum>(@"""" + status + @"""");
                }
                catch
                {
                    return new ObjectResult(null) { StatusCode = 400 };
                }
                statusesEnum.Add(statusEnum);
            }

            var pets = new List<Pet>();
            foreach (Pet pet in Pets.Values)
                if (statusesEnum.Contains(pet.PetStatus))
                    pets.Add(pet);

            return new ObjectResult(pets);
        }

        // Returns List<Pet> in ObjectResult
        public async Task<IActionResult> FindPetsByTags(List<string> tags)
        {
            var pets = new List<Pet>();

            foreach (Pet pet in Pets.Values)
            {
                var foundall = true;
                foreach (string tag in tags)
                {
                    var found = false;
                    foreach (Tag petTag in pet.Tags)
                        if (petTag.Name.Equals(tag, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                    {
                        foundall = false;
                        break;
                    }
                }
                if (foundall)
                    pets.Add(pet);
            }

            return new ObjectResult(pets);
        }



        #endregion
        #region store/orders
        // Returns Order in ObjectResult
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            long newKey = 0;
            foreach (long key in Orders.Keys)
                newKey = Math.Max(newKey, key);
            newKey++;
            order.Id = newKey;
            Orders.Add(newKey, order);
            return new ObjectResult(order);
        }

        // Returns Order in ObjectResult
        public async Task<IActionResult> GetOrderById(long id)
        {
            if (Orders.ContainsKey(id))
                return new ObjectResult(Orders[id]);

            return new ObjectResult(null) { StatusCode = 404 };
        }

        // Returns Status Code
        public async Task<IActionResult> DeleteOrder(long id)
        {
            if (Orders.ContainsKey(id))
            {
                Orders.Remove(id);
                return new ObjectResult(null);
            }
            return new ObjectResult(null) { StatusCode = 404 };
        }

        // Returns List<Order> in ObjectResult
        public async Task<IActionResult> OrderList()
        {
            return new ObjectResult(Orders.Values.ToList<Order>());
        }
        #endregion

    }
}
