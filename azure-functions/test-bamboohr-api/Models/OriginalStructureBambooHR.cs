using System;
using System.Collections.Generic;
using System.Text;

namespace test_bamboohr_api.Models
{
    class OriginalStructureBambooHR
    {
        public class Field
        {
            public string id { get; set; }
            public string type { get; set; }
            public string name { get; set; }
        }

        public class Employee
        {
            public string id { get; set; }
            public string firstName { get; set; }
            public string middleName { get; set; }
            public string lastName { get; set; }
            public object preferredName { get; set; }
            public string displayName { get; set; }
            public string dateOfBirth { get; set; }
            public string workEmail { get; set; }
            public string hireDate { get; set; }
            public string terminationDate { get; set; }
        }

        public class Root
        {
            public string title { get; set; }
            public List<Field> fields { get; set; }
            public List<Employee> employees { get; set; }
        }
    }
}
