using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PetStoreApi.Models;

namespace PetStoreRepo
{
    public interface IPetStoreRepo
    {
        Dictionary<long, Category> Categories { get; set; }
        Dictionary<long, Order> Orders { get; set; }
        Dictionary<long, Pet> Pets { get; set; }
        Dictionary<long, Tag> Tags { get; set; }

        Task<IActionResult> AddPet(Pet pet);
        Task<IActionResult> DeleteOrder(long id);
        Task<IActionResult> DeletePet(long id);
        Task<IActionResult> FindPetsByStatus(List<string> statuses);
        Task<IActionResult> FindPetsByTags(List<string> tags);
        Task<IActionResult> GetInventory();
        Task<IActionResult> GetOrderById(long id);
        Task<IActionResult> GetPetById(long id);
        Task<IActionResult> OrderList();
        Task<IActionResult> PlaceOrder(Order order);
        Task<IActionResult> UpdatePet(Pet pet);
        Task<IActionResult> UpdatePetWithForm(long petId, string name, string status);
    }
}