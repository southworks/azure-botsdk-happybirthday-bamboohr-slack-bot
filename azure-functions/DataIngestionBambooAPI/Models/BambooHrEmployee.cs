using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataIngestionBambooAPI.Models
{
    public class DirectoryResponse
    {
        public List<BambooHrEmployee> Employees { get; set; }
    }

    [XmlType(TypeName = "employee")]
    public class BambooHrEmployee
    {
        public string WorkEmail { get; set; }
        public string DateOfBirth { get; set; }
        public string TerminationDate { get; set; }
    }
}
