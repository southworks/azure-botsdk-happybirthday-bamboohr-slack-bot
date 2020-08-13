using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var originalData = GetEmployeeAPI();
            var transformData = TransformData(originalData);
            //log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            log.LogInformation(transformData);

        }

        private static string GetEmployeeAPI()
        {
            string jsonHrEmployees = "{\"title\":\"\",\"fields\":[{\"id\":\"firstName\",\"type\":\"text\",\"name\":\"First Name\"},{\"id\":\"middleName\",\"type\":\"text\",\"name\":\"Middle Name\"},{\"id\":\"lastName\",\"type\":\"text\",\"name\":\"Last Name\"},{\"id\":\"preferredName\",\"type\":\"text\",\"name\":\"Nickname\"},{\"id\":\"displayName\",\"type\":\"text\",\"name\":\"Display Name\"},{\"id\":\"dateOfBirth\",\"type\":\"date\",\"name\":\"Birth Date\"},{\"id\":\"workEmail\",\"type\":\"email\",\"name\":\"Work Email\"},{\"id\":\"hireDate\",\"type\":\"date\",\"name\":\"Hire Date\"},{\"id\":\"terminationDate\",\"type\":\"date\",\"name\":\"Termination Date\"}],\"employees\":[{\"id\":\"416\",\"firstName\":\"Juan\",\"middleName\":\"Alejandro\",\"lastName\":\"Arguello\",\"preferredName\":null,\"displayName\":\"Juan Alejandro Arguello\",\"dateOfBirth\":\"1978-04-06\",\"workEmail\":\"juan.arguello@southworks.com\",\"hireDate\":\"2006-10-19\",\"terminationDate\":\"0000-00-00\"},{\"id\":\"439\",\"firstName\":\"Ezequiel\",\"middleName\":null,\"lastName\":\"Jadib\",\"preferredName\":null,\"displayName\":\"Ezequiel Jadib\",\"dateOfBirth\":\"1982-11-07\",\"workEmail\":\"ezequiel.jadib@southworks.com\",\"hireDate\":\"2007-02-19\",\"terminationDate\":\"0000-00-00\"}]}";
            return jsonHrEmployees;
        }

        private static string TransformData(string originalData)
        {
            List<OriginalStructureBambooHR.Employee> listEmployees = DeserializeJsonEmployees(originalData);
            var jsonFormatEmployees = SerializeJsonEmployees(listEmployees);
            return jsonFormatEmployees;
        }
        private static List<OriginalStructureBambooHR.Employee> DeserializeJsonEmployees(string jsonEmployees)
        {
            var employees = new List<OriginalStructureBambooHR.Employee>();
            var listOriginalStructureBambooHR = JsonConvert.DeserializeObject<OriginalStructureBambooHR.Root>(jsonEmployees);
            foreach (var employee in listOriginalStructureBambooHR.employees)
            {
                employees.Add(employee);
            }
            return employees;
        }
        private static string SerializeJsonEmployees(List<OriginalStructureBambooHR.Employee> users)
        {
            List<ModifiedStructureBambooHREmployee> listEmployees = new List<ModifiedStructureBambooHREmployee>();
            foreach (var employee in users)
            {
                listEmployees.Add(new ModifiedStructureBambooHREmployee
                {
                    Birthday = employee.dateOfBirth,
                    Email = employee.workEmail,
                });
            }
            string jsonFormatEmployees = JsonConvert.SerializeObject(listEmployees, Formatting.Indented);
            return jsonFormatEmployees;
        }

        public class OriginalStructureBambooHR
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
        public class ModifiedStructureBambooHREmployee
        {
            public string Birthday { get; set; }
            public string Email { get; set; }
        }
    }
}
