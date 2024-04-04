// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Global suppressions
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly", Justification = "They are")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "No")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Competing Rules")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000:Keywords should be spaced correctly", Justification = "Competing Rules")]
[assembly: SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty", Justification = "Can't use Interface in this situation.")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1010:Opening square brackets should be spaced correctly", Justification = "Incompatible with new collection style/format.")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1127:Generic type constraints should be on their own line", Justification = "No methods have more than one.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1612:Element parameter documentation should match element parameters", Justification = "Creates issues w/ Extension Methods.")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Dont use _")]

// Specific suppressions
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Const", Scope = "member", Target = "~F:SatialInterfaces.Controls.Calendar.CalendarControl.HoursPerDay")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Const", Scope = "member", Target = "~F:SatialInterfaces.Controls.Calendar.CalendarControl.MaxDays")]
[assembly: SuppressMessage("Major Code Smell", "S6562:Always set the \"DateTimeKind\" when creating new \"DateTime\" instances", Justification = "This instance is empty.", Scope = "member", Target = "~F:SatialInterfaces.Controls.Calendar.AppointmentControl.EmptyDateTime")]