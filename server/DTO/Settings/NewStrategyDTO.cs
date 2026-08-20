using System.ComponentModel.DataAnnotations;

namespace Server.DTO.Settings;

public class NewStrategyDTO
{
  [Required(ErrorMessage = "Name field is required")]
  public string Name { get; set; } = string.Empty;
  [Required(ErrorMessage = "Description field is required")]
  public string Description { get; set; } = string.Empty;
}
