using System.Diagnostics.CodeAnalysis;


[assembly: SuppressMessage(
    "Major Code Smell",
    "S6206:Method can be static",
    Justification = "Los repositorios usan EF, DI, y contextos virtuales. No deben ser estáticos.",
    Scope = "namespaceanddescendants",
    Target = "~N:DamasChinas_Server"
)]