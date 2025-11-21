using System.Collections.Generic;
using Laboration.Models;

namespace Laboration.ViewModels
{
    public class PersonCreateViewModel
    {
        public Person Person { get; set; } = new Person();

        // Alla möjliga rätter att välja
        public List<Dish> AvailableDishes { get; set; } = new List<Dish>();

        // De rätter som användaren kryssar i i formuläret
        public List<int> SelectedDishIds { get; set; } = new List<int>();
    }
}
