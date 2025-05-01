using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobNest
{
    public class Vacancy
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public int Salary { get; set; }
        public string Region { get; set; }
        public string Schedule { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Education { get; set; }
        public string ExperienceRequirement { get; set; }

        public int EmployerId { get; set; }
        public string EmployerName { get; set; }
        public string EmployerCompany { get; set; }

        public string EmployerFirstName { get; set; }
        public string EmployerLastName { get; set; }

    }
}
