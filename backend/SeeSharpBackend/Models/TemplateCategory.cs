using System.ComponentModel.DataAnnotations;

namespace SeeSharpBackend.Models
{
    /// <summary>
    /// Template category model for organizing code templates
    /// </summary>
    public class TemplateCategory
    {
        /// <summary>
        /// Category ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Category description
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category icon (emoji or icon class)
        /// </summary>
        [MaxLength(50)]
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Display order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Whether this category is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Number of templates in this category
        /// </summary>
        public int TemplateCount { get; set; }
    }
}