using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ElmanhagPlatform.ViewModels
{
    public class EditCardVM
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public string ExpireAtString { get; set; }

        [NotMapped]
        public DateOnly ExpireAt { get; set; }
    }

}
