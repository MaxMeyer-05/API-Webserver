using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Database.Entities;

/// <summary>
/// Represents an ingredient and its required amount within a recipe.
/// </summary>
[Table("recipe_ingredients")]
public partial class RecipeIngredient
{
    /// <summary>
    /// Defines the unique identifier for the recipe ingredient entry.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Defines the identifier of the associated recipe.
    /// </summary>
    public int RecipeId { get; set; }

    /// <summary>
    /// Defines the identifier of the associated ingredient.
    /// </summary>
    public int IngredientId { get; set; }

    /// <summary>Defines the amount of the ingredient required by the recipe.</summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>Defines the ingredient required by the recipe.</summary>
    [ForeignKey(nameof(IngredientId))]
    public virtual Ingredient Ingredient { get; set; } = null!;

    /// <summary>Defines the recipe that requires this ingredient.</summary>
    [ForeignKey(nameof(RecipeId))]
    public virtual Recipe Recipe { get; set; } = null!;
}
