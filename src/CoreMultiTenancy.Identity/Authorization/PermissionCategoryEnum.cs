using System.ComponentModel.DataAnnotations;

namespace CoreMultiTenancy.Identity.Authorization
{
    /// <summary>
    /// Main application permission categories represented as an enum. Changing underlying byte 
    /// value WILL introduce breaking changes to database. Each value must have a DisplayAttribute 
    /// setting its Name value. Use [Obsolete] if one is deprecated.
    /// </summary>
    public enum PermissionCategoryEnum : byte
    {
        [Display(Name = "Default")]
        Default = 0,

        [Display(Name = "Resource Management (Aircraft, Simulators, etc.)")]
        Resource = 1,
    }
}