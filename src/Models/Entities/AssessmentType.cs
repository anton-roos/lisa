using System.ComponentModel.DataAnnotations;

namespace Lisa.Models.Entities
{
    public class AssessmentType
    {
        public int Id { get; set; }
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;
    }
}
